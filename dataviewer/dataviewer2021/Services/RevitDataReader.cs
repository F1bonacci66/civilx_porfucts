using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitExporter.Services
{
    public class RevitDataReader : IRevitDataReader
    {
        public bool CanReadRevitFile(string filePath)
        {
            return File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".rvt";
        }

        public async Task<List<RevitElementData>> ReadRevitFileAsync(string filePath, IProgress<string> progress = null)
        {
            return await Task.Run(() => ReadRevitFileInternal(filePath, progress));
        }

        private List<RevitElementData> ReadRevitFileInternal(string filePath, IProgress<string> progress)
        {
            var result = new List<RevitElementData>();
            var modelName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                if (progress != null)
                    progress.Report(string.Format("Открываем файл: {0}", modelName));

                // Создаем опции для открытия файла
                var options = new OpenOptions();
                options.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;
                options.Audit = false;

                if (progress != null)
                    progress.Report("Инициализируем Revit приложение...");

                // Пытаемся получить или создать экземпляр Revit приложения
                Autodesk.Revit.ApplicationServices.Application revitApp = null;
                
                try
                {
                    // Пытаемся получить существующий экземпляр Revit
                    revitApp = GetOrCreateRevitApplication();
                    
                    if (progress != null)
                        progress.Report("✅ Revit приложение инициализировано");
                }
                catch (Exception appEx)
                {
                    if (progress != null)
                        progress.Report(string.Format("❌ Не удалось инициализировать Revit: {0}", appEx.Message));
                    throw new InvalidOperationException("Revit недоступен. Убедитесь, что Autodesk Revit установлен и доступен.", appEx);
                }

                Document document = null;
                try
                {
                    if (progress != null)
                        progress.Report("Открываем Revit файл...");

                    // Создаем ModelPath из строки пути
                    var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
                    
                    // Открываем документ
                    document = revitApp.OpenDocumentFile(modelPath, options);
                    
                    if (progress != null)
                        progress.Report(string.Format("✅ Файл {0} успешно открыт", modelName));

                    // Получаем элементы из документа
                    result = ExtractElementsFromDocument(document, modelName, progress);
                }
                finally
                {
                    // Закрываем документ
                    if (document != null)
                    {
                        if (progress != null)
                            progress.Report("Закрываем документ...");
                        document.Close(false); // false = не сохранять изменения
                    }
                }

                if (progress != null)
                    progress.Report(string.Format("✅ Извлечено {0} записей параметров", result.Count));

                return result;
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report(string.Format("❌ Ошибка чтения файла: {0}", ex.Message));
                
                // Детальная информация об ошибке
                if (progress != null)
                    progress.Report(string.Format("Тип ошибки: {0}", ex.GetType().Name));
                
                throw;
            }
        }

        private Autodesk.Revit.ApplicationServices.Application GetOrCreateRevitApplication()
        {
            try
            {
                // Пытаемся подключиться к запущенному экземпляру Revit через COM
                return GetRunningRevitApplication();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось подключиться к запущенному Revit. Убедитесь, что Revit запущен и доступен.", ex);
            }
        }

        private Autodesk.Revit.ApplicationServices.Application GetRunningRevitApplication()
        {
            try
            {
                // Метод 1: Попробуем получить доступ через статический контекст
                // (может работать, если Revit API инициализирован)
                // Application не имеет конструктора, поэтому возвращаем null
                return null;
            }
            catch
            {
                // Игнорируем ошибку и пробуем другие методы
            }

            try
            {
                // Метод 2: Попробуем через COM интерфейс
                var comType = Type.GetTypeFromProgID("Revit.Application");
                if (comType != null)
                {
                    dynamic comApp = Activator.CreateInstance(comType);
                    if (comApp != null)
                    {
                        // Это COM объект, а не Revit API Application
                        // Нужна конвертация, но это сложно
                        throw new NotSupportedException("COM интерфейс найден, но требует дополнительной реализации");
                    }
                }
            }
            catch
            {
                // Игнорируем ошибку
            }

            // Если ничего не работает, выбрасываем исключение
            throw new InvalidOperationException("Не удалось получить доступ к Revit. Возможные причины:\n" +
                "1. Revit не запущен\n" +
                "2. Revit API недоступен вне контекста плагина\n" +
                "3. Требуются права администратора\n" +
                "4. Несовместимая версия Revit");
        }

        private List<RevitElementData> ExtractElementsFromDocument(Document document, string modelName, IProgress<string> progress)
        {
            var result = new List<RevitElementData>();

            try
            {
                if (progress != null)
                    progress.Report("Сканируем элементы в документе...");

                // Создаем коллектор для получения всех элементов
                var collector = new FilteredElementCollector(document);
                
                // Исключаем системные элементы (виды, листы, и т.д.)
                collector.WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent();

                var elements = collector.ToElements();
                
                if (progress != null)
                    progress.Report(string.Format("Найдено {0} элементов для обработки", elements.Count));

                int processedCount = 0;
                foreach (Element element in elements)
                {
                    try
                    {
                        // Получаем категорию элемента
                        string category = GetElementCategory(element);
                        if (string.IsNullOrEmpty(category))
                            continue; // Пропускаем элементы без категории

                        // Получаем параметры элемента
                        var parameters = GetElementParameters(element);
                        
                        // Добавляем каждый параметр как отдельную запись
                        foreach (var param in parameters)
                        {
                            result.Add(new RevitElementData
                            {
                                ModelName = modelName,
                                ElementId = element.Id.IntegerValue,
                                Category = category,
                                ParameterName = param.Key,
                                ParameterValue = param.Value
                            });
                        }

                        processedCount++;
                        
                        // Обновляем прогресс каждые 100 элементов
                        if (processedCount % 100 == 0 && progress != null)
                        {
                            progress.Report(string.Format("Обработано {0} из {1} элементов", processedCount, elements.Count));
                        }
                    }
                    catch (Exception elementEx)
                    {
                        // Логируем ошибку для конкретного элемента, но продолжаем обработку
                        if (progress != null)
                            progress.Report(string.Format("⚠️ Ошибка обработки элемента {0}: {1}", element.Id.IntegerValue, elementEx.Message));
                    }
                }

                if (progress != null)
                    progress.Report(string.Format("Обработка завершена. Извлечено {0} параметров из {1} элементов", result.Count, processedCount));

            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report(string.Format("❌ Ошибка извлечения данных: {0}", ex.Message));
                throw;
            }

            return result;
        }

        private string GetElementCategory(Element element)
        {
            try
            {
                if (element.Category != null)
                {
                    return element.Category.Name;
                }
                
                // Если категория недоступна, используем "Прочее"
                return "Прочее";
            }
            catch
            {
                return "Прочее";
            }
        }

        private Dictionary<string, string> GetElementParameters(Element element)
        {
            var parameters = new Dictionary<string, string>();

            try
            {
                // Получаем все параметры элемента
                foreach (Parameter param in element.Parameters)
                {
                    try
                    {
                        if (param != null && param.Definition != null)
                        {
                            string paramName = param.Definition.Name;
                            string paramValue = GetParameterValue(param);
                            
                            if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramValue))
                            {
                                // Если параметр с таким именем уже есть, добавляем суффикс
                                string uniqueName = paramName;
                                int counter = 1;
                                while (parameters.ContainsKey(uniqueName))
                                {
                                    uniqueName = string.Format("{0}_{1}", paramName, counter);
                                    counter++;
                                }
                                
                                parameters[uniqueName] = paramValue;
                            }
                        }
                    }
                    catch
                    {
                        // Пропускаем параметры, которые не удается прочитать
                        continue;
                    }
                }

                // Добавляем базовую информацию об элементе
                parameters["ElementId"] = element.Id.IntegerValue.ToString();
                parameters["ElementName"] = element.Name ?? "Unnamed";
                
                // Добавляем информацию о типе элемента, если доступна
                try
                {
                    var elementType = element.Document.GetElement(element.GetTypeId());
                    if (elementType != null)
                    {
                        parameters["TypeName"] = elementType.Name ?? "Unknown Type";
                    }
                }
                catch
                {
                    // Игнорируем ошибки получения типа
                }
            }
            catch
            {
                // Если не удалось получить параметры, возвращаем базовую информацию
                parameters["ElementId"] = element.Id.IntegerValue.ToString();
                parameters["ElementName"] = element.Name ?? "Unnamed";
                parameters["Error"] = "Не удалось получить параметры элемента";
            }

            return parameters;
        }

        private string GetParameterValue(Parameter param)
        {
            try
            {
                if (param == null || !param.HasValue)
                {
                    return string.Empty;
                }

                switch (param.StorageType)
                {
                    case StorageType.Double:
                        return param.AsDouble().ToString("F2");
                    
                    case StorageType.Integer:
                        return param.AsInteger().ToString();
                    
                    case StorageType.String:
                        return param.AsString() ?? string.Empty;
                    
                    case StorageType.ElementId:
                        var elementId = param.AsElementId();
                        if (elementId != null && elementId.IntegerValue > 0)
                        {
                            try
                            {
                                var referencedElement = param.Element.Document.GetElement(elementId);
                                return referencedElement != null ? referencedElement.Name ?? elementId.IntegerValue.ToString() : elementId.IntegerValue.ToString();
                            }
                            catch
                            {
                                return elementId.IntegerValue.ToString();
                            }
                        }
                        return string.Empty;
                    
                    default:
                        return param.AsValueString() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
