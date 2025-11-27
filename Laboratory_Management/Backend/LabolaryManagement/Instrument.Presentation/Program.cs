using Instrument.Application.Results;
using Instrument.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Instrument.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<InstrumentDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("InstrumentDb")));

builder.Services.AddHttpClient("testorder", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["TestOrderBaseUrl"]!);
});

// ===== JWT Authentication =====
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

// ===== Authorization Policies =====
builder.Services.AddAuthorization(options =>
{
    // Instrument permissions
    string[] instrumentPerms = new[]
    {
        "Instrument.List",
        "Instrument.View",
        "Instrument.Create",
        "Instrument.Update",
        "Instrument.Delete"
    };

    // Run permissions
    string[] runPerms = new[]
    {
        "Run.Start",
        "Run.View",
        "Run.Complete",
        "Run.Cancel"
    };

    var allPerms = instrumentPerms.Concat(runPerms);

    foreach (var p in allPerms)
    {
        options.AddPolicy($"perm:{p}", policy =>
            policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin")
                || ctx.User.HasClaim("perm", p)
                || ctx.User.HasClaim("permissions", p)
                || ctx.User.HasClaim("scope", p)));
    }
});

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

// Register Application Services
builder.Services.AddScoped<IResultGenerator, ResultGenerator>();
builder.Services.AddScoped<InstrumentService>();
builder.Services.AddScoped<RunService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Instrument API",
        Version = "v1",
        Description = "Laboratory Management - Instrument Service API"
    });

    // Add JWT Authentication to Swagger
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

var app = builder.Build();

var isDocker = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Docker", StringComparison.OrdinalIgnoreCase);

if (app.Environment.IsDevelopment() || isDocker)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Do not redirect to HTTPS inside container (no dev certs)
if (!isDocker)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/Images"
});

app.UseRouting();
app.UseCors("AllowFrontend");

// ✅ QUAN TRỌNG: Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
