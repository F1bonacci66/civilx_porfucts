using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace RevitExporterAddin
{
    public class RibbonManager
    {
        public static void CreateRibbonTab(UIControlledApplication application)
        {
            try
            {
                // Создаем вкладку CivilX
                string tabName = "CivilX";
                application.CreateRibbonTab(tabName);

                // Создаем группу Data
                RibbonPanel dataPanel = application.CreateRibbonPanel(tabName, "Data");

                // Получаем путь к сборке
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // Создаем кнопку Data Manager
                PushButtonData buttonData = new PushButtonData(
                    "DataManager",
                    "Data Manager",
                    assemblyPath,
                    "RevitExporterAddin.ExportCommand");

                buttonData.ToolTip = "Управление проектами и экспорт данных из Revit в CSV";
                buttonData.LongDescription = "Открывает интерфейс для управления проектами экспорта данных из Revit";
                
                // Создаем простую иконку для кнопки
                buttonData.Image = CreateSimpleIcon(16, 16);
                buttonData.LargeImage = CreateSimpleIcon(32, 32);
                
                // Устанавливаем контекст команды - всегда доступна
                buttonData.AvailabilityClassName = "RevitExporterAddin.AlwaysAvailable";

                // Добавляем кнопку в группу
                PushButton button = dataPanel.AddItem(buttonData) as PushButton;
                
                // Принудительно устанавливаем свойства кнопки
                if (button != null)
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка создания ленты", $"Ошибка при создании вкладки CivilX: {ex.Message}");
            }
        }

        private static BitmapImage CreateSimpleIcon(int width, int height)
        {
            try
            {
                // Создаем простое изображение
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.LightBlue);
                    g.DrawRectangle(System.Drawing.Pens.DarkBlue, 0, 0, width - 1, height - 1);
                    g.DrawString("DM", new System.Drawing.Font("Arial", 8, FontStyle.Bold), System.Drawing.Brushes.DarkBlue, 2, 2);
                }

                // Конвертируем в BitmapImage
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }
    }

    public class AlwaysAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true; // Команда всегда доступна
        }
    }
}

