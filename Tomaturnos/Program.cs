using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TomaTurnos.Data.Dependencies.Services;
using TomaTurnos.Data.Dependencies.Services.Requests;
using Tomaturnos;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configurar root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registrar servicios
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<MunicipioState>();
builder.Services.AddScoped<ITurnosService, TurnosService>();

await builder.Build().RunAsync();