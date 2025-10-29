using System.Windows;

namespace RevitExporterAddin.Views
{
    public partial class RenameWindow : Window
    {
        public string NewName { get; private set; }

        public RenameWindow(string currentName = "")
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
            NameTextBox.Focus();
            NameTextBox.SelectAll();
            NameTextBox.KeyDown += (s, e) => 
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    RenameButton_Click(s, e);
                }
            };
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите новое название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewName = NameTextBox.Text.Trim();
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

