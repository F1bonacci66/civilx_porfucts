# CivilX.Shared Library

Общая библиотека для всех плагинов CivilX, предоставляющая централизованную авторизацию, управление продуктами и API клиент.

## Возможности

- **Централизованная авторизация**: Единый механизм входа/регистрации для всех плагинов
- **Управление токенами**: Безопасное хранение и шифрование токенов авторизации
- **Проверка активации продуктов**: Автоматическая проверка доступности плагинов
- **API клиент**: Унифицированный клиент для работы с сервером
- **Кэширование**: Оптимизация производительности через кэширование данных
- **Менеджер плагинов**: Простой API для проверки доступности плагинов

## Структура

```
CivilX.Shared/
├── Auth/
│   ├── IAuthService.cs      # Интерфейс сервиса авторизации
│   └── AuthService.cs       # Реализация сервиса авторизации
├── Models/
│   ├── UserInfo.cs          # Модель пользователя
│   ├── ProductInfo.cs       # Модель продукта
│   └── AuthResult.cs        # Результат авторизации
├── Services/
│   ├── PluginManager.cs     # Менеджер плагинов
│   ├── CacheManager.cs      # Менеджер кэширования
│   └── ApiClient.cs         # API клиент
└── Examples/
    └── DataViewerExample.cs # Пример использования
```

## Использование

### 1. Проверка доступности плагина

```csharp
using CivilX.Shared.Services;

// Простая проверка
bool isAvailable = PluginManager.IsPluginAvailable("DataViewer", "2023");

// Проверка для текущей версии Revit
string revitApiVersion = "2023.12"; // Получается из Revit API
bool isAvailable = PluginManager.IsPluginAvailableForCurrentRevit("DataViewer", revitApiVersion);
```

### 2. Получение информации о продукте

```csharp
// Получение статуса продукта
string status = PluginManager.GetProductStatus("DataHub", "2023");

// Получение детальной информации
ProductInfo product = PluginManager.GetProductInfo("DataHub", "2023");
if (product != null && product.IsActive)
{
    // Продукт активирован
}
```

### 3. Работа с авторизацией

```csharp
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

var authService = new AuthService();

// Проверка авторизации
string token = authService.GetStoredToken();
bool isAuthorized = !string.IsNullOrEmpty(token);

// Получение информации о пользователе
if (isAuthorized)
{
    UserInfo user = await authService.GetUserInfoAsync(token);
    Console.WriteLine($"Пользователь: {user.FullName}");
}

// Вход в систему
AuthResult result = await authService.LoginAsync("email@example.com", "password");
if (result.Success)
{
    Console.WriteLine("Успешный вход!");
}
```

### 4. Получение списка продуктов

```csharp
// Получение всех продуктов пользователя
List<ProductInfo> products = PluginManager.GetUserProducts();

foreach (var product in products)
{
    Console.WriteLine($"Продукт: {product.ProductName}");
    Console.WriteLine($"Revit: {product.RevitVersion}");
    Console.WriteLine($"Статус: {product.Status}");
}
```

### 5. Использование кэширования

```csharp
using CivilX.Shared.Services;

// Сохранение в кэш
CacheManager.Set("user_products", products);

// Получение из кэша
List<ProductInfo> cachedProducts = CacheManager.Get<List<ProductInfo>>("user_products");

// Проверка существования
if (CacheManager.Exists("user_products"))
{
    // Данные есть в кэше
}
```

## Интеграция в плагины

### 1. Добавление ссылки

В файле `.csproj` добавьте ссылку на CivilX.Shared.dll:

```xml
<Reference Include="CivilX.Shared">
  <HintPath>..\CivilX_export\CivilX.Shared.dll</HintPath>
</Reference>
```

### 2. Пример для DataViewer

```csharp
using CivilX.Shared.Services;

public class ExportCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // Проверяем доступность плагина
        string revitVersion = commandData.Application.Application.VersionNumber;
        if (!PluginManager.IsPluginAvailableForCurrentRevit("DataViewer", revitVersion))
        {
            TaskDialog.Show("Ошибка", "DataViewer недоступен для данной версии Revit");
            return Result.Succeeded;
        }

        // Плагин доступен, продолжаем работу
        var mainWindow = new MainWindow();
        mainWindow.Show();
        
        return Result.Succeeded;
    }
}
```

### 3. Пример для плагина авторизации

```csharp
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

public partial class AuthWindow : Window
{
    private readonly IAuthService _authService = new AuthService();

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await _authService.LoginAsync(emailTextBox.Text, passwordBox.Password);
        
        if (result.Success)
        {
            // Успешный вход
            ShowUserProfileScreen();
        }
        else
        {
            MessageBox.Show(result.ErrorMessage);
        }
    }

    private async void LoadUserProducts()
    {
        var token = _authService.GetStoredToken();
        var products = await _authService.GetUserProductsAsync(token);
        
        // Обновляем UI
        UpdateProductsTable(products);
    }
}
```

## Конфигурация

### API URL

По умолчанию используется: `http://civilx.ru`

Для изменения URL отредактируйте константу `API_BASE_URL` в `AuthService.cs` и `ApiClient.cs`.

### Кэширование

Время жизни кэша по умолчанию: 5 минут

Для изменения отредактируйте константу `_cacheExpiration` в `CacheManager.cs`.

## Безопасность

- Токены авторизации шифруются с помощью AES
- Токены сохраняются в реестре Windows и в зашифрованном файле
- Все API запросы используют HTTPS (при настройке)
- Кэш хранится локально в AppData пользователя

## Требования

- .NET Framework 4.8
- Windows 7 или выше
- Доступ к интернету для API запросов

## Лицензия

Copyright © CivilX 2025
