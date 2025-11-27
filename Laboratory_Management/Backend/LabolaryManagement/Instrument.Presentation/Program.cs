using Instrument.Application.Results;
using Instrument.Application.Services;
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
builder.Services.AddSwaggerGen();

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
app.MapControllers();
app.Run();
