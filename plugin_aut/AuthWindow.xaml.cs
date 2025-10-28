using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Web.Script.Serialization;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;
using CivilX.Shared.Services;

namespace CivilXAuthPlugin
{
    public partial class AuthWindow : Window
    {
        // Константа группы плагина
        private const string PLUGIN_GROUP = "settings";
        
        // Константа версии Revit для этого плагина
        private const string REVIT_VERSION = "2023";
        
        private readonly IAuthService _authService;
        private UserInfo _currentUser;
        private ProductInfo _activatedProduct;

        public AuthWindow()
        {
            try
            {
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Starting initialization...");
                
                InitializeComponent();
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: InitializeComponent completed");
                
                _authService = new AuthService();
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: AuthService created");
                
                _activatedProduct = null;
                
                // Обновляем версию в заголовке
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Updating window title...");
                UpdateWindowTitle();
                
                // Загружаем логотип
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Loading logo...");
                LoadLogo();
                
                // Загружаем активированный продукт
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Loading activated product...");
                LoadActivatedProduct();
                
                // Проверяем авторизацию при запуске
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Checking authentication...");
                CheckAuthentication();
                
                // Проверяем группу плагина
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Checking plugin group...");
                CheckPluginGroup();
                
                // Тестовое логирование
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor: Plugin initialized successfully");
                
                // Показываем успешную инициализацию
                ShowSuccess("Плагин успешно инициализирован!");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Ошибка инициализации плагина: {ex.Message}\n\nТип ошибки: {ex.GetType().Name}\n\nСтек вызовов:\n{ex.StackTrace}";
                ShowError(errorMsg);
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AuthWindow constructor error: {ex.Message}\n{ex.StackTrace}";
                WriteToLogFile(logMessage);
                MessageBox.Show($"Ошибка инициализации плагина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLogo()
        {
            try
            {
                // Пытаемся загрузить логотип из встроенного ресурса
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "CivilXAuthPlugin.main_icon.jpg";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        
                        LogoImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки логотипа: {ex.Message}");
            }
        }

        private async void CheckAuthentication()
        {
            try
            {
                var token = _authService.GetStoredToken();
                if (!string.IsNullOrEmpty(token))
                {
                    // Показываем экран загрузки для авторизованных пользователей
                    ShowLoadingScreen();
                    
                    var isValid = await _authService.ValidateTokenAsync(token);
                    if (isValid)
                    {
                        _currentUser = await _authService.GetUserInfoAsync(token);
                        
                        // Задержка для показа экрана загрузки
                        await Task.Delay(1500);
                        
                        ShowMainScreen();
                        UpdateUserStatusIndicator();
                        return;
                    }
                    else
                    {
                        _authService.ClearToken();
                    }
                }
                
                ShowAuthScreen();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке авторизации: {ex.Message}");
                ShowAuthScreen();
            }
        }

        private void ShowAuthScreen()
        {
            AuthScreen.Visibility = Visibility.Visible;
            LoadingScreen.Visibility = Visibility.Collapsed;
            MainScreen.Visibility = Visibility.Collapsed;
        }

        private void ShowLoadingScreen()
        {
            AuthScreen.Visibility = Visibility.Collapsed;
            LoadingScreen.Visibility = Visibility.Visible;
            MainScreen.Visibility = Visibility.Collapsed;
            
            // Запускаем анимацию спиннера
            StartLoadingAnimation();
        }

        private void ShowMainScreen()
        {
            AuthScreen.Visibility = Visibility.Collapsed;
            LoadingScreen.Visibility = Visibility.Collapsed;
            MainScreen.Visibility = Visibility.Visible;
            ShowUserProfileScreen();
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Visible;
            RegisterForm.Visibility = Visibility.Collapsed;
            LoginTabButton.Background = new SolidColorBrush(Color.FromRgb(59, 130, 246));
            RegisterTabButton.Background = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginForm.Visibility = Visibility.Collapsed;
            RegisterForm.Visibility = Visibility.Visible;
            RegisterTabButton.Background = new SolidColorBrush(Color.FromRgb(59, 130, 246));
            LoginTabButton.Background = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HideMessages();
                
                var email = LoginEmail.Text;
                var password = LoginPassword.Password;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ShowError("Пожалуйста, заполните все поля");
                    return;
                }

                // Показываем процесс авторизации
                ShowError("Выполняется авторизация...");
                
                var result = await _authService.LoginAsync(email, password);
                
                // Логируем результат авторизации
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoginAsync: Success={result.Success}, ErrorMessage='{result.ErrorMessage}'";
                WriteToLogFile(logMessage);
                
                if (result.Success)
                {
                    _authService.SaveToken(result.Token);
                    _currentUser = result.User;
                    
                    // Логируем успешную авторизацию
                    var logSuccess = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoginAsync: SUCCESS - User: {_currentUser?.Username}, Email: {_currentUser?.Email}";
                    WriteToLogFile(logSuccess);
                    
                    ShowMainScreen();
                    UpdateUserStatusIndicator();
                    ShowSuccess("Успешный вход в систему!");
                }
                else
                {
                    var errorMsg = string.IsNullOrEmpty(result.ErrorMessage) ? "Неизвестная ошибка" : result.ErrorMessage;
                    var detailedError = $"Ошибка авторизации: {errorMsg}\n\nДетали:\n- Success: {result.Success}\n- ErrorMessage: '{result.ErrorMessage}'\n- Token: {(string.IsNullOrEmpty(result.Token) ? "НЕТ" : "ЕСТЬ")}";
                    ShowError(detailedError);
                }
            }
            catch (Exception ex)
            {
                var detailedError = $"Ошибка входа: {ex.Message}\n\nТип ошибки: {ex.GetType().Name}\n\nСтек вызовов:\n{ex.StackTrace}";
                ShowError(detailedError);
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoginButton_Click Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HideMessages();
                
                var userType = RegisterUserType.Text;
                var name = RegisterUserName.Text;
                var login = RegisterLogin.Text;
                var email = RegisterEmail.Text;
                var password = RegisterPassword.Password;
                var passwordConfirm = RegisterPasswordConfirm.Password;

                if (string.IsNullOrEmpty(userType) || string.IsNullOrEmpty(name) || 
                    string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email) || 
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
                {
                    ShowError("Пожалуйста, заполните все поля");
                    return;
                }

                if (password != passwordConfirm)
                {
                    ShowError("Пароли не совпадают");
                    return;
                }

                var result = await _authService.RegisterAsync(login, email, password);
                
                if (result.Success)
                {
                    _authService.SaveToken(result.Token);
                    _currentUser = result.User;
                    ShowMainScreen();
                    UpdateUserStatusIndicator();
                    ShowSuccess("Регистрация прошла успешно!");
                }
                else
                {
                    ShowError($"Ошибка регистрации: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void ShowUserProfileScreen()
        {
            // Очищаем все элементы
            MainGrid.Children.Clear();
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            // Левая панель - Вкладки параметров
            var leftPanel = CreateUserProfileLeftPanel();
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);
            
            // GridSplitter
            var splitter = new GridSplitter
            {
                Width = 5,
                Background = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetColumn(splitter, 1);
            grid.Children.Add(splitter);
            
            // Правая панель - Настройки (изначально показываем авторизацию)
            var rightPanel = CreateUserProfileRightPanel("Авторизация");
            Grid.SetColumn(rightPanel, 2);
            grid.Children.Add(rightPanel);
            
            MainGrid.Children.Add(grid);
        }

        private Border CreateUserProfileLeftPanel()
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };
            
            // Заголовок
            var title = new TextBlock
            {
                Text = "Профиль пользователя",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            stackPanel.Children.Add(title);
            
            // Подзаголовок
            var subtitle = new TextBlock
            {
                Text = "Настройки профиля:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(subtitle);
            
            // Список вкладок
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400
            };
            var tabsList = new StackPanel();
            
            // Вкладка "Информация о пользователе"
            var infoTab = CreateTabButton("👤 Информация о пользователе", "Информация о пользователе");
            tabsList.Children.Add(infoTab);
            
            // Вкладка "Мои продукты"
            var productsTab = CreateTabButton("📦 Мои продукты", "Мои продукты");
            tabsList.Children.Add(productsTab);
            
            // Вкладка "Управление профилем"
            var profileTab = CreateTabButton("⚙️ Управление профилем", "Управление профилем");
            tabsList.Children.Add(profileTab);
            
            // Вкладка "Управление аккаунтом"
            var authTab = CreateTabButton("🔐 Управление аккаунтом", "Авторизация");
            tabsList.Children.Add(authTab);
            
            scrollViewer.Content = tabsList;
            stackPanel.Children.Add(scrollViewer);
            
            border.Child = stackPanel;
            return border;
        }

        private Button CreateTabButton(string content, string tabName)
        {
            var button = new Button
            {
                Content = content,
                FontSize = 14,
                Height = 45,
                Margin = new Thickness(0, 0, 0, 8),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0)
            };
            
            button.Click += (s, e) => {
                // Обновляем правую панель
                var grid = MainGrid.Children.OfType<Grid>().FirstOrDefault();
                if (grid != null)
                {
                    var rightPanel = grid.Children.OfType<Border>().LastOrDefault();
                    if (rightPanel != null)
                    {
                        var newRightPanel = CreateUserProfileRightPanel(tabName);
                        Grid.SetColumn(newRightPanel, 2);
                        grid.Children.Remove(rightPanel);
                        grid.Children.Add(newRightPanel);
                    }
                }
            };
            
            return button;
        }

        private Border CreateUserProfileRightPanel(string selectedTab)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Padding = new Thickness(40)
            };

            var stackPanel = new StackPanel();

            switch (selectedTab)
            {
                case "Информация о пользователе":
                    LoadUserInfo(stackPanel);
                    break;
                case "Мои продукты":
                    LoadUserProducts(stackPanel);
                    break;
                case "Управление профилем":
                    LoadProfileEditForm(stackPanel);
                    break;
                case "Авторизация":
                    LoadAuthManagement(stackPanel);
                    break;
            }

            border.Child = stackPanel;
            return border;
        }

        private async void LoadUserInfo(StackPanel parentPanel)
        {
            var title = new TextBlock
            {
                Text = "Информация о пользователе",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            parentPanel.Children.Add(title);

            try
            {
                var token = _authService.GetStoredToken();
                var logToken = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserInfo: Token = {(string.IsNullOrEmpty(token) ? "EMPTY" : "FOUND")}";
                WriteToLogFile(logToken);
                
                if (!string.IsNullOrEmpty(token))
                {
                    var userInfo = await _authService.GetUserInfoAsync(token);
                    var logUserInfo = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserInfo: UserInfo = {(userInfo == null ? "NULL" : $"Username: {userInfo.Username}, Email: {userInfo.Email}")}";
                    WriteToLogFile(logUserInfo);
                    
                    if (userInfo != null)
                    {
                        AddInfoField(parentPanel, "Имя", userInfo.Name ?? "Не указано");
                        AddInfoField(parentPanel, "Логин", userInfo.Login ?? "Не указано");
                        AddInfoField(parentPanel, "Email", userInfo.Email ?? "Не указано");
                        AddInfoField(parentPanel, "Тип пользователя", GetUserTypeDisplay(userInfo.UserType));
                        AddInfoField(parentPanel, "Компания", userInfo.CompanyName ?? "Не указано");
                        AddInfoField(parentPanel, "Телефон", userInfo.Phone ?? "Не указано");
                        AddInfoField(parentPanel, "Дата регистрации", userInfo.CreatedAt ?? "Не указано");
                    }
                    else
                    {
                        var noUserText = new TextBlock
                        {
                            Text = "Информация о пользователе не найдена",
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                            Margin = new Thickness(0, 20, 0, 0)
                        };
                        parentPanel.Children.Add(noUserText);
                    }
                }
                else
                {
                    var noTokenText = new TextBlock
                    {
                        Text = "Токен авторизации не найден",
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(noTokenText);
                }
            }
            catch (Exception ex)
            {
                var logError = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserInfo Exception: {ex.Message}";
                WriteToLogFile(logError);
                
                var errorText = new TextBlock
                {
                    Text = $"Ошибка загрузки информации: {ex.Message}",
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Margin = new Thickness(0, 20, 0, 0)
                };
                parentPanel.Children.Add(errorText);
            }
        }

        private void AddInfoField(StackPanel parent, string label, string value)
        {
            var fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
            
            var labelText = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Margin = new Thickness(0, 0, 0, 5)
            };
            fieldPanel.Children.Add(labelText);
            
            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            fieldPanel.Children.Add(valueText);
            
            parent.Children.Add(fieldPanel);
        }

        private string GetUserTypeDisplay(string userType)
        {
            switch (userType?.ToLower())
            {
                case "user":
                    return "Пользователь";
                case "organization":
                    return "Организация";
                default:
                    return userType ?? "Не указано";
            }
        }

        private async void LoadUserProducts(StackPanel parentPanel)
        {
            var title = new TextBlock
            {
                Text = "Мои продукты",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            parentPanel.Children.Add(title);

            try
            {
                var token = _authService.GetStoredToken();
                var logToken = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserProducts: Token = {(string.IsNullOrEmpty(token) ? "EMPTY" : "FOUND")}";
                WriteToLogFile(logToken);
                
                if (!string.IsNullOrEmpty(token))
                {
                    var products = await GetUserProductsAsync(token);
                    var logProducts = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserProducts: Products count = {(products?.Count ?? 0)}";
                    WriteToLogFile(logProducts);
                    
                    if (products != null && products.Any())
                    {
                        CreateProductsTable(parentPanel, products);
                    }
                    else
                    {
                        var noProductsText = new TextBlock
                        {
                            Text = "У вас пока нет купленных продуктов",
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                            Margin = new Thickness(0, 20, 0, 0)
                        };
                        parentPanel.Children.Add(noProductsText);
                    }
                }
                else
                {
                    var noTokenText = new TextBlock
                    {
                        Text = "Токен авторизации не найден",
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(noTokenText);
                }
                
                // Добавляем блок состояния подписки
                CreateSubscriptionStatusBlock(parentPanel);
            }
            catch (Exception ex)
            {
                var logError = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadUserProducts Exception: {ex.Message}";
                WriteToLogFile(logError);
                
                var errorText = new TextBlock
                {
                    Text = $"Ошибка загрузки продуктов: {ex.Message}",
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Margin = new Thickness(0, 20, 0, 0)
                };
                parentPanel.Children.Add(errorText);
            }
        }

        private async Task<List<ProductInfo>> GetUserProductsAsync(string token)
        {
            try
            {
                // Используем Shared Library для получения продуктов
                var allProducts = await _authService.GetUserProductsAsync(token);
                
                // Фильтруем продукты - скрываем группу "authorization" от пользователя
                var filteredProducts = allProducts.Where(p => 
                    p.ProductName?.ToLower() != "authorization" && 
                    p.ProductName?.ToLower() != "settings"
                ).ToList();
                
                return filteredProducts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения продуктов: {ex.Message}");
                return new List<ProductInfo>();
            }
        }

        private void CreateProductsTable(StackPanel parentPanel, List<ProductInfo> products)
        {
            // Создаем панель с таблицей
            var tablePanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Margin = new Thickness(20, 20, 20, 0)
            };

            // Заголовок таблицы
            var headerPanel = new Grid();
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            var headerProduct = new TextBlock
            {
                Text = "Продукт",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerProduct, 0);

            var headerVersion = new TextBlock
            {
                Text = "Версия Revit + Продукт",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerVersion, 1);

            var headerStatus = new TextBlock
            {
                Text = "Статус",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerStatus, 2);

            var headerExpires = new TextBlock
            {
                Text = "Истекает",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerExpires, 3);

            var headerAction = new TextBlock
            {
                Text = "Действие",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerAction, 4);

            headerPanel.Children.Add(headerProduct);
            headerPanel.Children.Add(headerVersion);
            headerPanel.Children.Add(headerStatus);
            headerPanel.Children.Add(headerExpires);
            headerPanel.Children.Add(headerAction);

            tablePanel.Children.Add(headerPanel);

            // Добавляем строки для каждого продукта
            foreach (var product in products)
            {
                var rowPanel = new Grid();
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                // Продукт
                var productName = new TextBlock
                {
                    Text = GetProductDisplayName(product.ProductName),
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetColumn(productName, 0);

                // Версия Revit + Продукт
                var version = new TextBlock
                {
                    Text = $"Revit {product.RevitVersion} v{product.ProductVersion}",
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetColumn(version, 1);

                // Статус
                var status = new TextBlock
                {
                    Text = GetStatusDisplayText(product.ActivationStatus),
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(GetStatusColor(product.ActivationStatus))
                };
                Grid.SetColumn(status, 2);

                // Истекает
                var expires = new TextBlock
                {
                    Text = product.ActivatedAt?.ToString("dd.MM.yyyy") ?? "—",
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                Grid.SetColumn(expires, 3);

                // Действие
                var actionButton = new Button
                {
                    Content = GetActionButtonText(product.ActivationStatus),
                    FontSize = 12,
                    Height = 30,
                    Width = 100,
                    Margin = new Thickness(10, 10, 10, 10),
                    Background = new SolidColorBrush(GetActionButtonColor(product.ActivationStatus)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0)
                };
                actionButton.Click += (s, e) => {
                    WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Button clicked for product {product.ProductName} (ID: {product.Id})");
                    HandleProductAction(product);
                };
                Grid.SetColumn(actionButton, 4);

                rowPanel.Children.Add(productName);
                rowPanel.Children.Add(version);
                rowPanel.Children.Add(status);
                rowPanel.Children.Add(expires);
                rowPanel.Children.Add(actionButton);

                tablePanel.Children.Add(rowPanel);
            }

            parentPanel.Children.Add(tablePanel);
        }

        private string GetProductDisplayName(string productName)
        {
            switch (productName?.ToLower())
            {
                case "dataviewer":
                    return "DataViewer";
                default:
                    return productName ?? "Неизвестный продукт";
            }
        }

        private string GetStatusDisplayText(string status)
        {
            switch (status?.ToLower())
            {
                case "pending":
                    return "Ожидает активации";
                case "ready":
                    return "Готов";
                case "activated":
                case "active":
                    return "Активирован";
                case "expired":
                    return "Истек";
                default:
                    return "Готов"; // По умолчанию показываем "Готов" для неактивированных продуктов
            }
        }

        private Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "pending":
                    return Color.FromRgb(255, 193, 7);
                case "ready":
                    return Color.FromRgb(255, 193, 7); // Желтый цвет для "Готов"
                case "activated":
                case "active":
                    return Color.FromRgb(40, 167, 69); // Зеленый цвет для "Активирован"
                case "expired":
                    return Color.FromRgb(220, 53, 69);
                default:
                    return Color.FromRgb(255, 193, 7); // Желтый цвет по умолчанию для "Готов"
            }
        }

        private string GetActionButtonText(string status)
        {
            switch (status?.ToLower())
            {
                case "pending":
                    return "Активировать";
                case "ready":
                    return "Активировать";
                case "activated":
                case "active":
                    return "Отменить";
                case "expired":
                    return "Продлить";
                default:
                    return "Активировать"; // По умолчанию показываем "Активировать"
            }
        }

        private Color GetActionButtonColor(string status)
        {
            switch (status?.ToLower())
            {
                case "pending":
                    return Color.FromRgb(40, 167, 69);
                case "ready":
                    return Color.FromRgb(40, 167, 69); // Зеленый цвет для кнопки "Активировать"
                case "activated":
                case "active":
                    return Color.FromRgb(220, 53, 69); // Красный для кнопки "Отменить"
                case "expired":
                    return Color.FromRgb(0, 123, 255);
                default:
                    return Color.FromRgb(40, 167, 69); // Зеленый цвет по умолчанию для "Активировать"
            }
        }

        private async void HandleProductAction(ProductInfo product)
        {
            // Если статус пустой или null, считаем что продукт готов к активации
            var status = product.ActivationStatus?.ToLower() ?? "ready";
            
            // Записываем в файл логов
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] HandleProductAction: Product ID {product.Id}, Status: '{product.ActivationStatus}' -> '{status}', Name: {product.ProductName}";
            WriteToLogFile(logMessage);
            
            // Временная отладка через MessageBox
            MessageBox.Show($"Обработка активации:\nПродукт ID: {product.Id}\nСтатус: '{product.ActivationStatus}' -> '{status}'\nНазвание: {product.ProductName}", "Отладка активации");
            
            switch (status)
            {
                case "ready":
                case "pending":
                    await ActivateProductAsync(product);
                    break;
                case "expired":
                    // Открываем сайт для продления
                    System.Diagnostics.Process.Start("http://civilx.ru");
                    break;
                case "activated":
                case "active":
                    await DeactivateProductAsync(product);
                    break;
                default:
                    // По умолчанию пытаемся активировать
                    await ActivateProductAsync(product);
                    break;
            }
        }

        private async Task ActivateProductAsync(ProductInfo product)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Starting activation for product ID {product.Id}");
                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Product name: {product.ProductName}");
                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Current status: {product.ActivationStatus}");
                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Revit version: {REVIT_VERSION}");
                
                // Записываем в файл логов
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: Starting activation for product {product.ProductName} (ID: {product.Id})";
                WriteToLogFile(logMessage);
                
                // Временная отладка через MessageBox
                MessageBox.Show($"Начинаем активацию:\nПродукт: {product.ProductName}\nID: {product.Id}\nВерсия Revit: {REVIT_VERSION}", "Отладка активации");
                
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    var logTokenNotFound = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: Token not found";
                    WriteToLogFile(logTokenNotFound);
                    ShowError("Токен авторизации не найден");
                    return;
                }

                var logTokenFound = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: Token found, making API request";
                WriteToLogFile(logTokenFound);

                // Используем Shared Library для активации продукта
                var success = await _authService.ActivateProductAsync(token, product.Id, REVIT_VERSION, "1.0");
                
                if (success)
                {
                    var logSuccess = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: SUCCESS - Product activated successfully";
                    WriteToLogFile(logSuccess);
                    
                    // Обновляем статус продукта локально
                    product.ActivationStatus = "activated";
                    
                    // Активируем продукт локально
                    _activatedProduct = product;
                    SaveActivatedProduct();
                    UpdateUserStatusIndicator();
                    UpdateSubscriptionStatusBlock();
                    
                    // Перезагружаем список продуктов
                    ShowUserProfileScreen();
                    
                    ShowSuccess($"Продукт {GetProductDisplayName(product.ProductName)} успешно активирован!");
                }
                else
                {
                    var logError = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: ERROR - Activation failed";
                    WriteToLogFile(logError);
                    ShowError("Ошибка активации продукта");
                }
            }
            catch (Exception ex)
            {
                var logException = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ActivateProductAsync: EXCEPTION - {ex.Message}";
                WriteToLogFile(logException);
                ShowError($"Ошибка при активации продукта: {ex.Message}");
            }
        }

