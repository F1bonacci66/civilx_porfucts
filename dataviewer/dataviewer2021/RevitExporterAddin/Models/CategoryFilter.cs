using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RevitExporterAddin.Models
{
    /// <summary>
    /// Модель для фильтрации категорий по блокам
    /// </summary>
    public class CategoryFilter
    {
        public Dictionary<string, List<CategoryFilterItem>> CategoryBlocks { get; set; }

        public CategoryFilter()
        {
            CategoryBlocks = new Dictionary<string, List<CategoryFilterItem>>();
            InitializeDefaultCategories();
        }

        /// <summary>
        /// Инициализация категорий по блокам
        /// </summary>
        private void InitializeDefaultCategories()
        {
            // 1. Элементы модели
            CategoryBlocks["Элементы модели"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Антураж", true),
                new CategoryFilterItem("Арматурная сетка несущей конструкции", true),
                new CategoryFilterItem("Арматурные пучки несущих конструкций", true),
                new CategoryFilterItem("Балочные системы", true),
                new CategoryFilterItem("Ванты моста", true),
                new CategoryFilterItem("Временные конструкции", true),
                new CategoryFilterItem("Витражные системы", true),
                new CategoryFilterItem("Двери", true),
                new CategoryFilterItem("Дорожки", true),
                new CategoryFilterItem("Желоба", true),
                new CategoryFilterItem("Знаки", true),
                new CategoryFilterItem("Импосты витража", true),
                new CategoryFilterItem("Искусственный ландшафт", true),
                new CategoryFilterItem("Камеры", true),
                new CategoryFilterItem("Каркас моста", true),
                new CategoryFilterItem("Каркас несущий", true),
                new CategoryFilterItem("Колонны", true),
                new CategoryFilterItem("Комплекты мебели", true),
                new CategoryFilterItem("Координационная модель", true),
                new CategoryFilterItem("Крыши", true),
                new CategoryFilterItem("Лестницы", true),
                new CategoryFilterItem("Мебель", true),
                new CategoryFilterItem("Мостовое полотно", true),
                new CategoryFilterItem("Несущая арматура", true),
                new CategoryFilterItem("Несущие", true),
                new CategoryFilterItem("Несущие колонны", true),
                new CategoryFilterItem("Обобщенные модели", true),
                new CategoryFilterItem("Оборудование", true),
                new CategoryFilterItem("Оборудование для предприятий общественного питания", true),
                new CategoryFilterItem("Ограждение", true),
                new CategoryFilterItem("Озеленение", true),
                new CategoryFilterItem("Окна", true),
                new CategoryFilterItem("Опоры", true),
                new CategoryFilterItem("Панели витража", true),
                new CategoryFilterItem("Пандус", true),
                new CategoryFilterItem("Парковка", true),
                new CategoryFilterItem("Перекрытия", true),
                new CategoryFilterItem("Помещения", true),
                new CategoryFilterItem("Потолки", true),
                new CategoryFilterItem("Проемы для шахты", true),
                new CategoryFilterItem("Пространства", true),
                new CategoryFilterItem("Ребра жесткости несущей конструкции", true),
                new CategoryFilterItem("Сантехнические приборы", true),
                new CategoryFilterItem("Сборки", true),
                new CategoryFilterItem("Соединения несущих конструкций", true),
                new CategoryFilterItem("Соединители несущей арматуры", true),
                new CategoryFilterItem("Специальное оборудование", true),
                new CategoryFilterItem("Стены", true),
                new CategoryFilterItem("Топография", true),
                new CategoryFilterItem("Траектории лестницы", true),
                new CategoryFilterItem("Устои", true),
                new CategoryFilterItem("Участки", true),
                new CategoryFilterItem("Фермы", true),
                new CategoryFilterItem("Форма арматурного стержня", true),
                new CategoryFilterItem("Формы", true),
                new CategoryFilterItem("Фундамент несущей конструкции", true),
                new CategoryFilterItem("Части", true),
                new CategoryFilterItem("Шкафы", true)
            };

            // 2. Инженерные системы (MEP)
            CategoryBlocks["Инженерные системы (MEP)"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Арматура воздуховодов", true),
                new CategoryFilterItem("Арматура трубопроводов", true),
                new CategoryFilterItem("Аудиовизуальные устройства", true),
                new CategoryFilterItem("Воздуховоды", true),
                new CategoryFilterItem("Воздухораспределители", true),
                new CategoryFilterItem("Выключатели", true),
                new CategoryFilterItem("Гибкие воздуховоды", true),
                new CategoryFilterItem("Гибкие трубы", true),
                new CategoryFilterItem("Датчики", true),
                new CategoryFilterItem("Зоны ОВК", true),
                new CategoryFilterItem("Зоны системы", true),
                new CategoryFilterItem("Зоны электрической нагрузки", true),
                new CategoryFilterItem("Кабельные лотки", true),
                new CategoryFilterItem("Короба", true),
                new CategoryFilterItem("Медицинское оборудование", true),
                new CategoryFilterItem("Наборы оборудования", true),
                new CategoryFilterItem("Оборудование зоны", true),
                new CategoryFilterItem("Охранная сигнализация", true),
                new CategoryFilterItem("Подвески из базы данных производителя MEP", true),
                new CategoryFilterItem("Пожарная сигнализация", true),
                new CategoryFilterItem("Провода", true),
                new CategoryFilterItem("Сегменты труб", true),
                new CategoryFilterItem("Система коммутации", true),
                new CategoryFilterItem("Система противопожарной защиты", true),
                new CategoryFilterItem("Системы воздуховодов", true),
                new CategoryFilterItem("Системы воздухоснабжения", true),
                new CategoryFilterItem("Соединительные детали воздуховодов", true),
                new CategoryFilterItem("Соединительные детали кабельных лотков", true),
                new CategoryFilterItem("Соединительные детали коробов", true),
                new CategoryFilterItem("Соединительные детали трубопроводов", true),
                new CategoryFilterItem("Спринклеры", true),
                new CategoryFilterItem("Телефонные устройства", true),
                new CategoryFilterItem("Трассы", true),
                new CategoryFilterItem("Трубопроводные системы", true),
                new CategoryFilterItem("Трубы", true),
                new CategoryFilterItem("Устройства вызова и оповещения", true),
                new CategoryFilterItem("Устройства механического управления", true),
                new CategoryFilterItem("Устройства связи", true),
                new CategoryFilterItem("Электрические приборы", true),
                new CategoryFilterItem("Электрические цепи", true),
                new CategoryFilterItem("Электрооборудование", true),
                new CategoryFilterItem("Элементы герметизации из базы данных производителя MEP", true),
                new CategoryFilterItem("Элементы воздуховодов из базы данных производителя MEP", true)
            };

            // 3. Аналитические модели
            CategoryBlocks["Аналитические модели"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Аналитические модели балки", true),
                new CategoryFilterItem("Аналитические модели изолированных фундаментов", true),
                new CategoryFilterItem("Аналитические модели колонн", true),
                new CategoryFilterItem("Аналитические модели ленточных фундаментов", true),
                new CategoryFilterItem("Аналитические модели перекрытий", true),
                new CategoryFilterItem("Аналитические модели раскосов", true),
                new CategoryFilterItem("Аналитические модели стен", true),
                new CategoryFilterItem("Аналитические модели фундаментных плит", true),
                new CategoryFilterItem("Аналитические панели", true),
                new CategoryFilterItem("Аналитические поверхности", true),
                new CategoryFilterItem("Аналитические проемы", true),
                new CategoryFilterItem("Аналитические пространства", true),
                new CategoryFilterItem("Аналитические элементы", true),
                new CategoryFilterItem("Аналитический источник питания электрической цепи", true),
                new CategoryFilterItem("Аналитический переключатель питания электрооборудования", true),
                new CategoryFilterItem("Аналитический трансформатор электрооборудования", true),
                new CategoryFilterItem("Внутренние нагрузки несущих конструкций", true),
                new CategoryFilterItem("Жесткие связи", true),
                new CategoryFilterItem("Результаты расчета", true),
                new CategoryFilterItem("Соединения труб аналитической модели", true),
                new CategoryFilterItem("Узлы аналитической модели", true)
            };

            // 4. Аннотации и оформление
            CategoryBlocks["Аннотации и оформление"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Адаптивные точки", true),
                new CategoryFilterItem("Аннотации для нескольких арматурных стержней", true),
                new CategoryFilterItem("Аннотации к несущим элементам", true),
                new CategoryFilterItem("Вспомогательные линии", true),
                new CategoryFilterItem("Высотные отметки", true),
                new CategoryFilterItem("Головные части уровней", true),
                new CategoryFilterItem("Граница фрагмента", true),
                new CategoryFilterItem("Координаты точек", true),
                new CategoryFilterItem("Легенды цветовых обозначений", true),
                new CategoryFilterItem("Легенды цветовых обозначений трубопроводов", true),
                new CategoryFilterItem("Легенды к цветовому обозначению воздуховода", true),
                new CategoryFilterItem("Линии", true),
                new CategoryFilterItem("Линия сечения", true),
                new CategoryFilterItem("Марка мультивыноски", true),
                new CategoryFilterItem("Марки аналитических панелей", true),
                new CategoryFilterItem("Марки аналитических элементов", true),
                new CategoryFilterItem("Марки аналитического проема", true),
                new CategoryFilterItem("Марки анкеров", true),
                new CategoryFilterItem("Марки арматурной сетки несущей конструкции", true),
                new CategoryFilterItem("Марки арматурных пучков несущих конструкций", true),
                new CategoryFilterItem("Марки арматуры воздуховодов", true),
                new CategoryFilterItem("Марки армирования по площади несущих конструкций", true),
                new CategoryFilterItem("Марки армирования по траектории несущих конструкций", true),
                new CategoryFilterItem("Марки балочных систем", true),
                new CategoryFilterItem("Марки башен опор", true),
                new CategoryFilterItem("Марки болтов", true),
                new CategoryFilterItem("Марки вантов моста", true),
                new CategoryFilterItem("Марки витражных систем", true),
                new CategoryFilterItem("Марки вн. сосредоточенных нагрузок", true),
                new CategoryFilterItem("Марки внутренней изоляции воздуховодов", true),
                new CategoryFilterItem("Марки внутренних линейных нагрузок", true),
                new CategoryFilterItem("Марки внутренних распределенных нагрузок", true),
                new CategoryFilterItem("Марки воздуховодов", true),
                new CategoryFilterItem("Марки воздуховодов из базы данных производителя MEP", true),
                new CategoryFilterItem("Марки воздухораспределителей", true),
                new CategoryFilterItem("Марки временных конструкций", true),
                new CategoryFilterItem("Марки выступающих профилей", true),
                new CategoryFilterItem("Марки выключателей", true),
                new CategoryFilterItem("Марки гибких упоров", true),
                new CategoryFilterItem("Марки гибких труб", true),
                new CategoryFilterItem("Марки гибкого воздуховода", true),
                new CategoryFilterItem("Марки генплана", true),
                new CategoryFilterItem("Марки группы моделей", true),
                new CategoryFilterItem("Марки датчиков", true),
                new CategoryFilterItem("Марки дверей", true),
                new CategoryFilterItem("Марки для аудиовизуальных устройств", true),
                new CategoryFilterItem("Марки для вертикальной циркуляции", true),
                new CategoryFilterItem("Марки для знаков", true),
                new CategoryFilterItem("Марки для медицинского оборудования", true),
                new CategoryFilterItem("Марки для системы противопожарной защиты", true),
                new CategoryFilterItem("Марки демпфера вибрации", true),
                new CategoryFilterItem("Марки деталей", true),
                new CategoryFilterItem("Марки диафрагм моста", true),
                new CategoryFilterItem("Марки дорог", true),
                new CategoryFilterItem("Марки железобетонных конструкций", true),
                new CategoryFilterItem("Марки желобов", true),
                new CategoryFilterItem("Марки жестких связей", true),
                new CategoryFilterItem("Марки зоны", true),
                new CategoryFilterItem("Марки зоны системы", true),
                new CategoryFilterItem("Марки зон", true),
                new CategoryFilterItem("Марки изоляции воздуховодов", true),
                new CategoryFilterItem("Марки изоляции труб", true),
                new CategoryFilterItem("Марки импостов витражей", true),
                new CategoryFilterItem("Марки кабельных лотков", true),
                new CategoryFilterItem("Марки каркаса моста", true),
                new CategoryFilterItem("Марки ключевых пометок", true),
                new CategoryFilterItem("Марки колонн", true),
                new CategoryFilterItem("Марки комплектов мебели", true),
                new CategoryFilterItem("Марки коробов", true),
                new CategoryFilterItem("Марки крыш", true),
                new CategoryFilterItem("Марки лестниц", true),
                new CategoryFilterItem("Марки лестничного марша", true),
                new CategoryFilterItem("Марки лестничного отдыха", true),
                new CategoryFilterItem("Марки линейных нагрузок", true),
                new CategoryFilterItem("Марки лобовых досок", true),
                new CategoryFilterItem("Марки материалов", true),
                new CategoryFilterItem("Марки мебели", true),
                new CategoryFilterItem("Марки мостового полотна", true),
                new CategoryFilterItem("Марки набора оборудования", true),
                new CategoryFilterItem("Марки нагрузок на основе зоны", true),
                new CategoryFilterItem("Марки несущего каркаса", true),
                new CategoryFilterItem("Марки несущей арматуры", true),
                new CategoryFilterItem("Марки обобщенной модели", true),
                new CategoryFilterItem("Марки оборудования", true),
                new CategoryFilterItem("Марки оборудования для предприятий общественного питания", true),
                new CategoryFilterItem("Марки ограждения", true),
                new CategoryFilterItem("Марки оголовков опор", true),
                new CategoryFilterItem("Марки окон", true),
                new CategoryFilterItem("Марки опор", true),
                new CategoryFilterItem("Марки оснований", true),
                new CategoryFilterItem("Марки отметок", true),
                new CategoryFilterItem("Марки отверстий", true),
                new CategoryFilterItem("Марки охранной сигнализации", true),
                new CategoryFilterItem("Марки панелей витража", true),
                new CategoryFilterItem("Марки парковки", true),
                new CategoryFilterItem("Марки перекрытий", true),
                new CategoryFilterItem("Марки перекрестных раскосов моста", true),
                new CategoryFilterItem("Марки перил", true),
                new CategoryFilterItem("Марки пластин", true),
                new CategoryFilterItem("Марки по нескольким категориям", true),
                new CategoryFilterItem("Марки подвески из базы данных производителя MEP", true),
                new CategoryFilterItem("Марки подшивных досок", true),
                new CategoryFilterItem("Марки пожарной сигнализации", true),
                new CategoryFilterItem("Марки пометочных облаков", true),
                new CategoryFilterItem("Марки помещений", true),
                new CategoryFilterItem("Марки профиля", true),
                new CategoryFilterItem("Марки пандусов", true),
                new CategoryFilterItem("Марки разрезов", true),
                new CategoryFilterItem("Марки распределенных нагрузок", true),
                new CategoryFilterItem("Марки ребер плит", true),
                new CategoryFilterItem("Марки ребер жесткости несущей конструкции", true),
                new CategoryFilterItem("Марки связанных файлов RVT", true),
                new CategoryFilterItem("Марки сегментов границы участка", true),
                new CategoryFilterItem("Марки сантехнического оборудования", true),
                new CategoryFilterItem("Марки сантехнических приборов", true),
                new CategoryFilterItem("Марки сборок", true),
                new CategoryFilterItem("Марки свай опор", true),
                new CategoryFilterItem("Марки свай устоев", true),
                new CategoryFilterItem("Марки сварных швов", true),
                new CategoryFilterItem("Марки силовых электроприборов", true),
                new CategoryFilterItem("Марки соединителей несущей арматуры", true),
                new CategoryFilterItem("Марки соединительных деталей воздуховодов", true),
                new CategoryFilterItem("Марки соединительных деталей кабельных лотков", true),
                new CategoryFilterItem("Марки соединительных деталей коробов", true),
                new CategoryFilterItem("Марки соединительных деталей трубопроводов", true),
                new CategoryFilterItem("Марки соединений несущих конструкций", true),
                new CategoryFilterItem("Марки сосредоточенных нагрузок", true),
                new CategoryFilterItem("Марки специального оборудования", true),
                new CategoryFilterItem("Марки спринклеров", true),
                new CategoryFilterItem("Марки стен", true),
                new CategoryFilterItem("Марки стен опор", true),
                new CategoryFilterItem("Марки стен устоев", true),
                new CategoryFilterItem("Марки стоек опор", true),
                new CategoryFilterItem("Марки температурных швов", true),
                new CategoryFilterItem("Марки траектории движения", true),
                new CategoryFilterItem("Марки трассы", true),
                new CategoryFilterItem("Марки труб", true),
                new CategoryFilterItem("Марки труб из базы данных производителя MEP", true),
                new CategoryFilterItem("Марки узла аналитической модели", true),
                new CategoryFilterItem("Марки узлов", true),
                new CategoryFilterItem("Марки устоев", true),
                new CategoryFilterItem("Марки участков", true),
                new CategoryFilterItem("Марки устройств для вызова и оповещения", true),
                new CategoryFilterItem("Марки устройств механического управления", true),
                new CategoryFilterItem("Марки устройств связи", true),
                new CategoryFilterItem("Марки ферм", true),
                new CategoryFilterItem("Марки фермы", true),
                new CategoryFilterItem("Марки формообразующих элементов", true),
                new CategoryFilterItem("Марки формообразующих элементов-перекрытий", true),
                new CategoryFilterItem("Марки фундамента несущей конструкции", true),
                new CategoryFilterItem("Марки фундаментов опор", true),
                new CategoryFilterItem("Марки фундамента устоев", true),
                new CategoryFilterItem("Марки шкафов", true),
                new CategoryFilterItem("Марки электрооборудования", true),
                new CategoryFilterItem("Марки элементов антуража", true),
                new CategoryFilterItem("Марки элементов герметизации из базы данных производителя MEP", true),
                new CategoryFilterItem("Марки элементов озеленения", true),
                new CategoryFilterItem("Метки на горизонталях", true),
                new CategoryFilterItem("Метки пикетов трассы", true),
                new CategoryFilterItem("Наборы меток пикетов трассы", true),
                new CategoryFilterItem("Номера проступей/подступенков лестницы", true),
                new CategoryFilterItem("Область маскировки", true),
                new CategoryFilterItem("Обозначение направления пролета фундамента", true),
                new CategoryFilterItem("Обозначение направления пролетов", true),
                new CategoryFilterItem("Обозначения высотных отметок", true),
                new CategoryFilterItem("Обозначения для армирования по площади несущих конструкций", true),
                new CategoryFilterItem("Обозначения для армирования по траектории несущих конструкций", true),
                new CategoryFilterItem("Обозначения осей", true),
                new CategoryFilterItem("Обозначения раскосов на плане", true),
                new CategoryFilterItem("Обозначения соединений", true),
                new CategoryFilterItem("Опорные плоскости", true),
                new CategoryFilterItem("Опорные точки", true),
                new CategoryFilterItem("Оси", true),
                new CategoryFilterItem("Пометочные облака", true),
                new CategoryFilterItem("Размеры", true),
                new CategoryFilterItem("Разрезы", true),
                new CategoryFilterItem("Растровые изображения", true),
                new CategoryFilterItem("Ссылка на вид", true),
                new CategoryFilterItem("Ссылки на защитный слой арматуры", true),
                new CategoryFilterItem("Текстовые примечания", true),
                new CategoryFilterItem("Типовые аннотации", true),
                new CategoryFilterItem("Траектория смещения", true),
                new CategoryFilterItem("Уклоны в точках", true),
                new CategoryFilterItem("Фасады", true),
                new CategoryFilterItem("Цветовая область", true),
                new CategoryFilterItem("Элементы узлов", true)
            };

            // 5. Организация проекта и виды
            CategoryBlocks["Организация проекта и виды"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Видовые экраны", true),
                new CategoryFilterItem("Виды", true),
                new CategoryFilterItem("Включить/отключить набор арматурных стержней", true),
                new CategoryFilterItem("Границы 3D вида", true),
                new CategoryFilterItem("Границы обрезки", true),
                new CategoryFilterItem("Границы обрезки аннотаций", true),
                new CategoryFilterItem("Графика принципиальной схемы щита/панели", true),
                new CategoryFilterItem("Графика спецификации", true),
                new CategoryFilterItem("Группы модели", true),
                new CategoryFilterItem("Заголовки фрагментов", true),
                new CategoryFilterItem("Импорт в семействах", true),
                new CategoryFilterItem("Листы", true),
                new CategoryFilterItem("Области видимости", true),
                new CategoryFilterItem("Основные надписи", true),
                new CategoryFilterItem("Просмотр заголовков", true),
                new CategoryFilterItem("Сведения о проекте", true),
                new CategoryFilterItem("Спецификации", true),
                new CategoryFilterItem("Фрагмент плана", true),
                new CategoryFilterItem("Фрагменты", true)
            };

            // 6. Данные и системные категории
            CategoryBlocks["Данные и системные категории"] = new List<CategoryFilterItem>
            {
                new CategoryFilterItem("Варианты нагружений на конструкцию", true),
                new CategoryFilterItem("Граничные условия", true),
                new CategoryFilterItem("Графический стиль отображения расчетов", true),
                new CategoryFilterItem("Линии границ набора оборудования", true),
                new CategoryFilterItem("Материалы", true),
                new CategoryFilterItem("Материалы изоляции воздуховодов", true),
                new CategoryFilterItem("Материалы изоляции труб", true),
                new CategoryFilterItem("Материалы внутренней изоляции воздуховодов", true),
                new CategoryFilterItem("Нагрузки на конструкцию", true),
                new CategoryFilterItem("Настройки трассировки", true),
                new CategoryFilterItem("Облака точек", true),
                new CategoryFilterItem("Связанные файлы", true),
                new CategoryFilterItem("Схемы разрезки витража", true),
                new CategoryFilterItem("Уровни", true),
                new CategoryFilterItem("Цветовое обозначение воздуховода", true),
                new CategoryFilterItem("Цветовое обозначение трубопровода", true),
                new CategoryFilterItem("Электрическая аналитическая шина", true),
                new CategoryFilterItem("Электрические резервные/пространственные цепи", true)
            };
        }

        /// <summary>
        /// Получить все выбранные категории
        /// </summary>
        public List<string> GetSelectedCategories()
        {
            var selectedCategories = new List<string>();
            foreach (var block in CategoryBlocks.Values)
            {
                selectedCategories.AddRange(block.Where(item => item.IsSelected).Select(item => item.Name));
            }
            return selectedCategories;
        }

        /// <summary>
        /// Получить количество выбранных категорий
        /// </summary>
        public int GetSelectedCount()
        {
            return CategoryBlocks.Values.Sum(block => block.Count(item => item.IsSelected));
        }

        /// <summary>
        /// Получить общее количество категорий
        /// </summary>
        public int GetTotalCount()
        {
            return CategoryBlocks.Values.Sum(block => block.Count);
        }

        /// <summary>
        /// Получить количество выбранных категорий в конкретном блоке
        /// </summary>
        public int GetSelectedCountInBlock(string blockName)
        {
            if (CategoryBlocks.ContainsKey(blockName))
            {
                return CategoryBlocks[blockName].Count(item => item.IsSelected);
            }
            return 0;
        }

        /// <summary>
        /// Получить общее количество категорий в конкретном блоке
        /// </summary>
        public int GetTotalCountInBlock(string blockName)
        {
            if (CategoryBlocks.ContainsKey(blockName))
            {
                return CategoryBlocks[blockName].Count;
            }
            return 0;
        }

        /// <summary>
        /// Выбрать все категории в блоке
        /// </summary>
        public void SelectAllInBlock(string blockName)
        {
            if (CategoryBlocks.ContainsKey(blockName))
            {
                foreach (var item in CategoryBlocks[blockName])
                {
                    item.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Снять выбор со всех категорий в блоке
        /// </summary>
        public void DeselectAllInBlock(string blockName)
        {
            if (CategoryBlocks.ContainsKey(blockName))
            {
                foreach (var item in CategoryBlocks[blockName])
                {
                    item.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Выбрать все категории во всех блоках
        /// </summary>
        public void SelectAll()
        {
            foreach (var blockName in CategoryBlocks.Keys)
            {
                SelectAllInBlock(blockName);
            }
        }

        /// <summary>
        /// Снять выбор со всех категорий во всех блоках
        /// </summary>
        public void DeselectAll()
        {
            foreach (var blockName in CategoryBlocks.Keys)
            {
                DeselectAllInBlock(blockName);
            }
        }
    }

    /// <summary>
    /// Элемент фильтра категории
    /// </summary>
    public class CategoryFilterItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public CategoryFilterItem(string name, bool isSelected = true)
        {
            Name = name;
            IsSelected = isSelected;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
