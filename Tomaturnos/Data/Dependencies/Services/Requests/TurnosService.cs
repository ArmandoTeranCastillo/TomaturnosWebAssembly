using System.Net.Http.Json;
using TomaTurnos.Data.Models;

namespace TomaTurnos.Data.Dependencies.Services.Requests
{
    public interface ITurnosService
    {
        Task<List<Modulo>> GetModulos();
        Task<string> GetModulo(int moduloId);
        Task<AudioDto> GetAudio(int audioId);
        Task ActualizarVisor(int idModulo);
    }
    
    public class TurnosService(HttpClient http) : ITurnosService
    {
        public async Task<List<Modulo>> GetModulos()
        {
            try
            {
                List<Modulo>? response = await http.GetFromJsonAsync<List<Modulo>>("VisorTomaturnos/GetModulos");
                
                if (response == null)
                {
                    throw new Exception("No se pudo obtener la lista de municipios");
                }
                
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public async Task<string> GetModulo(int moduloId)
        {
            try
            {
                Modulo? response = await http.GetFromJsonAsync<Modulo>($"VisorTomaturnos/GetModulo?moduloId={moduloId}");
                
                if (response == null)
                {
                    throw new Exception("No se pudo obtener el módulo");
                }
                
                return response.Nombre;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public async Task<AudioDto> GetAudio(int audioId)
        {
            try
            {
                AudioDto? response = await http.GetFromJsonAsync<AudioDto>($"VisorTomaturnos/GetAudio?audioId={audioId}");
                
                if (response == null)
                {
                    throw new Exception("No se pudo obtener el audio");
                }
                
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public async Task ActualizarVisor(int idModulo)
        {
            try
            {
                await http.GetAsync($"VisorTomaturnos/ActualizarVisor?idModulo={idModulo}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}