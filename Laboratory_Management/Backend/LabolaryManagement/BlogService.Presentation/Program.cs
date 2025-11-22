using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using BlogService.Application.Services;

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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
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
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
