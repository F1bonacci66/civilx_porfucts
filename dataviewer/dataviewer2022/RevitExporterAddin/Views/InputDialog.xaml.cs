using System.Windows;

namespace RevitExporterAddin.Views
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string title, string description, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            TitleText.Text = title;
            DescriptionText.Text = description;
            InputTextBox.Text = defaultValue;
            InputTextBox.SelectAll();
            InputTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = InputTextBox.Text.Trim();
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


