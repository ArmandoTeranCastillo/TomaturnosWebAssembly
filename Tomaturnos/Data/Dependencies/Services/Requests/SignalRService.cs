using Microsoft.AspNetCore.SignalR.Client;
using TomaTurnos.Data.Dependencies.Helpers;
using TomaTurnos.Data.Models;

namespace TomaTurnos.Data.Dependencies.Services.Requests
{
    public interface ISignalRService
    {
        event Func<string, TypeLog, Task>? OnConsole; 
        event Func<List<CapturasDto>, bool, Task>? OnCapturas;
        event Func<List<Turno>, bool, Task>? OnTurnosPendientes;
        event Func<Turno, AudioDto, bool, Task>? OnTurnoActual;
        Task Start(int moduloId);
        void Dispose();
        bool IsConnected(int moduloId);
    }

    public class SignalRService : ISignalRService
    {
        private readonly Dictionary<int, HubConnection> _connections = new();
        public event Func<string, TypeLog, Task>? OnConsole; 
        public event Func<List<CapturasDto>, bool, Task>? OnCapturas;
        public event Func<List<Turno>, bool, Task>? OnTurnosPendientes;
        public event Func<Turno, AudioDto, bool, Task>? OnTurnoActual;
        
        private readonly VisorGlobalSemaphore _globalSemaphore = new(); 

        public async Task Start(int moduloId)
        {
            if (!IsConnected(moduloId)) // Verifica si ya está conectado
            {
                try
                {
                    LogToConsole("Iniciando conexión para el módulo " + moduloId, TypeLog.Information);
                    
                    HubConnection connection = CreateHubConnection();
                    RegisterEvents(connection, moduloId);

                    connection.Closed += async _ =>
                    {
                        LogToConsole("Conexión cerrada para el módulo " + moduloId + ". Intentando reconectar...", TypeLog.Warning);

                        // Limpiar la conexión antigua
                        await Stop(moduloId);

                        // Intentar reconectar
                        await Reconnect(moduloId);
                    };

                    await StartConnection(connection, moduloId);

                    _connections[moduloId] = connection; // Almacenar la conexión en el diccionario
                    LogToConsole("Conexión exitosa para el módulo " + moduloId, TypeLog.Information);
                }
                catch (Exception ex)
                {
                    LogToConsole("Error al intentar iniciar la conexión para el módulo " + moduloId + ex.Message, TypeLog.Error);
                    throw;
                }
            }
        }
        
        private async Task Reconnect(int moduloId)
        {
            int retryCount = 0;
            const int maxRetries = int.MaxValue; 
            const int baseDelayMilliseconds = 2000;
            Random jitter = new();

            while (true)
            {
                try
                {
                    await Start(moduloId);
                    LogToConsole($"Reconexión exitosa para el módulo {moduloId}.", TypeLog.Information);
                    return; 
                }
                catch (Exception ex)
                {
                    LogToConsole($"Error al intentar reconectar para el módulo {moduloId}: {ex.Message}", TypeLog.Error);

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        LogToConsole($"Máximo número de intentos alcanzado. No se pudo reconectar para el módulo {moduloId}.", TypeLog.Error);
                        break;
                    }
                    
                    int delay = (int)(Math.Pow(2, retryCount) * baseDelayMilliseconds) +
                                jitter.Next(0, 1000);

                    LogToConsole($"Intentando reconectar para el módulo {moduloId} en {delay / 1000.0} segundos...", TypeLog.Warning);
                    await Task.Delay(delay);
                }
            }
        }
        
        public bool IsConnected(int moduloId)
        {
            return _connections.TryGetValue(moduloId, out HubConnection? value) && value.State == HubConnectionState.Connected;
        }
        
        private static HubConnection CreateHubConnection()
        {
            HubConnection hub = new HubConnectionBuilder()
                .WithUrl(Variables.GetApiUrl() + "visoresNotificationHub")
                .Build();
            
            hub.HandshakeTimeout = TimeSpan.FromSeconds(5);
            hub.KeepAliveInterval = TimeSpan.FromSeconds(5);
            return hub;
        }

        private void RegisterEvents(HubConnection connection, int moduloId)
        {
            connection.On<List<CapturasDto>, bool>("ActualizarEstadoEquipos", (equipos, delay) =>
            {
                string? connectionId = connection.ConnectionId;
                if (!string.IsNullOrEmpty(connectionId)) 
                {
                    _globalSemaphore.EnqueueUpdate(connectionId, moduloId, "Capturas", delay, () =>
                        InvokeEvents.InvokeEventSafely(OnCapturas, equipos, delay, "ActualizarEstadoEquipos"));
                }
                return Task.CompletedTask;
            });

            connection.On<List<Turno>, bool>("ReceiveTurnosPendientesFromVisor", (turnosPendientes, delay) =>
            {
                string? connectionId = connection.ConnectionId;
                if (!string.IsNullOrEmpty(connectionId)) 
                {
                    _globalSemaphore.EnqueueUpdate(connectionId, moduloId, "TurnosPendientes", delay, () =>
                        InvokeEvents.InvokeEventSafely(OnTurnosPendientes, turnosPendientes, delay, "ReceiveTurnosPendientesFromVisor"));
                }
                return Task.CompletedTask;
            });

            connection.On<Turno, AudioDto, bool>("ReceiveTurnoFromVisor", (turno, audio, delay) =>
            {
                string? connectionId = connection.ConnectionId;
                if (!string.IsNullOrEmpty(connectionId)) 
                {
                    _globalSemaphore.EnqueueUpdate(connectionId, moduloId, "TurnoActual", delay, () =>
                        InvokeEvents.InvokeEventSafely(OnTurnoActual, turno, audio, delay, "ReceiveTurnoFromVisor"));
                }
                return Task.CompletedTask;
            });
        }
        
        private static async Task StartConnection(HubConnection connection, int moduloId)
        {
            await connection.StartAsync();
            await connection.InvokeAsync("ConectarVisor", moduloId.ToString());
        }

        private async Task Stop(int moduloId)
        {
            if (_connections.TryGetValue(moduloId, out HubConnection? value))
            {
                await value.StopAsync();
                await value.DisposeAsync();
                _connections.Remove(moduloId);
            }
        }

        public void Dispose()
        {
            foreach (HubConnection connection in _connections.Values)
            {
                connection.DisposeAsync();
            }
            _connections.Clear();
        }

        private void LogToConsole(string message, TypeLog type)
        {
            OnConsole?.Invoke(message, type);
        }
    }
    
    public enum TypeLog
    {
        Information,
        Warning,
        Error
    }
}