        private async Task DeactivateProductAsync(ProductInfo product)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Starting deactivation for product ID {product.Id}");
                
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("DeactivateProductAsync: Token not found");
                    ShowError("Токен авторизации не найден");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Token found, making API request");

                // Используем Shared Library для деактивации продукта
                var success = await _authService.DeactivateProductAsync(token, product.Id, REVIT_VERSION);
                
                if (success)
                {
                    var logSuccess = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DeactivateProductAsync: SUCCESS - Product deactivated successfully";
                    WriteToLogFile(logSuccess);
                    
                    // Обновляем статус продукта локально
                    product.ActivationStatus = "ready";
                    
                    // Если это был активированный продукт, очищаем его
                    if (_activatedProduct != null && _activatedProduct.Id == product.Id)
                    {
                        _activatedProduct = null;
                        SaveActivatedProduct();
                    }
                    
                    UpdateUserStatusIndicator();
                    UpdateSubscriptionStatusBlock();
                    
                    // Перезагружаем список продуктов
                    ShowUserProfileScreen();
                    
                    ShowSuccess($"Продукт {GetProductDisplayName(product.ProductName)} успешно отменен!");
                }
                else
                {
                    var logError = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DeactivateProductAsync: ERROR - Deactivation failed";
                    WriteToLogFile(logError);
                    ShowError("Ошибка деактивации продукта");
                }
            }
            catch (Exception ex)
            {
                var logException = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DeactivateProductAsync: EXCEPTION - {ex.Message}";
                WriteToLogFile(logException);
                ShowError($"Ошибка при отмене продукта: {ex.Message}");
            }
        }

        private void ActivateProduct(ProductInfo product)
        {
            _activatedProduct = product;
            SaveActivatedProduct();
            UpdateUserStatusIndicator();
            UpdateSubscriptionStatusBlock();
            ShowSuccess($"Продукт {GetProductDisplayName(product.ProductName)} успешно активирован!");
        }

        private void CreateSubscriptionStatusBlock(StackPanel parentPanel)
        {
            var statusBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 40, 0, 0),
                Padding = new Thickness(20)
            };

            var statusPanel = new StackPanel();
            
            var statusTitle = new TextBlock
            {
                Text = "Состояние подписки плагина",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            statusPanel.Children.Add(statusTitle);

            var activatedProductBlock = new StackPanel { Name = "ActivatedProductBlock" };
            statusPanel.Children.Add(activatedProductBlock);

            statusBorder.Child = statusPanel;
            parentPanel.Children.Add(statusBorder);

            // Загружаем активированный продукт и обновляем блок
            LoadActivatedProduct();
            UpdateSubscriptionStatusBlock();
        }

        private void UpdateSubscriptionStatusBlock()
        {
            var activatedProductBlock = FindName("ActivatedProductBlock") as StackPanel;
            if (activatedProductBlock == null) return;

            activatedProductBlock.Children.Clear();

            if (_activatedProduct != null)
            {
                var productInfo = new StackPanel();
                
                var productName = new TextBlock
                {
                    Text = $"Активированный продукт: {GetProductDisplayName(_activatedProduct.ProductName)}",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69))
                };
                productInfo.Children.Add(productName);

                var status = new TextBlock
                {
                    Text = $"Статус: {GetStatusDisplayText(_activatedProduct.ActivationStatus)}",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                productInfo.Children.Add(status);

                var expires = new TextBlock
                {
                    Text = $"Истекает: {_activatedProduct.ActivatedAt?.ToString("dd.MM.yyyy") ?? "Не указано"}",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 15),
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                productInfo.Children.Add(expires);

                var deactivateButton = new Button
                {
                    Content = "Деактивировать",
                    FontSize = 14,
                    Height = 35,
                    Width = 120,
                    Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0)
                };
                deactivateButton.Click += (s, e) => DeactivateProduct();
                productInfo.Children.Add(deactivateButton);

                activatedProductBlock.Children.Add(productInfo);
            }
            else
            {
                var noProductText = new TextBlock
                {
                    Text = "Нет активированных продуктов. Активируйте продукт из списка выше.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    TextWrapping = TextWrapping.Wrap
                };
                activatedProductBlock.Children.Add(noProductText);
            }
        }

        private void DeactivateProduct()
        {
            _activatedProduct = null;
            SaveActivatedProduct();
            UpdateUserStatusIndicator();
            UpdateSubscriptionStatusBlock();
            ShowSuccess("Продукт деактивирован");
        }

        private void LoadProfileEditForm(StackPanel parentPanel)
        {
            var title = new TextBlock
            {
                Text = "Управление профилем",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            parentPanel.Children.Add(title);

            var placeholderText = new TextBlock
            {
                Text = "Функция редактирования профиля будет добавлена в следующих версиях",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Margin = new Thickness(0, 20, 0, 0)
            };
            parentPanel.Children.Add(placeholderText);
        }

        private void LoadAuthManagement(StackPanel parentPanel)
        {
            var title = new TextBlock
            {
                Text = "Управление аккаунтом",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            parentPanel.Children.Add(title);

            if (_currentUser != null)
            {
                var userInfo = new TextBlock
                {
                    Text = $"Вы вошли как: {_currentUser.Name} ({_currentUser.Email})",
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                parentPanel.Children.Add(userInfo);
            }

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var websiteButton = new Button
            {
                Content = "🌐 Перейти на сайт",
                FontSize = 14,
                Height = 40,
                Width = 150,
                Margin = new Thickness(0, 0, 15, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            websiteButton.Click += (s, e) => System.Diagnostics.Process.Start("http://civilx.ru");
            buttonPanel.Children.Add(websiteButton);

            var logoutButton = new Button
            {
                Content = "🚪 Выйти из аккаунта",
                FontSize = 14,
                Height = 40,
                Width = 150,
                Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            logoutButton.Click += (s, e) => Logout();
            buttonPanel.Children.Add(logoutButton);

            parentPanel.Children.Add(buttonPanel);
        }

        private void Logout()
        {
            _authService.ClearToken();
            _currentUser = null;
            _activatedProduct = null;
            ShowAuthScreen();
            ShowSuccess("Вы успешно вышли из системы");
        }

        private void SaveActivatedProduct()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var civilXPath = Path.Combine(appDataPath, "CivilX", "AuthPlugin");
                Directory.CreateDirectory(civilXPath);
                var filePath = Path.Combine(civilXPath, "activated_product.json");

                if (_activatedProduct != null)
                {
                    var json = new JavaScriptSerializer().Serialize(_activatedProduct);
                    File.WriteAllText(filePath, json);
                }
                else
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения активированного продукта: {ex.Message}");
            }
        }

        private void LoadActivatedProduct()
        {
            try
            {
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Starting...");
                
                // Используем Shared Library для получения активированных продуктов
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Calling PluginManager.GetUserProducts()...");
                var products = PluginManager.GetUserProducts();
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Got {products?.Count ?? 0} products");
                
                // Ищем продукт для текущей версии Revit (независимо от статуса активации)
                _activatedProduct = products.FirstOrDefault(p => p.RevitVersion == REVIT_VERSION);
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Found activated product: {(_activatedProduct != null ? _activatedProduct.ProductName : "NONE")}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Exception: {ex.Message}");
                WriteToLogFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] LoadActivatedProduct: Stack trace: {ex.StackTrace}");
                _activatedProduct = null;
            }
        }

        private void UpdateUserStatusIndicator()
        {
            // Обновляем индикатор статуса пользователя
            if (_authService != null)
            {
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    // Пользователь не авторизован - прозрачный фон
                    SetUserIcon("👤", Colors.Transparent);
                }
                else if (_activatedProduct != null)
                {
                    // Продукт активирован - зеленый фон
                    SetUserIcon("👤", Colors.Green);
                }
                else
                {
                    // Пользователь авторизован, но продукт не активирован - красный фон
                    SetUserIcon("👤", Colors.Red);
                }
            }
        }

        private void SetUserIcon(string icon, Color backgroundColor)
        {
            // Этот метод будет обновлять иконку пользователя в интерфейсе
            // Пока что просто логируем
            System.Diagnostics.Debug.WriteLine($"User icon: {icon}, Background: {backgroundColor}");
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
            SuccessMessage.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessage.Text = message;
            SuccessMessage.Visibility = Visibility.Visible;
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
            SuccessMessage.Visibility = Visibility.Collapsed;
        }

        // Обработчики событий для полей ввода
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Name.Replace("Register", "").Replace("Login", ""))
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = textBox.Name.Replace("Register", "").Replace("Login", "");
                textBox.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && passwordBox.Password == "Пароль" || passwordBox.Password == "Подтвердите пароль")
            {
                passwordBox.Password = "";
                passwordBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrEmpty(passwordBox.Password))
            {
                passwordBox.Password = passwordBox.Name.Contains("Confirm") ? "Подтвердите пароль" : "Пароль";
                passwordBox.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            }
        }

        private void CheckPluginGroup()
        {
            // Плагин авторизации всегда доступен - не требует подписки
            EnablePlugin();
        }

        private void EnablePlugin()
        {
            // Плагин доступен - можно использовать все функции
            System.Diagnostics.Debug.WriteLine($"Плагин {PLUGIN_GROUP} активирован");
        }

        private void DisablePlugin()
        {
            // Плагин недоступен - показываем сообщение о необходимости подписки
            MessageBox.Show(
                $"Плагин {PLUGIN_GROUP} недоступен. Необходима активная подписка на группу {PLUGIN_GROUP}.",
                "Подписка неактивна",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            
            // Закрываем окно плагина
            this.Close();
        }

        private void StartLoadingAnimation()
        {
            try
            {
                var spinner = FindName("LoadingSpinner") as RotateTransform;
                if (spinner != null)
                {
                    var animation = new DoubleAnimation
                    {
                        From = 0,
                        To = 360,
                        Duration = TimeSpan.FromSeconds(1),
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    
                    spinner.BeginAnimation(RotateTransform.AngleProperty, animation);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка запуска анимации: {ex.Message}");
            }
        }

        private async void UpdateWindowTitle()
        {
            try
            {
                // Получаем версию сборки
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var versionString = $"{version.Major}.{version.Minor}";
                
                // Получаем версию с сервера
                var serverVersion = await GetVersionFromServer();
                
                // Обновляем заголовок окна
                if (!string.IsNullOrEmpty(serverVersion))
                {
                    this.Title = $"CivilX Auth Plugin v{serverVersion}";
                }
                else
                {
                    this.Title = $"CivilX Auth Plugin v{versionString}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления версии: {ex.Message}");
                this.Title = "CivilX Auth Plugin v1.0";
            }
        }

        private async Task<string> GetVersionFromServer()
        {
            try
            {
                // Получаем версию сборки для отправки на сервер
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var versionCode = version.ToString();
                
                // Отправляем запрос на сервер для получения версии
                var response = await _authService.GetVersionAsync(versionCode);
                
                if (response != null && response.ContainsKey("version_number"))
                {
                    return response["version_number"].ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения версии с сервера: {ex.Message}");
            }
            
            return null;
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
            catch (Exception ex)
            {
                // Если не можем записать в файл, показываем MessageBox
                MessageBox.Show($"Ошибка записи в лог: {ex.Message}\nСообщение: {message}", "Ошибка лога");
            }
        }
    }
}
