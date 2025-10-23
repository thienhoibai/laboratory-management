using IAM.Application.Auth;
using IAM.Application.Auth.DTOs;
using IAM.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddHealthChecks(); 
// Email & Notifications
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();
builder.Services.AddSingleton<INotificationPublisher, LoggingNotificationPublisher>();


// DbContext
var conn = builder.Configuration.GetConnectionString("LabIAM") ?? builder.Configuration["ConnectionStrings:LabIAM"] ?? "Server=localhost;Database=LabIAM;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<IamDbContext>(opt =>
{
    opt.UseSqlServer(conn);
});

// AuthN
var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// AuthZ
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();

// Filters via common library (under Common/Web/* compiled in Common project)
builder.Services.AddStandardApi();

// App services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IAM API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }, new string[] { }
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

// gRPC + reflection for tooling
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// gRPC (chọn đúng lớp service của bạn)
app.MapGrpcService<IamGrpcUserService>(); // hoặc UserService

// Reflection chỉ bật khi dev
if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService();

// Health & ping cho gateway
app.MapGet("/", () => Results.Ok("IAM up"));
app.MapHealthChecks("/healthz");

// REST controllers
app.MapControllers();

app.Run();
