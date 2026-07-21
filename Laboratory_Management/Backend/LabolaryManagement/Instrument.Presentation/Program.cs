using Instrument.Application.Results;
using Instrument.Application.Services;
using Instrument.Application.Statistics.Services; // ✅ Add Statistics
using Common.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Instrument.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Common.Web.Extensions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStandardApi();

builder.Services.AddDbContext<InstrumentDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("InstrumentDb")));

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

// ✅ ===== DYNAMIC AUTHORIZATION =====
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

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

// Register Application Services
builder.Services.AddScoped<IResultGenerator, ResultGenerator>();
builder.Services.AddScoped<InstrumentService>();
builder.Services.AddScoped<RunService>();
builder.Services.AddScoped<InstrumentStatisticsService>(); // ✅ Add Statistics Service

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Instrument API",
        Version = "v1",
        Description = "Laboratory Management - Instrument Service API"
    });

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

// Ensure database schema exists (PostgreSQL)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InstrumentDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/healthz", () => Results.Ok("Instrument up"));

var isDocker = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Docker", StringComparison.OrdinalIgnoreCase);

if (app.Environment.IsDevelopment() || isDocker)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!isDocker)
{
    app.UseHttpsRedirection();
}

// ✅ TẠO THƯ MỤC IMAGES TRƯỚC KHI CONFIGURE STATIC FILES
var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"🔍 ContentRootPath: {builder.Environment.ContentRootPath}");
logger.LogInformation($"🔍 Images path will be: {imagesPath}");

if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
    logger.LogInformation($"✅ Created Images directory at: {imagesPath}");
}
else
{
    logger.LogInformation($"✅ Images directory exists at: {imagesPath}");
    // List existing files
    var files = Directory.GetFiles(imagesPath);
    logger.LogInformation($"📁 Found {files.Length} file(s) in Images directory");
}

// ✅ THỨ TỰ MIDDLEWARE QUAN TRỌNG!
app.UseRouting();
app.UseCors("AllowFrontend");

// ✅ Configure static files AFTER Routing and CORS
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/Images"
});
logger.LogInformation($"✅ Static files configured for /Images -> {imagesPath}");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

logger.LogInformation("🚀 Instrument API is starting...");
app.Run();
