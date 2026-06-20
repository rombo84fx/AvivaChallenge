using System.Text.Json.Serialization;
using AvivaChallenge.Api.Providers;
using AvivaChallenge.Api.Repositories;
using AvivaChallenge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

var apiKey = builder.Configuration["PaymentProviders:ApiKey"] ?? string.Empty;

builder.Services.AddHttpClient<PagaFacilProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentProviders:PagaFacil:BaseUrl"]
                                 ?? "https://app-paga-chg-aviva.azurewebsites.net");
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
});

builder.Services.AddHttpClient<CazaPagosProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentProviders:CazaPagos:BaseUrl"]
                                 ?? "https://app-caza-chg-aviva.azurewebsites.net");
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
});

builder.Services.AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<PagaFacilProvider>());
builder.Services.AddSingleton<IPaymentProvider>(sp => sp.GetRequiredService<CazaPagosProvider>());
builder.Services.AddSingleton<IPaymentProviderSelector, PaymentProviderSelector>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
