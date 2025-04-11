namespace TomaTurnos.Data.Dependencies.Helpers
{
    public static class InvokeEvents
    {
        public static async Task InvokeEventSafely<T>(Func<T, bool, Task>? eventHandler, T arg, bool delay, string eventName)
        {
            if (eventHandler != null)
            {
                try
                {
                    await eventHandler.Invoke(arg, delay);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al invocar el evento {eventName}: {e.Message}");
                }
            }
        }
        
        public static async Task InvokeEventSafely<T1, T2>(Func<T1, T2, bool, Task>? eventHandler, T1 arg1, T2 arg2, bool delay, string eventName)
        {
            if (eventHandler != null)
            {
                try
                {
                    await eventHandler.Invoke(arg1, arg2, delay);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al invocar el evento {eventName}: {e.Message}");
                }
            }
        }
        
        public static async Task InvokeEventSafely<T1, T2, T3>(Func<T1, T2, T3, bool, Task>? eventHandler, T1 arg1, T2 arg2, T3 arg3, bool delay, string eventName)
        {
            if (eventHandler != null)
            {
                try
                {
                    await eventHandler.Invoke(arg1, arg2, arg3, delay);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error al invocar el evento {eventName}: {e.Message}");
                }
            }
        }
    }
}