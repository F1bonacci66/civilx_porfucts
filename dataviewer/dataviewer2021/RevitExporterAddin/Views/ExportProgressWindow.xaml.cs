using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RevitExporterAddin.Views
{
    public partial class ExportProgressWindow : Window
    {
        private bool _isCancelled = false;
        private DateTime _startTime;
        private DispatcherTimer _timer;

        public ExportProgressWindow()
        {
            InitializeComponent();
            _startTime = DateTime.Now;
            StartTimer();
        }

        public bool IsCancelled => _isCancelled;

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _startTime;
            ElapsedTimeText.Text = FormatTime(elapsed);
        }

        private string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        public void SetTotalModels(int totalModels)
        {
            Dispatcher.Invoke(() =>
            {
                TotalModelsText.Text = totalModels.ToString();
                RemainingModelsText.Text = totalModels.ToString();
            });
        }

        public void UpdateProgress(int processedModels, int totalModels, string currentModel, double currentModelProgress)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        ProcessedModelsText.Text = processedModels.ToString();
                        RemainingModelsText.Text = (totalModels - processedModels).ToString();
                        CurrentModelText.Text = currentModel;
                        CurrentModelProgressBar.Value = currentModelProgress;
                        ProgressText.Text = $"{currentModelProgress:F1}%";
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку обновления UI
                        System.Diagnostics.Debug.WriteLine($"Ошибка обновления UI: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Логируем ошибку Dispatcher
                System.Diagnostics.Debug.WriteLine($"Ошибка Dispatcher: {ex.Message}");
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем флаг отмены
            _isCancelled = true;
            
            // Останавливаем таймер
            if (_timer != null)
            {
                _timer.Stop();
            }
            
            // Обновляем UI
            Dispatcher.Invoke(() =>
            {
                CancelButton.IsEnabled = false;
                CancelButton.Content = "Отмена...";
                CancelButton.Background = System.Windows.Media.Brushes.Gray;
                
                // Обновляем текст текущей модели
                CurrentModelText.Text = "Отмена экспорта...";
                CurrentModelText.Foreground = System.Windows.Media.Brushes.Red;
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isCancelled = true;
            if (_timer != null)
            {
                _timer.Stop();
            }
            base.OnClosing(e);
        }

        private void WriteDetailedLog(string message)
        {
            try
            {
                // Получаем путь к детальному логу из MainWindow
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.WriteDetailedLog(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка записи в детальный лог: {ex.Message}");
            }
        }

        public void CompleteExport()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - НАЧАЛО");
                WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - НАЧАЛО");
                WriteDetailedLog($"🔍 ExportProgressWindow.CompleteExport() - Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                WriteDetailedLog($"🔍 ExportProgressWindow.CompleteExport() - IsCancelled: {_isCancelled}");
                
                // Останавливаем таймер
                if (_timer != null)
                {
                    WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - останавливаем таймер");
                    _timer.Stop();
                    System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - таймер остановлен");
                    WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - таймер остановлен");
                }
                else
                {
                    WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - таймер уже null");
                }

                WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - начинаем Dispatcher.Invoke");
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - ВНУТРИ Dispatcher.Invoke");
                        System.Diagnostics.Debug.WriteLine($"🔍 ExportProgressWindow.CompleteExport() - в Dispatcher.Invoke, _isCancelled = {_isCancelled}");
                        WriteDetailedLog($"🔍 ExportProgressWindow.CompleteExport() - в Dispatcher.Invoke, _isCancelled = {_isCancelled}");
                        WriteDetailedLog($"🔍 ExportProgressWindow.CompleteExport() - UI Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                        
                        if (_isCancelled)
                        {
                            // Если была отмена
                            System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - обработка отмены");
                            WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - обработка отмены");
                            CurrentModelText.Text = "Экспорт отменен!";
                            CurrentModelText.Foreground = System.Windows.Media.Brushes.Red;
                            CancelButton.Content = "Закрыть";
                            CancelButton.IsEnabled = true;
                            CancelButton.Background = System.Windows.Media.Brushes.Gray;
                        }
                        else
                        {
                            // Если экспорт завершен успешно
                            System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - обработка успешного завершения");
                            WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - обработка успешного завершения");
                            CurrentModelText.Text = "Экспорт завершен!";
                            CurrentModelText.Foreground = System.Windows.Media.Brushes.Green;
                            CurrentModelProgressBar.Value = 100;
                            ProgressText.Text = "100%";
                            CancelButton.Content = "Закрыть";
                            CancelButton.IsEnabled = true;
                            CancelButton.Background = System.Windows.Media.Brushes.Green;
                        }
                        
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - НАСТРОЙКА КНОПКИ");
                        System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - переподписка обработчика кнопки");
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - переподписка обработчика кнопки");
                        
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - отписываемся от старого обработчика");
                        // Переподписываем обработчик для кнопки "Закрыть"
                        CancelButton.Click -= CancelButton_Click;
                        
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - подписываемся на новый обработчик");
                        CancelButton.Click += (s, e) => 
                        {
                            WriteDetailedLog("🔍 ExportProgressWindow - КНОПКА 'ЗАКРЫТЬ' НАЖАТА");
                            System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow - кнопка 'Закрыть' нажата");
                            WriteDetailedLog("🔍 ExportProgressWindow - кнопка 'Закрыть' нажата");
                            WriteDetailedLog("🔍 ExportProgressWindow - вызываем Close()");
                            Close();
                            WriteDetailedLog("🔍 ExportProgressWindow - Close() выполнен");
                        };
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - обработчик кнопки настроен");
                        
                        System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - Dispatcher.Invoke завершен");
                        WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - Dispatcher.Invoke завершен");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ ExportProgressWindow.CompleteExport() - ошибка в Dispatcher.Invoke: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                        WriteDetailedLog($"❌ ExportProgressWindow.CompleteExport() - ошибка в Dispatcher.Invoke: {ex.Message}");
                        WriteDetailedLog($"❌ StackTrace: {ex.StackTrace}");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine("🔍 ExportProgressWindow.CompleteExport() - ЗАВЕРШЕН");
                WriteDetailedLog("🔍 ExportProgressWindow.CompleteExport() - ЗАВЕРШЕН");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ExportProgressWindow.CompleteExport() - критическая ошибка: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                WriteDetailedLog($"❌ ExportProgressWindow.CompleteExport() - критическая ошибка: {ex.Message}");
                WriteDetailedLog($"❌ StackTrace: {ex.StackTrace}");
            }
        }
    }
}
