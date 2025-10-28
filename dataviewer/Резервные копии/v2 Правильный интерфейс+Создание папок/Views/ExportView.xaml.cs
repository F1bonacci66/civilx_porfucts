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

        private void SelectTab(ExportTab tab)
        {
            _currentTab = tab;
            ShowTabContent(tab);
        }

        private void ShowTabContent(ExportTab tab)
        {
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
            var tabNumber = _export.Tabs.Count + 1;
            var newTab = new ExportTab
            {
                Name = $"Вкладка {tabNumber}",
                RevitVersion = "2023"
            };

            _export.Tabs.Add(newTab);
            RefreshTabs();
            SelectTab(newTab);
        }

        public string GetProjectPath()
        {
            return _projectView.GetProjectPath();
        }

        public string GetExportName()
        {
            return _export.Name;
        }
    }
}


