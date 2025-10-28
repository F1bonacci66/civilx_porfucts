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
                // Проверяем входные параметры
                if (document == null)
                {
                    throw new ArgumentNullException(nameof(document), "Документ Revit не может быть null");
                }

                if (string.IsNullOrEmpty(modelName))
                {
                    throw new ArgumentException("Имя модели не может быть пустым", nameof(modelName));
                }

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
                // 1. Получаем все обычные параметры элемента
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
                                AddParameter(parameters, paramName, paramValue);
                            }
                        }
                    }
                    catch
                    {
                        // Пропускаем параметры, которые не удается прочитать
                        continue;
                    }
                }

                // 2. Получаем параметры типа элемента
                try
                {
                    var elementType = element.Document.GetElement(element.GetTypeId());
                    if (elementType != null)
                    {
                        foreach (Parameter param in elementType.Parameters)
                        {
                            try
                            {
                                if (param != null && param.Definition != null)
                                {
                                    string paramName = "Type_" + param.Definition.Name;
                                    string paramValue = GetParameterValue(param);
                                    
                                    if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramValue))
                                    {
                                        AddParameter(parameters, paramName, paramValue);
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки получения параметров типа
                }

                // 3. Получаем все встроенные параметры (BuiltInParameter)
                try
                {
                    var builtInParams = Enum.GetValues(typeof(BuiltInParameter)).Cast<BuiltInParameter>();
                    foreach (var builtInParam in builtInParams)
                    {
                        try
                        {
                            var param = element.get_Parameter(builtInParam);
                            if (param != null && param.HasValue)
                            {
                                string paramName = "BuiltIn_" + builtInParam.ToString();
                                string paramValue = GetParameterValue(param);
                                
                                if (!string.IsNullOrEmpty(paramValue))
                                {
                                    AddParameter(parameters, paramName, paramValue);
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки получения встроенных параметров
                }

                // 4. Получаем специфичные параметры для разных типов элементов
                GetSpecificElementParameters(element, parameters);

                // 5. Добавляем базовую информацию об элементе
                parameters["ElementId"] = element.Id.IntegerValue.ToString();
                parameters["ElementName"] = element.Name ?? "Unnamed";
                parameters["ElementClass"] = element.GetType().Name;
                
                // Добавляем информацию о типе элемента
                try
                {
                    var elementType = element.Document.GetElement(element.GetTypeId());
                    if (elementType != null)
                    {
                        parameters["TypeName"] = elementType.Name ?? "Unknown Type";
                        parameters["TypeId"] = element.GetTypeId().IntegerValue.ToString();
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

        /// <summary>
        /// Добавляет параметр в словарь с уникальным именем
        /// </summary>
        private void AddParameter(Dictionary<string, string> parameters, string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(paramValue))
                return;

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

        /// <summary>
        /// Получает специфичные параметры для разных типов элементов
        /// </summary>
        private void GetSpecificElementParameters(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                // Общие геометрические свойства для всех элементов
                GetElementGeometry(element, parameters);
                GetElementLocation(element, parameters);
                GetElementBoundingBox(element, parameters);

                // Для стен
                if (element is Wall wall)
                {
                    try
                    {
                        var location = wall.Location as LocationCurve;
                        if (location != null)
                        {
                            var curve = location.Curve;
                            if (curve != null)
                            {
                                AddParameter(parameters, "Wall_Length", curve.Length.ToString("F3"));
                                
                                // Координаты начала и конца стены
                                var startPoint = curve.GetEndPoint(0);
                                var endPoint = curve.GetEndPoint(1);
                                AddParameter(parameters, "Wall_Start_X", startPoint.X.ToString("F3"));
                                AddParameter(parameters, "Wall_Start_Y", startPoint.Y.ToString("F3"));
                                AddParameter(parameters, "Wall_Start_Z", startPoint.Z.ToString("F3"));
                                AddParameter(parameters, "Wall_End_X", endPoint.X.ToString("F3"));
                                AddParameter(parameters, "Wall_End_Y", endPoint.Y.ToString("F3"));
                                AddParameter(parameters, "Wall_End_Z", endPoint.Z.ToString("F3"));
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        var wallType = wall.WallType;
                        if (wallType != null)
                        {
                            AddParameter(parameters, "WallType_Name", wallType.Name);
                        }
                    }
                    catch { }
                }

                // Для дверей и окон
                if (element is FamilyInstance familyInstance)
                {
                    try
                    {
                        var host = familyInstance.Host;
                        if (host != null)
                        {
                            AddParameter(parameters, "Host_ElementId", host.Id.IntegerValue.ToString());
                            AddParameter(parameters, "Host_Category", host.Category?.Name ?? "Unknown");
                        }
                    }
                    catch { }

                    try
                    {
                        var family = familyInstance.Symbol.Family;
                        if (family != null)
                        {
                            AddParameter(parameters, "Family_Name", family.Name);
                            AddParameter(parameters, "Family_FamilyCategory", family.FamilyCategory?.Name ?? "Unknown");
                        }
                    }
                    catch { }

                    try
                    {
                        var location = familyInstance.Location as LocationPoint;
                        if (location != null)
                        {
                            var point = location.Point;
                            AddParameter(parameters, "FamilyInstance_X", point.X.ToString("F3"));
                            AddParameter(parameters, "FamilyInstance_Y", point.Y.ToString("F3"));
                            AddParameter(parameters, "FamilyInstance_Z", point.Z.ToString("F3"));
                        }
                    }
                    catch { }

                    try
                    {
                        var transform = familyInstance.GetTransform();
                        AddParameter(parameters, "FamilyInstance_Rotation", transform.BasisZ.AngleTo(XYZ.BasisZ).ToString("F3"));
                    }
                    catch { }
                }

                // Для помещений (используем SpatialElement)
                if (element is SpatialElement spatialElement)
                {
                    try
                    {
                        var area = spatialElement.Area;
                        AddParameter(parameters, "SpatialElement_Area", area.ToString("F3"));
                    }
                    catch { }

                    try
                    {
                        var perimeter = spatialElement.Perimeter;
                        AddParameter(parameters, "SpatialElement_Perimeter", perimeter.ToString("F3"));
                    }
                    catch { }
                }

                // Для уровней
                if (element is Level level)
                {
                    try
                    {
                        var elevation = level.Elevation;
                        AddParameter(parameters, "Level_Elevation", elevation.ToString("F3"));
                    }
                    catch { }
                }

                // Для видов
                if (element is View view)
                {
                    try
                    {
                        AddParameter(parameters, "View_ViewType", view.ViewType.ToString());
                        AddParameter(parameters, "View_Scale", view.Scale.ToString());
                    }
                    catch { }
                }

                // Для материалов
                if (element is Material material)
                {
                    try
                    {
                        AddParameter(parameters, "Material_Class", material.MaterialClass);
                        AddParameter(parameters, "Material_AppearanceAssetId", material.AppearanceAssetId.IntegerValue.ToString());
                    }
                    catch { }
                }

                // Для колонн
                if (element is FamilyInstance column && column.Category?.Name == "Столбцы")
                {
                    try
                    {
                        var location = column.Location as LocationPoint;
                        if (location != null)
                        {
                            var point = location.Point;
                            AddParameter(parameters, "Column_X", point.X.ToString("F3"));
                            AddParameter(parameters, "Column_Y", point.Y.ToString("F3"));
                            AddParameter(parameters, "Column_Z", point.Z.ToString("F3"));
                        }
                    }
                    catch { }
                }

                // Для балок
                if (element is FamilyInstance beam && beam.Category?.Name == "Балки")
                {
                    try
                    {
                        var location = beam.Location as LocationCurve;
                        if (location != null)
                        {
                            var curve = location.Curve;
                            if (curve != null)
                            {
                                AddParameter(parameters, "Beam_Length", curve.Length.ToString("F3"));
                                var startPoint = curve.GetEndPoint(0);
                                var endPoint = curve.GetEndPoint(1);
                                AddParameter(parameters, "Beam_Start_X", startPoint.X.ToString("F3"));
                                AddParameter(parameters, "Beam_Start_Y", startPoint.Y.ToString("F3"));
                                AddParameter(parameters, "Beam_Start_Z", startPoint.Z.ToString("F3"));
                                AddParameter(parameters, "Beam_End_X", endPoint.X.ToString("F3"));
                                AddParameter(parameters, "Beam_End_Y", endPoint.Y.ToString("F3"));
                                AddParameter(parameters, "Beam_End_Z", endPoint.Z.ToString("F3"));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch
            {
                // Игнорируем ошибки получения специфичных параметров
            }
        }

        /// <summary>
        /// Получает геометрические свойства элемента
        /// </summary>
        private void GetElementGeometry(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                var geometry = element.get_Geometry(new Options());
                if (geometry != null)
                {
                    AddParameter(parameters, "Geometry_Count", geometry.Count().ToString());
                    
                    // Пытаемся получить объем
                    try
                    {
                        var solid = geometry.GetEnumerator().Current as Solid;
                        if (solid != null && solid.Volume > 0)
                        {
                            AddParameter(parameters, "Geometry_Volume", solid.Volume.ToString("F3"));
                            AddParameter(parameters, "Geometry_SurfaceArea", solid.SurfaceArea.ToString("F3"));
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>
        /// Получает координаты элемента
        /// </summary>
        private void GetElementLocation(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                var location = element.Location;
                if (location != null)
                {
                    if (location is LocationPoint pointLocation)
                    {
                        var point = pointLocation.Point;
                        AddParameter(parameters, "Location_X", point.X.ToString("F3"));
                        AddParameter(parameters, "Location_Y", point.Y.ToString("F3"));
                        AddParameter(parameters, "Location_Z", point.Z.ToString("F3"));
                    }
                    else if (location is LocationCurve curveLocation)
                    {
                        var curve = curveLocation.Curve;
                        if (curve != null)
                        {
                            var startPoint = curve.GetEndPoint(0);
                            var endPoint = curve.GetEndPoint(1);
                            AddParameter(parameters, "Location_Start_X", startPoint.X.ToString("F3"));
                            AddParameter(parameters, "Location_Start_Y", startPoint.Y.ToString("F3"));
                            AddParameter(parameters, "Location_Start_Z", startPoint.Z.ToString("F3"));
                            AddParameter(parameters, "Location_End_X", endPoint.X.ToString("F3"));
                            AddParameter(parameters, "Location_End_Y", endPoint.Y.ToString("F3"));
                            AddParameter(parameters, "Location_End_Z", endPoint.Z.ToString("F3"));
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Получает ограничивающий прямоугольник элемента
        /// </summary>
        private void GetElementBoundingBox(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                var boundingBox = element.get_BoundingBox(null);
                if (boundingBox != null)
                {
                    var min = boundingBox.Min;
                    var max = boundingBox.Max;
                    
                    AddParameter(parameters, "BoundingBox_Min_X", min.X.ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Min_Y", min.Y.ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Min_Z", min.Z.ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Max_X", max.X.ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Max_Y", max.Y.ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Max_Z", max.Z.ToString("F3"));
                    
                    // Размеры
                    AddParameter(parameters, "BoundingBox_Width", (max.X - min.X).ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Height", (max.Y - min.Y).ToString("F3"));
                    AddParameter(parameters, "BoundingBox_Depth", (max.Z - min.Z).ToString("F3"));
                }
            }
            catch { }
        }
    }
}
