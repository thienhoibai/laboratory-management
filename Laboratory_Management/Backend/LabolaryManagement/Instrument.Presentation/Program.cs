using Instrument.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Instrument.Application.Results;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<InstrumentDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("InstrumentDb")));

builder.Services.AddHttpClient("testorder", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["TestOrderBaseUrl"]!);
});

builder.Services.AddScoped<IResultGenerator, ResultGenerator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
