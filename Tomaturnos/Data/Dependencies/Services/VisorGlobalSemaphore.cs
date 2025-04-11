using System.Collections.Concurrent;

namespace TomaTurnos.Data.Dependencies.Services
{
    public class VisorGlobalSemaphore
    {
        private class SemaphoreQueue
        {
            public SemaphoreSlim CapturasSemaphore { get; } = new(1, 1);
            public SemaphoreSlim TurnoActualSemaphore { get; } = new(1, 1);
            public SemaphoreSlim TurnosPendientesSemaphore { get; } = new(1, 1);

            public ConcurrentQueue<Func<Task>> CapturasQueue { get; } = new();
            public ConcurrentQueue<Func<Task>> TurnoActualQueue { get; } = new();
            public ConcurrentQueue<Func<Task>> TurnosPendientesQueue { get; } = new();
        }

        private readonly ConcurrentDictionary<(string ConnectionId, int ModuloId), SemaphoreQueue> _globalQueues = new();
        public void EnqueueUpdate(string connectionId, int moduloId, string eventType, bool delay, Func<Task> updateAction)
        {
            Console.WriteLine("EnqueueUpdate: {0}, {1}, {2}", connectionId, moduloId, eventType);
            
            (string connectionId, int moduloId) key = (connectionId, moduloId);
            SemaphoreQueue semaphoreQueue = _globalQueues.GetOrAdd(key, _ => new SemaphoreQueue());

            Dictionary<string, (SemaphoreSlim semaphore, ConcurrentQueue<Func<Task>> queue)> eventMapping = new()
            {
                { "Capturas", (semaphoreQueue.CapturasSemaphore, semaphoreQueue.CapturasQueue) },
                { "TurnoActual", (semaphoreQueue.TurnoActualSemaphore, semaphoreQueue.TurnoActualQueue) },
                { "TurnosPendientes", (semaphoreQueue.TurnosPendientesSemaphore, semaphoreQueue.TurnosPendientesQueue) }
            };

            if (eventMapping.TryGetValue(eventType, out (SemaphoreSlim semaphore, ConcurrentQueue<Func<Task>> queue) eventResources))
            {
                eventResources.queue.Enqueue(updateAction);
                if (eventResources.semaphore.CurrentCount > 0)
                {
                    Task.Run(() => ProcessQueue(eventResources.semaphore, eventResources.queue, delay));
                }
            }
            else
            {
                Console.WriteLine("No se encontró el evento {0} en el mapeo de eventos", eventType);
            }
        }

        // Procesamiento de la cola
        private static async Task ProcessQueue(SemaphoreSlim semaphore, ConcurrentQueue<Func<Task>> queue, bool delay)
        {
            Console.WriteLine("ProcessQueue: {0}", queue.Count);
            
            await semaphore.WaitAsync();
            try
            {
                while (queue.TryDequeue(out Func<Task>? updateAction))
                {
                    await updateAction();

                    if (delay)
                    {
                        await Task.Delay(5000); // Delay de 5 segundos entre las tareas si se especifica.
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error al procesar la cola");
            }
            finally
            {
                semaphore.Release();
            }
        }

        // Método para eliminar la cola de un módulo y conexión cuando se desconecta
        public void RemoveQueue(string connectionId, int moduloId)
        {
            _globalQueues.TryRemove((connectionId, moduloId), out _);
        }
    }
}