using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CivilX.Shared.Services
{
    /// <summary>
    /// Централизованный клиент для работы с API
    /// </summary>
    public class ApiClient
    {
        private const string API_BASE_URL = "http://civilx.ru";
        private readonly HttpClient _httpClient;

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Выполняет GET запрос к API
        /// </summary>
        /// <param name="endpoint">Конечная точка API</param>
        /// <param name="token">Токен авторизации (опционально)</param>
        /// <returns>Ответ от API в виде Dictionary</returns>
        public async Task<Dictionary<string, object>> GetAsync(string endpoint, string token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{API_BASE_URL}/auth-api.php/api/{endpoint}");
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(content);
                }
                else
                {
                    return new Dictionary<string, object>
                    {
                        ["error"] = $"HTTP {response.StatusCode}: {content}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }

        /// <summary>
        /// Выполняет POST запрос к API
        /// </summary>
        /// <param name="endpoint">Конечная точка API</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="token">Токен авторизации (опционально)</param>
        /// <returns>Ответ от API в виде Dictionary</returns>
        public async Task<Dictionary<string, object>> PostAsync(string endpoint, object data, string token = null)
        {
            try
            {
                var json = new JavaScriptSerializer().Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{API_BASE_URL}/auth-api.php/api/{endpoint}")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                }
                else
                {
                    return new Dictionary<string, object>
                    {
                        ["error"] = $"HTTP {response.StatusCode}: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }

        /// <summary>
        /// Выполняет PUT запрос к API
        /// </summary>
        /// <param name="endpoint">Конечная точка API</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="token">Токен авторизации (опционально)</param>
        /// <returns>Ответ от API в виде Dictionary</returns>
        public async Task<Dictionary<string, object>> PutAsync(string endpoint, object data, string token = null)
        {
            try
            {
                var json = new JavaScriptSerializer().Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Put, $"{API_BASE_URL}/auth-api.php/api/{endpoint}")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                }
                else
                {
                    return new Dictionary<string, object>
                    {
                        ["error"] = $"HTTP {response.StatusCode}: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }

        /// <summary>
        /// Выполняет DELETE запрос к API
        /// </summary>
        /// <param name="endpoint">Конечная точка API</param>
        /// <param name="token">Токен авторизации (опционально)</param>
        /// <returns>Ответ от API в виде Dictionary</returns>
        public async Task<Dictionary<string, object>> DeleteAsync(string endpoint, string token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{API_BASE_URL}/auth-api.php/api/{endpoint}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(content);
                }
                else
                {
                    return new Dictionary<string, object>
                    {
                        ["error"] = $"HTTP {response.StatusCode}: {content}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }

        /// <summary>
        /// Проверяет успешность ответа API
        /// </summary>
        /// <param name="response">Ответ от API</param>
        /// <returns>True если ответ успешный</returns>
        public static bool IsSuccess(Dictionary<string, object> response)
        {
            return response != null && 
                   !response.ContainsKey("error") && 
                   (!response.ContainsKey("success") || Convert.ToBoolean(response["success"]));
        }

        /// <summary>
        /// Получает сообщение об ошибке из ответа API
        /// </summary>
        /// <param name="response">Ответ от API</param>
        /// <returns>Сообщение об ошибке или null</returns>
        public static string GetErrorMessage(Dictionary<string, object> response)
        {
            if (response == null)
            {
                return "Нет ответа от сервера";
            }

            if (response.ContainsKey("error"))
            {
                return response["error"]?.ToString();
            }

            if (response.ContainsKey("message"))
            {
                return response["message"]?.ToString();
            }

            return null;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
