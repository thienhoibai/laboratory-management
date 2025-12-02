using BlogService.Application.Services;
using BlogService.Application.Statistics.Services; // ✅ Add Statistics
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Repository;
using Common.Authorization; // ✅ Thêm
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization; // ✅ Thêm
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace BlogService.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddDbContext<DBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Dependency Injection for Repositories and Services
            builder.Services.AddScoped<TagRepository>();
            builder.Services.AddScoped<TagService>();
            builder.Services.AddScoped<BlogPostService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<BlogPostRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<BlogTagService>();
            builder.Services.AddScoped<BlogPostTagRepository>();
            builder.Services.AddScoped<CommentRepository>();
            builder.Services.AddScoped<CommentService>();
            builder.Services.AddScoped<BlogStatisticsService>(); // ✅ Add Statistics Service

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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BlogService API",
                    Version = "v1",
                    Description = "Laboratory Management - BlogService API"
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

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:5174",
                        "http://127.0.0.1:5174",
                        "http://hema-link.io.vn"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            var app = builder.Build();

            var isDocker = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Docker", StringComparison.OrdinalIgnoreCase);

            // Configure the HTTP request pipeline.
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

            app.UseRouting();
            app.UseCors("AllowFrontend");
            
            // ✅ QUAN TRỌNG: Authentication phải đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Ensure Images directory exists
            var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imagesPath),
                RequestPath = "/Images"
            });

            app.MapControllers();

            app.Run();
        }
    }
}
