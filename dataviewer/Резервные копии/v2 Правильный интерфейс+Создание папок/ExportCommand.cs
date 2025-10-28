using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExporterAddin.Models;
using RevitExporterAddin.Services;
using RevitExporterAddin.Views;
using System.Windows;

namespace RevitExporterAddin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Получаем доступ к приложению Revit
                var app = commandData.Application.Application;
                
                // Открываем основной интерфейс управления проектами
                var mainWindow = new Views.MainWindow(app);
                mainWindow.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = string.Format("Ошибка запуска приложения: {0}", ex.Message);
                
                // Показываем детальную ошибку пользователю
                TaskDialog.Show("Revit Exporter - Ошибка", 
                    string.Format("Произошла ошибка при запуске приложения:\n\n{0}\n\n{1}", 
                        ex.Message, ex.StackTrace));
                
                return Result.Failed;
            }
        }
    }
}
