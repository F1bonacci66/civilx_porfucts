# USER RULES: Структура файлов проекта CivilX

## Общие принципы организации

---

## 1. Структура проекта CivilX.Shared

**Назначение:** Общая библиотека для всех плагинов

```
CivilX.Shared/
├── Auth/                       # Сервисы авторизации
│   ├── IAuthService.cs         # Интерфейс сервиса авторизации
│   └── AuthService.cs          # Реализация сервиса авторизации
├── Config/                     # Конфигурация
│   └── GlobalConfig.cs         # Глобальные настройки
├── Models/                     # Модели данных
│   ├── AuthResult.cs           # Результат авторизации
│   ├── ProductInfo.cs          # Информация о продукте
│   ├── UserInfo.cs             # Информация о пользователе
│   └── UserProductsResponse.cs # Ответ API о продуктах пользователя
├── Services/                   # Бизнес-логика и сервисы
│   ├── ApiClient.cs            # HTTP клиент для API
│   ├── CacheManager.cs         # Менеджер кэширования
│   └── PluginManager.cs        # Менеджер плагинов
├── Examples/                   # Примеры использования
│   └── DataViewerExample.cs    # Пример интеграции DataViewer
├── Properties/                 # Свойства сборки
│   └── AssemblyInfo.cs         # Информация о сборке
├── bin/                        # Скомпилированные файлы (игнорируется в Git)
│   └── x64/
│       └── Release/
│           ├── CivilX.Shared.dll
│           └── CivilX.Shared.pdb
├── obj/                        # Промежуточные файлы сборки (игнорируется в Git)
├── CivilX.Shared.csproj        # Файл проекта
└── README.md                   # Документация библиотеки
```

**Правила:**
- Все классы организованы по функциональным папкам (Auth, Config, Models, Services)
- Интерфейсы имеют префикс `I` (например, `IAuthService`)
- Реализации находятся в той же папке, что и интерфейсы
- Папки `bin/` и `obj/` не коммитятся в репозиторий
- Каждый сервис имеет свой файл с логичным именем

---

## 2. Структура плагина авторизации (plugin_aut)

**Назначение:** Плагин авторизации для Revit

```
plugin_aut/
├── AuthApplication.cs          # Основной класс приложения плагина
├── AuthWindow.xaml             # XAML разметка окна авторизации
├── AuthWindow.xaml.cs          # Code-behind для окна авторизации
├── Properties/                 # Свойства сборки
│   └── AssemblyInfo.cs
├── bin/                        # Скомпилированные файлы (игнорируется)
├── obj/                        # Промежуточные файлы (игнорируется)
├── CivilXAuthPlugin.csproj     # Файл проекта
├── App.config                  # Конфигурация приложения
└── main_icon.jpg               # Ресурсы (иконки)
```

**Правила:**
- Имя проекта: `CivilXAuthPlugin`
- Имя сборки: `CivilXAuthPlugin.dll`
- Все UI файлы используют XAML + Code-behind
- Ресурсы (иконки, изображения) хранятся в корне проекта плагина или в папке `Resources/`

---

## 3. Структура плагина DataViewer (dataviewer)

**Назначение:** Основной плагин для экспорта данных из Revit

**Примечание:** Общая структура папки `dataviewer/` и организация версионности (папки `dataviewer2021/`, `dataviewer2022/`, `dataviewer2023/`) описана в `.cursor/rules/structure.mdc`. Здесь описывается детальная структура **внутри каждой версии** плагина.

### Структура внутри каждой версии (dataviewer2021, dataviewer2022, dataviewer2023):

