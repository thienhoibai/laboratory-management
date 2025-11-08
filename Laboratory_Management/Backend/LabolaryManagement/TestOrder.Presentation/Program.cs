using Microsoft.EntityFrameworkCore;
using TestOrder.Application.Services;
using TestOrder.Application.Services.Booking;
using TestOrder.Application.Services.Payment;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddDbContext<TestOrderDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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
            builder.Services.AddScoped<IVnPayService,PaymentService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PaymentRepository>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
