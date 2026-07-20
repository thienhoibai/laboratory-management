using StackExchange.Redis;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Messaging.Email;
using Notify.App.Consumers;
using Notify.Infrastructure;
using Notify.Api.Services;
using System.Globalization;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var conn = builder.Configuration.GetConnectionString("Notify") ?? builder.Configuration["ConnectionStrings:Notify"] ?? "Host=localhost;Database=notify;Username=postgres;Password=12345";
builder.Services.AddDbContext<NotifyDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();

// Register Redis Connection Multiplexer
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

// Register Consumer and Subscriber Background Service
builder.Services.AddScoped<NotificationRequestedConsumer>();
builder.Services.AddHostedService<RedisNotificationSubscriberService>();

var app = builder.Build();

// ✅ Set default culture to vi-VN
var supportedCultures = new[] { new CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Ensure DB exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotifyDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok("Notify up"));
app.MapHealthChecks("/healthz");

app.Run();
