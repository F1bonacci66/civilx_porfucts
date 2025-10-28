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
        private string _currentProjectName;
        private string _currentExportName;

        public MainWindow(RevitApp revitApp = null)
        {
            InitializeComponent();
            _dataService = new DataService();
            _projects = new List<Project>();
            LoadProjects();
            ShowProjectsScreen();
        }

        private void LoadProjects()
        {
            try
            {
                var projects = _dataService.LoadProjectsAsync();
                _projects = projects ?? new List<Project>();
            }
            catch
            {
                _projects = new List<Project>();
            }

            // Если проектов нет, создаем тестовые
            if (_projects.Count == 0)
            {
                _projects.Add(new Project { Name = "Проект 1" });
                _projects.Add(new Project { Name = "Проект 2" });
                _projects.Add(new Project { Name = "Проект 3" });
            }
        }

        private void SaveProjects()
        {
            try
            {
                _dataService.SaveProjectsAsync(_projects);
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
                    Height = 30,
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
            
            // Кнопка добавления проекта
            var addButton = new Button
            {
                Content = "➕ Добавить проект",
                Height = 50,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14
            };
            addButton.Click += AddProject_Click;
            stackPanel.Children.Add(addButton);
            
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
                    exportsList.Children.Add(exportButton);
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
                    // Если у выгрузки нет вкладок, создаем первую
                    if (export.Tabs.Count == 0)
                    {
                        var firstTab = new ExportTab
                        {
                            Name = "Вкладка 1",
                            RevitVersion = "2023"
                        };
                        export.Tabs.Add(firstTab);
                        SaveProjects();
                    }
                    
                    // Показываем первую вкладку
                    var firstTabToShow = export.Tabs.FirstOrDefault();
                    if (firstTabToShow != null)
                    {
                        var rightPanel = CreateTabSettingsRightPanel(projectName, exportName, firstTabToShow.Name);
                        Grid.SetColumn(rightPanel, 2);
                        grid.Children.RemoveAt(2); // Удаляем старую правую панель
                        grid.Children.Add(rightPanel); // Добавляем новую
                    }
                }
            }
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
            
            // Примеры параметров
            var sampleParameters = new[]
            {
                "📐 Площадь",
                "📏 Длина",
                "🏗️ Тип элемента",
                "📍 Уровень",
                "🏢 Имя семейства",
                "📝 Комментарии",
                "🔢 Количество",
                "💰 Стоимость"
            };
            
            foreach (var param in sampleParameters)
            {
                var paramBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15, 10, 15, 10),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                
                var paramGrid = new Grid();
                paramGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                paramGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                
                var paramText = new TextBlock
                {
                    Text = param,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(paramText, 0);
                paramGrid.Children.Add(paramText);
                
                var checkBox = new CheckBox
                {
                    IsChecked = true,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(checkBox, 1);
                paramGrid.Children.Add(checkBox);
                
                paramBorder.Child = paramGrid;
                parametersList.Children.Add(paramBorder);
            }
            
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
            var addProjectWindow = new AddProjectWindow();
            if (addProjectWindow.ShowDialog() == true)
            {
                var newProject = new Project
                {
                    Name = addProjectWindow.ProjectName
                };

                _projects.Add(newProject);
                SaveProjects();
                ShowProjectsScreen();
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
                    var newExport = new Export
                    {
                        Name = addExportWindow.ExportName
                    };

                    project.Exports.Add(newExport);
                    SaveProjects();
                    ShowProjectScreen(projectName);
                }
            }
        }

        private void AddTab_Click(string projectName, string exportName)
        {
            var project = _projects.Find(p => p.Name == projectName);
            var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
            if (export != null)
            {
                var tabNumber = export.Tabs.Count + 1;
                var newTab = new ExportTab
                {
                    Name = $"Вкладка {tabNumber}",
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
                var tabNumber = export.Tabs.Count + 1;
                var newTab = new ExportTab
                {
                    Name = $"Вкладка {tabNumber}",
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

        private void RenameProject(string projectName)
        {
            var renameWindow = new RenameWindow(projectName);
            if (renameWindow.ShowDialog() == true)
            {
                var project = _projects.Find(p => p.Name == projectName);
                if (project != null && !string.IsNullOrWhiteSpace(renameWindow.NewName))
                {
                    project.Name = renameWindow.NewName;
                    SaveProjects();
                    ShowProjectsScreen();
                }
            }
        }

        private void DeleteProject(string projectName)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить проект '{projectName}'?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                _projects.RemoveAll(p => p.Name == projectName);
                SaveProjects();
                ShowProjectsScreen();
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
                    tab.Name = renameWindow.NewName;
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
            }
        }

        private void DeleteTab(string projectName, string exportName, string tabName)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить вкладку '{tabName}'?\n\nЭто действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result == MessageBoxResult.Yes)
            {
                var project = _projects.Find(p => p.Name == projectName);
                var export = project?.Exports.FirstOrDefault(e => e.Name == exportName);
                
                if (export != null)
                {
                    var tabToRemove = export.Tabs.FirstOrDefault(t => t.Name == tabName);
                    if (tabToRemove != null)
                    {
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
        }

    }
}