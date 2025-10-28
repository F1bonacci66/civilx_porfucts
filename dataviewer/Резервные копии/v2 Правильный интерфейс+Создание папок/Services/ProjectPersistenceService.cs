using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public class ProjectPersistenceService
    {
        private readonly string _configFilePath;

        public ProjectPersistenceService()
        {
            // Создаем папку для конфигурации в AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFolder = Path.Combine(appDataPath, "RevitExporter");
            Directory.CreateDirectory(configFolder);
            
            _configFilePath = Path.Combine(configFolder, "projects.json");
        }

        public void SaveProjects(List<Project> projects)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("# RevitExporter Projects Configuration");
                sb.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();
                
                foreach (var project in projects)
                {
                    sb.AppendLine($"[PROJECT]");
                    sb.AppendLine($"Name={project.Name}");
                    sb.AppendLine($"Path={project.FolderPath}");
                    sb.AppendLine();
                    
                    foreach (var export in project.Exports)
                    {
                        sb.AppendLine($"  [EXPORT]");
                        sb.AppendLine($"  Name={export.Name}");
                        sb.AppendLine($"  Path={export.FolderPath}");
                        sb.AppendLine();
                        
                        foreach (var tab in export.Tabs)
                        {
                            sb.AppendLine($"    [TAB]");
                            sb.AppendLine($"    Name={tab.Name}");
                            sb.AppendLine($"    Path={tab.FolderPath}");
                            sb.AppendLine($"    RevitVersion={tab.RevitVersion}");
                            sb.AppendLine();
                        }
                    }
                }
                
                File.WriteAllText(_configFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении проектов: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public List<Project> LoadProjects()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return new List<Project>();
                }

                var lines = File.ReadAllLines(_configFilePath, Encoding.UTF8);
                var projects = new List<Project>();
                Project currentProject = null;
                Export currentExport = null;
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Пропускаем комментарии и пустые строки
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;
                    
                    if (trimmedLine == "[PROJECT]")
                    {
                        if (currentProject != null)
                        {
                            projects.Add(currentProject);
                        }
                        currentProject = new Project();
                        currentExport = null;
                    }
                    else if (trimmedLine == "[EXPORT]")
                    {
                        if (currentProject != null)
                        {
                            currentExport = new Export();
                            currentProject.Exports.Add(currentExport);
                        }
                    }
                    else if (trimmedLine == "[TAB]")
                    {
                        if (currentExport != null)
                        {
                            var tab = new ExportTab();
                            currentExport.Tabs.Add(tab);
                        }
                    }
                    else if (trimmedLine.StartsWith("Name="))
                    {
                        var name = trimmedLine.Substring(5);
                        if (currentExport != null && currentExport.Tabs.Count > 0)
                        {
                            // Это вкладка
                            currentExport.Tabs.Last().Name = name;
                        }
                        else if (currentExport != null)
                        {
                            // Это выгрузка
                            currentExport.Name = name;
                        }
                        else if (currentProject != null)
                        {
                            // Это проект
                            currentProject.Name = name;
                        }
                    }
                    else if (trimmedLine.StartsWith("Path="))
                    {
                        var path = trimmedLine.Substring(5);
                        if (currentExport != null && currentExport.Tabs.Count > 0)
                        {
                            // Это вкладка
                            currentExport.Tabs.Last().FolderPath = path;
                        }
                        else if (currentExport != null)
                        {
                            // Это выгрузка
                            currentExport.FolderPath = path;
                        }
                        else if (currentProject != null)
                        {
                            // Это проект
                            currentProject.FolderPath = path;
                        }
                    }
                    else if (trimmedLine.StartsWith("RevitVersion="))
                    {
                        var version = trimmedLine.Substring(13);
                        if (currentExport != null && currentExport.Tabs.Count > 0)
                        {
                            currentExport.Tabs.Last().RevitVersion = version;
                        }
                    }
                }
                
                // Добавляем последний проект
                if (currentProject != null)
                {
                    projects.Add(currentProject);
                }
                
                // Проверяем существование папок и добавляем валидные проекты
                var validProjects = new List<Project>();
                foreach (var project in projects)
                {
                    if (Directory.Exists(project.FolderPath))
                    {
                        validProjects.Add(project);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Папка проекта '{project.Name}' не найдена по пути: {project.FolderPath}", 
                            "Предупреждение", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }

                return validProjects;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Project>();
            }
        }

    }
}
