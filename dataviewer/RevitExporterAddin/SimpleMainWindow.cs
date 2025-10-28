using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using RevitDB = Autodesk.Revit.DB;
using RevitExporterAddin.Services;

namespace RevitExporterAddin
{
    public class SimpleMainWindow : Window
    {
        private LogWindow _logWindow;
        private readonly Autodesk.Revit.ApplicationServices.Application _revitApp;
        private readonly RevitDataReader _dataReader;
        private readonly RevitExporter _exporter;
        
        private string _currentProjectName = "Тест проект";
        private string _currentExportName = "Тест выгрузка";
        private string _currentTabName = "";
        
        // UI элементы для обновления
        private StackPanel _tabsPanel;
        private List<Button> _tabButtons = new List<Button>();
        private List<string> _tabNames = new List<string>();

        public SimpleMainWindow(Autodesk.Revit.ApplicationServices.Application revitApp)
        {
            Title = "Revit Exporter - Управление проектами";
            Width = 1200;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            
            _revitApp = revitApp;
            _dataReader = new RevitDataReader();
            _exporter = new RevitExporter();
            
            _logWindow = new LogWindow();
            _logWindow.Show();
            
            Log("=== SimpleMainWindow конструктор ===");
            Log(string.Format("✅ Revit приложение загружено: {0}", _revitApp.VersionName));
            
            CreateMainUI();
            
            Log("=== SimpleMainWindow конструктор завершен ===");
        }

