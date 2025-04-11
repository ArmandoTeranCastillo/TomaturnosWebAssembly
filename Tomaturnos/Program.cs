using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TomaTurnos.Data.Dependencies.Services;
using TomaTurnos.Data.Dependencies.Services.Requests;
using TomaTurnos.Data.Dependencies.Transients;
using Tomaturnos;
using TomaTurnos.Data.Dependencies;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configurar root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient
builder.Services.AddScoped(_ => new HttpClient 
{ 
    BaseAddress = new Uri(Variables.GetApiUrl()) 
});

// Registrar servicios
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<MunicipioState>();

// Registrar servicios personalizados
builder.Services.AddScoped<IHttpService, HttpService>(sp => 
{
    HttpClient httpClient = sp.GetRequiredService<HttpClient>();
    TurnosService turnosService = new(httpClient);
    return new HttpService(httpClient, turnosService);
});

builder.Services.AddScoped<ITurnosService, TurnosService>(sp => 
{
    HttpClient httpClient = sp.GetRequiredService<HttpClient>();
    return new TurnosService(httpClient);
});

await builder.Build().RunAsync();