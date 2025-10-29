using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RevitExporterAddin.Models;
using RevitExporterAddin.Services;
using RevitApp = Autodesk.Revit.ApplicationServices.Application;
using Document = Autodesk.Revit.DB.Document;
using CivilX.Shared.Auth;
using CivilX.Shared.Models;

namespace RevitExporterAddin.Views
{
    public partial class MainWindow : Window
    {
        // Константа группы плагина
        private const string PLUGIN_GROUP = "DataHub";
        
        private List<Project> _projects;
        private readonly IDataService _dataService;
        private readonly ProjectPersistenceService _persistenceService;
        private readonly IAuthService _authService;
        private readonly RevitApp _revitApp;
        private string _currentProjectName;
        private string _currentExportName;
        private string _currentLogFilePath;
        private UserInfo _currentUser;
        private ProductInfo _activatedProduct;

        public MainWindow(RevitApp revitApp = null)
        {
            InitializeComponent();
            _revitApp = revitApp;
            _dataService = new DataService();
            _persistenceService = new ProjectPersistenceService();
            _authService = new AuthService();
            _projects = new List<Project>();
            _activatedProduct = null;
            
            // Загружаем активированный продукт
            LoadActivatedProduct();
            
            // Проверяем авторизацию при запуске - УБРАНО
            // CheckAuthentication();
            
            // Проверка группы плагина перенесена в ExportCommand
            // CheckPluginGroupSync();
            
            // Сразу показываем основной экран
            ShowProjectsScreen();
        }


