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
            _isCancelled = true;
            if (_timer != null)
            {
                _timer.Stop();
            }
            CancelButton.IsEnabled = false;
            CancelButton.Content = "Отмена...";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isCancelled = true;
            base.OnClosing(e);
        }

        public void CompleteExport()
        {
            // Останавливаем таймер
            if (_timer != null)
            {
                _timer.Stop();
            }

            Dispatcher.Invoke(() =>
            {
                CurrentModelText.Text = "Экспорт завершен!";
                CurrentModelProgressBar.Value = 100;
                ProgressText.Text = "100%";
                CancelButton.Content = "Закрыть";
                CancelButton.IsEnabled = true;
                CancelButton.Click -= CancelButton_Click;
                CancelButton.Click += (s, e) => Close();
            });
        }
    }
}
