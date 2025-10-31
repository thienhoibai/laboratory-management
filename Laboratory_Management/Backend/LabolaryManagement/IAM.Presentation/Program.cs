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

// Email renderer for templates
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();

// Application services (DI)
builder.Services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

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
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        sql.CommandTimeout(30);
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

builder.Services.AddAuthorization(options =>
{
    // Register explicit permission policies expected by controllers
    string[] perms = new[]
    {
        "User.List","User.View","User.Create","User.Delete","User.Update","Role.Update"
    };
    foreach (var p in perms)
    {
        options.AddPolicy($"perm:{p}", policy =>
            policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin")
                || ctx.User.HasClaim("perm", p)
                || ctx.User.HasClaim("permissions", p)
                || ctx.User.HasClaim("scope", p)));
    }
});

// gRPC
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IamDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<IamGrpcUserService>();
if (app.Environment.IsDevelopment()) app.MapGrpcReflectionService();

app.MapGet("/", () => Results.Ok("IAM up"));
app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();
