using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Views
{
    public partial class ExportView : UserControl
    {
        private Export _export;
        private ProjectView _projectView;
        private ExportTab _currentTab;

        public ExportView(Export export, ProjectView projectView)
        {
            InitializeComponent();
            _export = export;
            _projectView = projectView;
            
            RefreshTabs();
            
            // Если вкладок нет, создаем первую
            if (_export.Tabs.Count == 0)
            {
                CreateDefaultTab();
            }
            else
            {
                SelectTab(_export.Tabs[0]);
            }
        }

        private void RefreshTabs()
        {
            TabsControl.Items.Clear();

            foreach (var tab in _export.Tabs)
            {
                var tabItem = CreateTabItem(tab);
                TabsControl.Items.Add(tabItem);
            }
        }

        private TabItem CreateTabItem(ExportTab tab)
        {
            var tabItem = new TabItem
            {
                Header = CreateTabHeader(tab),
                Content = CreateTabContent(tab)
            };

            // Контекстное меню для вкладки
            tabItem.ContextMenu = CreateTabContextMenu(tab);

            return tabItem;
        }

        private Grid CreateTabHeader(ExportTab tab)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = tab.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var closeButton = new Button
            {
                Content = "×",
                FontSize = 16,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => CloseTab(tab);
            Grid.SetColumn(closeButton, 1);
            grid.Children.Add(closeButton);

            return grid;
        }

        private UserControl CreateTabContent(ExportTab tab)
        {
            WriteToLogFile($"ExportView: Creating TabContentView for tab '{tab.Name}'");
            try
            {
                var tabContent = new TabContentView(tab, this);
                WriteToLogFile($"ExportView: TabContentView created successfully for tab '{tab.Name}'");
                return tabContent;
            }
            catch (Exception ex)
            {
                WriteToLogFile($"ExportView: Exception creating TabContentView for tab '{tab.Name}': {ex.Message}");
                WriteToLogFile($"ExportView: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private ContextMenu CreateTabContextMenu(ExportTab tab)
        {
            var contextMenu = new ContextMenu();

            var renameItem = new MenuItem { Header = "Переименовать" };
            renameItem.Click += (s, e) => RenameTab(tab);
            contextMenu.Items.Add(renameItem);

            var deleteItem = new MenuItem { Header = "Удалить" };
            deleteItem.Click += (s, e) => DeleteTab(tab);
            contextMenu.Items.Add(deleteItem);

            return contextMenu;
        }

        private void SelectTab(ExportTab tab)
        {
            _currentTab = tab;
            ShowTabContent(tab);
        }

        private void ShowTabContent(ExportTab tab)
        {
            WriteToLogFile($"ExportView: ShowTabContent called for tab '{tab.Name}'");
            TabContentArea.Children.Clear();
            try
            {
                var tabContentView = new TabContentView(tab, this);
                TabContentArea.Children.Add(tabContentView);
                WriteToLogFile($"ExportView: TabContentView added to UI for tab '{tab.Name}'");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"ExportView: Exception in ShowTabContent for tab '{tab.Name}': {ex.Message}");
                WriteToLogFile($"ExportView: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void RenameTab(ExportTab tab)
        {
            var inputDialog = new InputDialog("Переименовать вкладку", "Введите новое название вкладки:", tab.Name);
            if (inputDialog.ShowDialog() == true)
            {
                var newName = inputDialog.Answer;
                if (!string.IsNullOrEmpty(newName) && newName != tab.Name)
                {
                    tab.Name = newName;
                    RefreshTabs();
                }
            }
        }

        private void DeleteTab(ExportTab tab)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить вкладку '{tab.Name}'?", 
                "Подтверждение удаления", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _export.Tabs.Remove(tab);
                RefreshTabs();
                
                if (_currentTab == tab)
                {
                    TabContentArea.Children.Clear();
                    _currentTab = null;
                }
            }
        }

        private void CloseTab(ExportTab tab)
        {
            DeleteTab(tab);
        }

        private void CreateDefaultTab()
        {
            var defaultTab = new ExportTab
            {
                Name = "Вкладка 1",
                RevitVersion = "2023"
            };

            _export.Tabs.Add(defaultTab);
            RefreshTabs();
            SelectTab(defaultTab);
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            WriteToLogFile($"ExportView: AddTabButton_Click - creating new tab");
            try
            {
                var tabNumber = _export.Tabs.Count + 1;
                var newTab = new ExportTab
                {
                    Name = $"Вкладка {tabNumber}",
                    RevitVersion = "2023"
                };

                WriteToLogFile($"ExportView: New tab created: {newTab.Name}");
                _export.Tabs.Add(newTab);
                WriteToLogFile($"ExportView: Tab added to collection, total tabs: {_export.Tabs.Count}");
                
                RefreshTabs();
                WriteToLogFile($"ExportView: Tabs refreshed");
                
                SelectTab(newTab);
                WriteToLogFile($"ExportView: Tab selected: {newTab.Name}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"ExportView: Exception in AddTabButton_Click: {ex.Message}");
                WriteToLogFile($"ExportView: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public string GetProjectPath()
        {
            return _projectView.GetProjectPath();
        }

        public string GetExportName()
        {
            return _export.Name;
        }

        private List<string> _selectedCategories = new List<string>();

        /// <summary>
        /// Устанавливает выбранные категории для фильтрации экспорта
        /// </summary>
        public void SetCategoryFilters(List<string> selectedCategories)
        {
            _selectedCategories = selectedCategories ?? new List<string>();
            WriteToLogFile($"ExportView: SetCategoryFilters called with {_selectedCategories.Count} categories");
        }

        /// <summary>
        /// Запускает экспорт с учетом выбранных фильтров
        /// </summary>
        public void StartExport()
        {
            WriteToLogFile($"ExportView: StartExport called with {_selectedCategories.Count} selected categories");
            
            if (_currentTab == null)
            {
                WriteToLogFile("ExportView: No current tab selected");
                MessageBox.Show("Выберите вкладку для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, что папка вкладки существует (в ней должна быть папка "Модели")
            if (string.IsNullOrEmpty(_currentTab.FolderPath))
            {
                WriteToLogFile("ExportView: No tab folder path");
                MessageBox.Show("Папка вкладки не определена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var modelsFolder = Path.Combine(_currentTab.FolderPath, "Модели");
            if (!Directory.Exists(modelsFolder))
            {
                WriteToLogFile($"ExportView: Models folder does not exist: {modelsFolder}");
                MessageBox.Show($"Папка 'Модели' не найдена в папке вкладки:\n{modelsFolder}\n\nСоздайте папку 'Модели' и поместите в неё Revit файлы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Передаем управление в MainWindow для выполнения экспорта
            var mainWindow = Window.GetWindow(this) as Views.MainWindow;
            if (mainWindow != null)
            {
                WriteToLogFile("ExportView: Delegating export to MainWindow");
                mainWindow.StartExportWithFilters(_currentTab, _selectedCategories);
            }
            else
            {
                WriteToLogFile("ExportView: MainWindow not found");
                MessageBox.Show("Не удалось найти главное окно для выполнения экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}


