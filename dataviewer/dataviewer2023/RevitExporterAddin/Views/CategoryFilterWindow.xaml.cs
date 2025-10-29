using System;
using System.Windows;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Views
{
    public partial class CategoryFilterWindow : Window
    {
        public CategoryFilter CategoryFilter { get; private set; }
        public new bool DialogResult { get; private set; }

        public CategoryFilterWindow(CategoryFilter categoryFilter = null)
        {
            InitializeComponent();
            
            // Инициализируем фильтр
            CategoryFilter = categoryFilter ?? new CategoryFilter();
            DataContext = CategoryFilter;
            
            // Обновляем счетчики
            UpdateCounters();
            
            // Подписываемся на изменения
            SubscribeToChanges();
        }

        private void SubscribeToChanges()
        {
            // Подписываемся на изменения во всех блоках
            foreach (var block in CategoryFilter.CategoryBlocks.Values)
            {
                foreach (var item in block)
                {
                    // Создаем слабую ссылку для избежания утечек памяти
                    var weakRef = new WeakReference(this);
                    item.PropertyChanged += (s, e) =>
                    {
                        if (weakRef.IsAlive && weakRef.Target is CategoryFilterWindow window)
                        {
                            window.Dispatcher.Invoke(() => window.UpdateCounters());
                        }
                    };
                }
            }
        }

        private void UpdateCounters()
        {
            SelectedCountText.Text = CategoryFilter.GetSelectedCount().ToString();
            TotalCountText.Text = CategoryFilter.GetTotalCount().ToString();
        }

        #region Кнопки выбора по блокам

        private void SelectAllModel_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Элементы модели");
        }

        private void DeselectAllModel_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Элементы модели");
        }

        private void SelectAllMEP_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Инженерные системы (MEP)");
        }

        private void DeselectAllMEP_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Инженерные системы (MEP)");
        }

        private void SelectAllAnalytical_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Аналитические модели");
        }

        private void DeselectAllAnalytical_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Аналитические модели");
        }

        private void SelectAllAnnotations_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Аннотации и оформление");
        }

        private void DeselectAllAnnotations_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Аннотации и оформление");
        }

        private void SelectAllProject_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Организация проекта и виды");
        }

        private void DeselectAllProject_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Организация проекта и виды");
        }

        private void SelectAllSystem_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAllInBlock("Данные и системные категории");
        }

        private void DeselectAllSystem_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAllInBlock("Данные и системные категории");
        }

        #endregion

        #region Глобальные кнопки

        private void SelectAllGlobal_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.SelectAll();
        }

        private void DeselectAllGlobal_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilter.DeselectAll();
        }

        #endregion

        #region Кнопки диалога

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что выбрана хотя бы одна категория
            if (CategoryFilter.GetSelectedCount() == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы одну категорию для экспорта.", 
                               "Предупреждение", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
