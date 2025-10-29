using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitExporterAddin.TestWpfApp
{
    public partial class MainWindow : Window
    {
        private List<Project> _projects;
        private Project _currentProject;

        public MainWindow()
        {
            InitializeComponent();
            _projects = new List<Project>();
            LoadProjects();
            RefreshProjectsList();
        }

        private void LoadProjects()
        {
            // Загружаем проекты из конфигурации
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitExporter", "projects.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    _projects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Project>>(json) ?? new List<Project>();
                }
                catch
                {
                    _projects = new List<Project>();
                }
            }

            // Если проектов нет, создаем тестовые
            if (_projects.Count == 0)
            {
                _projects.Add(new Project { Name = "Проект 1", Path = @"C:\Projects\Project1" });
                _projects.Add(new Project { Name = "Проект 2", Path = @"C:\Projects\Project2" });
                _projects.Add(new Project { Name = "Проект 3", Path = @"C:\Projects\Project3" });
            }
        }

        private void SaveProjects()
        {
            try
            {
                var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RevitExporter");
                Directory.CreateDirectory(configDir);
                var configPath = Path.Combine(configDir, "projects.json");
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_projects, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshProjectsList()
        {
            ProjectsList.Children.Clear();

            foreach (var project in _projects)
            {
                var projectItem = CreateProjectItem(project);
                ProjectsList.Children.Add(projectItem);
            }
        }

        private Border CreateProjectItem(Project project)
        {
            var border = new Border
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(233, 236, 239)),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Название проекта
            var nameText = new TextBlock
            {
                Text = project.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 37, 41)),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            nameText.MouseLeftButtonDown += (s, e) => OpenProject(project);
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
            contextButton.Click += (s, e) => ShowProjectContextMenu(project, contextButton);
            Grid.SetColumn(contextButton, 2);
            grid.Children.Add(contextButton);

            border.Child = grid;
            return border;
        }

        private void OpenProject(Project project)
        {
            _currentProject = project;
            ShowProjectContent(project);
        }

        private void ShowProjectContent(Project project)
        {
            ContentArea.Children.Clear();
            var projectView = new ProjectView(project, this);
            ContentArea.Children.Add(projectView);
        }

        private void ShowProjectContextMenu(Project project, Button button)
        {
            var contextMenu = new ContextMenu();

            var renameItem = new MenuItem { Header = "Переименовать" };
            renameItem.Click += (s, e) => RenameProject(project);
            contextMenu.Items.Add(renameItem);

            var deleteItem = new MenuItem { Header = "Удалить" };
            deleteItem.Click += (s, e) => DeleteProject(project);
            contextMenu.Items.Add(deleteItem);

            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
        }

        private void RenameProject(Project project)
        {
            var inputDialog = new InputDialog("Переименовать проект", "Введите новое название проекта:", project.Name);
            if (inputDialog.ShowDialog() == true)
            {
                var newName = inputDialog.Answer;
                if (!string.IsNullOrEmpty(newName) && newName != project.Name)
                {
                    project.Name = newName;
                    RefreshProjectsList();
                    SaveProjects();
                }
            }
        }

        private void DeleteProject(Project project)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить проект '{project.Name}'?", 
                "Подтверждение удаления", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _projects.Remove(project);
                RefreshProjectsList();
                SaveProjects();
                
                if (_currentProject == project)
                {
                    ContentArea.Children.Clear();
                    _currentProject = null;
                }
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var addProjectWindow = new AddProjectWindow();
            if (addProjectWindow.ShowDialog() == true)
            {
                var newProject = new Project
                {
                    Name = addProjectWindow.ProjectName,
                    Path = addProjectWindow.ProjectPath
                };

                _projects.Add(newProject);
                RefreshProjectsList();
                SaveProjects();
            }
        }

        public void GoBackToProjects()
        {
            _currentProject = null;
            ContentArea.Children.Clear();
        }
    }
}
