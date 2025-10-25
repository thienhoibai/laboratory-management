using System.Text;
using Grpc.Net.Client;
using Iam.Grpc;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Patient.Application.Messaging;
using Patient.Application.Security;
using Patient.Application.Services;
using Patient.Infrastructure;
using Patient.Presentation.Infrastructure;
using Contracts.Notifications;
using Patient.Infrastructure.Outbox;
using RabbitMQ.Client;

// Allow gRPC over HTTP/2 (h2c) without TLS
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// MVC + Authorization
builder.Services.AddControllers();

// Authentication + Authorization
var issuer = builder.Configuration["Jwt:Issuer"] ?? "lab-iam";
var audience = builder.Configuration["Jwt:Audience"] ?? "lab-services";
var signingKey = builder.Configuration["Jwt:SigningKey"] ?? "Jx6n2QvB5pTf8Kz3Wm9aS4Ld7Yh0Nr2Xu8Cj5Pk1Vg3Mz7Rb0Hq4Tn6Wy8Le2";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// Swagger optional for demo
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks();

// Security: AES-GCM PII protector
builder.Services.AddSingleton<ISensitiveDataProtector, AesGcmProtector>();

// DbContext: allow InMemory for Docker demo
var useInMemory = builder.Configuration.GetValue("UseInMemoryDb", true);
if (useInMemory)
{
    builder.Services.AddDbContext<PatientDbContext>(opt => opt.UseInMemoryDatabase("PatientDb"));
}
else
{
    var conn = builder.Configuration.GetConnectionString("PatientService3");
    if (string.IsNullOrWhiteSpace(conn))
        conn = "Server=localhost;Database=PatientService3;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5";

    builder.Services.AddDbContext<PatientDbContext>(opt =>
        opt.UseSqlServer(conn, sql => sql.MigrationsAssembly("Patient.Migrations")));
}

builder.Services.AddScoped<IPatientService, PatientService>();

// Event publisher adapter
builder.Services.AddScoped<IPatientEventPublisher, MassTransitPatientEventPublisher>();

// Outbox services
builder.Services.AddScoped<OutboxWriter>();
builder.Services.AddHostedService<Patient.Infrastructure.Outbox.OutboxProcessor>();

// gRPC client to IAM (h2c). Use Grpc.Net.Client factory registration via generated client
builder.Services.AddGrpcClient<UserService.UserServiceClient>((sp, o) =>
{
    var url = builder.Configuration["Grpc:IamUrl"] ?? "http://iam.api:5001";
    o.Address = new Uri(url);
});

const string notifyExchange = "lab.notify.v1";

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";
        cfg.Host(host, h =>
        {
            h.Username(user);
            h.Password(pass);
        });
        cfg.Message<NotificationRequestedV1>(m => m.SetEntityName(notifyExchange));
        cfg.Publish<NotificationRequestedV1>(p =>
        {
            p.ExchangeType = ExchangeType.Topic;
            p.Durable = true;
            p.AutoDelete = false;
        });
    });
});

var app = builder.Build();

// Ensure DB exists when using real SQL
if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
    db.Database.Migrate();
    await Patient.Infrastructure.Outbox.OutboxSchemaInitializer.EnsureCreatedAsync(db);
}

app.MapGet("/", () => Results.Ok("Patient up"));
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// No HTTPS redirection for docker h2c
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