```
dataviewer[год]/
├── RevitExporterAddin/         # Основной проект плагина для Revit
│   ├── ExportCommand.cs        # Команда экспорта (IExternalCommand)
│   ├── RevitApplication.cs     # Класс приложения (IExternalApplication)
│   ├── RibbonManager.cs        # Управление лентой Revit
│   ├── PluginLoader.cs          # Загрузчик плагинов
│   ├── SimpleMainWindow.cs     # Упрощенное главное окно (если используется)
│   ├── MainWindow.xaml         # Главное окно (если используется)
│   ├── MainWindow.xaml.cs      # Code-behind главного окна
│   ├── Models/                 # Модели данных плагина
│   │   ├── RevitElementData.cs
│   │   ├── Project.cs
│   │   ├── Export.cs
│   │   ├── ExportTab.cs
│   │   ├── ProjectInfo.cs
│   │   └── CategoryFilter.cs
│   ├── Services/               # Сервисы плагина
│   │   ├── RevitDataReader.cs
│   │   ├── RevitExporter.cs
│   │   ├── RevitExporterService.cs
│   │   ├── UnitConverter.cs
│   │   ├── IDataService.cs
│   │   ├── DataService.cs
│   │   ├── IRevitExporter.cs
│   │   └── ProjectPersistenceService.cs
│   ├── ViewModels/             # ViewModels для MVVM
│   │   ├── BaseViewModel.cs
│   │   └── MainViewModel.cs
│   ├── Views/                  # XAML представления
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── ProjectView.xaml
│   │   ├── ExportView.xaml
│   │   ├── TabContentView.xaml
│   │   ├── AddProjectWindow.xaml
│   │   ├── AddExportWindow.xaml
│   │   ├── InputDialog.xaml
│   │   ├── RenameWindow.xaml
│   │   ├── CreateProjectWindow.xaml
│   │   ├── ExportProgressWindow.xaml
│   │   ├── CategoryFilterWindow.xaml
│   │   └── SingleBlockFilterWindow.xaml
│   ├── Converters/             # Конвертеры для Data Binding
│   │   ├── BooleanInverterConverter.cs
│   │   └── NullToVisibilityConverter.cs
│   ├── Properties/             # Свойства сборки
│   ├── RevitExporterAddin.addin # Манифест плагина
│   ├── RevitExporterAddin.csproj
│   └── bin/                    # Скомпилированные файлы (игнорируется)
├── Models/                     # Общие модели (если используются вне RevitExporterAddin)
│   ├── Project.cs
│   ├── Export.cs
│   └── ExportTab.cs
├── Services/                   # Общие сервисы (если используются вне RevitExporterAddin)
│   ├── DataService.cs
│   ├── IDataService.cs
│   ├── IRevitExporter.cs
│   ├── RevitExporter.cs
│   ├── RevitDataReader.cs
│   ├── IRevitDataReader.cs
│   ├── RevitDataReaderFactory.cs
│   └── FallbackRevitDataReader.cs
├── ViewModels/                 # Общие ViewModels
│   ├── BaseViewModel.cs
│   └── MainViewModel.cs
├── Views/                      # Общие представления
│   ├── MainWindow.xaml
│   └── MainWindow.xaml.cs
├── Converters/                 # Общие конвертеры
│   ├── BooleanInverterConverter.cs
│   └── NullToVisibilityConverter.cs
├── icons/                      # Иконки для плагина
│   ├── auth_16.png
│   ├── auth_32.png
│   ├── dataviewer_16.png
│   └── dataviewer_32.png
├── Проекты/                    # Тестовые проекты пользователя
│   └── [Название проекта]/
│       └── [Выгрузка]/
│           └── [Вкладка]/
├── Экспорт/                    # Экспортированные файлы для распространения
│   ├── RevitExporterAddin.dll
│   ├── RevitExporterAddin.pdb
│   └── RevitExporterAddin.addin
├── Резервные копии/            # Резервные копии разных версий
├── App.xaml                    # Точка входа WPF приложения (если используется)
├── App.xaml.cs                 # Code-behind для App.xaml
├── RevitExporter.csproj       # Файл проекта WPF приложения (если используется)
├── RevitExporter.code-workspace # Рабочее пространство VS Code
├── README.md                   # Основная документация
├── QUICK_START.md              # Быстрый старт
├── TESTING.md                  # Инструкции по тестированию
├── TROUBLESHOOTING.md          # Решение проблем
├── PROJECT_SUMMARY.md          # Резюме проекта
└── *.md                        # Дополнительная документация
```

**Правила:**
- Основной код плагина находится в `RevitExporterAddin/`
- Структура следует паттерну MVVM: Models, Views, ViewModels
- Конвертеры для Data Binding находятся в папке `Converters/`
- Каждый XAML файл имеет соответствующий `.xaml.cs` файл с тем же именем
- Все сервисы имеют интерфейсы с префиксом `I`
- Иконки хранятся в папке `icons/` с размерами в имени файла (16, 32)
- Тестовые данные пользователя хранятся в папке `Проекты/`
- Экспортированные файлы для распространения в папке `Экспорт/`

---

## 4. Структура установщиков (WiX)

**Расположение:** Корневой каталог проекта

```
CivilX/
├── CivilX-Revit2021-v1.5.wxs      # WiX скрипт для Revit 2021
├── CivilX-Revit2021-v1.5.wixobj  # Промежуточный файл (игнорируется)
├── CivilX-Revit2021-v1.5.wixpdb  # Промежуточный файл (игнорируется)
├── CivilX-Revit2021-v1.5.msi     # Готовый установщик
├── CivilX-Revit2022-v1.5.wxs
├── CivilX-Revit2022-v1.5.msi
├── CivilX-Revit2023-v1.5.wxs
└── CivilX-Revit2023-v1.5.msi
```

