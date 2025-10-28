using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitExporterAddin.TestWpfApp
{
    public partial class ProjectView : UserControl
    {
        private Project _project;
        private MainWindow _mainWindow;
        private Export _currentExport;

        public ProjectView(Project project, MainWindow mainWindow)
        {
            InitializeComponent();
            _project = project;
            _mainWindow = mainWindow;
            
            ProjectTitleText.Text = project.Name;
            RefreshExportsList();
            
            // Если выгрузок нет, показываем пустое состояние
            if (_project.Exports.Count == 0)
            {
                ShowEmptyState();
            }
        }

        private void RefreshExportsList()
        {
            ExportsList.Children.Clear();

            foreach (var export in _project.Exports)
            {
                var exportItem = CreateExportItem(export);
                ExportsList.Children.Add(exportItem);
            }
        }

        private Border CreateExportItem(Export export)
        {
            var border = new Border
            {
                Background = _currentExport == export ? new SolidColorBrush(Color.FromRgb(255, 248, 220)) : Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Название выгрузки
            var nameText = new TextBlock
            {
                Text = export.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                TextDecorations = TextDecorations.Underline
            };
            nameText.MouseLeftButtonDown += (s, e) => SelectExport(export);
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            // Три точки для контекстного меню
            var contextButton = new Button
            {
                Content = "⋯",
                FontSize = 18,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 30,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Center
            };
            contextButton.Click += (s, e) => ShowExportContextMenu(export, contextButton);
            Grid.SetColumn(contextButton, 2);
            grid.Children.Add(contextButton);

            border.Child = grid;
            return border;
        }

        private void SelectExport(Export export)
        {
            _currentExport = export;
            ShowExportContent(export);
            RefreshExportsList(); // Обновляем выделение
        }

        private void ShowExportContent(Export export)
        {
            ExportContentArea.Children.Clear();
            var exportView = new ExportView(export, this);
            ExportContentArea.Children.Add(exportView);
        }

        private void ShowExportContextMenu(Export export, Button button)
        {
            var contextMenu = new ContextMenu();

            var renameItem = new MenuItem { Header = "Переименовать" };
            renameItem.Click += (s, e) => RenameExport(export);
            contextMenu.Items.Add(renameItem);

            var deleteItem = new MenuItem { Header = "Удалить" };
            deleteItem.Click += (s, e) => DeleteExport(export);
            contextMenu.Items.Add(deleteItem);

            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }

        private void RenameExport(Export export)
        {
            var inputDialog = new InputDialog("Переименовать выгрузку", "Введите новое название выгрузки:", export.Name);
            if (inputDialog.ShowDialog() == true)
            {
                var newName = inputDialog.Answer;
                if (!string.IsNullOrEmpty(newName) && newName != export.Name)
                {
                    export.Name = newName;
                    RefreshExportsList();
                }
            }
        }

        private void DeleteExport(Export export)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить выгрузку '{export.Name}'?", 
                "Подтверждение удаления", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _project.Exports.Remove(export);
                RefreshExportsList();
                
                if (_currentExport == export)
                {
                    ExportContentArea.Children.Clear();
                    _currentExport = null;
                }
            }
        }

        private void ShowEmptyState()
        {
            ExportContentArea.Children.Clear();
            
            var emptyText = new TextBlock
            {
                Text = "Выберите выгрузку или создайте новую",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            ExportContentArea.Children.Add(emptyText);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.GoBackToProjects();
        }

        private void AddExportButton_Click(object sender, RoutedEventArgs e)
        {
            var addExportWindow = new AddExportWindow();
            if (addExportWindow.ShowDialog() == true)
            {
                var newExport = new Export
                {
                    Name = addExportWindow.ExportName,
                    RevitFilePath = addExportWindow.RevitFilePath
                };

                _project.Exports.Add(newExport);
                RefreshExportsList();
                
                // Автоматически выбираем новую выгрузку
                SelectExport(newExport);
            }
        }
    }
}