        private void CreateMainUI()
        {
            try
            {
                Log("Создаем основной UI управления проектами");
                
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Заголовок
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Кнопки управления
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Вкладки
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Содержимое
                
                // Заголовок
                var title = new TextBlock
                {
                    Text = "Revit Exporter - Управление проектами",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(30, 30, 30, 20),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetRow(title, 0);
                grid.Children.Add(title);
                
                // Панель кнопок управления
                var controlPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(30, 0, 30, 20),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetRow(controlPanel, 1);
                
                var addProjectButton = new Button
                {
                    Content = "➕ Добавить проект",
                    Height = 40,
                    Width = 150,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14
                };
                addProjectButton.Click += (s, e) => AddNewProject();
                controlPanel.Children.Add(addProjectButton);
                
                var exportButton = new Button
                {
                    Content = "📤 Экспорт данных",
                    Height = 40,
                    Width = 150,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14
                };
                exportButton.Click += (s, e) => StartExport();
                controlPanel.Children.Add(exportButton);
                
                grid.Children.Add(controlPanel);
                
                // Панель вкладок
                _tabsPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(30, 0, 30, 20),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetRow(_tabsPanel, 2);
                
                // Инициализируем тестовые вкладки
                _tabNames = new List<string> { "10.06", "15.07", "20.08" };
                RefreshTabs();
                
                grid.Children.Add(_tabsPanel);
                
                // Содержимое
                var contentBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(30, 0, 30, 30)
                };
                Grid.SetRow(contentBorder, 3);
                
                var contentText = new TextBlock
                {
                    Text = "Добро пожаловать в Revit Exporter!\n\n" +
                           "• Нажмите 'Добавить проект' для создания нового проекта\n" +
                           "• Используйте ПКМ на вкладках для переименования или удаления\n" +
                           "• Нажмите 'Экспорт данных' для выгрузки данных из Revit",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
                };
                contentBorder.Child = contentText;
                grid.Children.Add(contentBorder);
                
                Content = grid;
                Log("Основной UI создан успешно");
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при создании UI: {0}", ex.Message));
                MessageBox.Show(string.Format("Ошибка при создании UI: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private Button CreateTabButtonWithContextMenu(string tabName)
        {
            try
            {
                Log(string.Format("Создаем кнопку вкладки с контекстным меню: {0}", tabName));
                
                var tabButton = new Button
                {
                    Content = string.Format("📋 {0}", tabName),
                    Height = 40,
                    Width = 80,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    FontSize = 14
                };
                
                // Обработчик левой кнопки мыши
                tabButton.Click += (s, e) => 
                {
                    Log(string.Format("Левый клик по вкладке: {0}", tabName));
                    _currentTabName = tabName;
                };
                
                // Создаем контекстное меню
                var contextMenu = new ContextMenu();
                
                // Пункт "Переименовать"
                var renameMenuItem = new MenuItem
                {
                    Header = "✏️ Переименовать",
                    FontSize = 12
                };
                renameMenuItem.Click += (s, e) => 
                {
                    Log(string.Format("Выбрано 'Переименовать' для вкладки: {0}", tabName));
                    RenameTab(tabName);
                };
                contextMenu.Items.Add(renameMenuItem);
                
                // Пункт "Удалить"
                var deleteMenuItem = new MenuItem
                {
                    Header = "🗑️ Удалить",
                    FontSize = 12
                };
                deleteMenuItem.Click += (s, e) => 
                {
                    Log(string.Format("Выбрано 'Удалить' для вкладки: {0}", tabName));
                    DeleteTab(tabName);
                };
                contextMenu.Items.Add(deleteMenuItem);
                
                // Привязываем контекстное меню к кнопке
                tabButton.ContextMenu = contextMenu;
                
                // Обработчик правой кнопки мыши - принудительное открытие меню
                tabButton.MouseRightButtonDown += (s, e) =>
                {
                    Log(string.Format("Правый клик по вкладке: {0}", tabName));
                    e.Handled = true;
                    contextMenu.PlacementTarget = tabButton;
                    contextMenu.IsOpen = true;
                };
                
                // Дополнительный обработчик для PreviewMouseRightButtonDown
                tabButton.PreviewMouseRightButtonDown += (s, e) =>
                {
                    Log(string.Format("Preview правый клик по вкладке: {0}", tabName));
                    e.Handled = true;
                    contextMenu.PlacementTarget = tabButton;
                    contextMenu.IsOpen = true;
                };
                
                Log(string.Format("Кнопка вкладки с контекстным меню создана: {0}", tabName));
                return tabButton;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при создании кнопки вкладки {0}: {1}", tabName, ex.Message));
                return new Button { Content = tabName, Height = 40, Width = 80 };
            }
        }
        
        private void RenameTab(string oldTabName)
        {
            try
            {
                Log(string.Format("RenameTab вызван для: {0}", oldTabName));
                
                var inputBox = Microsoft.VisualBasic.Interaction.InputBox(
                    string.Format("Введите новое название для вкладки '{0}':", oldTabName),
                    "Переименование вкладки",
                    oldTabName
                );
                
                if (!string.IsNullOrWhiteSpace(inputBox) && inputBox != oldTabName)
                {
                    Log(string.Format("Переименование {0} → {1}", oldTabName, inputBox));
                    
                    // Обновляем список названий вкладок
                    int index = _tabNames.IndexOf(oldTabName);
                    if (index >= 0)
                    {
                        _tabNames[index] = inputBox;
                        RefreshTabs(); // Обновляем UI
                        Log(string.Format("UI обновлен: {0} → {1}", oldTabName, inputBox));
                    }
                    
                    MessageBox.Show(string.Format("Вкладка '{0}' переименована в '{1}'", oldTabName, inputBox), "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Log("Переименование отменено");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при переименовании вкладки: {0}", ex.Message));
                MessageBox.Show(string.Format("Ошибка: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DeleteTab(string tabName)
        {
            try
            {
                Log(string.Format("DeleteTab вызван для: {0}", tabName));
                
                var result = MessageBox.Show(
                    string.Format("Вы уверены, что хотите удалить вкладку '{0}'?", tabName),
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                
                if (result == MessageBoxResult.Yes)
                {
                    Log(string.Format("Удаление вкладки подтверждено: {0}", tabName));
                    
                    // Удаляем вкладку из списка
                    _tabNames.Remove(tabName);
                    RefreshTabs(); // Обновляем UI
                    Log(string.Format("UI обновлен: вкладка '{0}' удалена", tabName));
                    
                    MessageBox.Show(string.Format("Вкладка '{0}' удалена", tabName), "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Log("Удаление отменено");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при удалении вкладки: {0}", ex.Message));
                MessageBox.Show(string.Format("Ошибка: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RefreshTabs()
        {
            try
            {
                Log("Обновляем панель вкладок");
                
                // Очищаем панель вкладок
                _tabsPanel.Children.Clear();
                _tabButtons.Clear();
                
                // Создаем новые кнопки вкладок
                foreach (var tabName in _tabNames)
                {
                    var tabButton = CreateTabButtonWithContextMenu(tabName);
                    _tabsPanel.Children.Add(tabButton);
                    _tabButtons.Add(tabButton);
                    Log(string.Format("Создана кнопка вкладки: {0}", tabName));
                }
                
                Log(string.Format("Панель вкладок обновлена. Всего вкладок: {0}", _tabNames.Count));
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при обновлении вкладок: {0}", ex.Message));
            }
        }
        
        private void AddNewProject()
        {
            try
            {
                Log("Добавление нового проекта");
                
                var inputBox = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите название нового проекта:",
                    "Добавить проект",
                    "Новый проект"
                );
                
                if (!string.IsNullOrWhiteSpace(inputBox))
                {
                    _tabNames.Add(inputBox);
                    RefreshTabs();
                    Log(string.Format("Добавлен новый проект: {0}", inputBox));
                    MessageBox.Show(string.Format("Проект '{0}' добавлен", inputBox), "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Log("Добавление проекта отменено");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при добавлении проекта: {0}", ex.Message));
                MessageBox.Show(string.Format("Ошибка: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void StartExport()
        {
            try
            {
                Log("Запуск экспорта данных");
                
                if (_revitApp == null)
                {
                    MessageBox.Show("Ошибка: Revit приложение недоступно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Получаем активный документ через открытые документы
                var openDocuments = _revitApp.Documents;
                if (openDocuments == null || openDocuments.Size == 0)
                {
                    MessageBox.Show("Ошибка: Нет открытых документов в Revit", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Берем первый открытый документ
                var activeDocument = openDocuments.Cast<RevitDB.Document>().FirstOrDefault();
                if (activeDocument == null)
                {
                    MessageBox.Show("Ошибка: Не удалось получить активный документ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Log(string.Format("Начинаем экспорт из документа: {0}", activeDocument.Title));
                
                // Создаем окно прогресса
                var progressWindow = new ProgressWindow();
                progressWindow.Show();
                
                // Запускаем экспорт в отдельном потоке
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var modelName = activeDocument.Title ?? "Unknown Model";
                        var data = _dataReader.ExtractElementsFromDocument(activeDocument, modelName, progressWindow);
                        
                        Dispatcher.Invoke(() =>
                        {
                            var csvContent = _exporter.ExportToCsv(data, modelName);
                            _exporter.SaveToFile(csvContent, modelName);
                            
                            progressWindow.Close();
                            MessageBox.Show(string.Format("Экспорт завершен! Обработано {0} записей.\nФайл сохранен в папке 'Документы'.", data.Count), 
                                "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            progressWindow.Close();
                            MessageBox.Show(string.Format("Ошибка экспорта: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при запуске экспорта: {0}", ex.Message));
                MessageBox.Show(string.Format("Ошибка: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Log(string message)
        {
            try
            {
                if (_logWindow != null)
                {
                    _logWindow.Log(message);
                }
            }
            catch (Exception ex)
            {
                // Если логирование не работает, просто игнорируем
            }
        }
    }

    public class LogWindow : Window
    {
        private TextBox _logTextBox;
        private int _logCounter = 0;

        public LogWindow()
        {
            Title = "Лог выполнения";
            Width = 600;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 50;
            Top = 50;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            
            CreateUI();
        }

        private void CreateUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Заголовок
            var title = new TextBlock
            {
                Text = "📋 Лог выполнения",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 5),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetRow(title, 0);
            grid.Children.Add(title);

            // Текстовое поле для логов
            _logTextBox = new TextBox
            {
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(_logTextBox, 1);
            grid.Children.Add(_logTextBox);

            // Кнопки
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 5, 10, 10)
            };

            var copyButton = new Button
            {
                Content = "📋 Копировать",
                Height = 30,
                Width = 100,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            copyButton.Click += (s, e) => 
            {
                try
                {
                    Clipboard.SetText(_logTextBox.Text);
                    Log("✅ Лог скопирован в буфер обмена");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Ошибка копирования: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttonPanel.Children.Add(copyButton);

            var clearButton = new Button
            {
                Content = "🗑️ Очистить",
                Height = 30,
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            clearButton.Click += (s, e) => 
            {
                _logTextBox.Clear();
                _logCounter = 0;
                Log("🔄 Лог очищен");
            };
            buttonPanel.Children.Add(clearButton);

            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        public void Log(string message)
        {
            try
            {
                if (_logTextBox != null)
                {
                    _logCounter++;
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    var logMessage = string.Format("[{0}] #{1}: {2}", timestamp, _logCounter, message);
                    
                    Dispatcher.Invoke(() =>
                    {
                        _logTextBox.AppendText(logMessage + Environment.NewLine);
                        _logTextBox.ScrollToEnd();
                    });
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки логирования
            }
        }
    }

    public class ProgressWindow : Window, IProgress<string>
    {
        private TextBox _progressTextBox;
        private ProgressBar _progressBar;

        public ProgressWindow()
        {
            Title = "Экспорт данных - Прогресс";
            Width = 500;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            
            CreateUI();
        }

        private void CreateUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Заголовок
            var title = new TextBlock
            {
                Text = "📤 Экспорт данных из Revit",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 5),
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
            };
            Grid.SetRow(title, 0);
            grid.Children.Add(title);

            // Прогресс бар
            _progressBar = new ProgressBar
            {
                Height = 20,
                Margin = new Thickness(10, 5, 10, 5),
                IsIndeterminate = true
            };
            Grid.SetRow(_progressBar, 0);
            grid.Children.Add(_progressBar);

            // Текстовое поле для прогресса
            _progressTextBox = new TextBox
            {
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(_progressTextBox, 1);
            grid.Children.Add(_progressTextBox);

            // Кнопка отмены
            var cancelButton = new Button
            {
                Content = "❌ Отменить",
                Height = 30,
                Width = 100,
                Margin = new Thickness(10, 5, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            cancelButton.Click += (s, e) => Close();
            Grid.SetRow(cancelButton, 2);
            grid.Children.Add(cancelButton);

            Content = grid;
        }

        public void Report(string value)
        {
            Dispatcher.Invoke(() =>
            {
                _progressTextBox.AppendText(value + Environment.NewLine);
                _progressTextBox.ScrollToEnd();
            });
        }
    }
}