**Правила именования:**
- Формат: `CivilX-Revit{Год}-v{Версия}.wxs`
- Пример: `CivilX-Revit2022-v1.5.wxs`
- Промежуточные файлы (*.wixobj, *.wixpdb) не коммитятся в репозиторий
- Готовые MSI файлы можно хранить в репозитории или в отдельной папке релизов

---

## 5. Структура скриптов сборки

**Расположение:** Корневой каталог проекта

```
CivilX/
├── build_all_msi.bat           # Сборка всех версий установщиков
├── build_msi_2021.bat          # Сборка установщика для Revit 2021
├── build_msi_2022.bat          # Сборка установщика для Revit 2022
├── build_msi_2022_fixed.bat    # Исправленная версия для 2022
├── build_msi_2022_simple.bat   # Упрощенная версия для 2022
├── build_msi.bat               # Базовая сборка
├── build_projects.bat          # Сборка проектов C#
├── build_and_install.bat       # Сборка и установка
├── install_to_revit.bat        # Установка в Revit
├── install_2022_complete.bat   # Полная установка для 2022
├── test_build.bat              # Тестовая сборка
├── test_install_2022.bat       # Тестовая установка
├── check_installation.bat     # Проверка установки
├── update_files.bat            # Обновление файлов
├── quick_update.bat            # Быстрое обновление
├── create_addin_2022.bat       # Создание .addin файла для 2022
├── compile_fix.bat             # Исправление компиляции
└── compile_fix.ps1             # PowerShell скрипт исправления
```

**Правила именования:**
- Префикс `build_` - скрипты сборки
- Префикс `install_` - скрипты установки
- Префикс `test_` - тестовые скрипты
- Префикс `update_` - скрипты обновления
- Суффикс `_2021`, `_2022`, `_2023` - специфичные для версии Revit
- Суффикс `_fixed`, `_simple` - варианты реализации

---

## 6. Структура документации

**Расположение:** Корневой каталог и подпапки проектов

```
CivilX/
├── README_SCRIPTS.md           # Документация по скриптам
├── БЫСТРЫЙ_СТАРТ_MSI.md        # Быстрый старт с MSI
├── ИНСТРУКЦИЯ_СБОРКА_MSI.md    # Инструкция по сборке MSI
├── ОТЧЕТ_ИНСТРУКЦИИ_MSI.md     # Отчет по инструкциям MSI
├── ОТЧЕТ_ИСПРАВЛЕНИЕ_MSI_2022.md
├── ОТЧЕТ_СОЗДАНИЕ_MSI_2022.md
├── РЕШЕНИЕ_ПРОБЛЕМЫ_MSI_2022.md
├── CivilX_export/
│   ├── ИНСТРУКЦИЯ_УСТАНОВКИ.txt
│   ├── ГОТОВО_К_ТЕСТИРОВАНИЮ_v1.3.md
│   ├── ГОТОВО_К_ТЕСТИРОВАНИЮ_v1.4_UI_LOGGING.md
│   └── ОБНОВЛЕНИЕ_v1.5_ИСПРАВЛЕНИЯ.md
└── dataviewer/
    ├── README.md
    ├── QUICK_START.md
    ├── TESTING.md
    ├── TROUBLESHOOTING.md
    ├── PROJECT_SUMMARY.md
    └── *.md                    # Дополнительные отчеты
```

**Правила:**
- Основная документация в формате Markdown (.md)
- Инструкции по установке могут быть в формате .txt для простоты
- Отчеты об обновлениях имеют формат: `ОБНОВЛЕНИЕ_v{Версия}_*.md`
- Отчеты о готовности: `ГОТОВО_К_ТЕСТИРОВАНИЮ_v{Версия}_*.md`
- Документация должна быть актуальной и соответствовать текущей версии

---

## 7. Структура резервных копий

**Правила:**
- Резервные копии хранятся в папке `1. Резервные копии/` или `Резервные копии/`
- Архивы в формате ZIP, RAR хранятся в корне или в папке резервных копий
- Папки с суффиксами `v1`, `v2`, `v3` содержат версии функционала
- Резервные копии должны иметь понятные имена, указывающие на содержимое
- Не хранить рабочие файлы в папках резервных копий

---

## 8. Правила именования файлов

