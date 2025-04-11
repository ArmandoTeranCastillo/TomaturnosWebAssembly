using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TomaTurnos.Data.Dependencies.Services;
using TomaTurnos.Data.Dependencies.Services.Requests;
using Tomaturnos;
using TomaTurnos.Data.Dependencies;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configurar root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registrar servicios
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<MunicipioState>();
builder.Services.AddScoped(_ =>
{
    HttpClient httpClient = new()
    {
        BaseAddress = new Uri(Variables.GetApiUrl())
    };

    return httpClient;
});
builder.Services.AddScoped<ITurnosService, TurnosService>(sp =>
{
    HttpClient httpClient = sp.GetRequiredService<HttpClient>();
    return new TurnosService(httpClient);
});

await builder.Build().RunAsync();