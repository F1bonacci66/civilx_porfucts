using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using Microsoft.Win32;
using CivilX.Shared.Models;

namespace CivilX.Shared.Auth
{
    public class AuthService : IAuthService
    {
        private const string API_BASE_URL = "http://civilx.ru/auth-api.php";
        private const string TOKEN_REGISTRY_KEY = @"SOFTWARE\CivilX\AuthPlugin";
        private const string TOKEN_VALUE_NAME = "AuthToken";
        private const string APP_DATA_FOLDER = "CivilX";
        private const string PLUGIN_FOLDER = "AuthPlugin";
        
        private readonly HttpClient _httpClient;
        private readonly string _tokenFilePath;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Путь для сохранения токена в зашифрованном виде
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var civilXPath = Path.Combine(appDataPath, APP_DATA_FOLDER, PLUGIN_FOLDER);
            Directory.CreateDirectory(civilXPath);
            _tokenFilePath = Path.Combine(civilXPath, "auth_token.dat");
        }

        private void WriteToLogFile(string message)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var civilXPath = Path.Combine(appDataPath, "CivilX", "AuthPlugin");
                Directory.CreateDirectory(civilXPath);
                
                var logFilePath = Path.Combine(civilXPath, "auth_plugin_log.txt");
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки записи в лог
            }
        }

        #region Token Management

        public string GetStoredToken()
        {
            try
            {
                // Сначала пробуем получить из реестра
                using (var key = Registry.CurrentUser.OpenSubKey(TOKEN_REGISTRY_KEY))
                {
                    if (key != null)
                    {
                        var encryptedToken = key.GetValue(TOKEN_VALUE_NAME) as string;
                        if (!string.IsNullOrEmpty(encryptedToken))
                        {
                            WriteToLogFile($"[AuthService] GetStoredToken: Found encrypted token in registry, length: {encryptedToken.Length}");
                            WriteToLogFile($"[AuthService] GetStoredToken: Registry token preview: {encryptedToken.Substring(0, Math.Min(50, encryptedToken.Length))}...");
                            var decryptedToken = DecryptToken(encryptedToken);
                            WriteToLogFile($"[AuthService] GetStoredToken: Decrypted token length: {(decryptedToken?.Length ?? 0)}");
                            if (string.IsNullOrEmpty(decryptedToken))
                            {
                                WriteToLogFile("[AuthService] GetStoredToken: Decryption failed - token is empty");
                            }
                            return decryptedToken;
                        }
                    }
                }

                // Если в реестре нет, пробуем файл
                if (File.Exists(_tokenFilePath))
                {
                    var encryptedToken = File.ReadAllText(_tokenFilePath);
                    var decryptedToken = DecryptToken(encryptedToken);
                    WriteToLogFile($"[AuthService] GetStoredToken: Found token in file, length: {(decryptedToken?.Length ?? 0)}");
                    return decryptedToken;
                }

                WriteToLogFile("[AuthService] GetStoredToken: No token found");
                return null;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] GetStoredToken Exception: {ex.Message}");
                return null;
            }
        }

        public void SaveToken(string token)
        {
            try
            {
                var encryptedToken = EncryptToken(token);
                WriteToLogFile($"[AuthService] SaveToken: Saving token, length: {(token?.Length ?? 0)}");
                WriteToLogFile($"[AuthService] SaveToken: Encrypted token length: {encryptedToken?.Length ?? 0}");
                WriteToLogFile($"[AuthService] SaveToken: Encrypted token preview: {encryptedToken?.Substring(0, Math.Min(50, encryptedToken?.Length ?? 0))}...");

                // Сохраняем в реестр
                using (var key = Registry.CurrentUser.CreateSubKey(TOKEN_REGISTRY_KEY))
                {
                    key?.SetValue(TOKEN_VALUE_NAME, encryptedToken);
                    WriteToLogFile("[AuthService] SaveToken: Token saved to registry");
                }

                // Дублируем в файл для надежности
                File.WriteAllText(_tokenFilePath, encryptedToken);
                WriteToLogFile("[AuthService] SaveToken: Token saved to file");
                WriteToLogFile($"[AuthService] SaveToken: Token saved successfully to registry and file");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] SaveToken Exception: {ex.Message}");
            }
        }

        public void ClearToken()
        {
            try
            {
                // Удаляем из реестра
                using (var key = Registry.CurrentUser.OpenSubKey(TOKEN_REGISTRY_KEY, true))
                {
                    key?.DeleteValue(TOKEN_VALUE_NAME, false);
                }

                // Удаляем файл
                if (File.Exists(_tokenFilePath))
                {
                    File.Delete(_tokenFilePath);
                }
            }
            catch
            {
                // Игнорируем ошибки удаления
            }
        }

        #endregion

        #region Authentication

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var requestData = new
                {
                    email = email,
                    password = password
                };

                var json = new JavaScriptSerializer().Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var url = $"{API_BASE_URL}/api/login";
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                    
                    // Проверяем наличие ошибки
                    if (result.ContainsKey("error"))
                    {
                        var errorMessage = result["error"]?.ToString();
                        return AuthResult.CreateError(errorMessage);
                    }
                    
                    // Проверяем наличие токена (успешная авторизация)
                    if (result.ContainsKey("token"))
                    {
                        var token = result["token"]?.ToString();
                        var userData = result["user"] as Dictionary<string, object>;
                        
                        var user = new UserInfo
                        {
                            Id = userData?.ContainsKey("id") == true ? Convert.ToInt32(userData["id"]) : 0,
                            Username = userData?.ContainsKey("username") == true ? userData["username"]?.ToString() : "",
                            Email = userData?.ContainsKey("email") == true ? userData["email"]?.ToString() : "",
                            FullName = userData?.ContainsKey("full_name") == true ? userData["full_name"]?.ToString() : 
                                      userData?.ContainsKey("name") == true ? userData["name"]?.ToString() : "",
                            CreatedAt = userData?.ContainsKey("created_at") == true ? userData["created_at"]?.ToString() : ""
                        };

                        SaveToken(token);
                        return AuthResult.CreateSuccess(token, user);
                    }
                    
                    // Если нет ни ошибки, ни токена
                    return AuthResult.CreateError("Неизвестная ошибка сервера");
                }
                else
                {
                    return AuthResult.CreateError($"Ошибка сервера: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return AuthResult.CreateError($"Ошибка входа: {ex.Message}");
            }
        }

        public async Task<AuthResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var requestData = new
                {
                    username = username,
                    email = email,
                    password = password
                };

                var json = new JavaScriptSerializer().Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var url = $"{API_BASE_URL}/api/register";
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                    
                    // Проверяем наличие ошибки
                    if (result.ContainsKey("error"))
                    {
                        var errorMessage = result["error"]?.ToString();
                        return AuthResult.CreateError(errorMessage);
                    }
                    
                    // Проверяем наличие токена (успешная регистрация)
                    if (result.ContainsKey("token"))
                    {
                        var token = result["token"]?.ToString();
                        var userData = result["user"] as Dictionary<string, object>;
                        
                        var user = new UserInfo
                        {
                            Id = userData?.ContainsKey("id") == true ? Convert.ToInt32(userData["id"]) : 0,
                            Username = userData?.ContainsKey("username") == true ? userData["username"]?.ToString() : "",
                            Email = userData?.ContainsKey("email") == true ? userData["email"]?.ToString() : "",
                            FullName = userData?.ContainsKey("full_name") == true ? userData["full_name"]?.ToString() : 
                                      userData?.ContainsKey("name") == true ? userData["name"]?.ToString() : "",
                            CreatedAt = userData?.ContainsKey("created_at") == true ? userData["created_at"]?.ToString() : ""
                        };

                        SaveToken(token);
                        return AuthResult.CreateSuccess(token, user);
                    }
                    
                    // Если нет ни ошибки, ни токена
                    return AuthResult.CreateError("Неизвестная ошибка сервера");
                }
                else
                {
                    return AuthResult.CreateError($"Ошибка сервера: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return AuthResult.CreateError($"Ошибка регистрации: {ex.Message}");
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var userInfo = await GetUserInfoAsync(token);
                return userInfo != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(string token)
        {
            try
            {
                WriteToLogFile($"[AuthService] GetUserInfoAsync: Starting request, token length: {token?.Length ?? 0}");
                
                using (var client = new HttpClient())
                {
                    var requestData = new { token = token };
                    var json = new JavaScriptSerializer().Serialize(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var url = $"{API_BASE_URL}/api/me";
                    WriteToLogFile($"[AuthService] GetUserInfoAsync: URL: {url}");
                    WriteToLogFile($"[AuthService] GetUserInfoAsync: Request data: {json}");
                    
                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    WriteToLogFile($"[AuthService] GetUserInfoAsync: Response status: {response.StatusCode}");
                    WriteToLogFile($"[AuthService] GetUserInfoAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        
                        WriteToLogFile($"[AuthService] GetUserInfoAsync: Parsed result keys: {string.Join(", ", result.Keys)}");
                        
                        // Получаем данные пользователя из поля "user"
                        WriteToLogFile($"[AuthService] GetUserInfoAsync: Checking for 'user' field...");
                        var userData = result.ContainsKey("user") ? result["user"] as Dictionary<string, object> : null;
                        WriteToLogFile($"[AuthService] GetUserInfoAsync: userData is {(userData == null ? "NULL" : "NOT NULL")}");
                        
                        if (userData != null)
                        {
                            WriteToLogFile($"[AuthService] GetUserInfoAsync: User data keys: {string.Join(", ", userData.Keys)}");
                            
                            try
                            {
                                var userInfo = new UserInfo
                                {
                                    Id = userData.ContainsKey("id") ? Convert.ToInt32(userData["id"]) : 0,
                                    Username = userData.ContainsKey("login") ? userData["login"]?.ToString() : 
                                              userData.ContainsKey("username") ? userData["username"]?.ToString() : "",
                                    Email = userData.ContainsKey("email") ? userData["email"]?.ToString() : "",
                                    FullName = userData.ContainsKey("name") ? userData["name"]?.ToString() : 
                                              userData.ContainsKey("full_name") ? userData["full_name"]?.ToString() : "",
                                    CreatedAt = userData.ContainsKey("created_at") ? userData["created_at"]?.ToString() : ""
                                };
                                
                                WriteToLogFile($"[AuthService] GetUserInfoAsync: UserInfo created - Username: {userInfo.Username}, Email: {userInfo.Email}");
                                return userInfo;
                            }
                            catch (Exception ex)
                            {
                                WriteToLogFile($"[AuthService] GetUserInfoAsync: Exception creating UserInfo: {ex.Message}");
                            }
                        }
                        else
                        {
                            WriteToLogFile($"[AuthService] GetUserInfoAsync: No 'user' field found in response");
                        }
                    }
                    else
                    {
                        WriteToLogFile($"[AuthService] GetUserInfoAsync: Request failed with status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] GetUserInfoAsync: Exception: {ex.Message}");
            }
            
            WriteToLogFile($"[AuthService] GetUserInfoAsync: Returning NULL");
            return null;
        }

        #endregion

        #region Product Management

        public async Task<List<ProductInfo>> GetUserProductsAsync(string token)
        {
            try
            {
                WriteToLogFile($"[AuthService] GetUserProductsAsync: Starting request, token length: {token?.Length ?? 0}");
                
                using (var client = new HttpClient())
                {
                    var requestData = new { token = token };
                    var json = new JavaScriptSerializer().Serialize(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var url = $"{API_BASE_URL}/api/user-products";
                    WriteToLogFile($"[AuthService] GetUserProductsAsync: URL: {url}");
                    WriteToLogFile($"[AuthService] GetUserProductsAsync: Request data: {json}");
                    
                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    WriteToLogFile($"[AuthService] GetUserProductsAsync: Response status: {response.StatusCode}");
                    WriteToLogFile($"[AuthService] GetUserProductsAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        WriteToLogFile($"[AuthService] GetUserProductsAsync: Parsed result keys: {string.Join(", ", result.Keys)}");
                        
                        if (result.ContainsKey("products"))
                        {
                            var products = new List<ProductInfo>();
                            var productsList = result["products"] as System.Collections.ArrayList;
                            
                            WriteToLogFile($"[AuthService] GetUserProductsAsync: Products list is {(productsList == null ? "NULL" : "NOT NULL")}");
                            if (productsList != null)
                            {
                                WriteToLogFile($"[AuthService] GetUserProductsAsync: Products count: {productsList.Count}");
                            }
                            
                            if (productsList != null)
                            {
                                foreach (Dictionary<string, object> productData in productsList)
                                {
                                    WriteToLogFile($"[AuthService] GetUserProductsAsync: Processing product with keys: {string.Join(", ", productData.Keys)}");
                                    
                                    var product = new ProductInfo
                                    {
                                        Id = Convert.ToInt32(productData["id"]),
                                        ProductName = productData["product_name"]?.ToString(),
                                        RevitVersion = productData.ContainsKey("revit_version") ? productData["revit_version"]?.ToString() : "2023", // По умолчанию 2023
                                        ProductVersion = productData.ContainsKey("product_version") ? productData["product_version"]?.ToString() : "1.0", // По умолчанию 1.0
                                        ActivationStatus = productData["activation_status"]?.ToString()
                                    };
                                    
                                    // Обработка activated_at (может отсутствовать в API ответе)
                                    if (productData.ContainsKey("activated_at") && productData["activated_at"] != null)
                                    {
                                        if (DateTime.TryParse(productData["activated_at"]?.ToString(), out DateTime activatedAt))
                                        {
                                            product.ActivatedAt = activatedAt;
                                        }
                                    }
                                    
                                    // Обработка deactivated_at (может отсутствовать в API ответе)
                                    if (productData.ContainsKey("deactivated_at") && productData["deactivated_at"] != null)
                                    {
                                        if (DateTime.TryParse(productData["deactivated_at"]?.ToString(), out DateTime deactivatedAt))
                                        {
                                            product.DeactivatedAt = deactivatedAt;
                                        }
                                    }
                                    
                                    // Обработка expires_at (есть в API ответе)
                                    if (productData.ContainsKey("expires_at") && productData["expires_at"] != null)
                                    {
                                        if (DateTime.TryParse(productData["expires_at"]?.ToString(), out DateTime expiresAt))
                                        {
                                            product.ExpiresAt = expiresAt;
                                        }
                                    }
                                    
                                    products.Add(product);
                                    WriteToLogFile($"[AuthService] GetUserProductsAsync: Added product: {product.ProductName}, Status: {product.ActivationStatus}");
                                }
                            }
                            
                            WriteToLogFile($"[AuthService] GetUserProductsAsync: Returning {products.Count} products");
                            return products;
                        }
                        else
                        {
                            WriteToLogFile($"[AuthService] GetUserProductsAsync: No 'products' key in response");
                        }
                    }
                    else
                    {
                        WriteToLogFile($"[AuthService] GetUserProductsAsync: Request failed with status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] GetUserProductsAsync: Exception: {ex.Message}");
                WriteToLogFile($"[AuthService] GetUserProductsAsync: Stack trace: {ex.StackTrace}");
            }
            
            WriteToLogFile($"[AuthService] GetUserProductsAsync: Returning empty list");
            return new List<ProductInfo>();
        }

        public async Task<bool> ActivateProductAsync(string token, int productId, string revitVersion, string productVersion)
        {
            try
            {
                WriteToLogFile($"[AuthService] ActivateProductAsync: Starting activation, ProductId: {productId}, RevitVersion: {revitVersion}, ProductVersion: {productVersion}");
                
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        token = token,
                        product_id = productId,
                        revit_version = revitVersion,
                        product_version = productVersion
                    };
                    
                    var json = new JavaScriptSerializer().Serialize(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var url = $"{API_BASE_URL}/api/activate-product";
                    WriteToLogFile($"[AuthService] ActivateProductAsync: URL: {url}");
                    WriteToLogFile($"[AuthService] ActivateProductAsync: Request data: {json}");
                    
                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    WriteToLogFile($"[AuthService] ActivateProductAsync: Response status: {response.StatusCode}");
                    WriteToLogFile($"[AuthService] ActivateProductAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                            WriteToLogFile($"[AuthService] ActivateProductAsync: Parsed result keys: {string.Join(", ", result.Keys)}");
                            
                            // Проверяем наличие ошибки
                            if (result.ContainsKey("error"))
                            {
                                var errorMessage = result["error"]?.ToString();
                                WriteToLogFile($"[AuthService] ActivateProductAsync: Error from server: {errorMessage}");
                                return false;
                            }
                            
                            // Проверяем успешность операции
                            if (result.ContainsKey("success"))
                            {
                                var success = Convert.ToBoolean(result["success"]);
                                WriteToLogFile($"[AuthService] ActivateProductAsync: Success: {success}");
                                return success;
                            }
                            
                            // Если нет поля success, считаем успешным если нет ошибки
                            WriteToLogFile($"[AuthService] ActivateProductAsync: No 'success' field, assuming success");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            WriteToLogFile($"[AuthService] ActivateProductAsync: JSON deserialization error: {ex.Message}");
                            WriteToLogFile($"[AuthService] ActivateProductAsync: Response content: {responseContent}");
                            
                            // Проверяем, если сервер вернул HTML-ошибку
                            if (responseContent.Contains("<br") || responseContent.Contains("<html") || responseContent.Contains("<!DOCTYPE"))
                            {
                                WriteToLogFile($"[AuthService] ActivateProductAsync: Server returned HTML error instead of JSON");
                                WriteToLogFile($"[AuthService] ActivateProductAsync: This indicates a server-side PHP error");
                                WriteToLogFile($"[AuthService] ActivateProductAsync: HTML response content: {responseContent}");
                                
                                // Попробуем альтернативный подход - проверим, есть ли в ответе признаки успеха
                                if (responseContent.Contains("success") || responseContent.Contains("activated") || responseContent.Contains("true"))
                                {
                                    WriteToLogFile($"[AuthService] ActivateProductAsync: Found success indicators in HTML response, assuming success");
                                    return true;
                                }
                                
                                // Если это HTML-ошибка, но статус HTTP успешный, возможно активация прошла
                                // НО: нужно проверить, действительно ли активация прошла на сервере
                                WriteToLogFile($"[AuthService] ActivateProductAsync: HTML error but HTTP status OK, checking if activation actually succeeded");
                                
                                // Попробуем проверить статус продукта через API
                                try
                                {
                                    var checkResult = await CheckProductActivationStatus(token, productId);
                                    if (checkResult)
                                    {
                                        WriteToLogFile($"[AuthService] ActivateProductAsync: Product activation confirmed via status check");
                                        return true;
                                    }
                                    else
                                    {
                                        WriteToLogFile($"[AuthService] ActivateProductAsync: Product activation NOT confirmed via status check");
                                        return false;
                                    }
                                }
                                catch (Exception checkEx)
                                {
                                    WriteToLogFile($"[AuthService] ActivateProductAsync: Error checking activation status: {checkEx.Message}");
                                    // Если не можем проверить, считаем неуспешной
                                    return false;
                                }
                            }
                            
                            // Если не можем десериализовать, но статус успешный, считаем активацию успешной
                            if (responseContent.Contains("success") && responseContent.Contains("True"))
                            {
                                WriteToLogFile($"[AuthService] ActivateProductAsync: Found 'success' and 'True' in response, assuming success");
                                return true;
                            }
                            
                            return false;
                        }
                    }
                    else
                    {
                        WriteToLogFile($"[AuthService] ActivateProductAsync: Request failed with status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] ActivateProductAsync: Exception: {ex.Message}");
                WriteToLogFile($"[AuthService] ActivateProductAsync: Stack trace: {ex.StackTrace}");
            }
            
            WriteToLogFile($"[AuthService] ActivateProductAsync: Returning false");
            return false;
        }

        private async Task<bool> CheckProductActivationStatus(string token, int productId)
        {
            try
            {
                WriteToLogFile($"[AuthService] CheckProductActivationStatus: Checking status for product {productId}");
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var url = $"{API_BASE_URL}/api/user-products";
                    WriteToLogFile($"[AuthService] CheckProductActivationStatus: URL: {url}");
                    
                    var response = await client.GetAsync(url);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    WriteToLogFile($"[AuthService] CheckProductActivationStatus: Response status: {response.StatusCode}");
                    WriteToLogFile($"[AuthService] CheckProductActivationStatus: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                            
                            if (result.ContainsKey("products"))
                            {
                                var products = result["products"] as System.Collections.ArrayList;
                                if (products != null)
                                {
                                    foreach (Dictionary<string, object> product in products)
                                    {
                                        if (product.ContainsKey("id") && Convert.ToInt32(product["id"]) == productId)
                                        {
                                            var status = product.ContainsKey("activation_status") ? product["activation_status"]?.ToString() : "unknown";
                                            WriteToLogFile($"[AuthService] CheckProductActivationStatus: Product {productId} status: {status}");
                                            
                                            // Проверяем, активирован ли продукт
                                            return status?.ToLower() == "activated" || status?.ToLower() == "active";
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteToLogFile($"[AuthService] CheckProductActivationStatus: Error parsing response: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] CheckProductActivationStatus: Exception: {ex.Message}");
            }
            
            WriteToLogFile($"[AuthService] CheckProductActivationStatus: Product {productId} not found or not activated");
            return false;
        }

        public async Task<bool> DeactivateProductAsync(string token, int productId, string revitVersion)
        {
            try
            {
                WriteToLogFile($"[AuthService] DeactivateProductAsync: Starting deactivation, ProductId: {productId}, RevitVersion: {revitVersion}");
                
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        token = token,
                        product_id = productId,
                        revit_version = revitVersion
                    };
                    
                    var json = new JavaScriptSerializer().Serialize(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var url = $"{API_BASE_URL}/api/deactivate-product";
                    WriteToLogFile($"[AuthService] DeactivateProductAsync: URL: {url}");
                    WriteToLogFile($"[AuthService] DeactivateProductAsync: Request data: {json}");
                    
                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    WriteToLogFile($"[AuthService] DeactivateProductAsync: Response status: {response.StatusCode}");
                    WriteToLogFile($"[AuthService] DeactivateProductAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                            WriteToLogFile($"[AuthService] DeactivateProductAsync: Parsed result keys: {string.Join(", ", result.Keys)}");
                            
                            // Проверяем наличие ошибки
                            if (result.ContainsKey("error"))
                            {
                                var errorMessage = result["error"]?.ToString();
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: Error from server: {errorMessage}");
                                return false;
                            }
                            
                            // Проверяем успешность операции
                            if (result.ContainsKey("success"))
                            {
                                var success = Convert.ToBoolean(result["success"]);
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: Success: {success}");
                                return success;
                            }
                            
                            // Если нет поля success, считаем успешным если нет ошибки
                            WriteToLogFile($"[AuthService] DeactivateProductAsync: No 'success' field, assuming success");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            WriteToLogFile($"[AuthService] DeactivateProductAsync: JSON deserialization error: {ex.Message}");
                            WriteToLogFile($"[AuthService] DeactivateProductAsync: Response content: {responseContent}");
                            
                            // Проверяем, если сервер вернул HTML-ошибку
                            if (responseContent.Contains("<br") || responseContent.Contains("<html") || responseContent.Contains("<!DOCTYPE"))
                            {
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: Server returned HTML error instead of JSON");
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: This indicates a server-side PHP error");
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: HTML response content: {responseContent}");
                                
                                // Попробуем альтернативный подход - проверим, есть ли в ответе признаки успеха
                                if (responseContent.Contains("success") || responseContent.Contains("deactivated") || responseContent.Contains("true"))
                                {
                                    WriteToLogFile($"[AuthService] DeactivateProductAsync: Found success indicators in HTML response, assuming success");
                                    return true;
                                }
                                
                                // Если это HTML-ошибка, но статус HTTP успешный, проверим статус на сервере
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: HTML error but HTTP status OK, checking if deactivation actually succeeded");
                                
                                // Попробуем проверить статус продукта через API
                                try
                                {
                                    var checkResult = await CheckProductActivationStatus(token, productId);
                                    if (!checkResult)
                                    {
                                        WriteToLogFile($"[AuthService] DeactivateProductAsync: Product deactivation confirmed via status check");
                                        return true;
                                    }
                                    else
                                    {
                                        WriteToLogFile($"[AuthService] DeactivateProductAsync: Product deactivation NOT confirmed via status check");
                                        return false;
                                    }
                                }
                                catch (Exception checkEx)
                                {
                                    WriteToLogFile($"[AuthService] DeactivateProductAsync: Error checking deactivation status: {checkEx.Message}");
                                    // Если не можем проверить, считаем неуспешной
                                    return false;
                                }
                            }
                            
                            // Если не можем десериализовать, но статус успешный, считаем деактивацию успешной
                            if (responseContent.Contains("success") && responseContent.Contains("True"))
                            {
                                WriteToLogFile($"[AuthService] DeactivateProductAsync: Found 'success' and 'True' in response, assuming success");
                                return true;
                            }
                            
                            return false;
                        }
                    }
                    else
                    {
                        WriteToLogFile($"[AuthService] DeactivateProductAsync: Request failed with status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] DeactivateProductAsync: Exception: {ex.Message}");
                WriteToLogFile($"[AuthService] DeactivateProductAsync: Stack trace: {ex.StackTrace}");
            }
            
            WriteToLogFile($"[AuthService] DeactivateProductAsync: Returning false");
            return false;
        }

        #endregion

        #region Plugin Management

        public bool IsPluginAvailable(string pluginName, string revitVersion = "2023")
        {
            try
            {
                var token = GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                // Используем Task.Run для избежания deadlock
                var products = Task.Run(async () => await GetUserProductsAsync(token)).GetAwaiter().GetResult();
                
                foreach (var product in products)
                {
                    if ((product.ProductName == pluginName || 
                         (pluginName == "DataViewer" && product.ProductName == "DataHub")) &&
                        product.RevitVersion == revitVersion &&
                        product.IsActive)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAvailablePlugins(string revitVersion = "2023")
        {
            var availablePlugins = new List<string>();
            
            try
            {
                var token = GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    return availablePlugins;
                }

                var products = GetUserProductsAsync(token).Result;
                
                foreach (var product in products)
                {
                    if (product.RevitVersion == revitVersion && product.IsActive)
                    {
                        if (product.ProductName == "DataHub")
                        {
                            availablePlugins.Add("DataViewer");
                        }
                        else
                        {
                            availablePlugins.Add(product.ProductName);
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            
            return availablePlugins;
        }

        public async Task<Dictionary<string, object>> GetVersionAsync(string versionCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var requestData = new { version_code = versionCode };
                    var json = new JavaScriptSerializer().Serialize(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var url = $"{API_BASE_URL}/auth-api.php/api/get-version";
                    var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            
            return new Dictionary<string, object>();
        }

        #endregion

        #region Encryption

        private string EncryptToken(string token)
        {
            try
            {
                WriteToLogFile($"[AuthService] EncryptToken: Starting encryption, token length: {token?.Length ?? 0}");
                
                using (var aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String("Q2l2aWxYU2VjcmV0S2V5MTIzNDU2Nzg5MDEyMzQ1NkE="); // 32 bytes
                    aes.IV = Convert.FromBase64String("Q2l2aWxYSW5pdFZlY3Rvcg=="); // 16 bytes
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(token);
                        swEncrypt.Close();
                        var result = Convert.ToBase64String(msEncrypt.ToArray());
                        WriteToLogFile($"[AuthService] EncryptToken: Encryption successful, result length: {result?.Length ?? 0}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] EncryptToken: Encryption failed - {ex.Message}");
                return token; // Возвращаем исходный токен в случае ошибки
            }
        }

        private string DecryptToken(string encryptedToken)
        {
            try
            {
                WriteToLogFile($"[AuthService] DecryptToken: Starting decryption, encrypted length: {encryptedToken?.Length ?? 0}");
                
                using (var aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String("Q2l2aWxYU2VjcmV0S2V5MTIzNDU2Nzg5MDEyMzQ1NkE="); // 32 bytes
                    aes.IV = Convert.FromBase64String("Q2l2aWxYSW5pdFZlY3Rvcg=="); // 16 bytes
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedToken)))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        var result = srDecrypt.ReadToEnd();
                        WriteToLogFile($"[AuthService] DecryptToken: Decryption successful, result length: {result?.Length ?? 0}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[AuthService] DecryptToken: Decryption failed - {ex.Message}");
                return null; // Возвращаем null в случае ошибки
            }
        }

        #endregion
    }
}
