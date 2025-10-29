using System;
using System.Windows;
using RevitExporter.Services;
using RevitExporter.ViewModels;
using RevitExporter.Views;

namespace RevitExporter
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаем сервисы
            var dataService = new DataService();
            var revitExporter = new Services.RevitExporter();

            // Создаем главное окно и ViewModel
            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(dataService, revitExporter);
            
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
        }
    }
}
