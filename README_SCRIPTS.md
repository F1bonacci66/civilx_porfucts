# 🚀 CivilX Build Scripts

Автоматизированные скрипты для компиляции и установки CivilX DataViewer.

## 📋 Список скриптов

### **Основные скрипты:**

1. **`build_and_install.bat`** - 🎯 **ГЛАВНЫЙ СКРИПТ**
   - Полная автоматизация: компиляция + обновление + установка
   - Используйте для первой установки или полного обновления

2. **`build_projects.bat`** - 🔨 Компиляция проектов
   - Компилирует все проекты в конфигурации Release x64
   - Использует MSBuild для сборки

3. **`update_files.bat`** - 📦 Обновление файлов
   - Копирует скомпилированные файлы в папку CivilX_export
   - Обновляет DLL и PDB файлы

4. **`install_to_revit.bat`** - 🔧 Установка в Revit
   - Устанавливает .addin файл в папку Revit Addins
   - Проверяет права доступа и конфигурацию

### **Вспомогательные скрипты:**

5. **`quick_update.bat`** - ⚡ Быстрое обновление
   - Обновляет файлы без пересборки (если уже скомпилированы)
   - Используйте для быстрых обновлений

6. **`check_installation.bat`** - 🔍 Проверка установки
   - Проверяет корректность установки
   - Диагностирует проблемы

## 🎯 Быстрый старт

### **Для первой установки:**
```batch
build_and_install.bat
```

### **Для обновления (если уже компилировалось):**
```batch
quick_update.bat
```

### **Для проверки установки:**
```batch
check_installation.bat
```

## 📁 Структура файлов

```
CivilX/
├── build_and_install.bat      # Главный скрипт
├── build_projects.bat         # Компиляция
├── update_files.bat           # Обновление файлов
├── install_to_revit.bat       # Установка в Revit
├── quick_update.bat           # Быстрое обновление
├── check_installation.bat     # Проверка установки
├── README_SCRIPTS.md          # Эта документация
├── CivilX_export/             # Папка с готовыми файлами
│   ├── CivilX.addin
│   ├── CivilX.Shared.dll
│   ├── RevitExporterAddin.dll
│   └── CivilXAuthPlugin.dll
└── [исходные проекты]
```

## ⚙️ Требования

- **Visual Studio 2022** (Community/Professional/Enterprise)
- **.NET Framework 4.8**
- **Autodesk Revit 2023**
- **Права администратора** (для установки в Revit)

## 🔧 Настройка

### **Если Visual Studio установлена в нестандартное место:**

Отредактируйте `build_projects.bat` и измените пути:
```batch
REM Найдите эти строки и измените пути:
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
```

### **Если Revit установлен в другой версии:**

Отредактируйте `install_to_revit.bat` и измените путь:
```batch
REM Измените эту строку:
set "REVIT_ADDINS_PATH=C:\ProgramData\Autodesk\Revit\Addins\2023"
```

## 🐛 Устранение неполадок

### **Ошибка "Visual Studio не найдена":**
1. Установите Visual Studio 2022
2. Или измените пути в `build_projects.bat`

### **Ошибка "Нет прав доступа":**
1. Запустите скрипт от имени администратора
2. Проверьте права на папку `C:\ProgramData\Autodesk\Revit\Addins\2023\`

### **Ошибка "Файлы не найдены":**
1. Сначала запустите `build_projects.bat`
2. Проверьте, что компиляция прошла успешно

### **Плагин не появляется в Revit:**
1. Перезапустите Revit
2. Запустите `check_installation.bat`
3. Проверьте пути в файле `CivilX.addin`

## 📊 Логи и диагностика

Все скрипты выводят подробную информацию о процессе:
- ✅ Успешные операции
- ❌ Ошибки
- ⚠️ Предупреждения
- 🔍 Диагностическая информация

## 🎉 Готово!

После успешного выполнения скриптов:
1. **Перезапустите Revit**
2. **Найдите вкладку "CivilX"** в ленте
3. **Нажмите "Data Manager"** для управления проектами

---

**Время обновления:** ~2-3 минуты  
**Автоматизация:** 100%  
**Простота:** Один клик! 🚀




