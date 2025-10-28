using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleTestApp
{
    public class SimpleMainWindow : Window
    {
        private LogWindow _logWindow;
        private List<string> _projects = new List<string> { "Проект 1", "Проект 2", "Проект 3" };
        private Dictionary<string, List<string>> _exports = new Dictionary<string, List<string>>
        {
            { "Проект 1", new List<string> { "Выгрузка 1", "Выгрузка 2" } },
            { "Проект 2", new List<string> { "Выгрузка A", "Выгрузка B" } },
            { "Проект 3", new List<string> { "Выгрузка X", "Выгрузка Y" } }
        };

        public SimpleMainWindow()
        {
            Title = "CustomViewer - ОКНО ЛОГИРОВАНИЯ";
            Width = 1200;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            
            _logWindow = new LogWindow();
            _logWindow.Show();
            
            Log("=== SimpleMainWindow конструктор ===");
            CreateUI();
            Log("=== SimpleMainWindow конструктор завершен ===");
        }

        private void CreateUI()
        {
            try
            {
                Log("CreateUI: начало");
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Log("CreateUI: Grid создан с 3 колонками (левая, разделитель, правая)");
                
                // СОЗДАЕМ КОНТЕЙНЕР ДЛЯ ПРОЕКТОВ
                var projectContainer = new Grid();
                Log("CreateUI: projectContainer создан без ColumnDefinitions");
                
                                 var leftPanel = CreateProjectsPanel();
                 Log(string.Format("CreateUI: leftPanel создан, тип: {0}", leftPanel.GetType().Name));
                 
                 var rightPanel = CreateInitialRightPanel();
                 Log(string.Format("CreateUI: rightPanel создан, тип: {0}", rightPanel.GetType().Name));
                 
                 Log("CreateUI: панели созданы");
                
                                                  // УСТАНАВЛИВАЕМ КОЛОНКИ ДЛЯ ПАНЕЛЕЙ
                 Grid.SetColumn(leftPanel, 0);
                 Grid.SetColumn(rightPanel, 2);
                 Log("CreateUI: колонки установлены (левая=0, разделитель=1, правая=2)");
                 
                 // ДОБАВЛЯЕМ ПАНЕЛИ СНАЧАЛА
                 grid.Children.Add(leftPanel);
                 grid.Children.Add(rightPanel);
                 Log(string.Format("CreateUI: панели добавлены, grid.Children.Count = {0}", grid.Children.Count));
                 
                 // ПОТОМ ДОБАВЛЯЕМ GridSplitter МЕЖДУ ПАНЕЛЯМИ (в индекс 1)
                 var splitter = new GridSplitter
                 {
                     Width = 5,
                     Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                     HorizontalAlignment = HorizontalAlignment.Center,
                     VerticalAlignment = VerticalAlignment.Stretch,
                     Margin = new Thickness(0, 10, 0, 10)
                 };
                 Grid.SetColumn(splitter, 1);
                 grid.Children.Insert(1, splitter);
                 Log(string.Format("CreateUI: GridSplitter вставлен между панелями в индекс 1, тип: {0}", splitter.GetType().Name));
                 
                 // ПРОВЕРЯЕМ финальную структуру
                 Log(string.Format("CreateUI: финальная структура grid.Children.Count = {0}", grid.Children.Count));
                 if (grid.Children.Count >= 3)
                 {
                     Log(string.Format("CreateUI: левая панель в индексе 0: {0}", grid.Children[0].GetType().Name));
                     Log(string.Format("CreateUI: GridSplitter в индексе 1: {0}", grid.Children[1].GetType().Name));
                     Log(string.Format("CreateUI: правая панель в индексе 2: {0}", grid.Children[2].GetType().Name));
                 }
                
                Content = grid;
                Log("CreateUI: Content установлен");
                Log("CreateUI: завершено");
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateUI: {0}", ex.Message));
            }
        }

        private Border CreateProjectsPanel()
        {
            try
            {
                Log("CreateProjectsPanel: начало");
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(20, 20, 10, 20)
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };
                
                var title = new TextBlock
                {
                    Text = "Проекты",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                Log("CreateProjectsPanel: заголовок добавлен");

                foreach (var project in _projects)
                {
                    var projectButton = new Button
                    {
                        Content = string.Format("📁 {0}", project),
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
                    projectButton.Click += (s, e) => 
                    {
                        Log(string.Format("КЛИК по проекту: {0}", project));
                        ShowProjectView(project);
                    };
                    stackPanel.Children.Add(projectButton);
                    Log(string.Format("CreateProjectsPanel: кнопка проекта {0} добавлена", project));
                }

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
                Log("CreateProjectsPanel: кнопка добавления проекта добавлена");

                border.Child = stackPanel;
                Log("CreateProjectsPanel: завершено");
                return border;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateProjectsPanel: {0}", ex.Message));
                return new Border();
            }
        }

        private Border CreateInitialRightPanel()
        {
            try
            {
                Log("CreateInitialRightPanel: начало");
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250))
                };

                var stackPanel = new StackPanel { Margin = new Thickness(30) };
                
                var title = new TextBlock
                {
                    Text = "Добро пожаловать",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                stackPanel.Children.Add(title);
                
                var info = new TextBlock
                {
                    Text = "Выберите проект в левой панели для начала работы.",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                stackPanel.Children.Add(info);
                
                border.Child = stackPanel;
                Log("CreateInitialRightPanel: завершено");
                return border;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateInitialRightPanel: {0}", ex.Message));
                return new Border();
            }
        }

        private void ShowProjectView(string projectName)
        {
            try
            {
                Log(string.Format("=== ShowProjectView НАЧАЛО для проекта: {0} ===", projectName));
                
                var grid = Content as Grid;
                Log(string.Format("ShowProjectView: Content тип: {0}", Content != null ? Content.GetType().Name : "NULL"));
                Log(string.Format("ShowProjectView: grid тип: {0}", grid != null ? grid.GetType().Name : "NULL"));
                Log(string.Format("ShowProjectView: grid.Children.Count = {0}", grid != null ? grid.Children.Count : 0));
                
                if (grid != null && grid.Children.Count > 0)
                {
                    // Теперь нужно найти и заменить левую и правую панели напрямую
                    var leftPanel = CreateExportsLeftPanel(projectName);
                    var rightPanel = CreateExportsRightPanel(projectName);
                    Log("ShowProjectView: панели созданы");
                    
                    // Устанавливаем колонки для новых панелей
                    Grid.SetColumn(leftPanel, 0);
                    Grid.SetColumn(rightPanel, 2);
                    Log("ShowProjectView: колонки установлены (левая=0, правая=2)");
                    
                                         // Удаляем старые панели и добавляем новые
                     // ВАЖНО: сохраняем GridSplitter в индексе 1!
                     var splitter = grid.Children[1]; // Сохраняем GridSplitter
                     Log(string.Format("ShowProjectView: сохраняем GridSplitter типа: {0}", splitter.GetType().Name));
                     
                     // Удаляем панели, но НЕ трогаем GridSplitter
                     grid.Children.RemoveAt(2); // Удаляем правую панель
                     grid.Children.RemoveAt(0); // Удаляем левую панель
                     
                     // Добавляем новые панели, сохраняя GridSplitter в индексе 1
                     grid.Children.Insert(0, leftPanel);   // Вставляем левую панель в начало (индекс 0)
                     grid.Children.Add(rightPanel);        // Добавляем правую панель в конец (индекс 2)
                     
                     // Проверяем, что GridSplitter остался на месте
                     if (grid.Children[1] != splitter)
                     {
                         Log("ОШИБКА: GridSplitter был потерян!");
                         // Восстанавливаем GridSplitter
                         grid.Children.RemoveAt(1);
                         grid.Children.Insert(1, splitter);
                         Grid.SetColumn(splitter, 1);
                         Log("ShowProjectView: GridSplitter восстановлен");
                     }
                     else
                     {
                         Log("ShowProjectView: GridSplitter успешно сохранен");
                     }
                     
                                          // Проверяем, что панели добавлены правильно
                     Log(string.Format("ShowProjectView: после замены grid.Children.Count = {0}", grid.Children.Count));
                     Log(string.Format("ShowProjectView: левая панель в индексе 0: {0}", grid.Children[0].GetType().Name));
                     Log(string.Format("ShowProjectView: GridSplitter в индексе 1: {0}", grid.Children[1].GetType().Name));
                     Log(string.Format("ShowProjectView: правая панель в индексе 2: {0}", grid.Children[2].GetType().Name));
                     
                     Log("ShowProjectView: панели заменены в основном grid");
                     
                     // ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ UI
                     Log("ShowProjectView: принудительно обновляем UI");
                     grid.InvalidateVisual();
                     grid.UpdateLayout();
                     this.InvalidateVisual();
                     this.UpdateLayout();
                     Log("ShowProjectView: UI обновлен");
                     
                     Log(string.Format("=== ShowProjectView ЗАВЕРШЕН для проекта: {0} ===", projectName));
                }
                else
                {
                    Log("ОШИБКА: grid не найден или пуст!");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при показе проекта: {0}", ex.Message));
                Log(string.Format("Стек вызовов: {0}", ex.StackTrace));
            }
        }

        private Border CreateExportsLeftPanel(string projectName)
        {
            try
            {
                Log(string.Format("CreateExportsLeftPanel: начало для проекта {0}", projectName));
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(20, 20, 10, 20)
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };
                
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
                
                var backButton = new Button
                {
                    Content = "⬅️ Назад",
                    Height = 35,
                    Width = 80,
                    Background = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                backButton.Click += (s, e) => ShowMainView();
                headerPanel.Children.Add(backButton);
                
                var title = new TextBlock
                {
                    Text = string.Format("Проект: {0}", projectName),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                headerPanel.Children.Add(title);
                stackPanel.Children.Add(headerPanel);

                var exports = _exports.ContainsKey(projectName) ? _exports[projectName] : new List<string>();
                foreach (var export in exports)
                {
                    var exportButton = new Button
                    {
                        Content = string.Format("📊 {0}", export),
                        Height = 45,
                        Margin = new Thickness(0, 0, 0, 10),
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(10, 0, 0, 0)
                    };
                    exportButton.Click += (s, e) => SelectExport(export);
                    stackPanel.Children.Add(exportButton);
                }
                
                var addButton = new Button
                {
                    Content = "➕ Добавить выгрузку",
                    Height = 40,
                    Margin = new Thickness(0, 20, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontSize = 14
                };
                addButton.Click += AddExport_Click;
                stackPanel.Children.Add(addButton);
                
                border.Child = stackPanel;
                Log(string.Format("CreateExportsLeftPanel: завершено для проекта {0}", projectName));
                return border;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateExportsLeftPanel: {0}", ex.Message));
                return new Border();
            }
        }
        
                 private Border CreateExportsRightPanel(string projectName)
         {
             try
             {
                 Log(string.Format("CreateExportsRightPanel: начало для проекта {0}", projectName));
                 var border = new Border
                 {
                     Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                     BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                     BorderThickness = new Thickness(1),
                     Margin = new Thickness(0, 20, 20, 20)
                 };
                
                var stackPanel = new StackPanel { Margin = new Thickness(30) };
                
                var title = new TextBlock
                {
                    Text = "Выберите выгрузку для просмотра",
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20),
                    Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
                };
                stackPanel.Children.Add(title);
                
                var info = new TextBlock
                {
                    Text = "В левой панели выберите выгрузку, чтобы увидеть её детали и вкладки.",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
                };
                stackPanel.Children.Add(info);
                
                border.Child = stackPanel;
                Log(string.Format("CreateExportsRightPanel: завершено для проекта {0}", projectName));
                return border;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateExportsRightPanel: {0}", ex.Message));
                return new Border();
            }
        }
        
                private void SelectExport(string exportName)
        {
            try
            {
                Log(string.Format("SelectExport вызван для: {0}", exportName));
                
                var grid = Content as Grid;
                Log(string.Format("SelectExport: grid.Children.Count = {0}", grid.Children.Count));
                
                if (grid != null && grid.Children.Count >= 3)
                {
                    // Теперь панели находятся напрямую в grid:
                    // grid.Children[0] = левая панель
                    // grid.Children[1] = GridSplitter  
                    // grid.Children[2] = правая панель
                    
                    var rightPanel = grid.Children[2] as Border;
                    if (rightPanel != null)
                    {
                        Log("Правая панель найдена, обновляем содержимое");
                        
                        var exportView = CreateExportDetailView(exportName);
                        Log(string.Format("CreateExportDetailView создан, тип: {0}", exportView.GetType().Name));
                        
                        if (exportView != null)
                        {
                            Log("exportView не null, обновляем rightPanel.Child");
                            
                            var currentChild = rightPanel.Child;
                            Log(string.Format("Текущее содержимое правой панели: {0}", currentChild != null ? currentChild.GetType().Name : "NULL"));
                            
                            rightPanel.Child = exportView;
                            
                            if (rightPanel.Child == exportView)
                            {
                                Log("Содержимое правой панели успешно обновлено!");
                                
                                // ДЕЛАЕМ СОДЕРЖИМОЕ РАСТЯГИВАЕМЫМ НА ВСЮ ПРАВУЮ ЧАСТЬ
                                var fe = exportView as FrameworkElement;
                                if (fe != null)
                                {
                                    // УБИРАЕМ ПРИНУДИТЕЛЬНЫЕ РАЗМЕРЫ - ПУСТЬ РАСТЯГИВАЕТСЯ
                                    fe.Width = double.NaN;  // Auto
                                    fe.Height = double.NaN; // Auto
                                    fe.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    fe.VerticalAlignment = VerticalAlignment.Stretch;
                                    
                                    // ДЕЛАЕМ СОДЕРЖИМОЕ СЕРЫМ КАК ЛЕВАЯ ЧАСТЬ
                                    var panel = fe as Panel;
                                    if (panel != null)
                                    {
                                        panel.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                                    }
                                    
                                    Log("Содержимое сделано растягиваемым на всю правую часть, фон белый");
                                }
                                
                                Log(string.Format("rightPanel.Child размеры: Width={0}, Height={1}, Visibility={2}", 
                                    rightPanel.Child.RenderSize.Width, rightPanel.Child.RenderSize.Height, rightPanel.Child.Visibility));
                                
                                rightPanel.InvalidateVisual();
                                rightPanel.UpdateLayout();
                                
                                Log(string.Format("После обновления rightPanel.Child размеры: Width={0}, Height={1}", 
                                    rightPanel.Child.RenderSize.Width, rightPanel.Child.RenderSize.Height));
                                
                                Log("UI принудительно обновлен");
                                
                                this.InvalidateVisual();
                                this.UpdateLayout();
                                Log("ВСЕ ОКНО принудительно обновлено");
                            }
                            else
                            {
                                Log("ОШИБКА: rightPanel.Child не равен exportView после присваивания!");
                            }
                        }
                        else
                        {
                            Log("ОШИБКА: exportView равен null!");
                        }
                    }
                    else
                    {
                        Log("Не удалось найти правую панель в grid.Children[2]");
                    }
                }
                else
                {
                    Log(string.Format("Grid не найден или имеет неправильную структуру. Детей: {0}", grid != null ? grid.Children.Count : 0));
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при выборе выгрузки: {0}", ex.Message));
                Log(string.Format("Стек вызовов: {0}", ex.StackTrace));
            }
        }
        
        private UIElement CreateExportDetailView(string exportName)
        {
            try
            {
                Log(string.Format("CreateExportDetailView вызван для: {0}", exportName));
                
                // СОЗДАЕМ Grid ВМЕСТО StackPanel ДЛЯ ЛУЧШЕГО КОНТРОЛЯ
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Заголовок
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Вкладки
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Кнопка добавления вкладки
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Содержимое вкладки
                Log("Grid создан с рядами");
                
                // ЗАГОЛОВОК
                var title = new TextBlock
                {
                    Text = string.Format("Выгрузка: {0}", exportName),
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(30, 30, 30, 20),
                    Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                };
                Grid.SetRow(title, 0);
                grid.Children.Add(title);
                Log("Заголовок добавлен в Grid");
                
                                                  // ПАНЕЛЬ ВКЛАДОК (ГОРИЗОНТАЛЬНО) + КНОПКА ДОБАВЛЕНИЯ В ОДНОМ РЯДУ
                 var tabsPanel = new StackPanel 
                 { 
                     Orientation = Orientation.Horizontal,
                     Margin = new Thickness(30, 0, 30, 20),
                     HorizontalAlignment = HorizontalAlignment.Stretch
                 };
                 Grid.SetRow(tabsPanel, 1);
                 
                 var tabs = new List<string> { "10.06", "15.07", "20.08" };
                 foreach (var tab in tabs)
                 {
                     var tabButton = new Button
                     {
                         Content = string.Format("📋 {0}", tab),
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
                     tabButton.Click += (s, e) => SelectTab(tab);
                     tabsPanel.Children.Add(tabButton);
                     Log(string.Format("Кнопка вкладки {0} добавлена", tab));
                 }
                 
                 // КНОПКА ДОБАВЛЕНИЯ ВКЛАДКИ В ТОМ ЖЕ РЯДУ
                 var addTabButton = new Button
                 {
                     Content = "➕",  // ТОЛЬКО ПЛЮС!
                     Height = 40,
                     Width = 40,      // КВАДРАТНАЯ КНОПКА
                     Margin = new Thickness(0, 0, 0, 0),
                     Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                     Foreground = Brushes.White,
                     BorderThickness = new Thickness(0),
                     FontSize = 18,   // БОЛЬШЕ ПЛЮС
                     HorizontalAlignment = HorizontalAlignment.Left,
                     HorizontalContentAlignment = HorizontalAlignment.Center
                 };
                 addTabButton.Click += AddTab_Click;
                 tabsPanel.Children.Add(addTabButton);
                 Log("Кнопка добавления вкладки добавлена в ряд с вкладками");
                 
                 grid.Children.Add(tabsPanel);
                 Log("Панель вкладок с кнопкой добавления добавлена в Grid");
                
                                 // СОДЕРЖИМОЕ ВКЛАДКИ (ПОД ВКЛАДКАМИ)
                 var contentBorder = new Border
                 {
                     Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                     BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                     BorderThickness = new Thickness(1),
                     Margin = new Thickness(30, 0, 30, 30)
                 };
                 Grid.SetRow(contentBorder, 3);
                 
                 var contentPanel = new StackPanel 
                 { 
                     Margin = new Thickness(20),
                     HorizontalAlignment = HorizontalAlignment.Stretch
                 };
                 
                 var contentTitle = new TextBlock
                 {
                     Text = "Параметры выгрузки",
                     FontSize = 18,
                     FontWeight = FontWeights.SemiBold,
                     Margin = new Thickness(20, 20, 20, 15),
                     Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                 };
                 contentPanel.Children.Add(contentTitle);
                 
                 // ИСХОДНАЯ ПАПКА С МОДЕЛЯМИ
                 var sourceFolderPanel = new StackPanel 
                 { 
                     Orientation = Orientation.Horizontal,
                     Margin = new Thickness(20, 0, 20, 15),
                     HorizontalAlignment = HorizontalAlignment.Stretch
                 };
                 
                 var sourceIcon = new TextBlock
                 {
                     Text = "📂",
                     FontSize = 20,
                     VerticalAlignment = VerticalAlignment.Center,
                     Margin = new Thickness(0, 0, 10, 0)
                 };
                 sourceFolderPanel.Children.Add(sourceIcon);
                 
                 var sourceLabel = new TextBlock
                 {
                     Text = "Исходная папка с моделями:",
                     FontSize = 14,
                     FontWeight = FontWeights.SemiBold,
                     VerticalAlignment = VerticalAlignment.Center,
                     Margin = new Thickness(0, 0, 10, 0),
                     Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
                 };
                 sourceFolderPanel.Children.Add(sourceLabel);
                 
                 var sourcePath = new TextBlock
                 {
                     Text = "C:\\Models\\Project1\\",
                     FontSize = 14,
                     VerticalAlignment = VerticalAlignment.Center,
                     FontFamily = new FontFamily("Consolas"),
                     Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                 };
                 sourceFolderPanel.Children.Add(sourcePath);
                 
                 contentPanel.Children.Add(sourceFolderPanel);
                 
                 // ПАПКА КУДА СОХРАНЕНА ВКЛАДКА
                 var outputFolderPanel = new StackPanel 
                 { 
                     Orientation = Orientation.Horizontal,
                     Margin = new Thickness(20, 0, 20, 15),
                     HorizontalAlignment = HorizontalAlignment.Stretch
                 };
                 
                 var outputIcon = new TextBlock
                 {
                     Text = "📁",
                     FontSize = 20,
                     VerticalAlignment = VerticalAlignment.Center,
                     Margin = new Thickness(0, 0, 10, 0)
                 };
                 outputFolderPanel.Children.Add(outputIcon);
                 
                 var outputLabel = new TextBlock
                 {
                     Text = "Папка сохранения вкладки:",
                     FontSize = 14,
                     FontWeight = FontWeights.SemiBold,
                     VerticalAlignment = VerticalAlignment.Center,
                     Margin = new Thickness(0, 0, 10, 0),
                     Foreground = new SolidColorBrush(Color.FromRgb(73, 80, 87))
                 };
                 outputFolderPanel.Children.Add(outputLabel);
                 
                 var outputPath = new TextBlock
                 {
                     Text = "C:\\Projects\\Project1\\Export1\\10.06\\",
                     FontSize = 14,
                     VerticalAlignment = VerticalAlignment.Center,
                     FontFamily = new FontFamily("Consolas"),
                     Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41))
                 };
                 outputFolderPanel.Children.Add(outputPath);
                 
                 contentPanel.Children.Add(outputFolderPanel);
                 
                 // ИНФОРМАЦИЯ О СТРУКТУРЕ ПАПОК
                 var structureInfo = new TextBlock
                 {
                     Text = "Структура папок: Проект → Выгрузка → Вкладка",
                     FontSize = 12,
                     TextWrapping = TextWrapping.Wrap,
                     Margin = new Thickness(20, 0, 20, 20),
                     Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                     FontStyle = FontStyles.Italic
                 };
                                  contentPanel.Children.Add(structureInfo);
                 
                 contentBorder.Child = contentPanel;
                 grid.Children.Add(contentBorder);
                Log("Панель содержимого добавлена в Grid");
                
                Log(string.Format("CreateExportDetailView завершен. Grid содержит {0} элементов", grid.Children.Count));
                return grid;
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА в CreateExportDetailView: {0}", ex.Message));
                return new TextBlock { Text = "Ошибка создания представления" };
            }
        }

        private void SelectTab(string tabName)
        {
            Log(string.Format("Выбрана вкладка: {0}", tabName));
            // Здесь будет логика для показа содержимого вкладки
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            Log("Нажата кнопка 'Добавить проект'");
            MessageBox.Show("Функция добавления проекта будет реализована позже", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddExport_Click(object sender, RoutedEventArgs e)
        {
            Log("Нажата кнопка 'Добавить выгрузку'");
            MessageBox.Show("Функция добавления выгрузки будет реализована позже", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

                 private void AddTab_Click(object sender, RoutedEventArgs e)
         {
             try
             {
                 Log("Нажата кнопка 'Добавить вкладку'");
                 
                 // ГЕНЕРИРУЕМ НОВОЕ ИМЯ ВКЛАДКИ (например, текущая дата)
                 var newTabName = DateTime.Now.ToString("dd.MM");
                 Log(string.Format("Создаем новую вкладку: {0}", newTabName));
                 
                 // НАХОДИМ ПАНЕЛЬ ВКЛАДОК
                 var grid = Content as Grid;
                 if (grid != null && grid.Children.Count >= 3)
                 {
                     var rightPanel = grid.Children[2] as Border;
                     if (rightPanel != null && rightPanel.Child is Grid)
                     {
                         var exportGrid = rightPanel.Child as Grid;
                         if (exportGrid != null && exportGrid.Children.Count >= 2)
                         {
                             var tabsPanel = exportGrid.Children[1] as StackPanel;
                             if (tabsPanel != null)
                             {
                                 // СОЗДАЕМ НОВУЮ КНОПКУ ВКЛАДКИ
                                 var newTabButton = new Button
                                 {
                                     Content = string.Format("📋 {0}", newTabName),
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
                                 newTabButton.Click += (s, args) => SelectTab(newTabName);
                                 
                                 // ВСТАВЛЯЕМ ПЕРЕД КНОПКОЙ ДОБАВЛЕНИЯ (перед последним элементом)
                                 var insertIndex = tabsPanel.Children.Count - 1;
                                 tabsPanel.Children.Insert(insertIndex, newTabButton);
                                 
                                 Log(string.Format("Новая вкладка {0} добавлена в позицию {1}", newTabName, insertIndex));
                                 Log(string.Format("Теперь в панели вкладок {0} элементов", tabsPanel.Children.Count));
                             }
                         }
                     }
                 }
                 
                 MessageBox.Show(string.Format("Вкладка '{0}' добавлена!", newTabName), "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
             }
             catch (Exception ex)
             {
                 Log(string.Format("ОШИБКА при добавлении вкладки: {0}", ex.Message));
                 MessageBox.Show(string.Format("Ошибка при добавлении вкладки: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
             }
         }

        private void ShowMainView()
        {
            try
            {
                Log("Возврат к главному виду");
                CreateUI();
            }
            catch (Exception ex)
            {
                Log(string.Format("ОШИБКА при возврате к главному виду: {0}", ex.Message));
            }
        }

        private void Log(string message)
        {
            if (_logWindow != null)
            {
                _logWindow.Log(message);
            }
        }
    }

    public class LogWindow : Window
    {
        private TextBox _logTextBox;
        private Button _copyButton;
        private Button _clearButton;

        public LogWindow()
        {
            Title = "Лог отладки - CustomViewer";
            Width = 700;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 50;
            Top = 50;
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250));
            CreateUI();
        }

        private void CreateUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            _logTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10)
            };
            Grid.SetRow(_logTextBox, 0);
            grid.Children.Add(_logTextBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            Grid.SetRow(buttonPanel, 1);

            _copyButton = new Button
            {
                Content = "📋 Копировать весь лог",
                Width = 150,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Margin = new Thickness(0, 0, 10, 0)
            };
            _copyButton.Click += CopyButton_Click;
            buttonPanel.Children.Add(_copyButton);

            _clearButton = new Button
            {
                Content = "🗑️ Очистить лог",
                Width = 120,
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12
            };
            _clearButton.Click += ClearButton_Click;
            buttonPanel.Children.Add(_clearButton);

            grid.Children.Add(buttonPanel);
            Content = grid;
        }

        public void Log(string message)
        {
            var timestamp = string.Format("[{0:HH:mm:ss.fff}] ", DateTime.Now);
            var fullMessage = timestamp + message + Environment.NewLine;

            Dispatcher.Invoke(() =>
            {
                _logTextBox.AppendText(fullMessage);
                _logTextBox.ScrollToEnd();
            });
        }

        public void ClearLog()
        {
            Dispatcher.Invoke(() =>
            {
                _logTextBox.Clear();
            });
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_logTextBox.Text);
                MessageBox.Show("Весь лог скопирован в буфер обмена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка при копировании: {0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _logTextBox.Clear();
        }
    }

    public class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new SimpleMainWindow();
            mainWindow.Show();
        }
    }
    
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run();
        }
    }
}
