using System.Windows;

namespace RevitExporterAddin.TestWpfApp
{
    public partial class AddExportWindow : Window
    {
        public string ExportName { get; private set; }
        public string RevitFilePath { get; private set; }

        public AddExportWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите Revit файл",
                Filter = "Revit файлы (*.rvt)|*.rvt|Все файлы (*.*)|*.*",
                DefaultExt = "rvt"
            };

            if (dialog.ShowDialog() == true)
            {
                RevitFilePathTextBox.Text = dialog.FileName;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExportNameTextBox.Text))
            {
                MessageBox.Show("Введите название выгрузки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(RevitFilePathTextBox.Text))
            {
                MessageBox.Show("Выберите Revit файл", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ExportName = ExportNameTextBox.Text.Trim();
            RevitFilePath = RevitFilePathTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
