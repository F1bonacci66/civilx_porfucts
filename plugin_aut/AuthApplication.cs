using System;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace CivilXAuthPlugin
{
    public class AuthApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Сначала создаем вкладку CivilX, если она не существует
                try
                {
                    application.CreateRibbonTab("CivilX");
                }
                catch
                {
                    // Вкладка уже существует, это нормально
                }

                // Получаем или создаем панель "Настройки" в вкладке "CivilX"
                RibbonPanel panel = GetOrCreatePanel(application, "CivilX", "Настройки");
                
                // Создаем кнопку для авторизации
                PushButtonData buttonData = new PushButtonData(
                    "AuthButton",
                    "Авторизация",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    "CivilXAuthPlugin.AuthCommand"
                );
                
                buttonData.LongDescription = "Открыть окно авторизации CivilX";
                buttonData.ToolTip = "Авторизация в системе CivilX";
                
                // Загружаем иконки из файлов
                buttonData.Image = LoadIconFromFile("auth_16.png");
                buttonData.LargeImage = LoadIconFromFile("auth_32.png");
                
                // Устанавливаем контекст команды - всегда доступна
                buttonData.AvailabilityClassName = "CivilXAuthPlugin.AlwaysAvailable";
                
                PushButton button = panel.AddItem(buttonData) as PushButton;
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка при запуске плагина: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
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
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                string iconPath = Path.Combine(assemblyDir, iconFileName);

                // Проверяем существование файла
                if (!File.Exists(iconPath))
                {
                    // Fallback: создаем простую иконку
                    return CreateAuthIcon(32, 32);
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
                return CreateAuthIcon(32, 32);
            }
        }

        private static BitmapImage CreateAuthIcon(int width, int height)
        {
            try
            {
                // Создаем простое изображение для авторизации
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.LightGreen);
                    g.DrawRectangle(System.Drawing.Pens.DarkGreen, 0, 0, width - 1, height - 1);
                    g.DrawString("AU", new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.DarkGreen, 2, 2);
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

    [Transaction(TransactionMode.Manual)]
    public class AuthCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Открываем окно авторизации
                AuthWindow authWindow = new AuthWindow();
                authWindow.ShowDialog();
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = $"Ошибка при открытии окна авторизации: {ex.Message}";
                return Result.Failed;
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
