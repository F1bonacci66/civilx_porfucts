using System.Collections.Generic;
using System.Threading.Tasks;
using CivilX.Shared.Models;

namespace CivilX.Shared.Auth
{
    public interface IAuthService
    {
        // Управление токеном
        string GetStoredToken();
        void SaveToken(string token);
        void ClearToken();
        
        // Аутентификация
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RegisterAsync(string username, string email, string password);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserInfo> GetUserInfoAsync(string token);
        
        // Управление продуктами
        Task<List<ProductInfo>> GetUserProductsAsync(string token);
        Task<bool> ActivateProductAsync(string token, int productId, string revitVersion, string productVersion);
        Task<bool> DeactivateProductAsync(string token, int productId, string revitVersion);
        
        // Проверка доступа к плагинам
        bool IsPluginAvailable(string pluginName, string revitVersion = "2023");
        List<string> GetAvailablePlugins(string revitVersion = "2023");
        
        // Получение версии
        Task<Dictionary<string, object>> GetVersionAsync(string versionCode);
    }
}
