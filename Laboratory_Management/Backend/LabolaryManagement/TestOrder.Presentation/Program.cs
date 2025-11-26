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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
