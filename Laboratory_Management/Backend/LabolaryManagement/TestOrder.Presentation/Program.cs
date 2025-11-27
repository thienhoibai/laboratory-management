using Microsoft.EntityFrameworkCore;
using TestOrder.Application.Services;
using TestOrder.Application.Services.Booking;
using TestOrder.Application.Services.Payment;
using TestOrder.Application.Services.InstrumentBridge;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Repository;
using MassTransit;
using Contracts.Notifications;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace TestOrder.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // ===== DbContext Configuration =====
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Sử dụng PooledDbContextFactory cho cả Controllers và Background Services
            builder.Services.AddPooledDbContextFactory<TestOrderDBContext>(options =>
                options.UseSqlServer(connectionString));


            // Đăng ký DbContext với Scoped lifetime để inject vào Controllers/Services
            builder.Services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IDbContextFactory<TestOrderDBContext>>();
                return factory.CreateDbContext();
            });

            // Đăng ký HttpClient factory
            builder.Services.AddHttpClient();

            // CSV Ingest Worker options & hosted service
            builder.Services.Configure<TestOrder.Presentation.Workers.CsvIngestOptions>(
                builder.Configuration.GetSection("CsvIngest"));
            builder.Services.AddHostedService<TestOrder.Presentation.Workers.CsvIngestWorker>();

            // Dependency Injection for Repositories and Services
            builder.Services.AddScoped(typeof(GenericRepository<>));
            builder.Services.AddScoped<TestCatalogRepository>();
            builder.Services.AddScoped<TestCatalogService>();
            builder.Services.AddScoped<TestBundleRepository>();
            builder.Services.AddScoped<TestBundleService>();
            builder.Services.AddScoped<TestParameterRepository>();
            builder.Services.AddScoped<TestParameterService>();
            builder.Services.AddScoped<CatalogBundleRepository>();
            builder.Services.AddScoped<CatalogBundleService>();
            builder.Services.AddScoped<AppointmentSlotRepository>();
            builder.Services.AddScoped<AppointmentSlotService>();
            builder.Services.AddScoped<BookingRepository>();
            builder.Services.AddScoped<BookingService>();
            builder.Services.AddScoped<BookingTestService>();
            builder.Services.AddScoped<BookingTestRepository>();
            builder.Services.AddScoped<TimeBlockRepository>();
            builder.Services.AddScoped<TestResultRepository>();
            builder.Services.AddScoped<TestResultService>();

            // InstrumentBridge Service
            builder.Services.AddScoped<InstrumentBridgeService>();

            // Named client patient
            var patientBase = builder.Configuration["PatientServiceBaseUrl"];
            if (!string.IsNullOrWhiteSpace(patientBase))
            {
                builder.Services.AddHttpClient("patient", c => c.BaseAddress = new Uri(patientBase));
            }

            builder.Services.AddScoped<IVnPayService, PaymentService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PaymentRepository>();

            // ✅ THÊM MASSTRANSIT + RABBITMQ
            const string notifyExchange = "lab.notify.v1";
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
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
                // Booking permissions
                string[] bookingPerms = new[]
                {
                    "Booking.List",
                    "Booking.View",
                    "Booking.View.Own",
                    "Booking.Create",
                    "Booking.Update",
                    "Booking.Update.CheckIn",
                    "Booking.Update.CheckOut",
                    "Booking.Delete"
                };

                // TestCatalog permissions
                string[] catalogPerms = new[]
                {
                    "TestCatalog.List",
                    "TestCatalog.View",
                    "TestCatalog.Create",
                    "TestCatalog.Update",
                    "TestCatalog.Delete",
                    "TestCatalog.UpdateParameter",
                    "TestCatalog.DeleteParameter"
                };

                // TestBundle permissions
                string[] bundlePerms = new[]
                {
                    "TestBundle.List",
                    "TestBundle.View",
                    "TestBundle.Create",
                    "TestBundle.Update",
                    "TestBundle.Delete"
                };

                // CatalogBundle permissions
                string[] catalogBundlePerms = new[]
                {
                    "CatalogBundle.List",
                    "CatalogBundle.View",
                    "CatalogBundle.Create",
                    "CatalogBundle.Delete"
                };

                // TestParameter permissions
                string[] parameterPerms = new[]
                {
                    "TestParameter.List",
                    "TestParameter.View",
                    "TestParameter.Create",
                    "TestParameter.Delete"
                };

                // AppointmentSlot permissions
                string[] slotPerms = new[]
                {
                    "AppointmentSlot.List",
                    "AppointmentSlot.View",
                    "AppointmentSlot.Create",
                    "AppointmentSlot.Update",
                    "AppointmentSlot.Delete",
                    "AppointmentSlot.ByDate.View",
                    "AppointmentSlot.CountByDate.View",
                    "AppointmentSlot.CountAll.View"
                };

                // TestResult permissions
                string[] resultPerms = new[]
                {
                    "TestResult.List",
                    "TestResult.View",
                    "TestResult.Create",
                    "TestResult.Update",
                    "TestResult.Delete",
                    "TestResult.Approve"
                };

                // Payment permissions
                string[] paymentPerms = new[]
                {
                    "Payment.List",
                    "Payment.View",
                    "Payment.ByBooking.View",
                    "Payment.Create",
                    "Payment.Process"
                };

                var allPerms = bookingPerms
                    .Concat(catalogPerms)
                    .Concat(bundlePerms)
                    .Concat(catalogBundlePerms)
                    .Concat(parameterPerms)
                    .Concat(slotPerms)
                    .Concat(resultPerms)
                    .Concat(paymentPerms);

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
                    Title = "TestOrder API",
                    Version = "v1",
                    Description = "Laboratory Management - TestOrder Service API"
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
                .AddJsonOptions(x =>
                    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
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

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment() || isDocker)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Do not redirect to HTTPS inside container
            if (!isDocker)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseCors("AllowFrontend");
            
            // ✅ QUAN TRỌNG: Authentication phải đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}
