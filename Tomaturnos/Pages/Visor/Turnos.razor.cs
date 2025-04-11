using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using TomaTurnos.Data.Dependencies.Services.Requests;
using TomaTurnos.Data.Models;

namespace TomaTurnos.Pages.Visor
{
    public partial class Turnos : IDisposable
    {
        [Parameter]
        public int ModuloId { get; set; }
    
        private List<CapturasDto> _capturas = [];
        private List<Turno> _turnos = [];
        private Turno _turnoActual = new();
        private string _errorMessage = string.Empty;

        // Eventos
        private bool _isVisorInitialized;
        
        protected override async Task OnInitializedAsync()
        {
            if (!_isVisorInitialized) 
            {
                try
                {
                    // Suscribirse a eventos de SignalRService
                    SignalRService.OnCapturas += UpdateCapturas;
                    SignalRService.OnTurnoActual += UpdateTurnoActual;
                    SignalRService.OnTurnosPendientes += UpdateTurnosPendientes;
                    SignalRService.OnConsole += OnConsole;

                    // Obtener información del módulo
                    MunicipioState.Municipio = await TurnosService.GetModulo(ModuloId);

                    // Iniciar la conexión con SignalR
                    await SignalRService.Start(ModuloId);

                    // Actualizar Visor
                    await TurnosService.ActualizarVisor(ModuloId);

                    _isVisorInitialized = true;
                }
                catch (Exception ex)
                {
                    _errorMessage = "Error de conexión. Por favor, inténtelo nuevamente más tarde.";
                    await OnConsole("Error al inicializar el visor: " + ex.Message, TypeLog.Error);
                }
            }
        }

        private Task UpdateCapturas(List<CapturasDto> capturas, bool delay)
        {
            _capturas = capturas.OrderBy(c => int.TryParse(c.Equipo, out int n) ? n : 0).ToList();
            InvokeAsync(StateHasChanged); 
            return Task.CompletedTask;
        }

        private Task UpdateTurnoActual(Turno turno, AudioDto audio, bool delay)
        {
            _turnoActual = turno;

            // Ejecuta ambos métodos en paralelo
            Task turnoTask = OnTurnoReceived(turno);
            Task audioTask = OnAudioReceived(turno.Nombre, audio);

            // Espera a que ambos terminen sin bloquear la UI
            InvokeAsync(StateHasChanged);

            return Task.WhenAll(turnoTask, audioTask);
        }

        private Task UpdateTurnosPendientes(List<Turno> turnos, bool delay)
        {
            _turnos = turnos;
            InvokeAsync(StateHasChanged); 
            return Task.CompletedTask;
        }

        private async Task OnTurnoReceived(Turno turno)
        {
            await OnConsole("Recibiendo turno: " + turno.Nombre, TypeLog.Information);
            try
            {
                await JsRuntime.InvokeVoidAsync("addTurnoActualAnimation");
            }
            catch (Exception e)
            {
                await OnConsole("Error al recibir turno: " + turno.Nombre + e.Message, TypeLog.Error);
            }
        }

        private async Task OnAudioReceived(string turno, AudioDto audio)
        {
            await OnConsole("Recibiendo audio para el turno: " + turno, TypeLog.Information);
            try
            {
                // Detener cualquier audio en reproducción
                await JsRuntime.InvokeVoidAsync("stopAudio");
                
                // Obtener audio de la API
                AudioDto audioFromApi = await TurnosService.GetAudio(audio.Id ?? 0);

                if (audioFromApi.Content is { Length: > 0 })
                {
                    string base64Audio = Convert.ToBase64String(audioFromApi.Content);
                    await JsRuntime.InvokeVoidAsync("playAudioWithoutSave", base64Audio);
                }
                else
                {
                    await OnConsole("No se encontró audio para el turno: " + turno, TypeLog.Warning);
                }
            }
            catch (Exception ex)
            {
                await OnConsole("Error al reproducir audio para el turno: " + turno + ex.Message, TypeLog.Error);
            }
        }
        
        private async Task OnConsole(string message, TypeLog type)
        {
            string logMessage = $"Log: {message}"; 

            switch (type)
            {
                case TypeLog.Information:
                    Log.Information("{LogMessage}", logMessage);
                    await JsRuntime.InvokeVoidAsync("console.info", message);
                    break;
                case TypeLog.Warning:
                    Log.Warning("{LogMessage}", logMessage);
                    await JsRuntime.InvokeVoidAsync("console.warn", message);
                    break;
                case TypeLog.Error:
                    Log.Error("{LogMessage}", logMessage);
                    await JsRuntime.InvokeVoidAsync("console.error", message);
                    break;
                default:
                    await JsRuntime.InvokeVoidAsync("console.log", message);
                    break;
            }
        }

        public void Dispose()
        {
            SignalRService.OnCapturas -= UpdateCapturas;
            SignalRService.OnTurnoActual -= UpdateTurnoActual;
            SignalRService.OnTurnosPendientes -= UpdateTurnosPendientes;
            SignalRService.Dispose();
            
            GC.SuppressFinalize(this);
        }
    }
}