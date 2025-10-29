using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Views
{
    public partial class TabContentView : UserControl
    {
        private ExportTab _tab;
        private object _exportView;
        private CategoryFilter _modelElementsFilter;
        private CategoryFilter _mepSystemsFilter;
        private CategoryFilter _analyticsFilter;
        private CategoryFilter _annotationsFilter;
        private CategoryFilter _viewsFilter;
        private CategoryFilter _dataFilter;
        private StringBuilder _uiLog;

        public TabContentView(ExportTab tab, object exportView)
        {
            try
            {
                InitializeComponent();
                _tab = tab;
                _exportView = exportView;
                
                WriteToLogFile("TabContentView: Creating 6 separate CategoryFilters...");
                
                // Инициализируем UI лог
                _uiLog = new StringBuilder();
                AddToUILog("=== Инициализация TabContentView ===");
                AddToUILog($"Вкладка: {_tab.Name}");
                AddToUILog($"Версия Revit: {_tab.RevitVersion}");
                
                // Создаем отдельные фильтры для каждого блока
                AddToUILog("Создание фильтров категорий...");
                _modelElementsFilter = CreateFilterForBlock("Элементы модели");
                AddToUILog($"✓ Элементы модели: {_modelElementsFilter.GetTotalCount()} категорий");
                
                _mepSystemsFilter = CreateFilterForBlock("Инженерные системы (MEP)");
                AddToUILog($"✓ MEP системы: {_mepSystemsFilter.GetTotalCount()} категорий");
                
                _analyticsFilter = CreateFilterForBlock("Аналитические модели");
                AddToUILog($"✓ Аналитика: {_analyticsFilter.GetTotalCount()} категорий");
                
                _annotationsFilter = CreateFilterForBlock("Аннотации и оформление");
                AddToUILog($"✓ Аннотации: {_annotationsFilter.GetTotalCount()} категорий");
                
                _viewsFilter = CreateFilterForBlock("Организация проекта и виды");
                AddToUILog($"✓ Виды: {_viewsFilter.GetTotalCount()} категорий");
                
                _dataFilter = CreateFilterForBlock("Данные и системные категории");
                AddToUILog($"✓ Данные: {_dataFilter.GetTotalCount()} категорий");
                
                AddToUILog("=== Все фильтры созданы успешно ===");
                WriteToLogFile($"TabContentView: All 6 CategoryFilters created successfully");
                
                UpdateDisplay();
                WriteToLogFile("TabContentView: UpdateDisplay completed");
                
                // Подписываемся на изменения в фильтрах для автоматического обновления счетчиков
                SubscribeToFilterChanges();
                WriteToLogFile("TabContentView: Subscribed to filter changes");
                
                // Принудительное обновление UI
                AddToUILog("Принудительное обновление UI элементов...");
                
                // Проверяем все элементы фильтров
                var elements = new[] {
                    ("ModelElementsFiltersText", ModelElementsFiltersText),
                    ("MepSystemsFiltersText", MepSystemsFiltersText),
                    ("AnalyticsFiltersText", AnalyticsFiltersText),
                    ("AnnotationsFiltersText", AnnotationsFiltersText),
                    ("ViewsFiltersText", ViewsFiltersText),
                    ("DataFiltersText", DataFiltersText)
                };
                
                foreach (var (name, element) in elements)
                {
                    if (element != null)
                    {
                        element.Text = $"ТЕСТ: {name} найден!";
                        AddToUILog($"✓ {name} найден и обновлен");
                    }
                    else
                    {
                        AddToUILog($"❌ {name} НЕ НАЙДЕН!");
                    }
                }
                
                // Проверяем кнопки
                var buttons = new[] {
                    ("ViewModelElementsFiltersButton", ViewModelElementsFiltersButton),
                    ("ViewMepSystemsFiltersButton", ViewMepSystemsFiltersButton),
                    ("ViewAnalyticsFiltersButton", ViewAnalyticsFiltersButton),
                    ("ViewAnnotationsFiltersButton", ViewAnnotationsFiltersButton),
                    ("ViewViewsFiltersButton", ViewViewsFiltersButton),
                    ("ViewDataFiltersButton", ViewDataFiltersButton)
                };
                
                foreach (var (name, button) in buttons)
                {
                    if (button != null)
                    {
                        AddToUILog($"✓ {name} найден");
                    }
                    else
                    {
                        AddToUILog($"❌ {name} НЕ НАЙДЕН!");
                    }
                }
                
                // Принудительно обновляем лог
                AddToUILog("=== ТЕСТ ЗАВЕРШЕН ===");
                AddToUILog("Если вы видите этот текст, значит UI логирование работает!");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"TabContentView: Exception in constructor: {ex.Message}");
                WriteToLogFile($"TabContentView: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void UpdateDisplay()
        {
            // Отображаем путь к папке "Модели" внутри проекта, если она существует
            var modelsPath = Path.Combine(_tab.FolderPath, "Модели");
            if (Directory.Exists(modelsPath))
            {
                ModelsPathText.Text = modelsPath;
                _tab.ModelsFolder = modelsPath; // Обновляем путь в объекте вкладки
            }
            else
            {
                ModelsPathText.Text = "Не выбран";
            }
            
            ResultsPathText.Text = GetResultsFolderPath();
            UpdateAllFiltersDisplay();
        }

        private void UpdateAllFiltersDisplay()
        {
            UpdateFilterDisplay(_modelElementsFilter, ModelElementsFiltersText);
            UpdateFilterDisplay(_mepSystemsFilter, MepSystemsFiltersText);
            UpdateFilterDisplay(_analyticsFilter, AnalyticsFiltersText);
            UpdateFilterDisplay(_annotationsFilter, AnnotationsFiltersText);
            UpdateFilterDisplay(_viewsFilter, ViewsFiltersText);
            UpdateFilterDisplay(_dataFilter, DataFiltersText);
        }

        /// <summary>
        /// Подписывается на изменения во всех фильтрах для автоматического обновления счетчиков
        /// </summary>
        private void SubscribeToFilterChanges()
        {
            try
            {
                var filters = new[] {
                    ("Элементы модели", _modelElementsFilter),
                    ("Инженерные системы (MEP)", _mepSystemsFilter),
                    ("Аналитические модели", _analyticsFilter),
                    ("Аннотации и оформление", _annotationsFilter),
                    ("Организация проекта и виды", _viewsFilter),
                    ("Данные и системные категории", _dataFilter)
                };

                foreach (var (blockName, filter) in filters)
                {
                    if (filter != null && filter.CategoryBlocks.ContainsKey(blockName))
                    {
                        var blockItems = filter.CategoryBlocks[blockName];
                        foreach (var item in blockItems)
                        {
                            // Создаем слабую ссылку для избежания утечек памяти
                            var weakRef = new WeakReference(this);
                            item.PropertyChanged += (s, e) =>
                            {
                                if (weakRef.IsAlive && weakRef.Target is TabContentView view)
                                {
                                    if (e.PropertyName == nameof(CategoryFilterItem.IsSelected))
                                    {
                                        // Обновляем счетчики в UI потоке
                                        view.Dispatcher.Invoke(() => view.UpdateAllFiltersDisplay());
                                    }
                                }
                            };
                        }
                        AddToUILog($"✓ Подписка на изменения в блоке '{blockName}' ({blockItems.Count} элементов)");
                    }
                }
            }
            catch (Exception ex)
            {
                AddToUILog($"❌ Ошибка при подписке на изменения фильтров: {ex.Message}");
                WriteToLogFile($"TabContentView: Error subscribing to filter changes: {ex.Message}");
            }
        }

        private void UpdateFilterDisplay(CategoryFilter filter, System.Windows.Controls.TextBlock textBlock)
        {
            if (filter != null && textBlock != null)
            {
                // Получаем имя блока (первый и единственный блок в фильтре)
                var blockName = filter.CategoryBlocks.Keys.FirstOrDefault();
                
                if (!string.IsNullOrEmpty(blockName))
                {
                    var selectedCount = filter.GetSelectedCountInBlock(blockName);
                    var totalCount = filter.GetTotalCountInBlock(blockName);
                    
                    // Логируем для отладки
                    AddToUILog($"UpdateFilterDisplay: Block '{blockName}' - selectedCount={selectedCount}, totalCount={totalCount}");
                    AddToUILog($"UpdateFilterDisplay: CategoryBlocks.Count={filter.CategoryBlocks.Count}");
                    
                    // Дополнительная диагностика
                    foreach (var block in filter.CategoryBlocks)
                    {
                        var actualSelected = block.Value.Where(i => i.IsSelected).Count();
                        var actualTotal = block.Value.Count;
                        AddToUILog($"UpdateFilterDisplay: Block '{block.Key}' - actual: {actualSelected}/{actualTotal}");
                    }
                    
                    textBlock.Text = $"Выбрано: {selectedCount} из {totalCount} категорий";
                }
                else
                {
                    AddToUILog("UpdateFilterDisplay: No block found in filter!");
                    textBlock.Text = "Выбрано: 0 из 0 категорий";
                }
            }
        }

        private CategoryFilter CreateFilterForBlock(string blockName)
        {
            var fullFilter = new CategoryFilter();
            
            // Создаем пустой фильтр без инициализации всех блоков
            var blockFilter = new CategoryFilter();
            blockFilter.CategoryBlocks.Clear(); // Очищаем все блоки
            
            AddToUILog($"Создание фильтра для блока: '{blockName}'");
            AddToUILog($"Доступные блоки в fullFilter: {string.Join(", ", fullFilter.CategoryBlocks.Keys)}");
            
            // Копируем только категории нужного блока
            if (fullFilter.CategoryBlocks.ContainsKey(blockName))
            {
                var sourceBlock = fullFilter.CategoryBlocks[blockName];
                AddToUILog($"Блок '{blockName}' найден, содержит {sourceBlock.Count} категорий");
                
                blockFilter.CategoryBlocks[blockName] = new List<CategoryFilterItem>();
                foreach (var item in sourceBlock)
                {
                    blockFilter.CategoryBlocks[blockName].Add(new CategoryFilterItem(item.Name, item.IsSelected));
                }
                
                AddToUILog($"Скопировано {blockFilter.CategoryBlocks[blockName].Count} категорий в blockFilter");
                AddToUILog($"Блоков в blockFilter после создания: {blockFilter.CategoryBlocks.Count}");
            }
            else
            {
                AddToUILog($"❌ Блок '{blockName}' НЕ НАЙДЕН в fullFilter!");
            }
            
            return blockFilter;
        }

        private string GetResultsFolderPath()
        {
            // Получаем путь к папке вкладки (где будут результаты)
            if (_exportView != null)
            {
                try
                {
                    // Используем рефлексию для вызова методов
                    var getProjectPathMethod = _exportView.GetType().GetMethod("GetProjectPath");
                    var getExportNameMethod = _exportView.GetType().GetMethod("GetExportName");
                    
                    if (getProjectPathMethod != null && getExportNameMethod != null)
                    {
                        var projectPath = getProjectPathMethod.Invoke(_exportView, null) as string;
                        var exportName = getExportNameMethod.Invoke(_exportView, null) as string;
                        var exportPath = Path.Combine(projectPath, exportName);
                        var tabPath = Path.Combine(exportPath, _tab.Name);
                        AddToUILog($"GetResultsFolderPath: ExportView путь = {tabPath}");
                        return tabPath;
                    }
                }
                catch (Exception ex)
                {
                    AddToUILog($"GetResultsFolderPath: Ошибка при вызове методов через рефлексию: {ex.Message}");
                }
            }
            
            // Используем FolderPath из _tab, если он задан
            if (!string.IsNullOrEmpty(_tab.FolderPath))
            {
                AddToUILog($"GetResultsFolderPath: Tab.FolderPath = {_tab.FolderPath}");
                return _tab.FolderPath;
            }
            
            // Fallback для случая, когда ExportView не передан и FolderPath не задан
            var fallbackPath = Path.Combine("C:\\DataViewer", "Проекты", "DefaultProject", "DefaultExport", _tab.Name);
            AddToUILog($"GetResultsFolderPath: Fallback путь = {fallbackPath}");
            return fallbackPath;
        }

        private void BrowseModelsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку с моделями Revit",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _tab.ModelsFolder = dialog.SelectedPath;
                CopyModelsToTabFolder();
                UpdateDisplay();
            }
        }

        private void ViewResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var resultsPath = GetResultsFolderPath();
            
            // Создаем папку, если она не существует
            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
            }
            
            // Открываем папку в проводнике Windows
            Process.Start("explorer.exe", resultsPath);
        }

        private void CopyModelsToTabFolder()
        {
            if (string.IsNullOrEmpty(_tab.ModelsFolder) || !Directory.Exists(_tab.ModelsFolder))
                return;

            var resultsPath = GetResultsFolderPath();
            var modelsPath = Path.Combine(resultsPath, "Модели");

            try
            {
                // Удаляем существующую папку "Модели", если она есть
                if (Directory.Exists(modelsPath))
                {
                    Directory.Delete(modelsPath, true);
                }

                // Создаем новую папку "Модели"
                Directory.CreateDirectory(modelsPath);

                // Копируем все .rvt файлы с сохранением иерархии и подсчетом
                int copiedFilesCount = CopyRvtFilesRecursively(_tab.ModelsFolder, modelsPath);

                // Обновляем путь к папке "Модели" в объекте вкладки
                _tab.ModelsFolder = modelsPath;

                MessageBox.Show($"Модели успешно скопированы в папку:\n{modelsPath}\n\nСкопировано .rvt файлов: {copiedFilesCount}", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании моделей:\n{ex.Message}", 
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

        private void ViewModelElementsFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Элементы модели");
            ShowFilterWindow(_modelElementsFilter, "Элементы модели");
        }

        private void ViewMepSystemsFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Инженерные системы (MEP)");
            ShowFilterWindow(_mepSystemsFilter, "Инженерные системы (MEP)");
        }

        private void ViewAnalyticsFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Аналитические модели");
            ShowFilterWindow(_analyticsFilter, "Аналитические модели");
        }

        private void ViewAnnotationsFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Аннотации и оформление");
            ShowFilterWindow(_annotationsFilter, "Аннотации и оформление");
        }

        private void ViewViewsFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Организация проекта и виды");
            ShowFilterWindow(_viewsFilter, "Организация проекта и виды");
        }

        private void ViewDataFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("Открытие окна фильтра: Данные и системные категории");
            ShowFilterWindow(_dataFilter, "Данные и системные категории");
        }

        private void ShowFilterWindow(CategoryFilter filter, string blockName)
        {
            try
            {
                // Получаем только нужный блок категорий
                var blockCategories = filter.CategoryBlocks.ContainsKey(blockName) 
                    ? filter.CategoryBlocks[blockName] 
                    : new List<CategoryFilterItem>();
                
                AddToUILog($"Блок '{blockName}' содержит {blockCategories.Count} категорий");
                if (blockCategories.Count > 0)
                {
                    AddToUILog($"Первая категория: {blockCategories[0].Name}");
                }
                
                var filterWindow = new SingleBlockFilterWindow(blockCategories);
                filterWindow.Owner = Window.GetWindow(this);
                filterWindow.Title = $"Фильтр: {blockName}";
                
                AddToUILog($"Окно фильтра '{blockName}' открыто");
                
                if (filterWindow.ShowDialog() == true)
                {
                    var selectedCount = filter.GetSelectedCountInBlock(blockName);
                    var totalCount = filter.GetTotalCountInBlock(blockName);
                    AddToUILog($"✓ Фильтр '{blockName}' обновлен: {selectedCount}/{totalCount} категорий");
                    
                    // Проверяем, что изменения действительно применились
                    var actualSelected = blockCategories.Count(c => c.IsSelected);
                    AddToUILog($"✓ Проверка: фактически выбрано {actualSelected} из {blockCategories.Count} категорий");
                    
                    // Обновляем счетчики
                    UpdateAllFiltersDisplay();
                }
                else
                {
                    AddToUILog($"✗ Фильтр '{blockName}' отменен");
                }
            }
            catch (Exception ex)
            {
                AddToUILog($"✗ Ошибка в фильтре '{blockName}': {ex.Message}");
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            AddToUILog("=== НАЧАЛО ЭКСПОРТА ===");
            
            if (string.IsNullOrEmpty(_tab.ModelsFolder))
            {
                AddToUILog("✗ Ошибка: не выбран исходный Revit файл");
                MessageBox.Show("Выберите исходный Revit файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddToUILog($"✓ Исходная папка: {_tab.ModelsFolder}");

            // Проверяем, что выбрана хотя бы одна категория в любом из фильтров
            var totalSelected = _modelElementsFilter.GetSelectedCount() + 
                              _mepSystemsFilter.GetSelectedCount() + 
                              _analyticsFilter.GetSelectedCount() + 
                              _annotationsFilter.GetSelectedCount() + 
                              _viewsFilter.GetSelectedCount() + 
                              _dataFilter.GetSelectedCount();

            AddToUILog($"Проверка выбранных категорий:");
            AddToUILog($"  • Элементы модели: {_modelElementsFilter.GetSelectedCount()}/{_modelElementsFilter.GetTotalCount()}");
            AddToUILog($"  • MEP системы: {_mepSystemsFilter.GetSelectedCount()}/{_mepSystemsFilter.GetTotalCount()}");
            AddToUILog($"  • Аналитика: {_analyticsFilter.GetSelectedCount()}/{_analyticsFilter.GetTotalCount()}");
            AddToUILog($"  • Аннотации: {_annotationsFilter.GetSelectedCount()}/{_annotationsFilter.GetTotalCount()}");
            AddToUILog($"  • Виды: {_viewsFilter.GetSelectedCount()}/{_viewsFilter.GetTotalCount()}");
            AddToUILog($"  • Данные: {_dataFilter.GetSelectedCount()}/{_dataFilter.GetTotalCount()}");
            AddToUILog($"  • ИТОГО: {totalSelected} категорий");

            if (totalSelected == 0)
            {
                AddToUILog("✗ Ошибка: не выбрано ни одной категории");
                MessageBox.Show("Пожалуйста, выберите хотя бы одну категорию для экспорта в любом из блоков фильтров.\n\nМожно выбрать категории из любого блока - не обязательно из всех блоков.", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Собираем все выбранные категории из всех фильтров
                var allSelectedCategories = new List<string>();
                allSelectedCategories.AddRange(_modelElementsFilter.GetSelectedCategories());
                allSelectedCategories.AddRange(_mepSystemsFilter.GetSelectedCategories());
                allSelectedCategories.AddRange(_analyticsFilter.GetSelectedCategories());
                allSelectedCategories.AddRange(_annotationsFilter.GetSelectedCategories());
                allSelectedCategories.AddRange(_viewsFilter.GetSelectedCategories());
                allSelectedCategories.AddRange(_dataFilter.GetSelectedCategories());

                AddToUILog($"✓ Собрано {allSelectedCategories.Count} категорий для экспорта");
                AddToUILog("=== ЭКСПОРТ ГОТОВ К ВЫПОЛНЕНИЮ ===");

                // Передаем выбранные категории в MainWindow для экспорта
                if (_exportView != null)
                {
                    AddToUILog("Передача фильтров в ExportView для экспорта...");
                    
                    // Используем рефлексию для вызова методов
                    var setCategoryFiltersMethod = _exportView.GetType().GetMethod("SetCategoryFilters");
                    var startExportMethod = _exportView.GetType().GetMethod("StartExport");
                    
                    if (setCategoryFiltersMethod != null && startExportMethod != null)
                    {
                        setCategoryFiltersMethod.Invoke(_exportView, new object[] { allSelectedCategories });
                        startExportMethod.Invoke(_exportView, null);
                    }
                    else
                    {
                        AddToUILog("Методы SetCategoryFilters или StartExport не найдены");
                        MessageBox.Show($"Экспорт начат! Выбрано категорий: {allSelectedCategories.Count}\n\nВ реальном приложении здесь будет происходить выгрузка данных из Revit с учетом выбранных фильтров.", 
                            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    AddToUILog("ExportView не найден, используем fallback...");
                    // Fallback: показываем информационное сообщение
                    MessageBox.Show($"Экспорт начат! Выбрано категорий: {allSelectedCategories.Count}\n\nВ реальном приложении здесь будет происходить выгрузка данных из Revit с учетом выбранных фильтров.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получить все выбранные категории из всех фильтров
        /// </summary>
        public List<string> GetAllSelectedCategories()
        {
            var allSelectedCategories = new List<string>();
            allSelectedCategories.AddRange(_modelElementsFilter.GetSelectedCategories());
            allSelectedCategories.AddRange(_mepSystemsFilter.GetSelectedCategories());
            allSelectedCategories.AddRange(_analyticsFilter.GetSelectedCategories());
            allSelectedCategories.AddRange(_annotationsFilter.GetSelectedCategories());
            allSelectedCategories.AddRange(_viewsFilter.GetSelectedCategories());
            allSelectedCategories.AddRange(_dataFilter.GetSelectedCategories());
            return allSelectedCategories;
        }

        private void AddToUILog(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string logEntry = $"[{timestamp}] {message}";
                
                _uiLog.AppendLine(logEntry);
                
                // Обновляем UI в главном потоке
                Dispatcher.Invoke(() =>
                {
                    LogTextBlock.Text = _uiLog.ToString();
                    LogScrollViewer.ScrollToEnd();
                    LogStatusText.Text = $"Записей: {_uiLog.ToString().Split('\n').Length - 1}";
                });
                
                // Также записываем в файл
                WriteToLogFile($"UI: {message}");
            }
            catch (Exception ex)
            {
                WriteToLogFile($"Error in AddToUILog: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _uiLog.Clear();
            LogTextBlock.Text = "";
            LogStatusText.Text = "Лог очищен";
            AddToUILog("=== Лог очищен пользователем ===");
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"DataViewer_UI_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(desktopPath, fileName);
                
                File.WriteAllText(filePath, _uiLog.ToString());
                AddToUILog($"✓ Лог сохранен: {fileName}");
                LogStatusText.Text = $"Сохранено: {fileName}";
            }
            catch (Exception ex)
            {
                AddToUILog($"✗ Ошибка сохранения лога: {ex.Message}");
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


