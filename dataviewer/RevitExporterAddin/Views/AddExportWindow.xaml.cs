using System.Windows;

namespace RevitExporterAddin.Views
{
    public partial class AddExportWindow : Window
    {
        public string ExportName { get; private set; }

        public AddExportWindow()
        {
            InitializeComponent();
            ExportNameTextBox.Focus();
            ExportNameTextBox.KeyDown += (s, e) => 
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CreateButton_Click(s, e);
                }
            };
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExportNameTextBox.Text))
            {
                MessageBox.Show("Введите название выгрузки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ExportName = ExportNameTextBox.Text.Trim();
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
