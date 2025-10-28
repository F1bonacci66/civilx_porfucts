using System;
using System.IO;
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
                // Создаем вкладку CivilX, если она не существует
                string tabName = "CivilX";
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    // Вкладка уже существует, это нормально
                }

                // Получаем или создаем панель DataHub
                RibbonPanel dataPanel = GetOrCreatePanel(application, tabName, "DataHub");

                // Получаем путь к сборке
                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                // Создаем кнопку DataViewer
                PushButtonData buttonData = new PushButtonData(
                    "DataViewer",
                    "DataViewer",
                    assemblyPath,
                    "RevitExporterAddin.ExportCommand");

                buttonData.ToolTip = "Управление проектами и экспорт данных из Revit в CSV";
                buttonData.LongDescription = "Открывает интерфейс для управления проектами экспорта данных из Revit";
                
                // Загружаем иконки из файлов
                buttonData.Image = LoadIconFromFile("dataviewer_16.png");
                buttonData.LargeImage = LoadIconFromFile("dataviewer_32.png");
                
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

        private static RibbonPanel GetOrCreatePanel(UIControlledApplication application, string tabName, string panelName)
        {
            try
            {
                // Пытаемся получить существующую панель
                var panels = application.GetRibbonPanels(tabName);
                foreach (var panel in panels)
                {
                    if (panel.Name == panelName)
                    {
                        return panel;
                    }
                }
                
                // Если панель не найдена, создаем новую
                return application.CreateRibbonPanel(tabName, panelName);
            }
            catch
            {
                // Если что-то пошло не так, создаем панель
                return application.CreateRibbonPanel(tabName, panelName);
            }
        }

        private static BitmapImage LoadIconFromFile(string iconFileName)
        {
            try
            {
                // Получаем путь к папке с иконками (рядом с DLL)
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                string iconPath = Path.Combine(assemblyDir, iconFileName);

                // Проверяем существование файла
                if (!File.Exists(iconPath))
                {
                    // Fallback: создаем простую иконку
                    return CreateSimpleIcon(32, 32);
                }

                // Загружаем иконку из файла
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(iconPath);
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем простую иконку
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки иконки {iconFileName}: {ex.Message}");
                return CreateSimpleIcon(32, 32);
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

