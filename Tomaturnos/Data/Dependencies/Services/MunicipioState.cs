namespace TomaTurnos.Data.Dependencies.Services
{
    public class MunicipioState
    {
        public event Action? OnChange;
        
        private string _municipio = string.Empty;
        public string Municipio
        {
            get => _municipio;
            set
            {
                _municipio = value;
                NotifyStateChanged();
            }
        }
        
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}