        // Метод убран - кнопка пользователя больше не нужна
        // private void UserProfileButton_Click(object sender, RoutedEventArgs e)
        // {
        //     ShowUserProfileScreen();
        // }

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
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 30)
            };
            
            var backButton = new Button
            {
                Content = "←",
                FontSize = 18,
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 15, 0)
            };
            backButton.Click += (s, e) => ShowProjectsScreen();
            headerPanel.Children.Add(backButton);
            
            var title = new TextBlock
            {
                Text = "Профиль пользователя",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            headerPanel.Children.Add(title);
            stackPanel.Children.Add(headerPanel);
            
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
            var userInfoButton = new Button
            {
                Content = "👤 Информация о пользователе",
                Height = 50,
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0),
                FontSize = 14
            };
            userInfoButton.Click += (s, e) => UpdateUserProfileRightPanel("Информация о пользователе");
            tabsList.Children.Add(userInfoButton);

            // Вкладка "Мои продукты"
            var productsButton = new Button
            {
                Content = "📦 Мои продукты",
                Height = 50,
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0),
                FontSize = 14
            };
            productsButton.Click += (s, e) => UpdateUserProfileRightPanel("Мои продукты");
            tabsList.Children.Add(productsButton);

            // Вкладка "Управление профилем"
            var profileButton = new Button
            {
                Content = "⚙️ Управление профилем",
                Height = 50,
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0),
                FontSize = 14
            };
            profileButton.Click += (s, e) => UpdateUserProfileRightPanel("Управление профилем");
            tabsList.Children.Add(profileButton);

            // Вкладка "Авторизация"
            var authButton = new Button
            {
                Content = "🔐 Авторизация",
                Height = 50,
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(15, 0, 0, 0),
                FontSize = 14
            };
            authButton.Click += (s, e) => UpdateUserProfileRightPanel("Авторизация");
            tabsList.Children.Add(authButton);
            
            scrollViewer.Content = tabsList;
            stackPanel.Children.Add(scrollViewer);
            
            border.Child = stackPanel;
            return border;
        }

        private Border CreateUserProfileRightPanel(string selectedTab)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };
            
            if (selectedTab == "Информация о пользователе")
            {
                var title = new TextBlock
                {
                    Text = "Информация о пользователе",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                
                // Загружаем информацию о пользователе
                LoadUserInfo(stackPanel);
            }
            else if (selectedTab == "Мои продукты")
            {
                var title = new TextBlock
                {
                    Text = "Мои продукты",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                
                // Загружаем продукты пользователя
                LoadUserProducts(stackPanel);
            }
            else if (selectedTab == "Управление профилем")
            {
                var title = new TextBlock
                {
                    Text = "Управление профилем",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                
                // Форма редактирования профиля
                LoadProfileEditForm(stackPanel);
            }
            else if (selectedTab == "Авторизация")
            {
                var title = new TextBlock
                {
                    Text = "Управление аккаунтом",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                
                // Информация о текущем пользователе
                var userInfoPanel = new StackPanel
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Margin = new Thickness(20, 20, 20, 30)
                };
                
                var userInfoText = new TextBlock
                {
                    Text = "Вы авторизованы как:",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(20, 20, 20, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                userInfoPanel.Children.Add(userInfoText);
                
                var userNameText = new TextBlock
                {
                    Text = _currentUser?.Name ?? "Неизвестный пользователь",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(20, 0, 20, 20),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                userInfoPanel.Children.Add(userNameText);
                
                stackPanel.Children.Add(userInfoPanel);
                
                // Кнопка "Перейти на сайт"
                var websiteButton = new Button
                {
                    Content = "🌐 Перейти на сайт",
                    Height = 50,
                    Width = 200,
                    Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                websiteButton.Click += (s, e) => 
                {
                    try
                    {
                        System.Diagnostics.Process.Start("http://civilx.ru");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии сайта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                stackPanel.Children.Add(websiteButton);
                
                // Кнопка "Выйти из аккаунта"
                var logoutButton = new Button
                {
                    Content = "🚪 Выйти из аккаунта",
                    Height = 50,
                    Width = 200,
                    Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                logoutButton.Click += (s, e) => 
                {
                    var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Logout();
                    }
                };
                stackPanel.Children.Add(logoutButton);
            }
            
            border.Child = stackPanel;
            return border;
        }

        private void UpdateUserProfileRightPanel(string selectedTab)
        {
            // Находим правую панель в MainGrid и заменяем её содержимое
            var grid = MainGrid.Children[0] as Grid;
            if (grid != null && grid.Children.Count >= 3)
            {
                var rightPanel = CreateUserProfileRightPanel(selectedTab);
                Grid.SetColumn(rightPanel, 2);
                grid.Children.RemoveAt(2); // Удаляем старую правую панель
                grid.Children.Add(rightPanel); // Добавляем новую
            }
        }

        private void LoadProjects()
        {
            try
            {
                // Загружаем проекты из файла конфигурации
                var savedProjects = _persistenceService.LoadProjects();
                WriteToLogFile($"Загружено {savedProjects.Count} проектов из файла конфигурации");
                
                // ВСЕГДА сканируем папку "Проекты" для поиска новых/обновленных проектов
                var projectsPath = Path.Combine("C:\\DataViewer", "Проекты");
                WriteToLogFile($"Сканирование папки проектов: {projectsPath}");
                
                _projects = new List<Project>(); // Начинаем с пустого списка
                
                if (Directory.Exists(projectsPath))
                {
                    var projectFolders = Directory.GetDirectories(projectsPath);
                    WriteToLogFile($"Найдено {projectFolders.Length} папок проектов");
                    
                    foreach (var projectFolder in projectFolders)
                    {
                        var projectName = Path.GetFileName(projectFolder);
                        WriteToLogFile($"Сканирование проекта: {projectName} ({projectFolder})");
                        
                        var project = ScanProjectFolder(projectFolder, projectName);
                        if (project != null)
                        {
                            _projects.Add(project);
                            WriteToLogFile($"✓ Проект '{projectName}' загружен: {project.Exports.Count} выгрузок");
                        }
                        else
                        {
                            WriteToLogFile($"❌ Не удалось загрузить проект '{projectName}'");
                        }
                    }
                    
                    // Сохраняем найденные проекты в файл конфигурации
                    if (_projects.Count > 0)
                    {
                        SaveProjects();
                        WriteToLogFile($"Сохранено {_projects.Count} проектов в файл конфигурации");
                    }
                    else
                    {
                        WriteToLogFile("Проекты не найдены в папке");
                    }
                }
                else
                {
                    WriteToLogFile($"❌ Папка проектов не существует: {projectsPath}");
                    // Если папка не существует, используем сохраненные проекты
                    _projects = savedProjects;
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Ошибка при загрузке проектов: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _projects = new List<Project>();
            }
        }

        private void SaveProjects()
        {
            try
            {
                // Сохраняем проекты в файл конфигурации
                _persistenceService.SaveProjects(_projects);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Принудительно обновляет список проектов, сканируя папку C:\DataViewer\Проекты
        /// </summary>
        public void RefreshProjects()
        {
            WriteToLogFile("=== ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ ПРОЕКТОВ ===");
            LoadProjects();
            ShowProjectsScreen(); // Обновляем UI
            WriteToLogFile($"Обновление завершено. Загружено {_projects.Count} проектов");
        }

        /// <summary>
        /// Запускает экспорт с учетом выбранных фильтров категорий
        /// </summary>
        private List<string> _selectedCategories = new List<string>();
        private ExportTab _currentTab;

        /// <summary>
        /// Устанавливает выбранные категории для фильтрации экспорта
        /// </summary>
        public void SetCategoryFilters(List<string> selectedCategories)
        {
            _selectedCategories = selectedCategories ?? new List<string>();
            WriteToLogFile($"MainWindow: SetCategoryFilters called with {_selectedCategories.Count} categories");
        }

        /// <summary>
        /// Получает текущую вкладку
        /// </summary>
        private ExportTab GetCurrentTab()
        {
            return _currentTab;
        }

        /// <summary>
        /// Запускает экспорт с учетом выбранных фильтров
        /// </summary>
        public void StartExport()
        {
            WriteToLogFile($"MainWindow: StartExport called with {_selectedCategories.Count} selected categories");
            
            // Получаем текущую вкладку
            var currentTab = GetCurrentTab();
            if (currentTab == null)
            {
                WriteToLogFile("MainWindow: No current tab selected");
                MessageBox.Show("Выберите вкладку для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Запускаем экспорт с фильтрами
            StartExportWithFilters(currentTab, _selectedCategories);
        }

        public void StartExportWithFilters(ExportTab tab, List<string> selectedCategories)
        {
            WriteToLogFile($"=== ЗАПУСК ЭКСПОРТА С ФИЛЬТРАМИ ===");
            WriteToLogFile($"Вкладка: {tab.Name}");
            WriteToLogFile($"Исходная папка: {tab.ModelsFolder}");
            WriteToLogFile($"Папка результатов: {tab.FolderPath}");
            WriteToLogFile($"Выбрано категорий: {selectedCategories.Count}");
            
            if (selectedCategories.Count > 0)
            {
                WriteToLogFile("Выбранные категории:");
                foreach (var category in selectedCategories.Take(10)) // Показываем первые 10
                {
                    WriteToLogFile($"  • {category}");
                }
                if (selectedCategories.Count > 10)
                {
                    WriteToLogFile($"  ... и еще {selectedCategories.Count - 10} категорий");
                }
            }

            try
            {
                // Проверяем существование исходной папки
                if (!Directory.Exists(tab.ModelsFolder))
                {
                    WriteToLogFile($"❌ Исходная папка не существует: {tab.ModelsFolder}");
                    MessageBox.Show($"Исходная папка не существует:\n{tab.ModelsFolder}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем папку результатов если не существует
                if (!Directory.Exists(tab.FolderPath))
                {
                    Directory.CreateDirectory(tab.FolderPath);
                    WriteToLogFile($"✓ Создана папка результатов: {tab.FolderPath}");
                }

                // Получаем список .rvt файлов из папки "Модели" внутри папки вкладки
                var modelsFolder = Path.Combine(tab.FolderPath, "Модели");
                WriteToLogFile($"Поиск Revit файлов в папке: {modelsFolder}");
                
                if (!Directory.Exists(modelsFolder))
                {
                    WriteToLogFile($"❌ Папка 'Модели' не существует: {modelsFolder}");
                    MessageBox.Show($"Папка 'Модели' не найдена в папке вкладки:\n{modelsFolder}\n\nСоздайте папку 'Модели' и поместите в неё Revit файлы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var rvtFiles = Directory.GetFiles(modelsFolder, "*.rvt", SearchOption.AllDirectories);
                WriteToLogFile($"Найдено {rvtFiles.Length} .rvt файлов для экспорта");

                if (rvtFiles.Length == 0)
                {
                    WriteToLogFile("❌ Не найдено .rvt файлов в исходной папке");
                    MessageBox.Show("В исходной папке не найдено .rvt файлов", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаем окно прогресса
                var progressWindow = new ExportProgressWindow();
                progressWindow.Show();

                // Запускаем экспорт в отдельном потоке
                Task.Run(() => PerformExportWithFilters(rvtFiles.ToList(), modelsFolder, tab.FolderPath, selectedCategories, progressWindow));
            }
            catch (Exception ex)
            {
                WriteToLogFile($"❌ Ошибка при запуске экспорта: {ex.Message}");
                MessageBox.Show($"Ошибка при запуске экспорта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowProjectsScreen()
        {
            // Загружаем проекты перед показом экрана
            LoadProjects();
            
            // Очищаем все элементы
            MainGrid.Children.Clear();
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            // Левая панель - Список проектов
            var leftPanel = CreateProjectsLeftPanel();
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
            
            // Правая панель - Приветствие
            var rightPanel = CreateWelcomeRightPanel();
            Grid.SetColumn(rightPanel, 2);
            grid.Children.Add(rightPanel);
            
            MainGrid.Children.Add(grid);
        }

        private Border CreateProjectsLeftPanel()
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20, 20, 10, 20)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            
            // Заголовок
            var title = new TextBlock
            {
                Text = "Revit Exporter",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            stackPanel.Children.Add(title);
            
            // Подзаголовок
            var subtitle = new TextBlock
            {
                Text = "Выберите проект:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(subtitle);
            
            // Список проектов
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400
            };
            var projectsList = new StackPanel();
            
            foreach (var project in _projects)
            {
                var projectContainer = new Grid();
                projectContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                projectContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                var projectButton = new Button
                {
                    Content = $"📁 {project.Name}",
                    Height = 50,
                    Margin = new Thickness(0, 0, 0, 15),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(15, 0, 0, 0),
                    FontSize = 14
                };
                projectButton.Click += (s, e) => ShowProjectScreen(project.Name);
                Grid.SetColumn(projectButton, 0);
                projectContainer.Children.Add(projectButton);
                
                // Кнопка с тремя точками
                var menuButton = new Button
                {
                    Content = "⋮",
                    Width = 30,
                    Height = 50,
                    Margin = new Thickness(0, 0, 10, 15),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                menuButton.Click += (s, e) => ShowProjectContextMenu(project.Name, menuButton);
                Grid.SetColumn(menuButton, 1);
                projectContainer.Children.Add(menuButton);
                
                projectsList.Children.Add(projectContainer);
            }
            
            scrollViewer.Content = projectsList;
            stackPanel.Children.Add(scrollViewer);
            
            // Кнопка добавления нового проекта
            var addNewButton = new Button
            {
                Content = "➕ Добавить новый проект",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addNewButton.Click += AddProject_Click;
            stackPanel.Children.Add(addNewButton);
            
            // Кнопка добавления существующего проекта
            var addExistingButton = new Button
            {
                Content = "📁 Добавить существующий проект",
                Height = 50,
                Margin = new Thickness(0, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addExistingButton.Click += AddExistingProject_Click;
            stackPanel.Children.Add(addExistingButton);
            
            // Кнопка обновления проектов
            var refreshButton = new Button
            {
                Content = "🔄 Обновить проекты",
                Height = 50,
                Margin = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(23, 162, 184)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            refreshButton.Click += (s, e) => RefreshProjects();
            stackPanel.Children.Add(refreshButton);
            
            border.Child = stackPanel;
            return border;
        }

        private Border CreateWelcomeRightPanel()
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };
            
            var welcomeText = new TextBlock
            {
                Text = "Добро пожаловать в Revit Exporter!",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            stackPanel.Children.Add(welcomeText);
            
            var descriptionText = new TextBlock
            {
                Text = "Выберите проект в левой панели, чтобы начать работу с выгрузками данных из Revit.",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(descriptionText);
            
            border.Child = stackPanel;
            return border;
        }

        private void ShowProjectScreen(string projectName)
        {
            _currentProjectName = projectName;
            // Очищаем все элементы
            MainGrid.Children.Clear();
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            // Левая панель - Список выгрузок
            var leftPanel = CreateExportsLeftPanel(projectName);
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
            
            // Правая панель - Список вкладок и настроек (изначально пустая)
            var rightPanel = CreateExportsRightPanel(projectName);
            Grid.SetColumn(rightPanel, 2);
            grid.Children.Add(rightPanel);
            
            MainGrid.Children.Add(grid);
        }

        private Border CreateExportsLeftPanel(string projectName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20, 20, 10, 20)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            
            // Заголовок с кнопкой назад
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
            
            var backButton = new Button
            {
                Content = "←",
                FontSize = 18,
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 15, 0)
            };
            backButton.Click += (s, e) => ShowProjectsScreen();
            headerPanel.Children.Add(backButton);
            
            var title = new TextBlock
            {
                Text = projectName,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            headerPanel.Children.Add(title);
            stackPanel.Children.Add(headerPanel);
            
            // Подзаголовок
            var subtitle = new TextBlock
            {
                Text = "Все выгрузки:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(subtitle);
            
            // Список выгрузок
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400
            };
            var exportsList = new StackPanel();
            
            // Получаем проект
            var project = _projects.Find(p => p.Name == projectName);
            if (project != null)
            {
                foreach (var export in project.Exports)
                {
                    var exportContainer = new Grid();
                    exportContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    exportContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    
                    var exportButton = new Button
                    {
                        Content = $"📤 {export.Name}",
                        Height = 50,
                        Margin = new Thickness(0, 0, 0, 15),
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(15, 0, 0, 0),
                        FontSize = 14
                    };
                    exportButton.Click += (s, e) => ShowExportInRightPanel(projectName, export.Name);
                    Grid.SetColumn(exportButton, 0);
                    exportContainer.Children.Add(exportButton);
                    
                    // Кнопка с тремя точками для выгрузки
                    var exportMenuButton = new Button
                    {
                        Content = "⋮",
                        Width = 30,
                        Height = 50,
                        Margin = new Thickness(0, 0, 10, 15),
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    exportMenuButton.Click += (s, e) => ShowExportContextMenu(projectName, export.Name, exportMenuButton);
                    Grid.SetColumn(exportMenuButton, 1);
                    exportContainer.Children.Add(exportMenuButton);
                    
                    exportsList.Children.Add(exportContainer);
                }
            }
            
            scrollViewer.Content = exportsList;
            stackPanel.Children.Add(scrollViewer);
            
            // Кнопка добавления выгрузки
            var addButton = new Button
            {
                Content = "➕ Добавить выгрузку",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addButton.Click += (s, e) => AddExport_Click(projectName);
            stackPanel.Children.Add(addButton);
            
            border.Child = stackPanel;
            return border;
        }

        private Border CreateExportsRightPanel(string projectName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };
            
            var welcomeText = new TextBlock
            {
                Text = $"Проект: {projectName}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            stackPanel.Children.Add(welcomeText);
            
            var descriptionText = new TextBlock
            {
                Text = "Выберите выгрузку в левой панели, чтобы начать работу с вкладками экспорта.",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(descriptionText);
            
            border.Child = stackPanel;
            return border;
        }

        private void ShowExportInRightPanel(string projectName, string exportName)
        {
            _currentExportName = exportName;
            
            
            // Находим правую панель в MainGrid и заменяем её содержимое
            var grid = MainGrid.Children[0] as Grid;
            if (grid != null && grid.Children.Count >= 3)
            {
                // Получаем выгрузку
                var project = _projects.Find(p => p.Name == projectName);
                var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                
                if (export != null)
                {
                    // Показываем первую вкладку, если она есть
                    var firstTabToShow = export.Tabs.FirstOrDefault();
                    if (firstTabToShow != null)
                    {
                        var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, firstTabToShow.Name);
                        Grid.SetColumn(rightPanel, 2);
                        grid.Children.RemoveAt(2); // Удаляем старую правую панель
                        grid.Children.Add(rightPanel); // Добавляем новую
                    }
                    else
                    {
                        // Если вкладок нет, показываем панель для создания вкладок
                        var rightPanel = CreateEmptyExportRightPanel(projectName, exportName);
                        Grid.SetColumn(rightPanel, 2);
                        grid.Children.RemoveAt(2);
                        grid.Children.Add(rightPanel);
                    }
                }
                else
                {
                    MessageBox.Show($"Выгрузка '{exportName}' не найдена в проекте '{projectName}'", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Grid не найден или недостаточно элементов:\ngrid != null: {grid != null}\ngrid.Children.Count: {(grid?.Children.Count ?? 0)}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateEmptyExportRightPanel(string projectName, string exportName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };
            
            var welcomeText = new TextBlock
            {
                Text = $"📤 {exportName}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 20),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            stackPanel.Children.Add(welcomeText);
            
            var descriptionText = new TextBlock
            {
                Text = "У этой выгрузки пока нет вкладок.\nСоздайте первую вкладку, чтобы начать работу.",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 30),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            stackPanel.Children.Add(descriptionText);
            
            // Кнопка создания первой вкладки
            var createTabButton = new Button
            {
                Content = "➕ Создать первую вкладку",
                Height = 50,
                Width = 200,
                Margin = new Thickness(0, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            createTabButton.Click += (s, e) => AddTab_Click(projectName, exportName);
            stackPanel.Children.Add(createTabButton);
            
            border.Child = stackPanel;
            return border;
        }

        private Border CreateExportTabsRightPanel(string projectName, string exportName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            
            // Заголовок с названием выгрузки
            var headerPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 10) };
            var title = new TextBlock
            {
                Text = $"📤 {exportName}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            headerPanel.Children.Add(title);
            
            var subtitle = new TextBlock
            {
                Text = "Вкладки экспорта:",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };
            headerPanel.Children.Add(subtitle);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);
            
            // Список вкладок
            var tabsPanel = new StackPanel { Margin = new Thickness(20, 0, 20, 20) };
            var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var tabsList = new StackPanel();
            
            // Получаем выгрузку
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                foreach (var tab in export.Tabs)
                {
                    var tabButton = new Button
                    {
                        Content = $"📋 {tab.Name}",
                        Height = 50,
                        Margin = new Thickness(0, 0, 0, 10),
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(15, 0, 0, 0),
                        FontSize = 14
                    };
                    tabButton.Click += (s, e) => ShowTabSettingsInRightPanel(projectName, exportName, tab.Name);
                    tabsList.Children.Add(tabButton);
                }
            }
            
            scrollViewer.Content = tabsList;
            tabsPanel.Children.Add(scrollViewer);
            
            // Кнопка добавления вкладки
            var addButton = new Button
            {
                Content = "➕ Добавить вкладку",
                Height = 40,
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addButton.Click += (s, e) => AddTab_Click(projectName, exportName);
            tabsPanel.Children.Add(addButton);
            
            Grid.SetRow(tabsPanel, 1);
            grid.Children.Add(tabsPanel);
            
            border.Child = grid;
            return border;
        }

        private void ShowTabSettingsInRightPanel(string projectName, string exportName, string tabName)
        {
            // Находим правую панель в MainGrid и заменяем её содержимое настройками вкладки
            var grid = MainGrid.Children[0] as Grid;
            if (grid != null && grid.Children.Count >= 3)
            {
                var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, tabName);
                Grid.SetColumn(rightPanel, 2);
                grid.Children.RemoveAt(2); // Удаляем старую правую панель
                grid.Children.Add(rightPanel); // Добавляем новую
            }
        }

        private void ShowExportScreen(string projectName, string exportName)
        {
            _currentExportName = exportName;
            MainGrid.Children.Clear();
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            // Левая панель - Список вкладок
            var leftPanel = CreateTabsLeftPanel(projectName, exportName);
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
            
            // Правая панель - Настройки вкладки
            var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, "Выберите вкладку");
            Grid.SetColumn(rightPanel, 2);
            grid.Children.Add(rightPanel);
            
            MainGrid.Children.Add(grid);
        }

        private Border CreateTabsLeftPanel(string projectName, string exportName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20, 20, 10, 20)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };
            
            // Заголовок с кнопкой назад
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
            
            var backButton = new Button
            {
                Content = "←",
                FontSize = 18,
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 15, 0)
            };
            backButton.Click += (s, e) => ShowProjectScreen(projectName);
            headerPanel.Children.Add(backButton);
            
            var title = new TextBlock
            {
                Text = exportName,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            headerPanel.Children.Add(title);
            stackPanel.Children.Add(headerPanel);
            
            // Подзаголовок
            var subtitle = new TextBlock
            {
                Text = "Вкладки экспорта:",
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
            
            // Получаем выгрузку
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                foreach (var tab in export.Tabs)
                {
                    var tabButton = new Button
                    {
                        Content = $"📋 {tab.Name}",
                        Height = 50,
                        Margin = new Thickness(0, 0, 0, 15),
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(15, 0, 0, 0),
                        FontSize = 14
                    };
                    tabButton.Click += (s, e) => ShowTabSettingsScreen(projectName, exportName, tab.Name);
                    
                    // Добавляем контекстное меню для ПКМ
                    var contextMenu = new ContextMenu();
                    
                    var renameItem = new MenuItem
                    {
                        Header = "Переименовать",
                        Icon = new TextBlock { Text = "✏️", FontSize = 14 }
                    };
                    renameItem.Click += (s, e) => RenameTab(projectName, exportName, tab.Name);
                    contextMenu.Items.Add(renameItem);
                    
                    var deleteItem = new MenuItem
                    {
                        Header = "Удалить",
                        Icon = new TextBlock { Text = "🗑️", FontSize = 14 }
                    };
                    deleteItem.Click += (s, e) => DeleteTab(projectName, exportName, tab.Name);
                    contextMenu.Items.Add(deleteItem);
                    
                    tabButton.ContextMenu = contextMenu;
                    tabsList.Children.Add(tabButton);
                }
            }
            
            scrollViewer.Content = tabsList;
            stackPanel.Children.Add(scrollViewer);
            
            // Кнопка добавления вкладки
            var addButton = new Button
            {
                Content = "➕ Добавить вкладку",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addButton.Click += (s, e) => AddTab_Click(projectName, exportName);
            stackPanel.Children.Add(addButton);
            
            border.Child = stackPanel;
            return border;
        }

        private Border CreateTabSettingsRightPanel(string projectName, string exportName, string tabName)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Заголовок
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Горизонтальные вкладки
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Содержимое вкладки
            
            // Заголовок
            var headerPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 10) };
            var title = new TextBlock
            {
                Text = $"📤 {exportName} - {tabName}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            headerPanel.Children.Add(title);
            Grid.SetRow(headerPanel, 0);
            grid.Children.Add(headerPanel);
            
            // Горизонтальные вкладки
            var tabsPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 0, 20, 10)
            };
            
            // Получаем выгрузку и создаем вкладки
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                foreach (var tab in export.Tabs)
                {
                    var tabButton = new Button
                    {
                        Content = tab.Name,
                        Height = 35,
                        MinWidth = 100,
                        Margin = new Thickness(0, 0, 5, 0),
                        Background = tab.Name == tabName ? 
                            new SolidColorBrush(Color.FromRgb(0, 123, 255)) : 
                            new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Foreground = tab.Name == tabName ? 
                            Brushes.White : 
                            new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        FontSize = 12
                    };
                    tabButton.Click += (s, e) => ShowTabSettingsInRightPanel(projectName, exportName, tab.Name);
                    
                    // Добавляем контекстное меню для ПКМ
                    var contextMenu = new ContextMenu();
                    
                    var renameItem = new MenuItem
                    {
                        Header = "Переименовать",
                        Icon = new TextBlock { Text = "✏️", FontSize = 14 }
                    };
                    renameItem.Click += (s, e) => RenameTab(projectName, exportName, tab.Name);
                    contextMenu.Items.Add(renameItem);
                    
                    var deleteItem = new MenuItem
                    {
                        Header = "Удалить",
                        Icon = new TextBlock { Text = "🗑️", FontSize = 14 }
                    };
                    deleteItem.Click += (s, e) => DeleteTab(projectName, exportName, tab.Name);
                    contextMenu.Items.Add(deleteItem);
                    
                    tabButton.ContextMenu = contextMenu;
                    tabsPanel.Children.Add(tabButton);
                }
                
                // Кнопка добавления новой вкладки
                var addTabButton = new Button
                {
                    Content = "+",
                    Height = 35,
                    Width = 50, // Половина ширины от обычной вкладки (100/2)
                    Margin = new Thickness(0, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)), // Зеленый цвет
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                };
                addTabButton.Click += (s, e) => AddTabFromHorizontalPanel(projectName, exportName);
                tabsPanel.Children.Add(addTabButton);
            }
            
            Grid.SetRow(tabsPanel, 1);
            grid.Children.Add(tabsPanel);
            
            // Содержимое вкладки - перечень параметров
            var contentPanel = new ScrollViewer 
            { 
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(20, 0, 20, 20)
            };
            
            var parametersStack = new StackPanel();
            
            // Заголовок параметров
            var parametersTitle = new TextBlock
            {
                Text = "Параметры для экспорта:",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
            };
            parametersStack.Children.Add(parametersTitle);
            
            // Список параметров
            var parametersList = new StackPanel();
            
            // Новые параметры экспорта
            var sampleParameters = new[]
            {
                "📁 Путь к папке с моделями",
                "📂 Посмотреть результат"
            };
            
            // Создаем параметр "Путь к папке с моделями"
            var modelsPathBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 0, 0, 8)
            };
            
            var modelsPathGrid = new Grid();
            modelsPathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            modelsPathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var modelsPathText = new TextBlock
            {
                Text = "📁 Путь к папке с моделями:",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(modelsPathText, 0);
            modelsPathGrid.Children.Add(modelsPathText);
            
            var browseModelsButton = new Button
            {
                Content = "Обзор",
                Width = 80,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            browseModelsButton.Click += (s, e) => BrowseModelsFolder_Click(projectName, exportName, tabName);
            Grid.SetColumn(browseModelsButton, 1);
            modelsPathGrid.Children.Add(browseModelsButton);
            
            modelsPathBorder.Child = modelsPathGrid;
            parametersList.Children.Add(modelsPathBorder);
            
            // Создаем параметр "Посмотреть результат"
            var viewResultsBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 0, 0, 8)
            };
            
            var viewResultsGrid = new Grid();
            viewResultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            viewResultsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var viewResultsText = new TextBlock
            {
                Text = "📂 Папка результатов:",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(viewResultsText, 0);
            viewResultsGrid.Children.Add(viewResultsText);
            
            var viewResultsButton = new Button
            {
                Content = "Посмотреть результат",
                Width = 150,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            viewResultsButton.Click += (s, e) => ViewResultsFolder_Click(projectName, exportName, tabName);
            Grid.SetColumn(viewResultsButton, 1);
            viewResultsGrid.Children.Add(viewResultsButton);
            
            viewResultsBorder.Child = viewResultsGrid;
            parametersList.Children.Add(viewResultsBorder);
            
            parametersStack.Children.Add(parametersList);
            
            // Кнопка экспорта
            var exportButton = new Button
            {
                Content = "🚀 Начать экспорт",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            exportButton.Click += (s, e) => StartExport_Click(projectName, exportName, tabName);
            parametersStack.Children.Add(exportButton);
            
            // Заменяем программное создание контента на TabContentView
            var currentTab = export?.Tabs.FirstOrDefault(t => t.Name == tabName);
            if (currentTab != null)
            {
                try
                {
                    WriteToLogFile($"MainWindow: Creating TabContentView for tab '{tabName}'");
                    _currentTab = currentTab; // Устанавливаем текущую вкладку
                    var tabContentView = new TabContentView(currentTab, this); // Передаем MainWindow как ExportView
                    WriteToLogFile($"MainWindow: TabContentView created successfully for tab '{tabName}'");
                    
                    Grid.SetRow(tabContentView, 2);
                    grid.Children.Add(tabContentView);
                }
                catch (Exception ex)
                {
                    WriteToLogFile($"MainWindow: Exception creating TabContentView for tab '{tabName}': {ex.Message}");
                    WriteToLogFile($"MainWindow: Stack trace: {ex.StackTrace}");
                    
                    // Fallback к старому контенту в случае ошибки
                    contentPanel.Content = parametersStack;
                    Grid.SetRow(contentPanel, 2);
                    grid.Children.Add(contentPanel);
                }
            }
            else
            {
                // Если вкладка не найдена, показываем сообщение
                var notFoundContent = new TextBlock
                {
                    Text = "Вкладка не найдена",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    Margin = new Thickness(20),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(notFoundContent, 2);
                grid.Children.Add(notFoundContent);
            }
            
            border.Child = grid;
            return border;
        }

        private void ShowTabSettingsScreen(string projectName, string exportName, string tabName)
        {
            // Здесь будет экран настроек конкретной вкладки
            MessageBox.Show($"Настройки вкладки: {tabName}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var createProjectWindow = new AddProjectWindow();
            if (createProjectWindow.ShowDialog() == true)
            {
                try
                {
                    var projectName = createProjectWindow.ProjectName;
                    var projectPath = Path.Combine("C:\\DataViewer", "Проекты", projectName);
                    
                    // Создаем папку проекта
                    if (!Directory.Exists(projectPath))
                    {
                        Directory.CreateDirectory(projectPath);
                    }
                    
                    var newProject = new Project
                    {
                        Name = projectName,
                        FolderPath = projectPath
                    };

                    _projects.Add(newProject);
                    SaveProjects();
                    ShowProjectsScreen();
                    
                    MessageBox.Show($"Проект '{projectName}' успешно создан в папке:\n{projectPath}", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании проекта: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddExistingProject_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку существующего проекта";
                dialog.ShowNewFolderButton = false;
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var projectPath = dialog.SelectedPath;
                        var projectName = Path.GetFileName(projectPath);
                        
                        // Проверяем, не добавлен ли уже этот проект
                        if (_projects.Any(p => p.FolderPath.Equals(projectPath, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("Этот проект уже добавлен в список", "Предупреждение", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        
                        // Сканируем структуру папки и создаем проект
                        var project = ScanProjectFolder(projectPath, projectName);
                        
                        if (project != null)
                        {
                            _projects.Add(project);
                            SaveProjects();
                            ShowProjectsScreen();
                            
                            MessageBox.Show($"Проект '{projectName}' успешно добавлен!\n\n" +
                                          $"Найдено выгрузок: {project.Exports.Count}\n" +
                                          $"Найдено вкладок: {project.Exports.Sum(exp => exp.Tabs.Count)}", 
                                          "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении существующего проекта: {ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private Project ScanProjectFolder(string projectPath, string projectName)
        {
            try
            {
                var project = new Project
                {
                    Name = projectName,
                    FolderPath = projectPath
                };

                WriteToLogFile($"Сканирование папки проекта: {projectPath}");

                // Сканируем папки выгрузок
                if (Directory.Exists(projectPath))
                {
                    var exportFolders = Directory.GetDirectories(projectPath);
                    WriteToLogFile($"Найдено {exportFolders.Length} папок выгрузок в проекте '{projectName}'");
                    
                    foreach (var exportFolder in exportFolders)
                    {
                        var exportName = Path.GetFileName(exportFolder);
                        WriteToLogFile($"  Сканирование выгрузки: {exportName} ({exportFolder})");
                        
                        var export = new Export
                        {
                            Name = exportName,
                            FolderPath = exportFolder
                        };

                        // Сканируем папки вкладок внутри выгрузки
                        var tabFolders = Directory.GetDirectories(exportFolder);
                        WriteToLogFile($"    Найдено {tabFolders.Length} папок вкладок в выгрузке '{exportName}'");
                        
                        foreach (var tabFolder in tabFolders)
                        {
                            var tabName = Path.GetFileName(tabFolder);
                            WriteToLogFile($"      Сканирование вкладки: {tabName} ({tabFolder})");
                            
                            var tab = new ExportTab
                            {
                                Name = tabName,
                                FolderPath = tabFolder,
                                RevitVersion = "2023" // По умолчанию
                            };

                            // Проверяем наличие папки "Модели" и автоматически устанавливаем путь
                            var modelsPath = Path.Combine(tabFolder, "Модели");
                            if (Directory.Exists(modelsPath))
                            {
                                tab.ModelsFolder = modelsPath;
                                WriteToLogFile($"      ✓ Найдена папка 'Модели' в вкладке '{tabName}': {modelsPath}");
                                
                                // Подсчитываем количество .rvt файлов
                                var rvtFiles = Directory.GetFiles(modelsPath, "*.rvt", SearchOption.AllDirectories);
                                WriteToLogFile($"      ✓ Найдено {rvtFiles.Length} .rvt файлов в папке 'Модели'");
                            }
                            else
                            {
                                WriteToLogFile($"      ⚠️ Папка 'Модели' не найдена в вкладке '{tabName}'");
                            }

                            export.Tabs.Add(tab);
                        }

                        project.Exports.Add(export);
                        WriteToLogFile($"    ✓ Выгрузка '{exportName}' добавлена: {export.Tabs.Count} вкладок");
                    }
                }
                else
                {
                    WriteToLogFile($"❌ Папка проекта не существует: {projectPath}");
                }

                WriteToLogFile($"✓ Проект '{projectName}' отсканирован: {project.Exports.Count} выгрузок");
                return project;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Ошибка при сканировании папки проекта '{projectName}': {ex.Message}");
                MessageBox.Show($"Ошибка при сканировании папки проекта: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void AddExport_Click(string projectName)
        {
            var addExportWindow = new AddExportWindow();
            if (addExportWindow.ShowDialog() == true)
            {
                var project = _projects.Find(p => p.Name == projectName);
                if (project != null)
                {
                    try
                    {
                        var exportName = addExportWindow.ExportName;
                        var exportFolderPath = Path.Combine(project.FolderPath, exportName);
                        
                        // Создаем папку выгрузки
                        Directory.CreateDirectory(exportFolderPath);
                        
                        var newExport = new Export
                        {
                            Name = exportName,
                            FolderPath = exportFolderPath
                        };

                        project.Exports.Add(newExport);
                        SaveProjects();
                        ShowProjectScreen(projectName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании папки выгрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddTab_Click(string projectName, string exportName)
        {
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                try
                {
                    var tabNumber = export.Tabs.Count + 1;
                    var tabName = $"Вкладка {tabNumber}";
                    var tabFolderPath = Path.Combine(export.FolderPath, tabName);
                    
                    // Создаем папку вкладки
                    Directory.CreateDirectory(tabFolderPath);
                    
                    var newTab = new ExportTab
                    {
                        Name = tabName,
                        FolderPath = tabFolderPath,
                        RevitVersion = "2023"
                    };

                    export.Tabs.Add(newTab);
                    SaveProjects();
                    
                    // Сразу показываем новую вкладку с горизонтальными вкладками
                    var grid = MainGrid.Children[0] as Grid;
                    if (grid != null && grid.Children.Count >= 3)
                    {
                        var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, newTab.Name);
                        Grid.SetColumn(rightPanel, 2);
                        grid.Children.RemoveAt(2);
                        grid.Children.Add(rightPanel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании папки вкладки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void GoBackToProjects()
        {
            ShowProjectsScreen();
        }

        private void AddTabFromHorizontalPanel(string projectName, string exportName)
        {
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                try
                {
                    var tabNumber = export.Tabs.Count + 1;
                    var tabName = $"Вкладка {tabNumber}";
                    var tabFolderPath = Path.Combine(export.FolderPath, tabName);
                    
                    // Создаем папку вкладки
                    Directory.CreateDirectory(tabFolderPath);
                    
                    var newTab = new ExportTab
                    {
                        Name = tabName,
                        FolderPath = tabFolderPath,
                        RevitVersion = "2023"
                    };

                    export.Tabs.Add(newTab);
                    SaveProjects();
                    
                    // Сразу показываем новую вкладку с горизонтальными вкладками
                    var grid = MainGrid.Children[0] as Grid;
                    if (grid != null && grid.Children.Count >= 3)
                    {
                        var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, newTab.Name);
                        Grid.SetColumn(rightPanel, 2);
                        grid.Children.RemoveAt(2);
                        grid.Children.Add(rightPanel);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании папки вкладки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowProjectContextMenu(string projectName, Button menuButton)
        {
            var contextMenu = new ContextMenu();
            
            var renameItem = new MenuItem
            {
                Header = "Переименовать",
                Icon = new TextBlock { Text = "✏️", FontSize = 14 }
            };
            renameItem.Click += (s, e) => RenameProject(projectName);
            contextMenu.Items.Add(renameItem);
            
            var deleteItem = new MenuItem
            {
                Header = "Удалить",
                Icon = new TextBlock { Text = "🗑️", FontSize = 14 }
            };
            deleteItem.Click += (s, e) => DeleteProject(projectName);
            contextMenu.Items.Add(deleteItem);
            
            contextMenu.PlacementTarget = menuButton;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private void ShowExportContextMenu(string projectName, string exportName, Button menuButton)
        {
            var contextMenu = new ContextMenu();
            
            var renameItem = new MenuItem
            {
                Header = "Переименовать",
                Icon = new TextBlock { Text = "✏️", FontSize = 14 }
            };
            renameItem.Click += (s, e) => RenameExport(projectName, exportName);
            contextMenu.Items.Add(renameItem);
            
            var deleteItem = new MenuItem
            {
                Header = "Удалить",
                Icon = new TextBlock { Text = "🗑️", FontSize = 14 }
            };
            deleteItem.Click += (s, e) => DeleteExport(projectName, exportName);
            contextMenu.Items.Add(deleteItem);
            
            contextMenu.PlacementTarget = menuButton;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private void RenameProject(string projectName)
        {
            var renameWindow = new RenameWindow(projectName);
            if (renameWindow.ShowDialog() == true)
            {
                var project = _projects.Find(p => p.Name == projectName);
                if (project != null && !string.IsNullOrWhiteSpace(renameWindow.NewName))
                {
                    try
                    {
                        var oldPath = project.FolderPath;
                        var newPath = Path.Combine(Path.GetDirectoryName(oldPath), renameWindow.NewName);
                        
                        // Переименовываем папку
                        Directory.Move(oldPath, newPath);
                        
                        // Обновляем данные проекта
                        project.Name = renameWindow.NewName;
                        project.FolderPath = newPath;
                        
                        SaveProjects();
                        ShowProjectsScreen();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переименовании папки проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteProject(string projectName)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить проект '{projectName}'?\n\nЭто действие нельзя отменить и удалит всю папку проекта.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var project = _projects.Find(p => p.Name == projectName);
                    if (project != null)
                    {
                        // Удаляем папку проекта
                        if (Directory.Exists(project.FolderPath))
                        {
                            Directory.Delete(project.FolderPath, true);
                        }
                        
                        // Удаляем из списка проектов
                        _projects.RemoveAll(p => p.Name == projectName);
                        SaveProjects();
                        ShowProjectsScreen();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении папки проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenameExport(string projectName, string exportName)
        {
            var renameWindow = new RenameWindow(exportName);
            if (renameWindow.ShowDialog() == true)
            {
                var project = _projects.Find(p => p.Name == projectName);
                var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                if (export != null && !string.IsNullOrWhiteSpace(renameWindow.NewName))
                {
                    try
                    {
                        var oldPath = export.FolderPath;
                        var newPath = Path.Combine(Path.GetDirectoryName(oldPath), renameWindow.NewName);
                        
                        // Переименовываем папку выгрузки
                        Directory.Move(oldPath, newPath);
                        
                        // Обновляем данные выгрузки
                        export.Name = renameWindow.NewName;
                        export.FolderPath = newPath;
                        
                        // Обновляем пути всех вкладок
                        foreach (var tab in export.Tabs)
                        {
                            var oldTabPath = tab.FolderPath;
                            var newTabPath = Path.Combine(newPath, Path.GetFileName(oldTabPath));
                            tab.FolderPath = newTabPath;
                        }
                        
                        SaveProjects();
                        ShowProjectScreen(projectName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переименовании папки выгрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteExport(string projectName, string exportName)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить выгрузку '{exportName}'?\n\nЭто действие нельзя отменить и удалит всю папку выгрузки.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var project = _projects.Find(p => p.Name == projectName);
                    var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                    if (export != null)
                    {
                        // Удаляем папку выгрузки
                        if (Directory.Exists(export.FolderPath))
                        {
                            Directory.Delete(export.FolderPath, true);
                        }
                        
                        // Удаляем из списка выгрузок
                        project.Exports.Remove(export);
                        SaveProjects();
                        ShowProjectScreen(projectName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении папки выгрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenameTab(string projectName, string exportName, string tabName)
        {
            var renameWindow = new RenameWindow(tabName);
            if (renameWindow.ShowDialog() == true)
            {
                var project = _projects.Find(p => p.Name == projectName);
                var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                var tab = export?.Tabs.FirstOrDefault(t => t.Name == tabName);
                
                if (tab != null && !string.IsNullOrWhiteSpace(renameWindow.NewName))
                {
                    try
                    {
                        var oldPath = tab.FolderPath;
                        var newPath = Path.Combine(Path.GetDirectoryName(oldPath), renameWindow.NewName);
                        
                        // Переименовываем папку вкладки
                        Directory.Move(oldPath, newPath);
                        
                        // Обновляем данные вкладки
                        tab.Name = renameWindow.NewName;
                        tab.FolderPath = newPath;
                        
                        SaveProjects();
                        
                        // Обновляем интерфейс
                        var grid = MainGrid.Children[0] as Grid;
                        if (grid != null && grid.Children.Count >= 3)
                        {
                            var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, tab.Name);
                            Grid.SetColumn(rightPanel, 2);
                            grid.Children.RemoveAt(2);
                            grid.Children.Add(rightPanel);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при переименовании папки вкладки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteTab(string projectName, string exportName, string tabName)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить вкладку '{tabName}'?\n\nЭто действие нельзя отменить и удалит папку вкладки.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var project = _projects.Find(p => p.Name == projectName);
                    var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                    
                    if (export != null)
                    {
                        var tabToRemove = export.Tabs.FirstOrDefault(t => t.Name == tabName);
                        if (tabToRemove != null)
                        {
                            // Удаляем папку вкладки
                            if (Directory.Exists(tabToRemove.FolderPath))
                            {
                                Directory.Delete(tabToRemove.FolderPath, true);
                            }
                            
                            export.Tabs.Remove(tabToRemove);
                        }
                        SaveProjects();
                        
                        // Если удалили последнюю вкладку, показываем экран выгрузки
                        if (export.Tabs.Count == 0)
                        {
                            ShowProjectScreen(projectName);
                        }
                        else
                        {
                            // Показываем первую оставшуюся вкладку
                            var firstTab = export.Tabs.FirstOrDefault();
                            if (firstTab != null)
                            {
                                var grid = MainGrid.Children[0] as Grid;
                                if (grid != null && grid.Children.Count >= 3)
                                {
                                    var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, firstTab.Name);
                                    Grid.SetColumn(rightPanel, 2);
                                    grid.Children.RemoveAt(2);
                                    grid.Children.Add(rightPanel);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении папки вкладки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BrowseModelsFolder_Click(string projectName, string exportName, string tabName)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку с моделями Revit",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // Получаем путь к папке вкладки
                    var projectPath = Path.Combine("C:\\DataViewer", "Проекты", projectName);
                    var exportPath = Path.Combine(projectPath, exportName);
                    var tabPath = Path.Combine(exportPath, tabName);
                    var modelsPath = Path.Combine(tabPath, "Модели");


                    // Удаляем существующую папку "Модели", если она есть
                    if (Directory.Exists(modelsPath))
                    {
                        Directory.Delete(modelsPath, true);
                    }

                    // Создаем новую папку "Модели"
                    Directory.CreateDirectory(modelsPath);

                    // Копируем все .rvt файлы с сохранением иерархии и подсчетом
                    int copiedFilesCount = CopyRvtFilesRecursively(dialog.SelectedPath, modelsPath);

                    // Проверяем, что папка действительно создалась
                    if (Directory.Exists(modelsPath))
                    {
                        MessageBox.Show($"Модели успешно скопированы в папку:\n{modelsPath}\n\nСкопировано .rvt файлов: {copiedFilesCount}", 
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: Папка не была создана!\nОжидаемый путь: {modelsPath}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании моделей:\n{ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewResultsFolder_Click(string projectName, string exportName, string tabName)
        {
            try
            {
                // Получаем путь к папке вкладки
                var projectPath = Path.Combine("C:\\DataViewer", "Проекты", projectName);
                var exportPath = Path.Combine(projectPath, exportName);
                var tabPath = Path.Combine(exportPath, tabName);

                // Создаем папку, если она не существует
                if (!Directory.Exists(tabPath))
                {
                    Directory.CreateDirectory(tabPath);
                }

                // Открываем папку в проводнике Windows
                System.Diagnostics.Process.Start("explorer.exe", tabPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии папки результатов:\n{ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CopyRvtFilesRecursively(string sourceDir, string targetDir)
        {
            int copiedFilesCount = 0;
            
            // Создаем папку назначения, если она не существует
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Копируем все .rvt файлы из текущей папки
            var rvtFiles = Directory.GetFiles(sourceDir, "*.rvt");
            foreach (var file in rvtFiles)
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
                copiedFilesCount++;
            }

            // Рекурсивно обрабатываем подпапки
            var subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                var dirName = Path.GetFileName(subDir);
                var targetSubDir = Path.Combine(targetDir, dirName);
                copiedFilesCount += CopyRvtFilesRecursively(subDir, targetSubDir);
            }

            return copiedFilesCount;
        }

        private void StartExport_Click(string projectName, string exportName, string tabName)
        {
            try
            {
                // Проверяем входные параметры
                if (string.IsNullOrEmpty(projectName))
                {
                    MessageBox.Show("Имя проекта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(exportName))
                {
                    MessageBox.Show("Имя экспорта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(tabName))
                {
                    MessageBox.Show("Имя вкладки не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получаем путь к папке "Модели"
                var projectPath = Path.Combine("C:\\DataViewer", "Проекты", projectName);
                var exportPath = Path.Combine(projectPath, exportName);
                var tabPath = Path.Combine(exportPath, tabName);
                var modelsPath = Path.Combine(tabPath, "Модели");

                // Проверяем существование папки "Модели"
                if (!Directory.Exists(modelsPath))
                {
                    MessageBox.Show("Папка 'Модели' не найдена. Сначала выберите папку с моделями.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем все .rvt файлы в папке "Модели" (включая подпапки)
                var rvtFiles = GetAllRvtFiles(modelsPath);
                
                if (rvtFiles.Count == 0)
                {
                    MessageBox.Show("В папке 'Модели' не найдено .rvt файлов.", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаем и показываем окно прогресса
                var progressWindow = new ExportProgressWindow();
                if (progressWindow == null)
                {
                    MessageBox.Show("Не удалось создать окно прогресса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                progressWindow.SetTotalModels(rvtFiles.Count);
                progressWindow.Show();

                // Создаем папку для результатов экспорта
                var resultsPath = tabPath; // Результаты сохраняем в папку вкладки
                if (!Directory.Exists(resultsPath))
                {
                    Directory.CreateDirectory(resultsPath);
                }

                // Запускаем экспорт в главном потоке с обновлением UI
                try
                {
                    PerformExportWithUIUpdates(rvtFiles, modelsPath, resultsPath, progressWindow);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    System.Diagnostics.Debug.WriteLine($"Ошибка в PerformExport: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    
                    // Показываем ошибку
                    progressWindow.Close();
                    MessageBox.Show($"Критическая ошибка при экспорте:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
                // Экспорт завершен - таймер уже остановлен в PerformExportWithUIUpdates
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при экспорте:\n{ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformExportWithFilters(List<string> rvtFiles, string modelsPath, string resultsPath, List<string> selectedCategories, ExportProgressWindow progressWindow)
        {
            // Засекаем время начала экспорта
            var startTime = DateTime.Now;
            
            try
            {
                // Создаем путь к детальному лог-файлу в той же папке, где сохраняются CSV
                var logFileName = $"CivilX_Detailed_Export_Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                _currentLogFilePath = Path.Combine(resultsPath, logFileName);
                
                var successCount = 0;
                var errorCount = 0;
                var errorMessages = new List<string>();

                WriteToLogFile($"=== НАЧАЛО ЭКСПОРТА С ФИЛЬТРАМИ ===");
                WriteToLogFile($"Файлов для обработки: {rvtFiles.Count}");
                WriteToLogFile($"Выбрано категорий: {selectedCategories.Count}");
                
                // Записываем в детальный лог
                WriteDetailedLog("=== НАЧАЛО ДЕТАЛЬНОГО ЛОГИРОВАНИЯ ЭКСПОРТА ===");
                WriteDetailedLog($"Время начала: {startTime:yyyy-MM-dd HH:mm:ss}");
                WriteDetailedLog($"Папка результатов: {resultsPath}");
                WriteDetailedLog($"Папка с моделями: {modelsPath}");
                WriteDetailedLog($"Количество файлов для обработки: {rvtFiles.Count}");
                WriteDetailedLog($"Выбрано категорий для фильтрации: {selectedCategories.Count} из общего количества доступных категорий");

                // Обрабатываем каждый .rvt файл
                for (int i = 0; i < rvtFiles.Count; i++)
                {
                    // Проверяем, не была ли отмена
                    if (progressWindow.IsCancelled)
                    {
                        break;
                    }

                    var rvtFile = rvtFiles[i];
                    var fileName = Path.GetFileName(rvtFile);

                    try
                    {
                        // Получаем относительный путь для имени выходного файла
                        var relativePath = GetRelativePath(modelsPath, rvtFile);
                        var outputFileName = relativePath.Replace("\\", "_").Replace("/", "_").Replace(".rvt", ".csv");
                        var outputFile = Path.Combine(resultsPath, outputFileName);

                        // Обновляем прогресс - начало обработки
                        UpdateProgressWithUI(progressWindow, i, rvtFiles.Count, fileName, 10);

                        // Экспортируем данные из .rvt файла с фильтрами
                        ExportRvtFileDataWithFilters(rvtFile, outputFile, selectedCategories, progressWindow, i, rvtFiles.Count);

                        // Обновляем прогресс - завершение обработки
                        UpdateProgressWithUI(progressWindow, i + 1, rvtFiles.Count, fileName, 100);

                        successCount++;
                    }
                catch (Exception ex)
                {
                    errorCount++;
                    var errorMessage = $"Ошибка при обработке файла {fileName}: {ex.Message}";
                    errorMessages.Add(errorMessage);
                    WriteToLogFile($"❌ {errorMessage}");
                    
                    // Если это InternalException, добавляем дополнительную информацию
                    if (ex.Message.Contains("InternalException"))
                    {
                        WriteToLogFile($"⚠️ ВНИМАНИЕ: Файл {fileName} может быть поврежден или создан в несовместимой версии Revit");
                        WriteToLogFile($"💡 Рекомендация: Попробуйте открыть файл вручную в Revit для проверки");
                    }
                }
                }

                // Засекаем время завершения
                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                // Детальное логирование завершения экспорта
                WriteDetailedLog($"\n=== ЗАВЕРШЕНИЕ ЭКСПОРТА ===");
                WriteDetailedLog($"Время завершения: {endTime:yyyy-MM-dd HH:mm:ss.fff}");
                WriteDetailedLog($"Общее время выполнения: {duration.TotalMinutes:F2} минут ({duration.TotalSeconds:F2} секунд)");
                WriteDetailedLog($"📊 ИТОГОВАЯ СТАТИСТИКА ЭКСПОРТА:");
                WriteDetailedLog($"  • Успешно обработано файлов: {successCount}");
                WriteDetailedLog($"  • Файлов с ошибками: {errorCount}");
                WriteDetailedLog($"  • Общее количество файлов: {rvtFiles.Count}");
                WriteDetailedLog($"  • Папка результатов: {resultsPath}");
                WriteDetailedLog($"  • Детальный лог сохранен: {_currentLogFilePath}");
                WriteDetailedLog("=== КОНЕЦ ДЕТАЛЬНОГО ЛОГИРОВАНИЯ ===");

                // Экспорт завершен - всегда останавливаем таймер
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - ВЫЗОВ progressWindow.CompleteExport()");
                WriteDetailedLog("🔍 MainWindow - ВЫЗОВ progressWindow.CompleteExport()");
                try
                {
                    WriteDetailedLog("🔍 MainWindow - ВХОДИМ В TRY БЛОК CompleteExport");
                    progressWindow.CompleteExport();
                    WriteDetailedLog("🔍 MainWindow - CompleteExport ВЫПОЛНЕН БЕЗ ОШИБОК");
                }
                catch (Exception ex)
                {
                    WriteDetailedLog($"❌ MainWindow - ОШИБКА В CompleteExport: {ex.Message}");
                    WriteDetailedLog($"❌ MainWindow - StackTrace: {ex.StackTrace}");
                    WriteToLogFile($"❌ Ошибка в CompleteExport: {ex.Message}");
                    WriteToLogFile($"❌ StackTrace: {ex.StackTrace}");
                }
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - progressWindow.CompleteExport() ВЫПОЛНЕН");
                WriteDetailedLog("🔍 MainWindow - progressWindow.CompleteExport() ВЫПОЛНЕН");

                // Показываем результат
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - формирование сообщения о результате");
                WriteDetailedLog("🔍 MainWindow - формирование сообщения о результате");
                var resultMessage = $"Экспорт завершен!\n\n" +
                                  $"✅ Успешно обработано: {successCount} файлов\n" +
                                  $"❌ Ошибок: {errorCount} файлов\n" +
                                  $"⏱️ Время выполнения: {duration.TotalMinutes:F1} минут\n" +
                                  $"📁 Результаты сохранены в: {resultsPath}";

                if (errorMessages.Count > 0)
                {
                    resultMessage += $"\n\nОшибки:\n{string.Join("\n", errorMessages.Take(5))}";
                    if (errorMessages.Count > 5)
                    {
                        resultMessage += $"\n... и еще {errorMessages.Count - 5} ошибок";
                    }
                }

                WriteToLogFile($"=== ЭКСПОРТ ЗАВЕРШЕН ===");
                WriteToLogFile($"Успешно: {successCount}, Ошибок: {errorCount}, Время: {duration.TotalMinutes:F1} мин");

                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - ПОКАЗ MessageBox.Show");
                WriteDetailedLog("🔍 MainWindow - ПОКАЗ MessageBox.Show");
                MessageBox.Show(resultMessage, "Экспорт завершен", MessageBoxButton.OK, 
                    errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - MessageBox.Show ЗАВЕРШЕН");
                WriteDetailedLog("🔍 MainWindow - MessageBox.Show ЗАВЕРШЕН");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MainWindow - КРИТИЧЕСКАЯ ОШИБКА в блоке try: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                WriteDetailedLog($"❌ MainWindow - КРИТИЧЕСКАЯ ОШИБКА в блоке try: {ex.Message}");
                WriteDetailedLog($"❌ StackTrace: {ex.StackTrace}");
                WriteToLogFile($"❌ Критическая ошибка экспорта: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - ВЫЗОВ progressWindow.CompleteExport() в catch");
                WriteDetailedLog("🔍 MainWindow - ВЫЗОВ progressWindow.CompleteExport() в catch");
                progressWindow.CompleteExport();
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - progressWindow.CompleteExport() в catch ВЫПОЛНЕН");
                WriteDetailedLog("🔍 MainWindow - progressWindow.CompleteExport() в catch ВЫПОЛНЕН");
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - ПОКАЗ MessageBox.Show с ошибкой");
                WriteDetailedLog("🔍 MainWindow - ПОКАЗ MessageBox.Show с ошибкой");
                MessageBox.Show($"Критическая ошибка экспорта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow - MessageBox.Show с ошибкой ЗАВЕРШЕН");
                WriteDetailedLog("🔍 MainWindow - MessageBox.Show с ошибкой ЗАВЕРШЕН");
            }
        }

        private void PerformExportWithUIUpdates(List<string> rvtFiles, string modelsPath, string resultsPath, ExportProgressWindow progressWindow)
        {
            // Засекаем время начала экспорта
            var startTime = DateTime.Now;
            
            try
            {
                var successCount = 0;
                var errorCount = 0;
                var errorMessages = new List<string>();

                // Обрабатываем каждый .rvt файл
                for (int i = 0; i < rvtFiles.Count; i++)
                {
                    // Проверяем, не была ли отмена
                    if (progressWindow.IsCancelled)
                    {
                        break;
                    }

                    var rvtFile = rvtFiles[i];
                    var fileName = Path.GetFileName(rvtFile);

                    try
                    {
                        // Получаем относительный путь для имени выходного файла
                        var relativePath = GetRelativePath(modelsPath, rvtFile);
                        var outputFileName = relativePath.Replace("\\", "_").Replace("/", "_").Replace(".rvt", ".csv");
                        var outputFile = Path.Combine(resultsPath, outputFileName);

                        // Обновляем прогресс - начало обработки
                        UpdateProgressWithUI(progressWindow, i, rvtFiles.Count, fileName, 10);

                        // Экспортируем данные из .rvt файла
                        ExportRvtFileData(rvtFile, outputFile, progressWindow, i, rvtFiles.Count);

                        // Обновляем прогресс - завершение обработки
                        UpdateProgressWithUI(progressWindow, i + 1, rvtFiles.Count, fileName, 100);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errorMessages.Add($"Ошибка в файле {fileName}: {ex.Message}");
                        UpdateProgressWithUI(progressWindow, i + 1, rvtFiles.Count, fileName, 100);
                    }
                }

                // Если экспорт не был отменен, показываем результат
                if (!progressWindow.IsCancelled)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - экспорт НЕ отменен, показываем результат");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - экспорт НЕ отменен, показываем результат");
                    // СНАЧАЛА останавливаем таймер
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - ВЫЗОВ progressWindow.CompleteExport()");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - ВЫЗОВ progressWindow.CompleteExport()");
                    progressWindow.CompleteExport();
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - progressWindow.CompleteExport() ВЫПОЛНЕН");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - progressWindow.CompleteExport() ВЫПОЛНЕН");
                    
                    var resultMessage = $"Экспорт завершен!\n\nУспешно обработано: {successCount} файлов";
                    if (errorCount > 0)
                    {
                        resultMessage += $"\nОшибок: {errorCount}";
                        resultMessage += $"\n\nДетали ошибок:\n{string.Join("\n", errorMessages.Take(5))}";
                        if (errorMessages.Count > 5)
                        {
                            resultMessage += $"\n... и еще {errorMessages.Count - 5} ошибок";
                        }
                    }

                    // Показываем результат (мы уже в главном потоке)
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - ПОКАЗ MessageBox.Show");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - ПОКАЗ MessageBox.Show");
                    MessageBox.Show(resultMessage, "Результат экспорта", MessageBoxButton.OK, 
                        errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - MessageBox.Show ЗАВЕРШЕН");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - MessageBox.Show ЗАВЕРШЕН");

                    // Открываем папку с результатами
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - ОТКРЫТИЕ папки с результатами");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - ОТКРЫТИЕ папки с результатами");
                    System.Diagnostics.Process.Start("explorer.exe", resultsPath);
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - папка с результатами ОТКРЫТА");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - папка с результатами ОТКРЫТА");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🔍 PerformExportWithUIUpdates - экспорт ОТМЕНЕН");
                    WriteDetailedLog("🔍 PerformExportWithUIUpdates - экспорт ОТМЕНЕН");
                }
            }
            catch (Exception ex)
            {
                // Логируем критическую ошибку
                System.Diagnostics.Debug.WriteLine($"Критическая ошибка в PerformExport: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Останавливаем таймер при ошибке
                progressWindow.CompleteExport();
                
                // Показываем ошибку (мы уже в главном потоке)
                MessageBox.Show($"Критическая ошибка при выполнении экспорта:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", 
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                throw; // Перебрасываем исключение для обработки в вызывающем методе
            }
        }

        private void UpdateProgressWithUI(ExportProgressWindow progressWindow, int processedModels, int totalModels, string currentModel, double currentModelProgress)
        {
            // Обновляем прогресс
            progressWindow.UpdateProgress(processedModels, totalModels, currentModel, currentModelProgress);
            
            // Принудительно обновляем UI
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Принудительно обновляем UI
            System.Windows.Forms.Application.DoEvents();
            
            // Также используем Dispatcher для WPF
            Dispatcher.BeginInvoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{duration.Days}д {duration.Hours}ч {duration.Minutes}м {duration.Seconds}с";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours}ч {duration.Minutes}м {duration.Seconds}с";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes}м {duration.Seconds}с";
            }
            else
            {
                return $"{duration.TotalSeconds:F1}с";
            }
        }

        private void PerformExport(List<string> rvtFiles, string modelsPath, string resultsPath, ExportProgressWindow progressWindow)
        {
            int successCount = 0;
            int errorCount = 0;
            var errorMessages = new List<string>();

            try
            {
                // Проверяем входные параметры
                if (rvtFiles == null || rvtFiles.Count == 0)
                {
                    throw new ArgumentException("Список файлов для экспорта пуст");
                }

                if (progressWindow == null)
                {
                    throw new ArgumentNullException(nameof(progressWindow), "Окно прогресса не может быть null");
                }

                if (!Directory.Exists(modelsPath))
                {
                    throw new DirectoryNotFoundException($"Папка с моделями не найдена: {modelsPath}");
                }

                if (!Directory.Exists(resultsPath))
                {
                    throw new DirectoryNotFoundException($"Папка результатов не найдена: {resultsPath}");
                }

                for (int i = 0; i < rvtFiles.Count; i++)
                {
                    // Проверяем, не была ли отменена операция
                    if (progressWindow.IsCancelled)
                    {
                        break;
                    }

                    var rvtFile = rvtFiles[i];
                    var fileName = Path.GetFileName(rvtFile);

                    try
                    {
                        // Обновляем прогресс
                        progressWindow.UpdateProgress(i, rvtFiles.Count, fileName, 0);

                    // Создаем имя файла результата на основе пути к .rvt файлу
                    var relativePath = GetRelativePath(modelsPath, rvtFile);
                    var outputFileName = relativePath.Replace("\\", "_").Replace("/", "_").Replace(".rvt", ".csv");
                    var outputFile = Path.Combine(resultsPath, outputFileName);

                    // Обновляем прогресс - начало обработки
                    progressWindow.UpdateProgress(i, rvtFiles.Count, fileName, 10);

                    // Экспортируем данные из .rvt файла
                    ExportRvtFileData(rvtFile, outputFile, progressWindow, i, rvtFiles.Count);

                    // Обновляем прогресс - завершение обработки
                    progressWindow.UpdateProgress(i + 1, rvtFiles.Count, fileName, 100);

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errorMessages.Add($"Ошибка в файле {fileName}: {ex.Message}");
                    progressWindow.UpdateProgress(i + 1, rvtFiles.Count, fileName, 100);
                }
            }

                // Если экспорт не был отменен, показываем результат
                if (!progressWindow.IsCancelled)
                {
                    var resultMessage = $"Экспорт завершен!\n\nУспешно обработано: {successCount} файлов";
                    if (errorCount > 0)
                    {
                        resultMessage += $"\nОшибок: {errorCount}";
                        resultMessage += $"\n\nДетали ошибок:\n{string.Join("\n", errorMessages.Take(5))}";
                        if (errorMessages.Count > 5)
                        {
                            resultMessage += $"\n... и еще {errorMessages.Count - 5} ошибок";
                        }
                    }

                    // Показываем результат (мы уже в главном потоке)
                    MessageBox.Show(resultMessage, "Результат экспорта", MessageBoxButton.OK, 
                        errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                    // Открываем папку с результатами
                    System.Diagnostics.Process.Start("explorer.exe", resultsPath);
                }
            }
            catch (Exception ex)
            {
                // Логируем критическую ошибку
                System.Diagnostics.Debug.WriteLine($"Критическая ошибка в PerformExport: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Останавливаем таймер при ошибке
                progressWindow.CompleteExport();
                
                // Показываем ошибку (мы уже в главном потоке)
                MessageBox.Show($"Критическая ошибка при выполнении экспорта:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", 
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                
                throw; // Перебрасываем исключение для обработки в вызывающем методе
            }
        }

        private List<string> GetAllRvtFiles(string directory)
        {
            var rvtFiles = new List<string>();
            
            // Получаем все .rvt файлы в текущей папке
            rvtFiles.AddRange(Directory.GetFiles(directory, "*.rvt"));
            
            // Рекурсивно получаем .rvt файлы из подпапок
            var subDirs = Directory.GetDirectories(directory);
            foreach (var subDir in subDirs)
            {
                rvtFiles.AddRange(GetAllRvtFiles(subDir));
            }
            
            return rvtFiles;
        }

        private string GetRelativePath(string basePath, string targetPath)
        {
            var baseUri = new Uri(basePath + "\\");
            var targetUri = new Uri(targetPath);
            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        private void ExportRvtFileData(string rvtFilePath, string outputFilePath, ExportProgressWindow progressWindow, int currentIndex, int totalFiles)
        {
            Document document = null;
            try
            {
                // Проверяем отмену в начале обработки файла
                if (progressWindow.IsCancelled)
                {
                    return;
                }
                
                var fileName = Path.GetFileName(rvtFilePath);
                
                // Детальная диагностика файла
                WriteToLogFile($"🔍 Начинаем экспорт файла: {fileName}");
                WriteToLogFile($"📁 Полный путь: {rvtFilePath}");
                
                // Проверяем существование файла
                if (!File.Exists(rvtFilePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {rvtFilePath}");
                }

                var fileInfo = new FileInfo(rvtFilePath);
                WriteToLogFile($"📊 Размер файла: {fileInfo.Length} байт");
                WriteToLogFile($"📅 Дата создания: {fileInfo.CreationTime}");
                WriteToLogFile($"📅 Последнее изменение: {fileInfo.LastWriteTime}");
                
                // Проверяем, что RevitApp инициализирован
                if (_revitApp == null)
                {
                    throw new Exception("RevitApp не инициализирован. Убедитесь, что плагин запущен из Revit.");
                }
                
                // Проверяем размер файла
                if (fileInfo.Length == 0)
                {
                    throw new Exception($"Файл пуст: {rvtFilePath}");
                }

                // Проверяем, что это действительно файл Revit
                if (!rvtFilePath.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Файл не является файлом Revit: {rvtFilePath}");
                }
                
                // Обновляем прогресс - открытие документа
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 20);
                UpdateUI();
                
                // Проверяем, что файл не заблокирован
                try
                {
                    using (var fileStream = File.Open(rvtFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Файл доступен для чтения
                    }
                }
                catch (Exception fileEx)
                {
                    throw new Exception($"Файл заблокирован или недоступен: {fileEx.Message}");
                }
                
                // Открываем документ Revit
                try
                {
                    // Проверяем, что RevitApp готов к работе
                    if (_revitApp.Documents == null)
                    {
                        throw new Exception("RevitApp не готов к работе с документами");
                    }

                    // Пытаемся открыть файл
                    document = _revitApp.OpenDocumentFile(rvtFilePath);
                }
                catch (Exception openEx)
                {
                    // Детальная диагностика ошибки
                    var errorDetails = new List<string>();
                    errorDetails.Add($"Ошибка открытия файла: {openEx.Message}");
                    errorDetails.Add($"Тип исключения: {openEx.GetType().Name}");
                    
                    if (openEx.InnerException != null)
                    {
                        errorDetails.Add($"Внутренняя ошибка: {openEx.InnerException.Message}");
                    }
                    
                    errorDetails.Add($"Файл: {rvtFilePath}");
                    errorDetails.Add($"Размер файла: {fileInfo.Length} байт");
                    
                    // Пропускаем проблемный файл вместо остановки всего процесса
                    WriteToLogFile("⚠️ ПРОПУСКАЕМ ПРОБЛЕМНЫЙ ФАЙЛ");
                    return; // Выходим из метода, пропуская этот файл
                }
                
                if (document == null)
                {
                    throw new Exception($"Не удалось открыть документ Revit. Файл: {rvtFilePath}, Размер: {fileInfo.Length} байт");
                }

                // Проверяем отмену перед извлечением данных
                if (progressWindow.IsCancelled)
                {
                    SafeCloseDocument(document, rvtFilePath);
                    WriteDetailedLog("✅ Документ закрыт (операция отменена)");
                    return;
                }

                // Обновляем прогресс - извлечение данных
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 50);
                UpdateUI();

                // Извлекаем данные из документа
                var dataReader = new Services.RevitDataReader();
                var modelName = Path.GetFileNameWithoutExtension(rvtFilePath);
                var elementsData = dataReader.ExtractElementsFromDocument(document, modelName, null, () => progressWindow.IsCancelled);

                // Проверяем, что данные извлечены
                if (elementsData == null || elementsData.Count == 0)
                {
                    throw new Exception("Не удалось извлечь данные из документа. Возможно, документ пуст или поврежден.");
                }

                // Проверяем отмену перед экспортом в CSV
                if (progressWindow.IsCancelled)
                {
                    SafeCloseDocument(document, rvtFilePath);
                    WriteDetailedLog("✅ Документ закрыт (операция отменена перед CSV)");
                    return;
                }

                // Обновляем прогресс - экспорт в CSV
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 80);
                UpdateUI();

                // Экспортируем в CSV
                var exporter = new Services.RevitExporter();
                var csvContent = exporter.ExportToCsv(elementsData, modelName);

                // Проверяем, что CSV создан
                if (string.IsNullOrEmpty(csvContent))
                {
                    throw new Exception("Не удалось создать CSV контент.");
                }

                // Сохраняем файл
                File.WriteAllText(outputFilePath, csvContent, System.Text.Encoding.UTF8);

                // Обновляем прогресс - закрытие документа
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 95);
                UpdateUI();

                WriteDetailedLog("✅ Экспорт данных завершен успешно");
            }
            catch (Exception ex)
            {
                var fileName = Path.GetFileName(rvtFilePath);
                WriteToLogFile($"❌ Ошибка экспорта файла {fileName}: {ex.Message}");
                
                // Создаем детальный отчет об ошибке
                var errorContent = CreateDetailedErrorReport(ex, rvtFilePath, "Экспорт без фильтров");
                
                // Сохраняем файл с ошибкой
                File.WriteAllText(outputFilePath, errorContent, System.Text.Encoding.UTF8);
                
                // НЕ перебрасываем исключение - продолжаем обработку других файлов
                WriteToLogFile($"⚠️ Файл {fileName} пропущен из-за ошибки, продолжаем с другими файлами");
            }
            finally
            {
                // Безопасно закрываем документ в блоке finally
                SafeCloseDocument(document, rvtFilePath);
            }
        }

        private void ExportRvtFileDataWithFilters(string rvtFilePath, string outputFilePath, List<string> selectedCategories, ExportProgressWindow progressWindow, int currentIndex, int totalFiles)
        {
            Document document = null;
            try
            {
                // Проверяем отмену в начале обработки файла
                if (progressWindow.IsCancelled)
                {
                    return;
                }
                
                var fileName = Path.GetFileName(rvtFilePath);
                
                WriteToLogFile($"Экспорт файла с фильтрами: {fileName}");
                WriteToLogFile($"Выбрано категорий для фильтрации: {selectedCategories.Count}");
                
                // Детальное логирование начала обработки файла
                WriteDetailedLog($"\n=== ОБРАБОТКА ФАЙЛА {currentIndex + 1} из {totalFiles} ===");
                WriteDetailedLog($"Файл: {fileName}");
                WriteDetailedLog($"Полный путь: {rvtFilePath}");
                WriteDetailedLog($"Выходной CSV: {outputFilePath}");
                WriteDetailedLog($"Время начала обработки: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                
                // Проверяем, что RevitApp инициализирован
                if (_revitApp == null)
                {
                    throw new Exception("RevitApp не инициализирован. Убедитесь, что плагин запущен из Revit.");
                }
                
                // Проверяем существование файла
                if (!File.Exists(rvtFilePath))
                {
                    throw new Exception($"Файл не найден: {rvtFilePath}");
                }

                // Проверяем размер файла
                var fileInfo = new FileInfo(rvtFilePath);
                if (fileInfo.Length == 0)
                {
                    throw new Exception($"Файл пуст: {rvtFilePath}");
                }

                // Проверяем, что это действительно файл Revit
                if (!rvtFilePath.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Файл не является файлом Revit: {rvtFilePath}");
                }
                
                // Обновляем прогресс - открытие документа
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 20);
                UpdateUI();
                
                // Проверяем, что файл не заблокирован
                try
                {
                    using (var fileStream = File.Open(rvtFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Файл доступен для чтения
                    }
                }
                catch (Exception fileEx)
                {
                    throw new Exception($"Файл заблокирован или недоступен: {fileEx.Message}");
                }
                
                // Открываем документ Revit
                try
                {
                    WriteToLogFile($"=== ДЕТАЛЬНАЯ ДИАГНОСТИКА ОТКРЫТИЯ ФАЙЛА ===");
                    WriteToLogFile($"Файл: {rvtFilePath}");
                    WriteToLogFile($"Размер файла: {fileInfo.Length} байт");
                    WriteToLogFile($"Версия Revit: {_revitApp.VersionName}");
                    WriteToLogFile($"Количество открытых документов: {_revitApp.Documents?.Size ?? -1}");
                    
                    // Дополнительная информация о файле
                    WriteToLogFile($"Дата создания файла: {fileInfo.CreationTime}");
                    WriteToLogFile($"Последнее изменение: {fileInfo.LastWriteTime}");
                    
                    // Проверяем, что RevitApp готов к работе
                    if (_revitApp.Documents == null)
                    {
                        WriteToLogFile("❌ RevitApp.Documents == null");
                        throw new Exception("RevitApp не готов к работе с документами");
                    }
                    
                    WriteToLogFile("✓ RevitApp.Documents доступен");
                    
                    // Проверяем, не открыт ли уже этот файл
                    var openDocs = _revitApp.Documents.Cast<Autodesk.Revit.DB.Document>();
                    var alreadyOpen = openDocs.FirstOrDefault(d => d.PathName.Equals(rvtFilePath, StringComparison.OrdinalIgnoreCase));
                    if (alreadyOpen != null)
                    {
                        WriteToLogFile($"⚠️ Файл уже открыт в Revit: {alreadyOpen.Title}");
                        document = alreadyOpen;
                    }
                    else
                    {
                        WriteToLogFile("Файл не открыт, пытаемся открыть...");
                        
                        // Проверяем права доступа к файлу (рекомендация из интернета)
                        WriteToLogFile("Проверка прав доступа к файлу...");
                        try
                        {
                            using (var fileStream = System.IO.File.Open(rvtFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                            {
                                WriteToLogFile("✓ Файл доступен для чтения");
                            }
                        }
                        catch (Exception accessEx)
                        {
                            WriteToLogFile($"❌ Файл недоступен для чтения: {accessEx.Message}");
                            WriteToLogFile("⚠️ ПРОПУСКАЕМ ПРОБЛЕМНЫЙ ФАЙЛ");
                            return;
                        }
                        
                        // Пробуем открыть файл напрямую с разными способами
                        WriteToLogFile("Попытка открытия файла напрямую...");
                        
                        try
                        {
                            // Способ 1: Простое открытие (как в рабочем коде)
                            WriteToLogFile("Способ 1: Простое открытие...");
                            document = _revitApp.OpenDocumentFile(rvtFilePath);
                            
                            if (document != null)
                            {
                                WriteToLogFile($"✓ Файл успешно открыт простым способом: {document.Title}");
                                WriteDetailedLog($"✓ Документ успешно открыт простым способом");
                                WriteDetailedLog($"  Название документа: {document.Title}");
                                WriteDetailedLog($"  Путь к документу: {document.PathName}");
                                WriteDetailedLog($"  Количество открытых документов в Revit: {_revitApp.Documents.Size}");
                            }
                            else
                            {
                                throw new Exception("OpenDocumentFile вернул null");
                            }
                        }
                        catch (Exception simpleEx)
                        {
                            WriteToLogFile($"❌ Простой способ не сработал: {simpleEx.Message}");
                            
                            try
                            {
                                // Способ 2: С OpenOptions
                                WriteToLogFile("Способ 2: С OpenOptions...");
                                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                                openOptions.Audit = false;
                                openOptions.AllowOpeningLocalByWrongUser = true;
                                
                                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(rvtFilePath);
                                document = _revitApp.OpenDocumentFile(modelPath, openOptions);
                                
                                if (document != null)
                                {
                                    WriteToLogFile($"✓ Файл успешно открыт с OpenOptions: {document.Title}");
                                }
                                else
                                {
                                    throw new Exception("OpenDocumentFile с OpenOptions вернул null");
                                }
                            }
                            catch (Exception optionsEx)
                            {
                                WriteToLogFile($"❌ Способ с OpenOptions не сработал: {optionsEx.Message}");
                                
                                try
                                {
                                // Способ 3: С DoNotDetach (как в рабочем коде)
                                WriteToLogFile("Способ 3: С DoNotDetach (как в рабочем коде)...");
                                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DoNotDetach;
                                openOptions.Audit = false;
                                    
                                    var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(rvtFilePath);
                                    document = _revitApp.OpenDocumentFile(modelPath, openOptions);
                                    
                                    if (document != null)
                                    {
                                        WriteToLogFile($"✓ Файл успешно открыт с DetachFromCentralOption: {document.Title}");
                                    }
                                    else
                                    {
                                        throw new Exception("OpenDocumentFile с DetachFromCentralOption вернул null");
                                    }
                                }
                                catch (Exception detachEx)
                                {
                                    WriteToLogFile($"❌ Способ с DoNotDetach не сработал: {detachEx.Message}");
                                    
                                    try
                                    {
                                        // Способ 4: Проверка контекста выполнения (рекомендация из интернета)
                                        WriteToLogFile("Способ 4: Проверка контекста выполнения...");
                                        WriteToLogFile($"Текущий поток: {System.Threading.Thread.CurrentThread.Name ?? "Unnamed"}");
                                        WriteToLogFile($"ID потока: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                                        WriteToLogFile($"IsBackground: {System.Threading.Thread.CurrentThread.IsBackground}");
                                        
                                        // Пробуем открыть в том же потоке
                                        var openOptions = new Autodesk.Revit.DB.OpenOptions();
                                        openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DoNotDetach;
                                        openOptions.Audit = false;
                                        
                                        var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(rvtFilePath);
                                        document = _revitApp.OpenDocumentFile(modelPath, openOptions);
                                        
                                        if (document != null)
                                        {
                                            WriteToLogFile($"✓ Файл успешно открыт в правильном контексте: {document.Title}");
                                        }
                                        else
                                        {
                                            throw new Exception("Открытие в правильном контексте вернуло null");
                                        }
                                    }
                                    catch (Exception exactEx)
                                    {
                                        WriteToLogFile($"❌ Способ с проверкой контекста не сработал: {exactEx.Message}");
                                        
                                        try
                                        {
                                            // Способ 5: Выполнение в основном потоке (решение проблемы IsBackground: True)
                                            WriteToLogFile("Способ 5: Выполнение в основном потоке через Dispatcher...");
                                            
                                            Autodesk.Revit.DB.Document mainThreadDocument = null;
                                            Exception mainThreadException = null;
                                            
                                            // Выполняем в основном потоке UI
                                            this.Dispatcher.Invoke(() =>
                                            {
                                                try
                                                {
                                                    WriteToLogFile("Выполнение в основном потоке UI...");
                                                    var openOptions = new Autodesk.Revit.DB.OpenOptions();
                                                    openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DoNotDetach;
                                                    openOptions.Audit = false;
                                                    
                                                    var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(rvtFilePath);
                                                    mainThreadDocument = _revitApp.OpenDocumentFile(modelPath, openOptions);
                                                    
                                                    if (mainThreadDocument != null)
                                                    {
                                                        WriteToLogFile($"✓ Файл успешно открыт в основном потоке: {mainThreadDocument.Title}");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    mainThreadException = ex;
                                                    WriteToLogFile($"❌ Ошибка в основном потоке: {ex.Message}");
                                                }
                                            });
                                            
                                            if (mainThreadDocument != null)
                                            {
                                                document = mainThreadDocument;
                                                WriteToLogFile($"✓ Файл успешно открыт в основном потоке: {document.Title}");
                                            }
                                            else if (mainThreadException != null)
                                            {
                                                throw mainThreadException;
                                            }
                                            else
                                            {
                                                throw new Exception("Открытие в основном потоке вернуло null");
                                            }
                                        }
                                        catch (Exception mainThreadEx)
                                        {
                                            WriteToLogFile($"❌ Все способы не сработали:");
                                            WriteToLogFile($"  Простой: {simpleEx.Message}");
                                            WriteToLogFile($"  OpenOptions: {optionsEx.Message}");
                                            WriteToLogFile($"  DoNotDetach: {detachEx.Message}");
                                            WriteToLogFile($"  Контекст: {exactEx.Message}");
                                            WriteToLogFile($"  Основной поток: {mainThreadEx.Message}");
                                            WriteToLogFile("⚠️ ПРОПУСКАЕМ ПРОБЛЕМНЫЙ ФАЙЛ");
                                            return; // Выходим из метода, пропуская этот файл
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception openEx)
                {
                    WriteToLogFile($"❌ ОШИБКА ОТКРЫТИЯ ФАЙЛА:");
                    WriteToLogFile($"  Сообщение: {openEx.Message}");
                    WriteToLogFile($"  Тип исключения: {openEx.GetType().Name}");
                    WriteToLogFile($"  StackTrace: {openEx.StackTrace}");
                    
                    if (openEx.InnerException != null)
                    {
                        WriteToLogFile($"  Внутренняя ошибка: {openEx.InnerException.Message}");
                        WriteToLogFile($"  Тип внутренней ошибки: {openEx.InnerException.GetType().Name}");
                    }
                    
                    // Дополнительная диагностика
                    WriteToLogFile($"=== ДОПОЛНИТЕЛЬНАЯ ДИАГНОСТИКА ===");
                    WriteToLogFile($"RevitApp == null: {_revitApp == null}");
                    WriteToLogFile($"RevitApp.Documents == null: {_revitApp?.Documents == null}");
                    WriteToLogFile($"Файл существует: {File.Exists(rvtFilePath)}");
                    WriteToLogFile($"Файл доступен для чтения: {CanReadFile(rvtFilePath)}");
                    
                    // Детальная диагностика ошибки
                    var errorDetails = new List<string>();
                    errorDetails.Add($"Ошибка открытия файла: {openEx.Message}");
                    errorDetails.Add($"Тип исключения: {openEx.GetType().Name}");
                    
                    if (openEx.InnerException != null)
                    {
                        errorDetails.Add($"Внутренняя ошибка: {openEx.InnerException.Message}");
                    }
                    
                    errorDetails.Add($"Файл: {rvtFilePath}");
                    errorDetails.Add($"Размер файла: {fileInfo.Length} байт");
                    
                    // Пропускаем проблемный файл вместо остановки всего процесса
                    WriteToLogFile("⚠️ ПРОПУСКАЕМ ПРОБЛЕМНЫЙ ФАЙЛ");
                    return; // Выходим из метода, пропуская этот файл
                }
                
                if (document == null)
                {
                    throw new Exception($"Не удалось открыть документ Revit. Файл: {rvtFilePath}, Размер: {fileInfo.Length} байт");
                }

                // Обновляем прогресс - извлечение данных
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 50);
                UpdateUI();

               // Создаем фильтр категорий на уровне Revit API
               WriteDetailedLog("🔧 Создаем фильтр категорий на уровне Revit API...");
               var categoryFilter = CreateCategoryFilter(selectedCategories);
               
               // Получаем список нефильтруемых категорий
               var nonFilterableCategories = selectedCategories.Where(cat => IsNonFilterableCategory(cat)).ToList();
               
               // Извлекаем данные из документа С фильтрацией на уровне API
               WriteDetailedLog("🔍 Начинаем извлечение элементов с фильтрацией на уровне API...");
               var dataReader = new Services.RevitDataReader();
               var modelName = Path.GetFileNameWithoutExtension(rvtFilePath);
               var elementsData = dataReader.ExtractElementsFromDocumentWithFilter(document, modelName, categoryFilter, null, () => progressWindow.IsCancelled, WriteDetailedLog, nonFilterableCategories);
               
               WriteToLogFile($"Извлечено {elementsData.Count} элементов с фильтрацией на уровне API");
               WriteDetailedLog($"📊 КОЛИЧЕСТВО ЭЛЕМЕНТОВ ПОСЛЕ API ФИЛЬТРАЦИИ: {elementsData.Count}");

                WriteToLogFile($"Извлечено {elementsData.Count} элементов после фильтрации");

                // Проверяем, что данные извлечены
                if (elementsData == null || elementsData.Count == 0)
                {
                    WriteToLogFile("⚠️ Не найдено элементов, соответствующих выбранным фильтрам");
                    // Создаем пустой CSV с информацией о фильтрах
                    var emptyCsvContent = $"ModelName,ElementId,Category,ParameterName,ParameterValue\n" +
                                        $"\"{modelName}\",\"N/A\",\"Фильтрация\",\"Выбранные категории\",\"{string.Join("; ", selectedCategories)}\"\n" +
                                        $"\"{modelName}\",\"N/A\",\"Фильтрация\",\"Результат\",\"Элементы не найдены\"\n";
                    File.WriteAllText(outputFilePath, emptyCsvContent, System.Text.Encoding.UTF8);
                    WriteToLogFile($"Создан пустой CSV файл: {outputFilePath}");
                    return;
                }

                // Обновляем прогресс - экспорт в CSV
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 80);
                UpdateUI();

                // Экспортируем в CSV
                var exporter = new Services.RevitExporter();
                var csvContent = exporter.ExportToCsv(elementsData, modelName);

                // Проверяем, что CSV создан
                if (string.IsNullOrEmpty(csvContent))
                {
                    throw new Exception("Не удалось создать CSV контент.");
                }

                // Используем CSV без комментариев
                var filteredCsvContent = csvContent;

                // Сохраняем файл
                File.WriteAllText(outputFilePath, filteredCsvContent, System.Text.Encoding.UTF8);

                WriteToLogFile($"✓ CSV файл сохранен: {outputFilePath}");

                // Обновляем прогресс - закрытие документа
                progressWindow.UpdateProgress(currentIndex, totalFiles, fileName, 95);
                UpdateUI();

                WriteDetailedLog("✅ Экспорт данных завершен успешно");
            }
            catch (Exception ex)
            {
                var fileName = Path.GetFileName(rvtFilePath);
                WriteToLogFile($"❌ Ошибка экспорта файла {fileName}: {ex.Message}");
                
                // Создаем детальный отчет об ошибке с информацией о фильтрах
                var errorContent = CreateDetailedErrorReport(ex, rvtFilePath, "Экспорт с фильтрами", selectedCategories);
                
                // Сохраняем файл с ошибкой
                File.WriteAllText(outputFilePath, errorContent, System.Text.Encoding.UTF8);
                
                // НЕ перебрасываем исключение - продолжаем обработку других файлов
                WriteToLogFile($"⚠️ Файл {fileName} пропущен из-за ошибки, продолжаем с другими файлами");
            }
            finally
            {
                // Безопасно закрываем документ в блоке finally
                SafeCloseDocument(document, rvtFilePath);
            }
        }

        /// <summary>
        /// Создает детальный отчет об ошибке экспорта
        /// </summary>
        private string CreateDetailedErrorReport(Exception ex, string rvtFilePath, string exportType, List<string> selectedCategories = null)
        {
            var fileName = Path.GetFileName(rvtFilePath);
            var fileInfo = new FileInfo(rvtFilePath);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            var report = new StringBuilder();
            report.AppendLine("ModelName,ElementId,Category,ParameterName,ParameterValue");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Тип экспорта\",\"{exportType}\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Время ошибки\",\"{timestamp}\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Файл\",\"{rvtFilePath}\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Размер файла\",\"{fileInfo.Length} байт\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Тип ошибки\",\"{ex.GetType().Name}\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Сообщение\",\"{ex.Message}\"");
            
            if (selectedCategories != null && selectedCategories.Count > 0)
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Выбранные категории\",\"{string.Join("; ", selectedCategories)}\"");
            }
            
            // Добавляем информацию о системе
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Версия Revit\",\"{_revitApp?.VersionName ?? "Неизвестно"}\"");
            report.AppendLine($"\"{fileName}\",\"ERROR\",\"Система\",\"Количество открытых документов\",\"{_revitApp?.Documents?.Size ?? -1}\"");
            
            // Добавляем рекомендации по решению
            if (ex.Message.Contains("InternalException"))
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Проблема\",\"Файл может быть поврежден или создан в несовместимой версии Revit\"");
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Решение\",\"Попробуйте открыть файл вручную в Revit для проверки\"");
            }
            else if (ex.Message.Contains("не найден"))
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Проблема\",\"Файл не найден или недоступен\"");
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Решение\",\"Проверьте путь к файлу и права доступа\"");
            }
            else if (ex.Message.Contains("заблокирован"))
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Проблема\",\"Файл заблокирован другим процессом\"");
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Решение\",\"Закройте файл в Revit и попробуйте снова\"");
            }
            else
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Проблема\",\"Неизвестная ошибка\"");
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"Рекомендация\",\"Решение\",\"Обратитесь к разработчику с этим отчетом\"");
            }
            
            // Добавляем StackTrace (первые 5 строк)
            var stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() ?? new string[0];
            for (int i = 0; i < stackTrace.Length; i++)
            {
                report.AppendLine($"\"{fileName}\",\"ERROR\",\"StackTrace\",\"Строка {i + 1}\",\"{stackTrace[i].Trim()}\"");
            }
            
            return report.ToString();
        }

        #region Authentication Methods

        private async void CheckAuthentication()
        {
            try
            {
                var token = _authService.GetStoredToken();
                
                if (!string.IsNullOrEmpty(token))
                {
                    var isValid = await _authService.ValidateTokenAsync(token);
                    if (isValid)
                    {
                        _currentUser = await _authService.GetUserInfoAsync(token);
                        ShowMainScreen();
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
                // В случае ошибки показываем экран авторизации
                ShowAuthScreen();
            }
        }

        private void ShowAuthScreen()
        {
            AuthScreen.Visibility = Visibility.Visible;
            MainScreen.Visibility = Visibility.Collapsed;
        }

        private void ShowMainScreen()
        {
            AuthScreen.Visibility = Visibility.Collapsed;
            MainScreen.Visibility = Visibility.Visible;
            
            // Загружаем проекты и показываем основной экран
            LoadProjects();
            ShowProjectsScreen();
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToLoginTab();
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToRegisterTab();
        }

        private void SwitchToLoginTab()
        {
            LoginForm.Visibility = Visibility.Visible;
            RegisterForm.Visibility = Visibility.Collapsed;
            
            LoginTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14b8a6"));
            LoginTabButton.BorderThickness = new Thickness(0, 0, 0, 2);
            LoginTabButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14b8a6"));
            
            RegisterTabButton.Foreground = new SolidColorBrush(Colors.White);
            RegisterTabButton.BorderThickness = new Thickness(0);
            
            HideMessages();
        }

        private void SwitchToRegisterTab()
        {
            LoginForm.Visibility = Visibility.Collapsed;
            RegisterForm.Visibility = Visibility.Visible;
            
            RegisterTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14b8a6"));
            RegisterTabButton.BorderThickness = new Thickness(0, 0, 0, 2);
            RegisterTabButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14b8a6"));
            
            LoginTabButton.Foreground = new SolidColorBrush(Colors.White);
            LoginTabButton.BorderThickness = new Thickness(0);
            
            HideMessages();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HideMessages();
                
                var email = LoginEmail.Text.Trim();
                var password = LoginPassword.Password;
                
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ShowError("Заполните все поля");
                    return;
                }
                
                LoginButton.Content = "Вход...";
                LoginButton.IsEnabled = false;
                
                var result = await _authService.LoginAsync(email, password);
                
                if (result.Success)
                {
                    _authService.SaveToken(result.Token);
                    _currentUser = result.User;
                    
                    ShowSuccess("Успешный вход! Переход к основному интерфейсу...");
                    await Task.Delay(1500);
                    
                    ShowMainScreen();
                }
                else
                {
                    string errorMessage = result.ErrorMessage ?? "Неизвестная ошибка";
                    ShowError($"Ошибка авторизации: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}\n\nДетали:\n{ex.StackTrace}");
            }
            finally
            {
                LoginButton.Content = "Войти";
                LoginButton.IsEnabled = true;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HideMessages();
                
                var userType = (RegisterUserType.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                var name = RegisterUserName.Text.Trim();
                var login = RegisterLogin.Text.Trim();
                var email = RegisterEmail.Text.Trim();
                var password = RegisterPassword.Password;
                var passwordConfirm = RegisterPasswordConfirm.Password;
                
                if (string.IsNullOrEmpty(userType) || userType == "Выберите тип")
                {
                    ShowError("Выберите тип пользователя");
                    return;
                }
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(login) || 
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ShowError("Заполните все обязательные поля");
                    return;
                }
                
                if (password != passwordConfirm)
                {
                    ShowError("Пароли не совпадают");
                    return;
                }
                
                RegisterButton.Content = "Регистрация...";
                RegisterButton.IsEnabled = false;
                
                var result = await _authService.RegisterAsync(login, email, password);
                
                if (result.Success)
                {
                    _authService.SaveToken(result.Token);
                    _currentUser = result.User;
                    
                    ShowSuccess("Регистрация успешна! Переход к основному интерфейсу...");
                    await Task.Delay(1500);
                    
                    ShowMainScreen();
                }
                else
                {
                    string errorMessage = result.ErrorMessage ?? "Неизвестная ошибка";
                    ShowError($"Ошибка регистрации: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}\n\nДетали:\n{ex.StackTrace}");
            }
            finally
            {
                RegisterButton.Content = "Зарегистрироваться";
                RegisterButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
            SuccessMessage.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessageText.Text = message;
            SuccessMessage.Visibility = Visibility.Visible;
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
            SuccessMessage.Visibility = Visibility.Collapsed;
        }

        public void Logout()
        {
            _authService.ClearToken();
            _currentUser = null;
            
            // Очищаем поля авторизации
            LoginEmail.Text = "";
            LoginPassword.Password = "";
            RegisterUserType.SelectedIndex = 0;
            RegisterUserName.Text = "";
            RegisterLogin.Text = "";
            RegisterEmail.Text = "";
            RegisterPassword.Password = "";
            RegisterPasswordConfirm.Password = "";
            
            ShowAuthScreen();
        }

        #endregion

        #region User Profile Methods

        private async void LoadUserInfo(StackPanel parentPanel)
        {
            try
            {
                if (_authService == null)
                {
                    var errorText = new TextBlock
                    {
                        Text = "Ошибка: Сервис авторизации не инициализирован",
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(errorText);
                    return;
                }

                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    var errorText = new TextBlock
                    {
                        Text = "Ошибка: Пользователь не авторизован",
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(errorText);
                    return;
                }

                var userInfo = await _authService.GetUserInfoAsync(token);
                if (userInfo == null)
                {
                    var errorText = new TextBlock
                    {
                        Text = "Ошибка: Не удалось загрузить информацию о пользователе",
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(errorText);
                    return;
                }

                // Создаем панель с информацией о пользователе
                var infoPanel = new StackPanel
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Margin = new Thickness(20, 20, 20, 0)
                };

                // Имя пользователя
                AddInfoField(infoPanel, "Имя:", userInfo.Name ?? "Не указано");
                
                // Логин
                AddInfoField(infoPanel, "Логин:", userInfo.Login ?? "Не указано");
                
                // Email
                AddInfoField(infoPanel, "Email:", userInfo.Email ?? "Не указано");
                
                // Тип пользователя
                var userTypeText = userInfo.UserType == "user" ? "Пользователь" : "Организация";
                AddInfoField(infoPanel, "Тип пользователя:", userTypeText);
                
                // Название компании (если есть)
                if (!string.IsNullOrEmpty(userInfo.CompanyName))
                {
                    AddInfoField(infoPanel, "Компания:", userInfo.CompanyName);
                }
                
                // Телефон (если есть)
                if (!string.IsNullOrEmpty(userInfo.Phone))
                {
                    AddInfoField(infoPanel, "Телефон:", userInfo.Phone);
                }
                
                // Дата регистрации
                if (userInfo.CreatedAt != null)
                {
                    AddInfoField(infoPanel, "Дата регистрации:", userInfo.CreatedAt);
                }

                parentPanel.Children.Add(infoPanel);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Ошибка при загрузке информации: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                parentPanel.Children.Add(errorText);
            }
        }

        private void AddInfoField(StackPanel parent, string label, string value)
        {
            var fieldPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            
            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var valueText = new TextBlock
            {
                Text = value,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            
            fieldPanel.Children.Add(labelText);
            fieldPanel.Children.Add(valueText);
            parent.Children.Add(fieldPanel);
        }

        private async void LoadUserProducts(StackPanel parentPanel)
        {
            try
            {
                if (_authService == null)
                {
                    var errorText = new TextBlock
                    {
                        Text = "Ошибка: Сервис авторизации не инициализирован",
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(errorText);
                    return;
                }

                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    var errorText = new TextBlock
                    {
                        Text = "Ошибка: Пользователь не авторизован",
                        Foreground = new SolidColorBrush(Colors.Red),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    parentPanel.Children.Add(errorText);
                    return;
                }

                // Загружаем продукты пользователя через API
                var products = await GetUserProductsAsync(token);
                if (products == null || products.Count == 0)
                {
                    var noProductsText = new TextBlock
                    {
                        Text = "У вас пока нет приобретенных продуктов.\nПерейдите на сайт для покупки.",
                        Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                        FontSize = 14,
                        Margin = new Thickness(0, 20, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    parentPanel.Children.Add(noProductsText);
                    return;
                }

                // Создаем таблицу продуктов
                CreateProductsTable(parentPanel, products);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Ошибка при загрузке продуктов: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                parentPanel.Children.Add(errorText);
            }
        }

        private void LoadProfileEditForm(StackPanel parentPanel)
        {
            try
            {
                // TODO: Реализовать форму редактирования профиля
                var editText = new TextBlock
                {
                    Text = "Функция редактирования профиля будет реализована в следующем обновлении",
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                parentPanel.Children.Add(editText);
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Ошибка при загрузке формы редактирования: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                parentPanel.Children.Add(errorText);
            }
        }

        private async Task<List<ProductInfo>> GetUserProductsAsync(string token)
        {
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync("http://civilx.ru/auth-api.php/api/user-products");
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<UserProductsResponse>(responseContent);
                        return result.Products ?? new List<ProductInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при получении продуктов: {ex.Message}");
            }
            
            return new List<ProductInfo>();
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

            var headerStatus = new TextBlock
            {
                Text = "Статус",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerStatus, 1);

            var headerExpires = new TextBlock
            {
                Text = "Истекает",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerExpires, 2);

            var headerAction = new TextBlock
            {
                Text = "Действие",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(10, 10, 10, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetColumn(headerAction, 3);

            headerPanel.Children.Add(headerProduct);
            headerPanel.Children.Add(headerStatus);
            headerPanel.Children.Add(headerExpires);
            headerPanel.Children.Add(headerAction);

            tablePanel.Children.Add(headerPanel);

            // Добавляем строки для каждого продукта
            foreach (var product in products)
            {
                var rowPanel = new Grid();
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                // Название продукта
                var productName = new TextBlock
                {
                    Text = GetProductDisplayName(product.ProductName),
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetColumn(productName, 0);

                // Статус
                var statusText = new TextBlock
                {
                    Text = GetStatusDisplayText(product.ActivationStatus),
                    FontSize = 12,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(GetStatusColor(product.ActivationStatus))
                };
                Grid.SetColumn(statusText, 1);

                // Дата истечения
                var expiresText = new TextBlock
                {
                    Text = product.ExpiresAt?.ToString("dd.MM.yyyy") ?? "Не указано",
                    FontSize = 12,
                    Margin = new Thickness(10, 10, 10, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                Grid.SetColumn(expiresText, 2);

                // Кнопка действия
                var actionButton = new Button
                {
                    Content = GetActionButtonText(product.ActivationStatus),
                    Height = 30,
                    FontSize = 12,
                    Margin = new Thickness(5, 5, 5, 5),
                    Background = new SolidColorBrush(GetActionButtonColor(product.ActivationStatus)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0)
                };
                actionButton.Click += (s, e) => HandleProductAction(product);
                Grid.SetColumn(actionButton, 3);

                rowPanel.Children.Add(productName);
                rowPanel.Children.Add(statusText);
                rowPanel.Children.Add(expiresText);
                rowPanel.Children.Add(actionButton);

                tablePanel.Children.Add(rowPanel);
            }

            parentPanel.Children.Add(tablePanel);
            
            // Добавляем блок состояния подписки
            CreateSubscriptionStatusBlock(parentPanel);
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
                case "activated":
                    return "Активен";
                case "pending":
                    return "Ожидает";
                case "ready":
                    return "Готов";
                case "expired":
                    return "Истек";
                default:
                    return status ?? "Неизвестно";
            }
        }

        private Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "activated":
                    return Color.FromRgb(40, 167, 69); // Зеленый
                case "pending":
                    return Color.FromRgb(255, 193, 7); // Желтый
                case "ready":
                    return Color.FromRgb(255, 193, 7); // Желтый для "Готов"
                case "expired":
                    return Color.FromRgb(220, 53, 69); // Красный
                default:
                    return Color.FromRgb(108, 117, 125); // Серый
            }
        }

        private string GetActionButtonText(string status)
        {
            switch (status?.ToLower())
            {
                case "activated":
                    return "Отменить";
                case "pending":
                    return "Активировать";
                case "ready":
                    return "Активировать";
                case "expired":
                    return "Продлить";
                default:
                    return "Неизвестно";
            }
        }

        private Color GetActionButtonColor(string status)
        {
            switch (status?.ToLower())
            {
                case "activated":
                    return Color.FromRgb(220, 53, 69); // Красный для кнопки "Отменить"
                case "pending":
                    return Color.FromRgb(0, 123, 255); // Синий
                case "ready":
                    return Color.FromRgb(40, 167, 69); // Зеленый для кнопки "Активировать"
                case "expired":
                    return Color.FromRgb(255, 193, 7); // Желтый
                default:
                    return Color.FromRgb(108, 117, 125); // Серый
            }
        }

        private async void HandleProductAction(ProductInfo product)
        {
            switch (product.ActivationStatus?.ToLower())
            {
                case "ready":
                    await ActivateProductAsync(product);
                    break;
                case "pending":
                    // Активировать продукт
                    ActivateProduct(product);
                    break;
                case "expired":
                    // Продлить подписку - открыть сайт
                    try
                    {
                        System.Diagnostics.Process.Start("http://civilx.ru");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии сайта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "activated":
                    await DeactivateProductAsync(product);
                    break;
            }
        }

        private async Task ActivateProductAsync(ProductInfo product)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Starting activation for product ID {product.Id}");
                
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("ActivateProductAsync: Token not found");
                    MessageBox.Show("Токен авторизации не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Token found, making API request");

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var requestData = new { product_id = product.Id };
                    var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(requestData);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Sending request to API with product_id: {product.Id}");
                    
                    var response = await client.PostAsync("http://civilx.ru/auth-api.php/api/activate-product", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Response status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"ActivateProductAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Обновляем статус продукта локально
                        product.ActivationStatus = "activated";
                        
                        // Активируем продукт локально
                        _activatedProduct = product;
                        SaveActivatedProduct();
                        // UpdateUserStatusIndicator(); // Убрано - кнопка пользователя больше не нужна
                        UpdateSubscriptionStatusBlock();
                        
                        // Перезагружаем список продуктов
                        ShowUserProfileScreen();
                        
                        MessageBox.Show($"Продукт {GetProductDisplayName(product.ProductName)} успешно активирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorResult = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        MessageBox.Show($"Ошибка активации: {(errorResult.ContainsKey("error") ? errorResult["error"] : "Неизвестная ошибка")}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при активации продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Токен авторизации не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Token found, making API request");

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var requestData = new { product_id = product.Id };
                    var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(requestData);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Sending request to API with product_id: {product.Id}");
                    
                    var response = await client.PostAsync("http://civilx.ru/auth-api.php/api/deactivate-product", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Response status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"DeactivateProductAsync: Response content: {responseContent}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        var newStatus = result.ContainsKey("new_status") ? result["new_status"].ToString() : "ready";
                        
                        // Обновляем статус продукта локально
                        product.ActivationStatus = newStatus;
                        
                        // Если это был активированный продукт, очищаем его
                        if (_activatedProduct != null && _activatedProduct.Id == product.Id)
                        {
                            _activatedProduct = null;
                            SaveActivatedProduct();
                        }
                        
                        // UpdateUserStatusIndicator(); // Убрано - кнопка пользователя больше не нужна
                        UpdateSubscriptionStatusBlock();
                        
                        // Перезагружаем список продуктов
                        ShowUserProfileScreen();
                        
                        MessageBox.Show($"Продукт {GetProductDisplayName(product.ProductName)} успешно отменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorResult = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        MessageBox.Show($"Ошибка отмены: {(errorResult.ContainsKey("error") ? errorResult["error"] : "Неизвестная ошибка")}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActivateProduct(ProductInfo product)
        {
            try
            {
                // Активируем продукт локально
                _activatedProduct = product;
                SaveActivatedProduct();
                
                // Обновляем индикатор статуса - убрано
                // UpdateUserStatusIndicator();
                
                // Обновляем блок состояния подписки
                UpdateSubscriptionStatusBlock();
                
                MessageBox.Show($"Продукт {GetProductDisplayName(product.ProductName)} активирован для текущего плагина", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при активации продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Методы убраны - кнопка пользователя больше не нужна
        // private async void UpdateUserStatusIndicator() { ... }
        // private void SetUserIcon(string icon, Color backgroundColor) { ... }

        private void CreateSubscriptionStatusBlock(StackPanel parentPanel)
        {
            // Создаем блок состояния подписки
            var statusBlock = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                Margin = new Thickness(20, 20, 20, 0),
                CornerRadius = new CornerRadius(8)
            };

            var statusPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            var statusTitle = new TextBlock
            {
                Text = "Состояние подписки плагина",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            statusPanel.Children.Add(statusTitle);

            // Блок для отображения активированного продукта
            var productBlock = new StackPanel
            {
                Name = "ActivatedProductBlock",
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Margin = new Thickness(0, 0, 0, 10),
                MinHeight = 80
            };

            // Загружаем активированный продукт
            LoadActivatedProduct();
            UpdateSubscriptionStatusBlock();

            statusPanel.Children.Add(productBlock);
            statusBlock.Child = statusPanel;
            parentPanel.Children.Add(statusBlock);
        }

        private void UpdateSubscriptionStatusBlock()
        {
            var productBlock = FindName("ActivatedProductBlock") as StackPanel;
            if (productBlock == null) return;

            productBlock.Children.Clear();

            if (_activatedProduct == null)
            {
                // Нет активированного продукта
                var emptyText = new TextBlock
                {
                    Text = "Нет активированного продукта\nНажмите 'Активировать' в списке продуктов выше",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(20)
                };
                productBlock.Children.Add(emptyText);
            }
            else
            {
                // Есть активированный продукт
                var productInfo = new StackPanel
                {
                    Margin = new Thickness(20)
                };

                var productName = new TextBlock
                {
                    Text = $"Активированный продукт: {GetProductDisplayName(_activatedProduct.ProductName)}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var productStatus = new TextBlock
                {
                    Text = $"Статус: {GetStatusDisplayText(_activatedProduct.ActivationStatus)}",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(GetStatusColor(_activatedProduct.ActivationStatus)),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var productExpires = new TextBlock
                {
                    Text = $"Истекает: {_activatedProduct.ExpiresAt?.ToString("dd.MM.yyyy") ?? "Не указано"}",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var deactivateButton = new Button
                {
                    Content = "Деактивировать",
                    Height = 30,
                    Width = 120,
                    Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                deactivateButton.Click += (s, e) => DeactivateProduct();

                productInfo.Children.Add(productName);
                productInfo.Children.Add(productStatus);
                productInfo.Children.Add(productExpires);
                productInfo.Children.Add(deactivateButton);

                productBlock.Children.Add(productInfo);
            }
        }

        private void DeactivateProduct()
        {
            var result = MessageBox.Show("Вы уверены, что хотите деактивировать продукт?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _activatedProduct = null;
                SaveActivatedProduct();
                // UpdateUserStatusIndicator(); // Убрано
                UpdateSubscriptionStatusBlock();
                MessageBox.Show("Продукт деактивирован", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveActivatedProduct()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var civilXPath = Path.Combine(appDataPath, "CivilX", "DataViewer");
                Directory.CreateDirectory(civilXPath);
                var filePath = Path.Combine(civilXPath, "activated_product.json");

                if (_activatedProduct == null)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                else
                {
                    var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(_activatedProduct);
                    File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при сохранении активированного продукта: {ex.Message}");
            }
        }

        private void LoadActivatedProduct()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var civilXPath = Path.Combine(appDataPath, "CivilX", "DataViewer");
                var filePath = Path.Combine(civilXPath, "activated_product.json");

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    _activatedProduct = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ProductInfo>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке активированного продукта: {ex.Message}");
                _activatedProduct = null;
            }
        }

        private void CheckPluginGroupSync()
        {
            try
            {
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    // Если нет токена, плагин недоступен
                    DisablePluginSync();
                    return;
                }

                // Простая проверка - если есть токен, считаем что плагин доступен
                // Более детальная проверка будет в асинхронном методе
                EnablePlugin();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке группы плагина: {ex.Message}");
                DisablePluginSync();
            }
        }

        private async void CheckPluginGroup()
        {
            try
            {
                var token = _authService.GetStoredToken();
                if (string.IsNullOrEmpty(token))
                {
                    // Если нет токена, плагин недоступен
                    DisablePlugin();
                    return;
                }

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    var requestData = new { plugin_group = PLUGIN_GROUP };
                    var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(requestData);
                    var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var response = await client.PostAsync("http://civilx.ru/auth-api.php/api/check-plugin-group", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(responseContent);
                        var hasActiveSubscription = Convert.ToBoolean(result["has_active_subscription"]);
                        
                        if (hasActiveSubscription)
                        {
                            EnablePlugin();
                        }
                        else
                        {
                            DisablePlugin();
                        }
                    }
                    else
                    {
                        DisablePlugin();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке группы плагина: {ex.Message}");
                DisablePlugin();
            }
        }

        private void EnablePlugin()
        {
            // Плагин доступен - можно использовать все функции
            System.Diagnostics.Debug.WriteLine($"Плагин {PLUGIN_GROUP} активирован");
        }

        private void DisablePluginSync()
        {
            // Плагин недоступен - показываем сообщение о необходимости подписки
            MessageBox.Show(
                $"Плагин {PLUGIN_GROUP} недоступен. Необходима активная подписка на группу {PLUGIN_GROUP}.",
                "Подписка неактивна",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            
            // НЕ закрываем окно плагина - пользователь может авторизоваться
            WriteToLogFile("⚠️ Плагин недоступен - требуется авторизация");
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
            
            // НЕ закрываем окно плагина - пользователь может авторизоваться
            WriteToLogFile("⚠️ Плагин недоступен - требуется авторизация");
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow.OnClosed() - НАЧАЛО");
                WriteDetailedLog("🔍 MainWindow.OnClosed() - НАЧАЛО");
                System.Diagnostics.Debug.WriteLine($"🔍 MainWindow.OnClosed() - причина: {e?.ToString() ?? "неизвестно"}");
                WriteDetailedLog($"🔍 MainWindow.OnClosed() - причина: {e?.ToString() ?? "неизвестно"}");
                
                // Логируем стек вызовов
                var stackTrace = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.Debug.WriteLine($"🔍 MainWindow.OnClosed() - StackTrace:");
                WriteDetailedLog($"🔍 MainWindow.OnClosed() - StackTrace:");
                for (int i = 0; i < Math.Min(10, stackTrace.FrameCount); i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var frameInfo = $"  {i}: {frame.GetMethod()?.DeclaringType?.Name}.{frame.GetMethod()?.Name}";
                    System.Diagnostics.Debug.WriteLine(frameInfo);
                    WriteDetailedLog(frameInfo);
                }
                
                base.OnClosed(e);
                System.Diagnostics.Debug.WriteLine("🔍 MainWindow.OnClosed() - ЗАВЕРШЕН");
                WriteDetailedLog("🔍 MainWindow.OnClosed() - ЗАВЕРШЕН");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MainWindow.OnClosed() - ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                WriteDetailedLog($"❌ MainWindow.OnClosed() - ОШИБКА: {ex.Message}");
                WriteDetailedLog($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private void WriteToLogFile(string message)
        {
            try
            {
                string logPath = @"C:\Users\dimag\AppData\Roaming\CivilX\DataViewer\dataviewer_log.txt";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                
                // Создаем директорию если не существует
                string logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        public void WriteDetailedLog(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentLogFilePath))
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] {message}\n";
                    File.AppendAllText(_currentLogFilePath, logEntry, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки логирования, чтобы не прерывать основной процесс
                System.Diagnostics.Debug.WriteLine($"Ошибка записи в детальный лог: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает фильтр для выбранных категорий на уровне Revit API
        /// </summary>
        private Autodesk.Revit.DB.ElementFilter CreateCategoryFilter(List<string> selectedCategories)
        {
            if (selectedCategories == null || selectedCategories.Count == 0)
            {
                return null; // Нет фильтрации
            }

            var categoryFilters = new List<Autodesk.Revit.DB.ElementFilter>();
            var matchedCategories = new List<string>();
            var unmatchedCategories = new List<string>();

            var nonFilterableCategories = new List<string>();

            foreach (var categoryName in selectedCategories)
            {
                // Проверяем, является ли категория нефильтруемой (view-элементы)
                if (IsNonFilterableCategory(categoryName))
                {
                    nonFilterableCategories.Add(categoryName);
                    WriteDetailedLog($"⚠️ Категория '{categoryName}' не поддерживает API-фильтрацию (view-элемент)");
                    continue;
                }

                var builtInCategory = GetBuiltInCategoryByName(categoryName);
                if (builtInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                {
                    var categoryFilter = new Autodesk.Revit.DB.ElementCategoryFilter(builtInCategory);
                    categoryFilters.Add(categoryFilter);
                    matchedCategories.Add(categoryName);
                }
                else
                {
                    unmatchedCategories.Add(categoryName);
                }
            }

            WriteDetailedLog($"🔧 Создание фильтра категорий:");
            WriteDetailedLog($"  • Найдено совпадений с BuiltInCategory: {matchedCategories.Count}");
            WriteDetailedLog($"  • Не найдено совпадений: {unmatchedCategories.Count}");
            WriteDetailedLog($"  • Нефильтруемые категории (view-элементы): {nonFilterableCategories.Count}");

            if (unmatchedCategories.Count > 0)
            {
                WriteDetailedLog($"  • Несовпадающие категории: {string.Join(", ", unmatchedCategories.Take(5))}");
                if (unmatchedCategories.Count > 5)
                {
                    WriteDetailedLog($"  • ... и еще {unmatchedCategories.Count - 5} категорий");
                }
            }

            if (nonFilterableCategories.Count > 0)
            {
                WriteDetailedLog($"  • Нефильтруемые категории: {string.Join(", ", nonFilterableCategories)}");
            }

            if (categoryFilters.Count == 0)
            {
                return null; // Нет валидных категорий для фильтрации
            }
            else if (categoryFilters.Count == 1)
            {
                return categoryFilters[0]; // Один фильтр
            }
            else
            {
                // Несколько фильтров - объединяем через OR
                return new Autodesk.Revit.DB.LogicalOrFilter(categoryFilters);
            }
        }

        /// <summary>
        /// Конвертирует название категории в BuiltInCategory
        /// </summary>
        private Autodesk.Revit.DB.BuiltInCategory GetBuiltInCategoryByName(string categoryName)
        {
            // Словарь соответствий русских названий категорий и BuiltInCategory
            var categoryMappings = new Dictionary<string, Autodesk.Revit.DB.BuiltInCategory>(StringComparer.OrdinalIgnoreCase)
            {
                // Основные категории
                {"Стены", Autodesk.Revit.DB.BuiltInCategory.OST_Walls},
                {"Стена", Autodesk.Revit.DB.BuiltInCategory.OST_Walls},
                {"Walls", Autodesk.Revit.DB.BuiltInCategory.OST_Walls},
                {"Wall", Autodesk.Revit.DB.BuiltInCategory.OST_Walls},
                
                {"Перекрытия", Autodesk.Revit.DB.BuiltInCategory.OST_Floors},
                {"Перекрытие", Autodesk.Revit.DB.BuiltInCategory.OST_Floors},
                {"Floors", Autodesk.Revit.DB.BuiltInCategory.OST_Floors},
                {"Floor", Autodesk.Revit.DB.BuiltInCategory.OST_Floors},
                
                {"Колонны", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns},
                {"Колонна", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns},
                {"Columns", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns},
                {"Column", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns},
                
                {"Балки", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming},
                {"Балка", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming},
                {"Beams", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming},
                {"Beam", Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming},
                
                {"Двери", Autodesk.Revit.DB.BuiltInCategory.OST_Doors},
                {"Дверь", Autodesk.Revit.DB.BuiltInCategory.OST_Doors},
                {"Doors", Autodesk.Revit.DB.BuiltInCategory.OST_Doors},
                {"Door", Autodesk.Revit.DB.BuiltInCategory.OST_Doors},
                
                {"Окна", Autodesk.Revit.DB.BuiltInCategory.OST_Windows},
                {"Окно", Autodesk.Revit.DB.BuiltInCategory.OST_Windows},
                {"Windows", Autodesk.Revit.DB.BuiltInCategory.OST_Windows},
                {"Window", Autodesk.Revit.DB.BuiltInCategory.OST_Windows},
                
                {"Потолки", Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings},
                {"Потолок", Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings},
                {"Ceilings", Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings},
                {"Ceiling", Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings},
                
                {"Крыши", Autodesk.Revit.DB.BuiltInCategory.OST_Roofs},
                {"Крыша", Autodesk.Revit.DB.BuiltInCategory.OST_Roofs},
                {"Roofs", Autodesk.Revit.DB.BuiltInCategory.OST_Roofs},
                {"Roof", Autodesk.Revit.DB.BuiltInCategory.OST_Roofs},
                
                {"Лестницы", Autodesk.Revit.DB.BuiltInCategory.OST_Stairs},
                {"Лестница", Autodesk.Revit.DB.BuiltInCategory.OST_Stairs},
                {"Stairs", Autodesk.Revit.DB.BuiltInCategory.OST_Stairs},
                {"Stair", Autodesk.Revit.DB.BuiltInCategory.OST_Stairs},
                
                {"Пандусы", Autodesk.Revit.DB.BuiltInCategory.OST_Ramps},
                {"Пандус", Autodesk.Revit.DB.BuiltInCategory.OST_Ramps},
                {"Ramps", Autodesk.Revit.DB.BuiltInCategory.OST_Ramps},
                {"Ramp", Autodesk.Revit.DB.BuiltInCategory.OST_Ramps},
                
                {"Мебель", Autodesk.Revit.DB.BuiltInCategory.OST_Furniture},
                {"Furniture", Autodesk.Revit.DB.BuiltInCategory.OST_Furniture},
                
                {"Оборудование", Autodesk.Revit.DB.BuiltInCategory.OST_PlumbingFixtures},
                {"Plumbing Fixtures", Autodesk.Revit.DB.BuiltInCategory.OST_PlumbingFixtures},
                
                {"Освещение", Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixtures},
                {"Lighting Fixtures", Autodesk.Revit.DB.BuiltInCategory.OST_LightingFixtures},
                
                {"Антураж", Autodesk.Revit.DB.BuiltInCategory.OST_Site},
                {"Site", Autodesk.Revit.DB.BuiltInCategory.OST_Site},
                
                {"Пространства", Autodesk.Revit.DB.BuiltInCategory.OST_Rooms},
                {"Пространство", Autodesk.Revit.DB.BuiltInCategory.OST_Rooms},
                {"Rooms", Autodesk.Revit.DB.BuiltInCategory.OST_Rooms},
                {"Room", Autodesk.Revit.DB.BuiltInCategory.OST_Rooms},
                
                {"Зоны", Autodesk.Revit.DB.BuiltInCategory.OST_Areas},
                {"Зона", Autodesk.Revit.DB.BuiltInCategory.OST_Areas},
                {"Areas", Autodesk.Revit.DB.BuiltInCategory.OST_Areas},
                {"Area", Autodesk.Revit.DB.BuiltInCategory.OST_Areas}
            };

            if (categoryMappings.TryGetValue(categoryName, out var builtInCategory))
            {
                return builtInCategory;
            }

            // Если точного совпадения нет, попробуем найти по частичному совпадению
            foreach (var mapping in categoryMappings)
            {
                if (categoryName.IndexOf(mapping.Key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    mapping.Key.IndexOf(categoryName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mapping.Value;
                }
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /// <summary>
        /// Проверяет, является ли категория нефильтруемой через ElementCategoryFilter
        /// </summary>
        private bool IsNonFilterableCategory(string categoryName)
        {
            var nonFilterableCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Листы", "Лист", "Sheets", "Sheet",
                "Линии", "Линия", "Lines", "Line", 
                "Спецификации", "Спецификация", "Schedules", "Schedule",
                "Виды", "Вид", "Views", "View",
                "Аннотации", "Аннотация", "Annotations", "Annotation",
                "Размеры", "Размер", "Dimensions", "Dimension",
                "Текст", "Text", "Тексты", "Texts",
                "Метки", "Метка", "Tags", "Tag",
                "Символы", "Символ", "Symbols", "Symbol"
            };

            return nonFilterableCategories.Contains(categoryName);
        }

        /// <summary>
        /// Безопасно закрывает документ Revit с обработкой всех типов исключений
        /// </summary>
        private void SafeCloseDocument(Document document, string filePath)
        {
            WriteDetailedLog($"🔍 SafeCloseDocument - НАЧАЛО для файла: {Path.GetFileName(filePath)}");
            WriteDetailedLog($"🔍 SafeCloseDocument - Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            
            if (document == null)
            {
                WriteDetailedLog("⚠️ SafeCloseDocument - документ null, пропускаем закрытие");
                return;
            }

            WriteDetailedLog("🔍 SafeCloseDocument - документ не null, проверяем валидность");
            try
            {
                // Проверяем, что документ еще открыт
                WriteDetailedLog("🔍 SafeCloseDocument - проверяем IsValidObject");
                bool isValid = document.IsValidObject;
                WriteDetailedLog($"🔍 SafeCloseDocument - IsValidObject = {isValid}");
                
                if (isValid)
                {
                    WriteDetailedLog("🔍 SafeCloseDocument - документ валиден, пытаемся закрыть");
                    try
                    {
                        // Пытаемся закрыть документ с дополнительной защитой
                        WriteDetailedLog("🔍 SafeCloseDocument - вызываем document.Close(false)");
                        document.Close(false);
                        WriteDetailedLog("✅ SafeCloseDocument - document.Close() выполнен успешно");
                    }
                    catch (System.Runtime.InteropServices.SEHException sehEx)
                    {
                        // Обрабатываем SEHException на уровне вызова Close()
                        WriteDetailedLog($"❌ SafeCloseDocument - SEHException в document.Close(): {sehEx.Message}");
                        WriteDetailedLog($"❌ SafeCloseDocument - SEHException StackTrace: {sehEx.StackTrace}");
                        WriteDetailedLog("⚠️ SafeCloseDocument - SEHException в document.Close(), но продолжаем");
                        // Не перебрасываем исключение - просто логируем
                    }
                    catch (Exception ex)
                    {
                        // Обрабатываем другие исключения
                        WriteDetailedLog($"❌ SafeCloseDocument - Exception в document.Close(): {ex.Message}");
                        WriteDetailedLog($"❌ SafeCloseDocument - Exception StackTrace: {ex.StackTrace}");
                        WriteDetailedLog("⚠️ SafeCloseDocument - Exception в document.Close(), но продолжаем");
                    }
                    WriteDetailedLog("✅ SafeCloseDocument - попытка закрытия завершена");
                }
                else
                {
                    WriteDetailedLog("⚠️ SafeCloseDocument - документ уже закрыт или недействителен");
                }
            }
            catch (System.Runtime.InteropServices.SEHException sehEx)
            {
                // Обрабатываем SEHException (внешний компонент создал исключение)
                WriteDetailedLog($"❌ SafeCloseDocument - SEHException: {sehEx.Message}");
                WriteDetailedLog($"❌ SafeCloseDocument - SEHException StackTrace: {sehEx.StackTrace}");
                WriteDetailedLog($"⚠️ SEHException при закрытии документа: {sehEx.Message}");
                WriteToLogFile($"⚠️ SEHException при закрытии документа {Path.GetFileName(filePath)}: {sehEx.Message}");
                WriteToLogFile($"⚠️ Это системное исключение, которое может возникать при работе с COM-объектами Revit");
                WriteToLogFile($"⚠️ Исключение проигнорировано для продолжения работы");
                WriteDetailedLog("🔍 SafeCloseDocument - SEHException обработан, продолжаем");
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                // Обрабатываем COMException
                WriteDetailedLog($"⚠️ COMException при закрытии документа: {comEx.Message}");
                WriteToLogFile($"⚠️ COMException при закрытии документа {Path.GetFileName(filePath)}: {comEx.Message}");
                WriteToLogFile($"⚠️ Исключение проигнорировано для продолжения работы");
            }
            catch (Exception ex)
            {
                // Обрабатываем все остальные исключения
                WriteDetailedLog($"⚠️ Ошибка при закрытии документа: {ex.Message}");
                WriteToLogFile($"⚠️ Ошибка при закрытии документа {Path.GetFileName(filePath)}: {ex.Message}");
                WriteToLogFile($"⚠️ Тип исключения: {ex.GetType().Name}");
                WriteToLogFile($"⚠️ Исключение проигнорировано для продолжения работы");
                WriteDetailedLog("🔍 SafeCloseDocument - общее исключение обработано, продолжаем");
            }
            
            WriteDetailedLog($"🔍 SafeCloseDocument - ЗАВЕРШЕНИЕ для файла: {Path.GetFileName(filePath)}");
        }

        private bool CanReadFile(string filePath)
        {
            try
            {
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // ExternalEvent для безопасного открытия файлов
        private Autodesk.Revit.UI.ExternalEvent _openDocumentEvent;
        private OpenDocumentRequest _openDocumentRequest;

        private void InitializeExternalEvent()
        {
            _openDocumentRequest = new OpenDocumentRequest();
            _openDocumentEvent = Autodesk.Revit.UI.ExternalEvent.Create(_openDocumentRequest);
        }

        private Autodesk.Revit.DB.Document OpenDocumentSafely(string filePath)
        {
            try
            {
                WriteToLogFile($"Безопасное открытие файла: {filePath}");
                
                // Инициализируем ExternalEvent если не инициализирован
                if (_openDocumentEvent == null)
                {
                    InitializeExternalEvent();
                }

                // Устанавливаем путь к файлу
                _openDocumentRequest.SetFilePath(filePath);
                
                // Выполняем открытие через ExternalEvent
                var result = _openDocumentEvent.Raise();
                WriteToLogFile($"ExternalEvent.Raise() результат: {result}");
                
                // Ждем завершения выполнения (ExternalEvent асинхронный)
                System.Threading.Thread.Sleep(5000); // Увеличиваем время ожидания до 5 секунд
                
                // Проверяем результат
                var document = _openDocumentRequest.GetDocument();
                var exception = _openDocumentRequest.GetException();
                
                if (document != null)
                {
                    WriteToLogFile($"✓ Файл успешно открыт через ExternalEvent: {document.Title}");
                    return document;
                }
                else if (exception != null)
                {
                    WriteToLogFile($"❌ Ошибка в ExternalEvent: {exception.Message}");
                    throw new Exception($"Ошибка открытия файла через ExternalEvent: {exception.Message}", exception);
                }
                else
                {
                    WriteToLogFile($"❌ ExternalEvent выполнился, но документ не открыт");
                    return null;
                }
            }
            catch (Exception ex)
            {
                WriteToLogFile($"❌ Ошибка безопасного открытия файла: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Применяет фильтры категорий к уже извлеченным элементам
        /// </summary>
        /// <param name="allElementsData">Все извлеченные элементы</param>
        /// <param name="selectedCategories">Выбранные категории для фильтрации</param>
        /// <returns>Отфильтрованные элементы</returns>
        private List<Models.RevitElementData> ApplyCategoryFilters(List<Models.RevitElementData> allElementsData, List<string> selectedCategories)
        {
            try
            {
                WriteToLogFile($"=== ПРИМЕНЕНИЕ ФИЛЬТРОВ КАТЕГОРИЙ ===");
                WriteToLogFile($"Всего элементов до фильтрации: {allElementsData.Count}");
                WriteToLogFile($"Выбранных категорий: {selectedCategories.Count}");
                
                if (selectedCategories.Count == 0)
                {
                    WriteToLogFile("⚠️ Нет выбранных категорий, возвращаем все элементы");
                    return allElementsData;
                }

                var filteredElements = new List<Models.RevitElementData>();
                
                foreach (var elementData in allElementsData)
                {
                    // Проверяем, есть ли категория элемента в списке выбранных
                    if (selectedCategories.Contains(elementData.Category))
                    {
                        filteredElements.Add(elementData);
                    }
                }
                
                WriteToLogFile($"Элементов после фильтрации: {filteredElements.Count}");
                WriteToLogFile($"Процент отфильтрованных: {((double)(allElementsData.Count - filteredElements.Count) / allElementsData.Count * 100):F1}%");
                
                return filteredElements;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"❌ Ошибка применения фильтров: {ex.Message}");
                // В случае ошибки возвращаем все элементы
                return allElementsData;
            }
        }

    }

    // Класс для безопасного открытия документов через ExternalEvent
    public class OpenDocumentRequest : Autodesk.Revit.UI.IExternalEventHandler
    {
        private string _filePath;
        private Autodesk.Revit.DB.Document _document;
        private Exception _exception;

        public void SetFilePath(string filePath)
        {
            _filePath = filePath;
            _document = null;
            _exception = null;
        }

        public Autodesk.Revit.DB.Document GetDocument()
        {
            return _document;
        }

        public Exception GetException()
        {
            return _exception;
        }

        public void Execute(Autodesk.Revit.UI.UIApplication app)
        {
            try
            {
                // Получаем приложение Revit
                var revitApp = app.Application;
                
                // Проверяем, не открыт ли уже этот файл
                var existingDoc = revitApp.Documents.Cast<Autodesk.Revit.DB.Document>()
                    .FirstOrDefault(d => d.PathName.Equals(_filePath, StringComparison.OrdinalIgnoreCase));
                
                if (existingDoc != null)
                {
                    _document = existingDoc;
                    return;
                }

                // Определяем тип модели и выбираем оптимальную стратегию открытия
                _document = OpenModelWithOptimization(revitApp, _filePath);
                
                if (_document == null)
                {
                    throw new Exception("OpenDocumentFile вернул null");
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
                _document = null;
            }
        }

        private Autodesk.Revit.DB.Document OpenModelWithOptimization(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // НОВАЯ СТРАТЕГИЯ: Сначала пробуем максимально агрессивные настройки
                try
                {
                    return OpenWithMaximumOptimization(revitApp, filePath);
                }
                catch (Exception ex)
                {
                    // Fallback к ультра оптимизации
                    try
                    {
                        return OpenWithUltraOptimization(revitApp, filePath);
                    }
                    catch (Exception ex2)
                    {
                        // Fallback к стандартной логике
                        return OpenWithStandardLogic(revitApp, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка оптимизированного открытия модели: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithMaximumOptimization(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // МАКСИМАЛЬНО АГРЕССИВНЫЕ НАСТРОЙКИ
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.Audit = false; // Отключаем аудит
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // Закрываем ВСЕ рабочие наборы для максимальной скорости
                var worksetConfig = new Autodesk.Revit.DB.WorksetConfiguration(Autodesk.Revit.DB.WorksetConfigurationOption.CloseAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(worksetConfig);
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
                
                throw new Exception("Не удалось открыть с максимальными настройками");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка максимальной оптимизации: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithUltraOptimization(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // УЛЬТРА АГРЕССИВНЫЕ НАСТРОЙКИ - только для критических случаев
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.Audit = false; // Отключаем аудит
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // Закрываем ВСЕ рабочие наборы для максимальной скорости
                var worksetConfig = new Autodesk.Revit.DB.WorksetConfiguration(Autodesk.Revit.DB.WorksetConfigurationOption.CloseAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(worksetConfig);
                
                // Дополнительные настройки для максимальной скорости
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
                
                throw new Exception("Не удалось открыть с ультра настройками");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка ультра оптимизации: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithExtremeOptimization(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // ЭКСТРЕМАЛЬНЫЕ НАСТРОЙКИ - для самых сложных случаев
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.Audit = false; // Отключаем аудит
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // Закрываем ВСЕ рабочие наборы для максимальной скорости
                var worksetConfig = new Autodesk.Revit.DB.WorksetConfiguration(Autodesk.Revit.DB.WorksetConfigurationOption.CloseAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(worksetConfig);
                
                // Дополнительные настройки для максимальной скорости
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
                
                throw new Exception("Не удалось открыть с экстремальными настройками");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка экстремальной оптимизации: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithStandardLogic(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // Сначала пробуем быстрое открытие для определения типа модели
                Autodesk.Revit.DB.Document tempDoc = null;
                bool isWorkshared = false;
                
                try
                {
                    // Быстрое открытие для проверки типа
                    tempDoc = revitApp.OpenDocumentFile(filePath);
                    if (tempDoc != null)
                    {
                        isWorkshared = tempDoc.IsWorkshared;
                        
                        // Если это хранилище, закрываем и открываем с оптимизацией
                        if (isWorkshared)
                        {
                            tempDoc.Close(false);
                            return OpenWorksharedModel(revitApp, filePath);
                        }
                        else
                        {
                            // Локальная модель - возвращаем как есть
                            return tempDoc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Если быстрое открытие не сработало, пробуем с OpenOptions
                    if (tempDoc != null)
                    {
                        tempDoc.Close(false);
                    }
                    return OpenWithFallbackOptions(revitApp, filePath);
                }
                
                return tempDoc;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка стандартной логики: {ex.Message}", ex);
            }
        }

        private Autodesk.Revit.DB.Document OpenWorksharedModel(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // АГРЕССИВНАЯ ОПТИМИЗАЦИЯ для хранилища
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DetachAndPreserveWorksets;
                openOptions.Audit = false; // Отключаем аудит
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // МАКСИМАЛЬНАЯ ОПТИМИЗАЦИЯ рабочих наборов
                var worksetConfig = new Autodesk.Revit.DB.WorksetConfiguration(Autodesk.Revit.DB.WorksetConfigurationOption.CloseAllWorksets);
                openOptions.SetOpenWorksetsConfiguration(worksetConfig);
                
                // Дополнительные оптимизации
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
                
                // Fallback: пробуем без отсоединения
                return OpenWithFallbackOptions(revitApp, filePath);
            }
            catch (Exception ex)
            {
                // Fallback к стандартному открытию
                return OpenWithFallbackOptions(revitApp, filePath);
            }
        }

        private Autodesk.Revit.DB.Document OpenWithFallbackOptions(Autodesk.Revit.ApplicationServices.Application revitApp, string filePath)
        {
            try
            {
                // МАКСИМАЛЬНАЯ ОПТИМИЗАЦИЯ для локальной модели
                var openOptions = new Autodesk.Revit.DB.OpenOptions();
                openOptions.DetachFromCentralOption = Autodesk.Revit.DB.DetachFromCentralOption.DoNotDetach;
                openOptions.Audit = false; // Отключаем аудит
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                // Дополнительные оптимизации
                openOptions.AllowOpeningLocalByWrongUser = true;
                
                var modelPath = Autodesk.Revit.DB.ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                var document = revitApp.OpenDocumentFile(modelPath, openOptions);
                
                if (document != null)
                {
                    return document;
                }
            }
            catch (Exception optionsEx)
            {
                try
                {
                    // Способ 2: Простое открытие
                    var document = revitApp.OpenDocumentFile(filePath);
                    if (document != null)
                    {
                        return document;
                    }
                }
                catch (Exception simpleEx)
                {
                    throw new Exception($"Все способы открытия файла не сработали. OpenOptions: {optionsEx.Message}, Простой: {simpleEx.Message}");
                }
            }
            
            return null;
        }

        public string GetName()
        {
            return "OpenDocumentRequest";
        }
    }

    // Классы для работы с продуктами
}