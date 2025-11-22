using Yarp.ReverseProxy;
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

// No HTTPS redirection to support HTTP-only and gRPC passthrough
app.MapGet("/", () => Results.Ok("Gateway up"));
app.UseCors("AllowAll");

app.MapReverseProxy();

app.Run();
