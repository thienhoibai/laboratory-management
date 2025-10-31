
using Microsoft.EntityFrameworkCore;
using TestOrder.Application.Services;
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
            builder.Services.AddScoped<CatalogParameterService>();
            builder.Services.AddScoped<CatalogParameterRepository>();
            builder.Services.AddScoped<AppointmentSlotRepository>();
            builder.Services.AddScoped<AppointmentSlotService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
