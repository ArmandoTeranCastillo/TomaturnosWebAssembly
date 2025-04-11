#nullable enable
using System.Text;
using System.Text.Json;
using TomaTurnos.Data.Dependencies.Helpers;
using TomaTurnos.Infrastructure;

namespace Tomaturnos.Data.Dependencies
{
    public static class Http
    {
        private static readonly HttpClient Client = new();

        static Http()
        {
            foreach (KeyValuePair<string, string> header in Headers.DefaultHeaders)
            {
                Client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private const int Milliseconds = 40000;
        
        public static async Task<bool> GetAsync(string url, int timeoutMilliseconds = Milliseconds)
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                using HttpResponseMessage response = await Client.GetAsync(url, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }

                return bool.Parse(responseBody);
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a problemas de conexión con el servidor.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
        }
        
        public static async Task<string> GetStringAsync(string url, int timeoutMilliseconds = Milliseconds)
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                using HttpResponseMessage response = await Client.GetAsync(url, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }

                return responseBody; // Cambio aquí para retornar la cadena directamente
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a problemas de conexión con el servidor.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
        }
        
        public static async Task<int> GetIntAsync(string url)
        {
            try
            {
                using HttpResponseMessage response = await Client.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }
                
                return int.Parse(responseBody);
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (FormatException e)
            {
                throw new ApiServerException(e.Message, "FORMAT_EXCEPTION");
            }
        }
        
        public static async Task<TData> GetAsync<TData>(string url, Dictionary<string, string>? headers = null, int timeoutMilliseconds = Milliseconds) where TData : class
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                // Crea una instancia de HttpRequestMessage para configurar la solicitud
                HttpRequestMessage requestMessage = new(HttpMethod.Get, url);

                // Añade los headers específicos a la solicitud si se han proporcionado
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
                
                using HttpResponseMessage response = await Client.SendAsync(requestMessage, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }

                if (response.Content.Headers.ContentType?.MediaType != "application/json")
                {
                    throw new ApiServerException(responseBody, "NON_JSON_RESPONSE");
                }

                TData? result = JsonSerializer.Deserialize<TData>(responseBody, JsonHelper.CaseInsensitiveOptions);
                if (result == null)
                {
                    throw new ApiServerException("La respuesta de la API estaba vacía o no pudo ser deserializada correctamente.", "DESERIALIZATION_FAILED");
                }

                return result;
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a problemas de conexión con el servidor.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (JsonException e)
            {
                throw new ApiServerException(e.Message, "JSON_EXCEPTION");
            }
        }
        
        public static async Task<TData> PostAsync<TData, TPostData>(string url, TPostData postData, int timeoutMilliseconds = Milliseconds)
            where TData : class
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                StringContent content = new(JsonSerializer.Serialize(postData, JsonHelper.CaseInsensitiveOptions), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await Client.PostAsync(url, content, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }

                TData? result = JsonSerializer.Deserialize<TData>(responseBody, JsonHelper.CaseInsensitiveOptions);
                if (result == null)
                {
                    throw new ApiServerException("La respuesta de la API estaba vacía o no pudo ser deserializada correctamente.", "DESERIALIZATION_FAILED");
                }

                return result;
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a problemas de conexión con el servidor.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (JsonException e)
            {
                throw new ApiServerException(e.Message, "JSON_EXCEPTION");
            }
        }
        
        public static async Task<TData?> PostAsyncNullable<TData, TPostData>(string url, TPostData postData, int timeoutMilliseconds = Milliseconds)
            where TData : class
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                StringContent content = new(JsonSerializer.Serialize(postData, JsonHelper.CaseInsensitiveOptions), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await Client.PostAsync(url, content, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }
                
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    return null; 
                }

                TData? result = JsonSerializer.Deserialize<TData>(responseBody, JsonHelper.CaseInsensitiveOptions);
                return result;
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a problemas de conexión con el servidor.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (JsonException e)
            {
                throw new ApiServerException(e.Message, "JSON_EXCEPTION");
            }
        }
        
        public static async Task<string> PostAsyncTask<TPostData>(string url, TPostData postData, int timeoutMilliseconds = Milliseconds)
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                StringContent content = new(JsonSerializer.Serialize(postData, JsonHelper.CaseInsensitiveOptions), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await Client.PostAsync(url, content, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }
        
                return responseBody;
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a un exceso del tiempo de espera establecido.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (JsonException e)
            {
                throw new ApiServerException(e.Message, "JSON_EXCEPTION");
            }
        }
        
        public static async Task<TData> PutAsync<TData, TPutData>(string url, TPutData putData, int timeoutMilliseconds = Milliseconds) where TData : class
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                StringContent content = new(JsonSerializer.Serialize(putData, JsonHelper.CaseInsensitiveOptions), Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await Client.PutAsync(url, content, tokenSource?.Token ?? CancellationToken.None);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }

                TData? result = JsonSerializer.Deserialize<TData>(responseBody, JsonHelper.CaseInsensitiveOptions);
                if (result == null)
                {
                    throw new ApiServerException("La respuesta de la API estaba vacía o no pudo ser deserializada correctamente.", "DESERIALIZATION_FAILED");
                }

                return result;
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a un exceso del tiempo de espera establecido.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
            catch (JsonException e)
            {
                throw new ApiServerException(e.Message, "JSON_EXCEPTION");
            }
        }
        
        public static async Task DeleteAsync(string url, int timeoutMilliseconds = Milliseconds)
        {
            using CancellationTokenSource? tokenSource = timeoutMilliseconds > 0 ? new CancellationTokenSource(timeoutMilliseconds) : null;
            try
            {
                using HttpResponseMessage response = await Client.DeleteAsync(url, tokenSource?.Token ?? CancellationToken.None);
                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    throw new ApiServerException(responseBody, response.StatusCode.ToString());
                }
            }
            catch (OperationCanceledException) when (tokenSource?.IsCancellationRequested ?? false)
            {
                throw new ApiServerException("La solicitud fue cancelada debido a un exceso del tiempo de espera establecido.", "TIMEOUT_EXCEPTION");
            }
            catch (HttpRequestException e)
            {
                throw new ApiServerException(e.Message, "HTTP_REQUEST_EXCEPTION");
            }
        }
    }
    
    public class ApiServerException(string message, string errorCode) : Exception(message)
    {
        public string ErrorCode { get; } = errorCode;
    }
}