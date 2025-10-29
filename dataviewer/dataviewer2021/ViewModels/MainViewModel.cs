using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RevitExporter.Models;
using RevitExporter.Services;

namespace RevitExporter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IRevitExporter _revitExporter;
        private Project _selectedProject;
        private Export _selectedExport;
        private ExportTab _selectedTab;
        private string _statusMessage = string.Empty;
        private bool _isExporting;

        public ObservableCollection<Project> Projects { get; private set; }
        public ObservableCollection<Export> Exports { get; private set; }
        public ObservableCollection<ExportTab> Tabs { get; private set; }

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    LoadExports();
                }
            }
        }

        public Export SelectedExport
        {
            get { return _selectedExport; }
            set
            {
                if (SetProperty(ref _selectedExport, value))
                {
                    LoadTabs();
                }
            }
        }

        public ExportTab SelectedTab
        {
            get { return _selectedTab; }
            set { SetProperty(ref _selectedTab, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public bool IsExporting
        {
            get { return _isExporting; }
            set { SetProperty(ref _isExporting, value); }
        }

        public ICommand AddProjectCommand { get; private set; }
        public ICommand RemoveProjectCommand { get; private set; }
        public ICommand AddExportCommand { get; private set; }
        public ICommand RemoveExportCommand { get; private set; }
        public ICommand AddTabCommand { get; private set; }
        public ICommand RemoveTabCommand { get; private set; }
        public ICommand BrowseFolderCommand { get; private set; }
        public ICommand StartExportCommand { get; private set; }

        public MainViewModel(IDataService dataService, IRevitExporter revitExporter)
        {
            _dataService = dataService;
            _revitExporter = revitExporter;

            Projects = new ObservableCollection<Project>();
            Exports = new ObservableCollection<Export>();
            Tabs = new ObservableCollection<ExportTab>();

            AddProjectCommand = new RelayCommand(AddProject);
            RemoveProjectCommand = new RelayCommand(RemoveProject, CanRemoveProject);
            AddExportCommand = new RelayCommand(AddExport, CanAddExport);
            RemoveExportCommand = new RelayCommand(RemoveExport, CanRemoveExport);
            AddTabCommand = new RelayCommand(AddTab, CanAddTab);
            RemoveTabCommand = new RelayCommand(RemoveTab, CanRemoveTab);
            BrowseFolderCommand = new RelayCommand(BrowseFolder, CanBrowseFolder);
            StartExportCommand = new RelayCommand(StartExport, CanStartExport);

            LoadProjects();
        }

        private void LoadProjects()
        {
            try
            {
                var projects = _dataService.LoadProjectsAsync();
                Projects.Clear();
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Ошибка загрузки проектов: {0}", ex.Message);
            }
        }

        private void LoadExports()
        {
            Exports.Clear();
            if (SelectedProject != null)
            {
                foreach (var export in SelectedProject.Exports)
                {
                    Exports.Add(export);
                }
            }
        }

        private void LoadTabs()
        {
            Tabs.Clear();
            if (SelectedExport != null)
            {
                foreach (var tab in SelectedExport.Tabs)
                {
                    Tabs.Add(tab);
                }
            }
        }

        private void AddProject()
        {
            var projectName = string.Format("Новый проект {0}", Projects.Count + 1);
            var project = _dataService.CreateProjectAsync(projectName);
            Projects.Add(project);
            SelectedProject = project;
            SaveProjects();
        }

        private void RemoveProject()
        {
            if (SelectedProject != null)
            {
                Projects.Remove(SelectedProject);
                SelectedProject = null;
                SaveProjects();
            }
        }

        private bool CanRemoveProject() { return SelectedProject != null; }

        private void AddExport()
        {
            if (SelectedProject != null)
            {
                var exportName = string.Format("Выгрузка {0}", SelectedProject.Exports.Count + 1);
                var export = _dataService.CreateExportAsync(exportName);
                SelectedProject.Exports.Add(export);
                Exports.Add(export);
                SelectedExport = export;
                SaveProjects();
            }
        }

        private bool CanAddExport() { return SelectedProject != null; }

        private void RemoveExport()
        {
            if (SelectedExport != null && SelectedProject != null)
            {
                SelectedProject.Exports.Remove(SelectedExport);
                Exports.Remove(SelectedExport);
                SelectedExport = null;
                SaveProjects();
            }
        }

        private bool CanRemoveExport() { return SelectedExport != null; }

        private void AddTab()
        {
            if (SelectedExport != null)
            {
                var tabName = string.Format("Вкладка {0}", SelectedExport.Tabs.Count + 1);
                var tab = _dataService.CreateExportTabAsync(tabName);
                SelectedExport.Tabs.Add(tab);
                Tabs.Add(tab);
                SelectedTab = tab;
                SaveProjects();
            }
        }

        private bool CanAddTab() { return SelectedExport != null; }

        private void RemoveTab()
        {
            if (SelectedTab != null && SelectedExport != null)
            {
                SelectedExport.Tabs.Remove(SelectedTab);
                Tabs.Remove(SelectedTab);
                SelectedTab = null;
                SaveProjects();
            }
        }

        private bool CanRemoveTab() { return SelectedTab != null; }

        private void BrowseFolder()
        {
            if (SelectedTab != null)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Выберите папку с моделями Revit"
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedTab.ModelsFolder = dialog.SelectedPath;
                }
            }
        }

        private bool CanBrowseFolder() { return SelectedTab != null; }

        private async void StartExport()
        {
            if (SelectedTab == null) return;

            IsExporting = true;
            StatusMessage = "Начинаем экспорт...";

            try
            {
                var progress = new Progress<string>(message => StatusMessage = message);
                await _revitExporter.ExportToCsvAsync(SelectedTab, progress);
                StatusMessage = "Экспорт успешно завершен!";
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Ошибка экспорта: {0}", ex.Message);
            }
            finally
            {
                IsExporting = false;
            }
        }

        private bool CanStartExport() { return SelectedTab != null && !IsExporting; }

        private void SaveProjects()
        {
            try
            {
                // В упрощенной версии просто сохраняем без async
                _dataService.SaveProjectsAsync(new List<Project>(Projects));
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format("Ошибка сохранения: {0}", ex.Message);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) 
        { 
            if (_canExecute != null)
                return _canExecute();
            return true;
        }

        public void Execute(object parameter) { _execute(); }
    }
}
