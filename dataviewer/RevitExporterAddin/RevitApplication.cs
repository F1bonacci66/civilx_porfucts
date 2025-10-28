using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitExporterAddin
{
    [Transaction(TransactionMode.Manual)]
    public class RevitApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Создаем вкладку CivilX при запуске Revit
                RibbonManager.CreateRibbonTab(application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка запуска", $"Ошибка при создании вкладки CivilX: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}