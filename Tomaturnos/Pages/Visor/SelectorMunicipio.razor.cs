using TomaTurnos.Data.Models;

namespace TomaTurnos.Pages.Visor
{
    public partial class SelectorMunicipio
    {
        private List<Modulo> _modulos = [];
        private int _selectedMunicipioId;
        private string _errorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            MunicipioState.Municipio = string.Empty;
            
            try
            {
                _modulos = await TurnosService.GetModulos();
                if (_modulos.Any())
                {
                    _selectedMunicipioId = _modulos.First().IdModulo;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = "Error al cargar los módulos. Por favor, inténtelo nuevamente más tarde.";
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void GoToTurnos()
        {
            Navigation.NavigateTo($"/Turnos/{_selectedMunicipioId}");
        }
    }
}