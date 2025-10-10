
using Microsoft.EntityFrameworkCore;
using Patient.Application;
using Patient.Infrastructure.Data;
using Patient.Infrastructure.Repository;


namespace Patient.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // 2. Add DbContext for PatientDB
            builder.Services.AddDbContext<PatientDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("PatientDB")));
            builder.Services.AddScoped<IPatientRepositories, PatientRepositories>();
            builder.Services.AddScoped<PatientService>();


            // 3. Register dependencies (DI)
            builder.Services.AddScoped<IPatientRepositories, PatientRepositories>();
            //builder.Services.AddScoped<IMedicalRecordRepositories, MedicalRecordRepositories>();


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
