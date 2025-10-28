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
            
            CreateTestUI();
            
            Log("=== SimpleMainWindow конструктор завершен ===");
        }

        private void CreateTestUI()
        {
            try
            {
                Log("Создаем тестовый UI");
                
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Заголовок
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Вкладки
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Содержимое
                
                // Заголовок
                var title = new TextBlock
                {
                    Text = "Тест контекстного меню вкладок",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(30, 30, 30, 20),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetRow(title, 0);
                grid.Children.Add(title);
                
                // Панель вкладок
                var tabsPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(30, 0, 30, 20),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetRow(tabsPanel, 1);
                
                // Создаем тестовые вкладки
                var testTabs = new List<string> { "10.06", "15.07", "20.08" };
                foreach (var tab in testTabs)
                {
                    var tabButton = CreateTabButtonWithContextMenu(tab);
                    tabsPanel.Children.Add(tabButton);
                    Log(string.Format("Создана кнопка вкладки: {0}", tab));
                }
                
                grid.Children.Add(tabsPanel);
                
                // Содержимое
                var contentBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(30, 0, 30, 30)
                };
                Grid.SetRow(contentBorder, 2);
                
                var contentText = new TextBlock
                {
                    Text = "Нажмите ПРАВОЙ кнопкой мыши на любую вкладку выше,\nчтобы увидеть контекстное меню с опциями 'Переименовать' и 'Удалить'.",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
                };
                contentBorder.Child = contentText;
                grid.Children.Add(contentBorder);
                
                Content = grid;
                Log("Тестовый UI создан успешно");
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
}


