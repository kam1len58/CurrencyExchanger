using CurrencyExchanger.BLL;
using CurrencyExchanger.BLL.Validators;
using CurrencyExchanger.DAL.Dao;
using Scalar.AspNetCore;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500",
                                              "http://localhost:5500",
                                              "https://currencyexchanger.duckdns.org/")
                          .WithMethods("GET", "PUT", "POST", "PATCH")
                          .AllowAnyHeader();
                      });
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<DBConnectionProvider>();
builder.Services.AddScoped<CurrencyDao>();
builder.Services.AddScoped<ExchangeRateDao>();
builder.Services.AddSingleton<CurrencyValidator>();
builder.Services.AddSingleton<ExchangeRateValidator>();
builder.Services.AddScoped<ExchangeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
