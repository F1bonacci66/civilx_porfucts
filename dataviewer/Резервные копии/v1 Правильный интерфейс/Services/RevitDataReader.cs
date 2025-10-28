using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public class RevitDataReader
    {
        private UnitConverter _unitConverter;

        public List<RevitElementData> ExtractElementsFromDocument(Document document, string modelName, IProgress<string> progress = null)
        {
            var result = new List<RevitElementData>();

            try
            {
                // Инициализируем конвертер единиц измерения
                _unitConverter = new UnitConverter(document);
                
                if (progress != null)
                {
                    progress.Report(string.Format("Сканируем элементы в документе: {0}", modelName));
                    progress.Report(string.Format("Единицы измерения: {0}", _unitConverter.GetUnitsInfo()));
                }

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
                
                // Если категория недоступна, пытаемся получить тип элемента
                if (element.GetType() != null)
                {
                    return element.GetType().Name;
                }
                
                return "Unknown";
            }
            catch
            {
                return "Unknown";
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
                        // Используем конвертер единиц измерения для корректного отображения
                        return _unitConverter.ConvertToDisplayUnits(param.AsDouble(), param);
                    
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
                        // Пытаемся получить значение в единицах отображения
                        var displayValue = param.AsValueString();
                        if (!string.IsNullOrEmpty(displayValue))
                        {
                            return displayValue;
                        }
                        
                        // Если не удалось, используем конвертер для числовых значений
                        try
                        {
                            if (param.AsDouble() != 0)
                            {
                                return _unitConverter.ConvertToDisplayUnits(param.AsDouble(), param);
                            }
                        }
                        catch
                        {
                            // Игнорируем ошибки конвертации
                        }
                        
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
