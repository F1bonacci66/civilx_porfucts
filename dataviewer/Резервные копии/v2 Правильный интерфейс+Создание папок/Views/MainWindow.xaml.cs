using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RevitExporterAddin.Models;
using RevitExporterAddin.Services;
using RevitApp = Autodesk.Revit.ApplicationServices.Application;

namespace RevitExporterAddin.Views
{
    public partial class MainWindow : Window
    {
        private List<Project> _projects;
        private readonly IDataService _dataService;
        private readonly ProjectPersistenceService _persistenceService;
        private string _currentProjectName;
        private string _currentExportName;

        public MainWindow(RevitApp revitApp = null)
        {
            InitializeComponent();
            _dataService = new DataService();
            _persistenceService = new ProjectPersistenceService();
            _projects = new List<Project>();
            LoadProjects();
            ShowProjectsScreen();
        }

        private void LoadProjects()
        {
            try
            {
                // Загружаем проекты из сохраненных путей
                _projects = _persistenceService.LoadProjects();
            }
            catch
            {
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

        public void ShowProjectsScreen()
        {
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
            parametersStack.Children.Add(exportButton);
            
            contentPanel.Content = parametersStack;
            Grid.SetRow(contentPanel, 2);
            grid.Children.Add(contentPanel);
            
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
            var createProjectWindow = new CreateProjectWindow();
            if (createProjectWindow.ShowDialog() == true)
            {
                var newProject = new Project
                {
                    Name = createProjectWindow.ProjectName,
                    FolderPath = createProjectWindow.ProjectPath
                };

                _projects.Add(newProject);
                SaveProjects();
                ShowProjectsScreen();
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

                // Сканируем папки выгрузок
                if (Directory.Exists(projectPath))
                {
                    var exportFolders = Directory.GetDirectories(projectPath);
                    
                    foreach (var exportFolder in exportFolders)
                    {
                        var exportName = Path.GetFileName(exportFolder);
                        var export = new Export
                        {
                            Name = exportName,
                            FolderPath = exportFolder
                        };

                        // Сканируем папки вкладок внутри выгрузки
                        var tabFolders = Directory.GetDirectories(exportFolder);
                        foreach (var tabFolder in tabFolders)
                        {
                            var tabName = Path.GetFileName(tabFolder);
                            var tab = new ExportTab
                            {
                                Name = tabName,
                                FolderPath = tabFolder,
                                RevitVersion = "2023" // По умолчанию
                            };

                            export.Tabs.Add(tab);
                        }

                        project.Exports.Add(export);
                    }
                }

                return project;
            }
            catch (Exception ex)
            {
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
                    var projectPath = Path.Combine("C:\\RevitExporter", projectName);
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

                    // Копируем все .rvt файлы с сохранением иерархии
                    CopyRvtFilesRecursively(dialog.SelectedPath, modelsPath);

                    MessageBox.Show($"Модели успешно скопированы в папку:\n{modelsPath}", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var projectPath = Path.Combine("C:\\RevitExporter", projectName);
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

        private void CopyRvtFilesRecursively(string sourceDir, string targetDir)
        {
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
            }

            // Рекурсивно обрабатываем подпапки
            var subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                var dirName = Path.GetFileName(subDir);
                var targetSubDir = Path.Combine(targetDir, dirName);
                CopyRvtFilesRecursively(subDir, targetSubDir);
            }
        }

    }
}