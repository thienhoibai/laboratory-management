using IAM.Application.Auth.Services;
using IAM.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Security.Authorization;
using Security.Jwt;
using IAM.Presentation.Middlewares;
using IAM.Application.Security;
using IAM.Application.Users.Services;
using Common.Web.Extensions;
using Common.Authorization;
using Messaging.Email;
using Messaging.Notifications;
using IAM.Presentation.Grpc;
using IAM.Infrastructure.Notifications;
using MassTransit;
using Contracts.Notifications;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using IAM.Application.Roles.Services;
using IAM.Application.Permissions;
using IAM.Application.Statistics.Services; // ✅ Add Statistics

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IAM API",
        Version = "v1",
        Description = "Laboratory Management - IAM Service API"
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

// Email renderer for templates
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();

// Application services (DI)
builder.Services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IPermissionQuery, PermissionQuery>();
builder.Services.AddScoped<UserStatisticsService>(); // ✅ Add Statistics Service

// Notifications via MassTransit -> RabbitMQ (no Outbox)
builder.Services.AddScoped<INotificationPublisher, MassTransitNotificationPublisher>();

const string notifyExchange = "lab.notify.v1";

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";
        cfg.Host(host, h => { h.Username(user); h.Password(pass); });

        cfg.Message<NotificationRequestedV1>(m => m.SetEntityName(notifyExchange));
        cfg.Publish<NotificationRequestedV1>(p =>
        {
            p.ExchangeType = ExchangeType.Topic;
            p.Durable = true;
            p.AutoDelete = false;
        });
    });
});

// DbContext
var conn = builder.Configuration.GetConnectionString("LabIAM") ?? builder.Configuration["ConnectionStrings:LabIAM"] ?? "Server=localhost;Database=LabIAM;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<IamDbContext>(opt =>
{
    opt.UseSqlServer(conn, sql =>
    {
        // ❌ DISABLED: EnableRetryOnFailure conflicts with BeginTransaction in RolePermissionService
        // If you need retry logic, wrap transactions with CreateExecutionStrategy()
        sql.CommandTimeout(180);
    });
});

// AuthN & AuthZ
var issuer = builder.Configuration["Jwt:Issuer"] ?? "lab-iam";
var audience = builder.Configuration["Jwt:Audience"] ?? "lab.api";
var signingKey = builder.Configuration["Jwt:SigningKey"] ?? "Jx6n2QvB5pTf8Kz3Wm9aS4Ld7Yh0Nr2Xu8Cj5Pk1Vg3Mz7Rb0Hq4Tn6Wy8Le2";

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

// ✅ ===== DYNAMIC AUTHORIZATION =====
builder.Services.AddSingleton<IAuthorizationPolicyProvider, Common.Authorization.DynamicAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, Common.Authorization.PermissionAuthorizationHandler>();
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
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed. Connection: {Conn}", conn);
        throw;
    }
}

app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGrpcService<IamGrpcUserService>();
if (app.Environment.IsDevelopment()) app.MapGrpcReflectionService();

app.MapGet("/", () => Results.Ok("IAM up"));
app.MapHealthChecks("/healthz");

app.Run();
