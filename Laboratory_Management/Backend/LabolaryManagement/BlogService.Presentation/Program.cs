using BlogService.Application.Services;
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                // BlogPost permissions
                string[] blogPostPerms = new[]
                {
                    "BlogPost.List",
                    "BlogPost.View",
                    "BlogPost.Create",
                    "BlogPost.Update",
                    "BlogPost.Delete",
                    "BlogPost.Approved.View",
                    "BlogPost.Status.Update"
                };

                // BlogCategory permissions
                string[] categoryPerms = new[]
                {
                    "BlogCategory.List",
                    "BlogCategory.View",
                    "BlogCategory.Create",
                    "BlogCategory.Update",
                    "BlogCategory.Delete"
                };

                // Tag permissions
                string[] tagPerms = new[]
                {
                    "Tag.List",
                    "Tag.View",
                    "Tag.Create",
                    "Tag.Update",
                    "Tag.Delete"
                };

                // BlogTag permissions
                string[] blogTagPerms = new[]
                {
                    "BlogTag.BlogPost.View",
                    "BlogTag.Tag.View",
                    "BlogTag.Create",
                    "BlogTag.Delete"
                };

                // Comment permissions
                string[] commentPerms = new[]
                {
                    "Comment.Post.View",
                    "Comment.View",
                    "Comment.Create",
                    "Comment.Update",
                    "Comment.Delete",
                    "Comment.Search"
                };

                var allPerms = blogPostPerms
                    .Concat(categoryPerms)
                    .Concat(tagPerms)
                    .Concat(blogTagPerms)
                    .Concat(commentPerms);

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
                        "http://127.0.0.1:5174"
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
