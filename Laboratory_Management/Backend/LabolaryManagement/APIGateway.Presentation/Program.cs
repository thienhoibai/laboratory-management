using Yarp.ReverseProxy;
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Cho phép override địa chỉ backend qua env var khi deploy (Render/k8s),
// giữ nguyên giá trị trong appsettings.json khi chạy local/docker-compose
var destinationOverrides = new Dictionary<string, string?>
{
    ["ReverseProxy:Clusters:iam-rest:Destinations:iam:Address"] = Environment.GetEnvironmentVariable("IAM_URL"),
    ["ReverseProxy:Clusters:iam-grpc:Destinations:iam:Address"] = Environment.GetEnvironmentVariable("IAM_URL"),
    ["ReverseProxy:Clusters:patient-cluster:Destinations:patient:Address"] = Environment.GetEnvironmentVariable("PATIENT_URL"),
    ["ReverseProxy:Clusters:testorder-cluster:Destinations:testorder:Address"] = Environment.GetEnvironmentVariable("TESTORDER_URL"),
    ["ReverseProxy:Clusters:blog-cluster:Destinations:blog:Address"] = Environment.GetEnvironmentVariable("BLOG_URL"),
    ["ReverseProxy:Clusters:instrument-cluster:Destinations:instrument:Address"] = Environment.GetEnvironmentVariable("INSTRUMENT_URL"),
};
builder.Configuration.AddInMemoryCollection(
    destinationOverrides.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)));

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
app.MapGet("/healthz", () => Results.Ok("Gateway up"));
app.UseCors("AllowAll");

app.MapReverseProxy();

app.Run();
