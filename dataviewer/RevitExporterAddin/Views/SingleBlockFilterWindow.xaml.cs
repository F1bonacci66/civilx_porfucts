using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Views
{
    public partial class SingleBlockFilterWindow : Window
    {
        public List<CategoryFilterItem> Categories { get; private set; }
        public new bool DialogResult { get; private set; }

        public SingleBlockFilterWindow(List<CategoryFilterItem> categories)
        {
            InitializeComponent();
            Categories = categories;
            DataContext = this;
            UpdateTitleCount();
        }

        private void UpdateTitleCount()
        {
            var selectedCount = Categories.Count(c => c.IsSelected);
            var totalCount = Categories.Count;
            Title = $"Выбор категорий для экспорта (Выбрано: {selectedCount} из {totalCount})";
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Categories)
            {
                item.IsSelected = true;
            }
            UpdateTitleCount();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Categories)
            {
                item.IsSelected = false;
            }
            UpdateTitleCount();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Убираем проверку - теперь можно снять все галочки в блоке
            // Проверка будет на общем уровне в TabContentView
            System.Diagnostics.Debug.WriteLine($"ApplyButton_Click: Setting DialogResult = true");
            this.DialogResult = true;
            System.Diagnostics.Debug.WriteLine($"ApplyButton_Click: DialogResult set to {this.DialogResult}");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"CancelButton_Click: DialogResult = false");
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to changes to update the title count
            foreach (var item in Categories)
            {
                item.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(CategoryFilterItem.IsSelected))
                    {
                        UpdateTitleCount();
                    }
                };
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Window_Closing: DialogResult = {this.DialogResult}");
            System.Diagnostics.Debug.WriteLine($"Window_Closing: Cancel = {e.Cancel}");
        }
    }
}

