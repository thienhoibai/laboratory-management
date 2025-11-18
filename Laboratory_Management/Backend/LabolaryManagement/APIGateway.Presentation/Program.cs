using Yarp.ReverseProxy;
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// No HTTPS redirection to support HTTP-only and gRPC passthrough
app.MapGet("/", () => Results.Ok("Gateway up"));

app.MapReverseProxy();

app.Run();
