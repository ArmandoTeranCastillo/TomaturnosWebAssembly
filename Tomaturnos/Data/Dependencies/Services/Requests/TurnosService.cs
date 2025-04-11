using Tomaturnos.Data.Dependencies;
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
    
    public class TurnosService : ITurnosService
    {
        private readonly string _baseUrl = Variables.GetApiUrl();
        public async Task<List<Modulo>> GetModulos()
        {
            try
            {
                string url = $"{_baseUrl}VisorTomaturnos/GetModulos";
                List<Modulo>? response = await Http.GetAsync<List<Modulo>>(url);
                
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
                string url = $"{_baseUrl}VisorTomaturnos/GetModulo?moduloId={moduloId}";
                Modulo? response = await Http.GetAsync<Modulo>(url);
                
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
                string url = $"{_baseUrl}VisorTomaturnos/GetAudio?audioId={audioId}";
                AudioDto? response = await Http.GetAsync<AudioDto>(url);
                
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
                string url = $"{_baseUrl}VisorTomaturnos/ActualizarVisor?idModulo={idModulo}";
                await Http.GetAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}