### C# файлы:
- **Классы:** PascalCase (например, `AuthService.cs`)
- **Интерфейсы:** PascalCase с префиксом `I` (например, `IAuthService.cs`)
- **XAML файлы:** PascalCase (например, `MainWindow.xaml`)
- **Code-behind:** PascalCase с `.xaml.cs` (например, `MainWindow.xaml.cs`)

### Проекты:
- **Название проекта:** `{Название}.csproj` (например, `CivilX.Shared.csproj`)
- **Имя сборки:** Совпадает с названием проекта без расширения

### Установщики:
- **WiX файлы:** `CivilX-Revit{Год}-v{Версия}.wxs`
- **MSI файлы:** `CivilX-Revit{Год}-v{Версия}.msi`

### Скрипты:
- **Batch файлы:** `{действие}_{объект}_{версия}.bat` (например, `build_msi_2022.bat`)
- **PowerShell скрипты:** `{действие}_{объект}.ps1`

### Документация:
- **Markdown:** `НАЗВАНИЕ_ВЕРХНИЙ_РЕГИСТР.md` или `README.md`
- **Текстовые файлы:** `НАЗВАНИЕ_ВЕРХНИЙ_РЕГИСТР.txt`

---

## 9. Правила организации папок

### Обязательные папки для C# проекта:
- `Models/` - модели данных
- `Services/` - бизнес-логика и сервисы
- `Views/` - XAML представления (для WPF)
- `ViewModels/` - ViewModels для MVVM (для WPF)
- `Properties/` - свойства сборки
- `Converters/` - конвертеры для Data Binding (для WPF)

### Опциональные папки:
- `Config/` - конфигурация
- `Examples/` - примеры использования
- `Resources/` - ресурсы (изображения, строки)
- `Utils/` или `Helpers/` - утилиты и вспомогательные классы
- `Tests/` - тесты (если используются)

### Папки, которые НЕ коммитятся:
- `bin/` - скомпилированные файлы
- `obj/` - промежуточные файлы сборки
- `*.wixobj`, `*.wixpdb` - промежуточные файлы WiX
- `*.user`, `*.suo` - пользовательские настройки Visual Studio

---

## 10. Правила для манифестов плагинов (.addin)

**Расположение:**
- В папке `C:\ProgramData\Autodesk\Revit\Addins\{Год}\` для установки
- В папке `CivilX_export/` для распространения
- В корне проекта для разработки

**Структура файла:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application" or "Command">
    <!-- Настройки плагина -->
  </AddIn>
</RevitAddIns>
```

**Правила:**
- Один `.addin` файл может содержать несколько плагинов
- Пути к DLL должны быть абсолютными или относительными к папке установки
- Все DLL файлы должны быть в одной папке с `.addin` файлом

---

## 11. Контрольный список при создании новой структуры

- [ ] Создана правильная структура папок согласно этим правилам
- [ ] Все файлы имеют правильные имена согласно правилам именования
- [ ] Интерфейсы имеют префикс `I`
- [ ] XAML файлы имеют соответствующие `.xaml.cs` файлы
- [ ] Папки `bin/` и `obj/` добавлены в `.gitignore`
- [ ] Документация создана и актуальна
- [ ] Скрипты сборки настроены правильно
- [ ] Файлы проекта ссылаются на правильные версии Revit API
- [ ] Все DLL файлы компилируются для платформы x64
- [ ] Манифест `.addin` настроен правильно

---

## 12. Исключения и особые случаи

### Версии для разных Revit:
**Примечание:** Организация версионности описана в `.cursor/rules/structure.mdc`

- Если требуется поддержка нескольких версий Revit с разными версиями кода:
  - Создавать отдельные папки: `dataviewer2021/`, `dataviewer2022/`, `dataviewer2023/` (структура описана в `structure.mdc`)
  - Или использовать условную компиляцию с директивами `#if`

### Общие компоненты:
- Общая библиотека `CivilX.Shared` используется всеми плагинами
- Общие модели и сервисы находятся внутри каждой версии (`dataviewer2021/`, `dataviewer2022/`, `dataviewer2023/`)

### Тестовые данные:
- Папка `Проекты/` содержит тестовые данные пользователя
- Не коммитится в репозиторий или добавляется в `.gitignore`

---

## Заключение

Эти правила должны соблюдаться при создании и поддержке проекта CivilX. Они обеспечивают:
- **Единообразие** структуры проекта
- **Понятность** организации файлов
- **Легкость** навигации и поиска
- **Масштабируемость** проекта
- **Совместимость** с инструментами сборки и установки

При нарушении правил структуры, проект становится сложнее поддерживать и расширять.

