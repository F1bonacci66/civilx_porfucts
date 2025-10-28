using System.Windows;

namespace RevitExporterAddin.Views
{
    public partial class AddProjectWindow : Window
    {
        public string ProjectName { get; private set; }

        public AddProjectWindow()
        {
            InitializeComponent();
            ProjectNameTextBox.Focus();
            ProjectNameTextBox.KeyDown += (s, e) => 
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CreateButton_Click(s, e);
                }
            };
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                MessageBox.Show("Введите название проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProjectName = ProjectNameTextBox.Text.Trim();
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
