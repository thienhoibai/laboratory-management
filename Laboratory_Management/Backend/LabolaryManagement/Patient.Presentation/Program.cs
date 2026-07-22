using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Iam.Grpc;
using Common.Authorization; // ✅ Thêm
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization; // ✅ Thêm
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Patient.Application.Messaging;
using Patient.Application.Security;
using Patient.Application.Services;
using Patient.Infrastructure;
using Patient.Presentation.Infrastructure;
using System.Text;
using Common.Web.Extensions;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// MVC + Authorization
builder.Services.AddStandardApi();

// Authentication + Authorization
var issuer = builder.Configuration["Jwt:Issuer"] ?? "lab-iam";
var audience = builder.Configuration["Jwt:Audience"] ?? "lab.api";
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

// ✅ ===== DYNAMIC AUTHORIZATION =====
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Patient API",
    Version = "v1",
    Description = "Laboratory Management - Patient Service API"
});

// Add JWT Authentication
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                  "Enter your token in the text input below.\r\n\r\n" +
                  "Example: '12345abcdef'"
});

options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
        new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHealthChecks();

// Security: AES-GCM PII protector
builder.Services.AddSingleton<ISensitiveDataProtector, AesGcmProtector>();

// DbContext - SQL Server only (removed InMemory database logic)
var connectionString = builder.Configuration.GetConnectionString("PatientService4")
    ?? throw new InvalidOperationException("Connection string 'PatientService4' not found in appsettings.json");

Console.WriteLine($"✅ Using PostgreSQL: {connectionString}");
builder.Services.AddDbContext<PatientDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IPatientEventPublisher, MassTransitPatientEventPublisher>();

// gRPC Client
// Dùng gRPC-Web để gọi được IAM qua proxy HTTP/1.1 của Render.
// Local/docker-compose vẫn chạy tốt vì gRPC-Web tương thích cả HTTP/2 lẫn HTTP/1.1.
builder.Services.AddScoped<UserService.UserServiceClient>(provider =>
{
    var url = builder.Configuration["Grpc:IamUrl"] ?? "http://localhost:5001";
    var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
    var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpHandler = httpHandler });
    return new UserService.UserServiceClient(channel);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5174",
            "http://127.0.0.1:5174",
            "http://hema-link.io.vn",
            "https://laboratory-management-fe.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
    db.Database.EnsureCreated();
    Console.WriteLine("✅ Database connection verified and schema ensured");
}

app.MapGet("/", () => Results.Ok("Patient up"));
app.MapHealthChecks("/healthz");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
