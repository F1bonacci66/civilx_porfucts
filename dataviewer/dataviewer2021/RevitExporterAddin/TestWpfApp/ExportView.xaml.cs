using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitExporterAddin.TestWpfApp
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
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            return grid;
        }

        private UserControl CreateTabContent(ExportTab tab)
        {
            return new TabContentView(tab, this);
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

        private void CreateDefaultTab()
        {
            var defaultTab = new ExportTab
            {
                Name = DateTime.Now.ToString("dd.MM"),
                RevitFilePath = _export.RevitFilePath,
                OutputPath = GenerateOutputPath(_export, DateTime.Now.ToString("dd.MM"))
            };

            _export.Tabs.Add(defaultTab);
            RefreshTabs();
            SelectTab(defaultTab);
        }

        private string GenerateOutputPath(Export export, string tabName)
        {
            var projectPath = Path.GetDirectoryName(export.RevitFilePath);
            var exportFolder = Path.Combine(projectPath, export.Name);
            var tabFolder = Path.Combine(exportFolder, tabName);
            var fileName = $"Выгрузка данных с модели {Path.GetFileNameWithoutExtension(export.RevitFilePath)} {tabName}.csv";
            
            return Path.Combine(tabFolder, fileName);
        }

        private void SelectTab(ExportTab tab)
        {
            _currentTab = tab;
            TabContentArea.Children.Clear();
            var tabContentView = new TabContentView(tab, this);
            TabContentArea.Children.Add(tabContentView);
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
                    tab.OutputPath = GenerateOutputPath(_export, newName);
                    RefreshTabs();
                }
            }
        }

        private void DeleteTab(ExportTab tab)
        {
            if (_export.Tabs.Count <= 1)
            {
                MessageBox.Show("Нельзя удалить последнюю вкладку", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    SelectTab(_export.Tabs[0]);
                }
            }
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            var newTab = new ExportTab
            {
                Name = DateTime.Now.ToString("dd.MM"),
                RevitFilePath = _export.RevitFilePath,
                OutputPath = GenerateOutputPath(_export, DateTime.Now.ToString("dd.MM"))
            };

            _export.Tabs.Add(newTab);
            RefreshTabs();
            SelectTab(newTab);
        }
    }
}
