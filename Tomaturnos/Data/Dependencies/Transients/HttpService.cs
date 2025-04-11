using TomaTurnos.Data.Dependencies.Services.Requests;
using TomaTurnos.Infrastructure;

namespace TomaTurnos.Data.Dependencies.Transients
{
    public interface IHttpService
    {
        public ITurnosService Turnos { get; }
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _http;
        public HttpService(HttpClient http, ITurnosService turnosService)
        {
            _http = http;
            AddDefaultHeaders();
            
            Turnos = turnosService;
        }
        
        public ITurnosService Turnos { get; } 
        
        private void AddDefaultHeaders()
        {
            foreach (KeyValuePair<string, string> header in Headers.DefaultHeaders)
            {
                _http.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}