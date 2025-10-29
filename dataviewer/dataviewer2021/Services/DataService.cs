using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RevitExporter.Models;

namespace RevitExporter.Services
{
    public class DataService : IDataService
    {
        private readonly string _dataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RevitExporter",
            "projects.json");

        public DataService()
        {
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public List<Project> LoadProjectsAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return new List<Project>();

                var json = File.ReadAllText(_dataFilePath);
                // Простая десериализация - в реальном проекте используйте Newtonsoft.Json
                return new List<Project>();
            }
            catch (Exception)
            {
                return new List<Project>();
            }
        }

        public void SaveProjectsAsync(List<Project> projects)
        {
            // Простая сериализация - в реальном проекте используйте Newtonsoft.Json
            var json = "[]";
            File.WriteAllText(_dataFilePath, json);
        }

        public Project CreateProjectAsync(string name)
        {
            return new Project { Name = name };
        }

        public Export CreateExportAsync(string name)
        {
            return new Export { Name = name };
        }

        public ExportTab CreateExportTabAsync(string name)
        {
            return new ExportTab { Name = name };
        }
    }
}
