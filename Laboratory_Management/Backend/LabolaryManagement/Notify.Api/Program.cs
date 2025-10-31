using MassTransit;
using Microsoft.EntityFrameworkCore;
using Messaging.Email;
using Notify.App.Consumers;
using Notify.Infrastructure;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var conn = builder.Configuration.GetConnectionString("Notify") ?? builder.Configuration["ConnectionStrings:Notify"] ?? "Server=localhost;Database=notify;Trusted_Connection=True;TrustServerCertificate=True";
builder.Services.AddDbContext<NotifyDbContext>(opt => opt.UseSqlServer(conn));

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, FileEmailTemplateRenderer>();

const string notifyExchange = "lab.notify.v1";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationRequestedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";
        cfg.Host(host, h => { h.Username(user); h.Password(pass); });

        cfg.Message<Contracts.Notifications.NotificationRequestedV1>(m => m.SetEntityName(notifyExchange));
        cfg.Publish<Contracts.Notifications.NotificationRequestedV1>(p =>
        {
            p.ExchangeType = ExchangeType.Topic;
            p.Durable = true;
            p.AutoDelete = false;
        });

        cfg.ReceiveEndpoint("notify.email", e =>
        {
            e.Bind(notifyExchange, x => { x.RoutingKey = "email"; x.ExchangeType = ExchangeType.Topic; });
            // Retry đúng yêu cầu: 3 lần exponential (5s, 15s, 30s)
            e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<NotificationRequestedConsumer>(context);
        });
    });
});

var app = builder.Build();

// Ensure DB exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotifyDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Ok("Notify up"));
app.MapHealthChecks("/healthz");

app.Run();
