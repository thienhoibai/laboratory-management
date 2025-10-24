using IAM.Application.Auth;
using IAM.Application.Auth.DTOs;
using IAM.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Security.Authorization;
using Security.Jwt;
using IAM.Presentation.Middlewares;
using IAM.Application.Security;
using IAM.Application.Users;
using Common.Web.Extensions;
using Messaging.Email;
using Messaging.Notifications;
using IAM.Presentation.Grpc;
using IAM.Infrastructure.Outbox;
using IAM.Infrastructure.Notifications;
using MassTransit;
using Contracts.Notifications;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddHealthChecks(); 

// MVC + Filters
builder.Services.AddControllers();
builder.Services.AddStandardApi();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Email & Notifications
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();
// Replace logging publisher with outbox-backed
builder.Services.AddScoped<INotificationPublisher, OutboxNotificationPublisher>();

// Application services (DI)
builder.Services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authorization dynamic permissions
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Outbox services
builder.Services.AddScoped<OutboxWriter>();
builder.Services.AddHostedService<OutboxProcessor>();

// DbContext
var conn = builder.Configuration.GetConnectionString("LabIAM") ?? builder.Configuration["ConnectionStrings:LabIAM"] ?? "Server=localhost;Database=LabIAM;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<IamDbContext>(opt =>
{
    opt.UseSqlServer(conn);
});

const string notifyExchange = "lab.notify.v1";

// MassTransit publish topology for notifications
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";
        cfg.Host(host, h => { h.Username(user); h.Password(pass); });

        // Set logical entity (exchange) name and type
        cfg.Message<NotificationRequestedV1>(m => m.SetEntityName(notifyExchange));
        cfg.Publish<NotificationRequestedV1>(p =>
        {
            p.ExchangeType = ExchangeType.Topic;
            p.Durable = true;
            p.AutoDelete = false;
        });
    });
});

// AuthN & AuthZ
var issuer = builder.Configuration["Jwt:Issuer"] ?? "lab-iam";
var audience = builder.Configuration["Jwt:Audience"] ?? "lab-services";
var signingKey = builder.Configuration["Jwt:SigningKey"] ?? "DevSecretKey_MustBe_AtLeast_32Chars!!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5174",   
            "http://127.0.0.1:5174"   
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers();
// gRPC + reflection for tooling
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Ensure DB exists + outbox schema
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
    db.Database.Migrate();
    await OutboxSchemaInitializer.EnsureCreatedAsync(db);
}

app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<IamGrpcUserService>();
if (app.Environment.IsDevelopment()) app.MapGrpcReflectionService();

app.MapGet("/", () => Results.Ok("IAM up"));
app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();
