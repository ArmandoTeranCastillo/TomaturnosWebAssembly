using System.Text.Json;

namespace TomaTurnos.Data.Dependencies.Helpers
{
    public static class JsonHelper
    {
        public static JsonSerializerOptions CaseInsensitiveOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}