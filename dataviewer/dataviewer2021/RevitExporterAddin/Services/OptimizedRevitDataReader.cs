using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    /// <summary>
    /// Оптимизированный читатель данных Revit с приоритетом на качество и производительность
    /// </summary>
    public class OptimizedRevitDataReader
    {
        private UnitConverter _unitConverter;
        private Document _document;
        private string _modelName;
        
        // Кэши для избежания повторных операций
        private readonly Dictionary<ElementId, Element> _elementTypeCache = new Dictionary<ElementId, Element>();
        private readonly Dictionary<ElementId, string> _categoryCache = new Dictionary<ElementId, string>();
        private readonly Dictionary<ElementId, string> _elementNameCache = new Dictionary<ElementId, string>();
        private readonly Dictionary<ElementId, string> _typeNameCache = new Dictionary<ElementId, string>();
        
        // Кэш для BuiltInParameter - создается один раз
        private readonly List<BuiltInParameter> _relevantBuiltInParams;
        
        // Переиспользуемые коллекции
        private readonly List<RevitElementData> _resultBuffer = new List<RevitElementData>();
        private readonly Dictionary<string, string> _parameterBuffer = new Dictionary<string, string>();

        public OptimizedRevitDataReader()
        {
            // Инициализируем только релевантные BuiltInParameter один раз
            _relevantBuiltInParams = GetRelevantBuiltInParameters();
        }

        public List<RevitElementData> ExtractElementsFromDocument(Document document, string modelName, IProgress<string> progress = null)
        {
            _document = document;
            _modelName = modelName;
            _unitConverter = new UnitConverter(document);
            
            // Очищаем кэши для нового документа
            ClearCaches();
            
            if (progress != null)
            {
                progress.Report($"Сканируем элементы в документе: {modelName}");
                progress.Report($"Единицы измерения: {_unitConverter.GetUnitsInfo()}");
            }

            try
            {
                // Получаем все элементы одним запросом
                var elements = GetElementsOptimized(document);
                
                if (progress != null)
                    progress.Report($"Найдено {elements.Count} элементов для обработки");

                // Обрабатываем элементы батчами для лучшей производительности
                return ProcessElementsInBatches(elements, progress);
            }
            catch (Exception ex)
            {
                if (progress != null)
                    progress.Report($"❌ Ошибка извлечения данных: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Оптимизированное получение элементов с предварительной фильтрацией
        /// </summary>
        private List<Element> GetElementsOptimized(Document document)
        {
            var collector = new FilteredElementCollector(document);
            
            // Используем более эффективные фильтры
            return collector
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .WherePasses(new ElementCategoryFilter(BuiltInCategory.INVALID, true)) // Исключаем системные категории
                .ToElements()
                .Where(e => e.Category != null && !IsSystemCategory(e.Category))
                .ToList();
        }

        /// <summary>
        /// Обработка элементов батчами для оптимизации памяти и производительности
        /// </summary>
        private List<RevitElementData> ProcessElementsInBatches(List<Element> elements, IProgress<string> progress)
        {
            const int batchSize = 50; // Оптимальный размер батча
            var allResults = new List<RevitElementData>();
            
            for (int i = 0; i < elements.Count; i += batchSize)
            {
                var batch = elements.Skip(i).Take(batchSize).ToList();
                var batchResults = ProcessBatch(batch, progress);
                allResults.AddRange(batchResults);
                
                if (progress != null && i % (batchSize * 10) == 0)
                {
                    progress.Report($"Обработано {Math.Min(i + batchSize, elements.Count)} из {elements.Count} элементов");
                }
            }
            
            if (progress != null)
                progress.Report($"Обработка завершена. Извлечено {allResults.Count} параметров из {elements.Count} элементов");
            
            return allResults;
        }

        /// <summary>
        /// Обработка батча элементов
        /// </summary>
        private List<RevitElementData> ProcessBatch(List<Element> batch, IProgress<string> progress)
        {
            _resultBuffer.Clear();
            
            foreach (var element in batch)
            {
                try
                {
                    ProcessElement(element);
                }
                catch (Exception ex)
                {
                    if (progress != null)
                        progress.Report($"⚠️ Ошибка обработки элемента {element.Id.IntegerValue}: {ex.Message}");
                }
            }
            
            return new List<RevitElementData>(_resultBuffer);
        }

        /// <summary>
        /// Оптимизированная обработка одного элемента
        /// </summary>
        private void ProcessElement(Element element)
        {
            var elementId = element.Id;
            
            // Получаем категорию с кэшированием
            string category = GetCachedCategory(element);
            if (string.IsNullOrEmpty(category))
                return;

            // Получаем параметры элемента
            _parameterBuffer.Clear();
            GetElementParametersOptimized(element, _parameterBuffer);
            
            // Добавляем результаты в буфер
            foreach (var param in _parameterBuffer)
            {
                _resultBuffer.Add(new RevitElementData
                {
                    ModelName = _modelName,
                    ElementId = elementId.IntegerValue,
                    Category = category,
                    ParameterName = param.Key,
                    ParameterValue = param.Value
                });
            }
        }

        /// <summary>
        /// Оптимизированное получение параметров элемента
        /// </summary>
        private void GetElementParametersOptimized(Element element, Dictionary<string, string> parameters)
        {
            var elementId = element.Id;
            
            // 1. Обычные параметры элемента
            ProcessElementParameters(element, parameters);
            
            // 2. Параметры типа элемента (с кэшированием)
            ProcessElementTypeParameters(element, parameters);
            
            // 3. Релевантные встроенные параметры (оптимизированный список)
            ProcessRelevantBuiltInParameters(element, parameters);
            
            // 4. Специфичные параметры для типов элементов
            ProcessSpecificElementParameters(element, parameters);
            
            // 5. Базовая информация об элементе (с кэшированием)
            AddBasicElementInfo(element, parameters);
        }

        /// <summary>
        /// Обработка обычных параметров элемента
        /// </summary>
        private void ProcessElementParameters(Element element, Dictionary<string, string> parameters)
        {
            foreach (Parameter param in element.Parameters)
            {
                if (param?.Definition != null && param.HasValue)
                {
                    string paramName = param.Definition.Name;
                    string paramValue = GetParameterValueOptimized(param);
                    
                    if (!string.IsNullOrEmpty(paramValue))
                    {
                        AddParameterOptimized(parameters, paramName, paramValue);
                    }
                }
            }
        }

        /// <summary>
        /// Обработка параметров типа элемента с кэшированием
        /// </summary>
        private void ProcessElementTypeParameters(Element element, Dictionary<string, string> parameters)
        {
            var typeId = element.GetTypeId();
            if (typeId == ElementId.InvalidElementId)
                return;

            var elementType = GetCachedElementType(typeId);
            if (elementType == null)
                return;

            foreach (Parameter param in elementType.Parameters)
            {
                if (param?.Definition != null && param.HasValue)
                {
                    string paramName = "Type_" + param.Definition.Name;
                    string paramValue = GetParameterValueOptimized(param);
                    
                    if (!string.IsNullOrEmpty(paramValue))
                    {
                        AddParameterOptimized(parameters, paramName, paramValue);
                    }
                }
            }
        }

        /// <summary>
        /// Обработка только релевантных встроенных параметров
        /// </summary>
        private void ProcessRelevantBuiltInParameters(Element element, Dictionary<string, string> parameters)
        {
            foreach (var builtInParam in _relevantBuiltInParams)
            {
                try
                {
                    var param = element.get_Parameter(builtInParam);
                    if (param?.HasValue == true)
                    {
                        string paramName = "BuiltIn_" + builtInParam.ToString();
                        string paramValue = GetParameterValueOptimized(param);
                        
                        if (!string.IsNullOrEmpty(paramValue))
                        {
                            AddParameterOptimized(parameters, paramName, paramValue);
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибки для отдельных параметров
                }
            }
        }

        /// <summary>
        /// Обработка специфичных параметров для разных типов элементов
        /// </summary>
        private void ProcessSpecificElementParameters(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                // Геометрические свойства
                ProcessElementGeometry(element, parameters);
                
                // Специфичные параметры по типам
                switch (element)
                {
                    case Wall wall:
                        ProcessWallParameters(wall, parameters);
                        break;
                    case FamilyInstance familyInstance:
                        ProcessFamilyInstanceParameters(familyInstance, parameters);
                        break;
                    case SpatialElement spatialElement:
                        ProcessSpatialElementParameters(spatialElement, parameters);
                        break;
                    case Level level:
                        ProcessLevelParameters(level, parameters);
                        break;
                }
            }
            catch
            {
                // Игнорируем ошибки специфичных параметров
            }
        }

        /// <summary>
        /// Добавление базовой информации об элементе с кэшированием
        /// </summary>
        private void AddBasicElementInfo(Element element, Dictionary<string, string> parameters)
        {
            var elementId = element.Id;
            
            parameters["ElementId"] = elementId.IntegerValue.ToString();
            parameters["ElementName"] = GetCachedElementName(element);
            parameters["ElementClass"] = element.GetType().Name;
            
            // Информация о типе элемента
            var typeId = element.GetTypeId();
            if (typeId != ElementId.InvalidElementId)
            {
                parameters["TypeName"] = GetCachedTypeName(typeId);
                parameters["TypeId"] = typeId.IntegerValue.ToString();
            }
        }

        /// <summary>
        /// Оптимизированное получение значения параметра
        /// </summary>
        private string GetParameterValueOptimized(Parameter param)
        {
            if (param == null || !param.HasValue)
                return string.Empty;

            try
            {
                switch (param.StorageType)
                {
                    case StorageType.Double:
                        return _unitConverter.ConvertToDisplayUnits(param.AsDouble(), param);
                    
                    case StorageType.Integer:
                        return param.AsInteger().ToString();
                    
                    case StorageType.String:
                        return param.AsString() ?? string.Empty;
                    
                    case StorageType.ElementId:
                        return GetElementIdParameterValue(param);
                    
                    default:
                        return GetDefaultParameterValue(param);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Получение значения параметра типа ElementId
        /// </summary>
        private string GetElementIdParameterValue(Parameter param)
        {
            var elementId = param.AsElementId();
            if (elementId?.IntegerValue > 0)
            {
                try
                {
                    var referencedElement = _document.GetElement(elementId);
                    return referencedElement?.Name ?? elementId.IntegerValue.ToString();
                }
                catch
                {
                    return elementId.IntegerValue.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Получение значения параметра по умолчанию
        /// </summary>
        private string GetDefaultParameterValue(Parameter param)
        {
            try
            {
                var displayValue = param.AsValueString();
                if (!string.IsNullOrEmpty(displayValue))
                    return displayValue;
                
                if (param.AsDouble() != 0)
                    return _unitConverter.ConvertToDisplayUnits(param.AsDouble(), param);
            }
            catch { }
            
            return string.Empty;
        }

        /// <summary>
        /// Оптимизированное добавление параметра
        /// </summary>
        private void AddParameterOptimized(Dictionary<string, string> parameters, string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(paramValue))
                return;

            // Проверяем на дубликаты и добавляем суффикс если нужно
            string uniqueName = paramName;
            int counter = 1;
            while (parameters.ContainsKey(uniqueName))
            {
                uniqueName = $"{paramName}_{counter}";
                counter++;
            }
            
            parameters[uniqueName] = paramValue;
        }

        #region Кэширование

        /// <summary>
        /// Получение категории элемента с кэшированием
        /// </summary>
        private string GetCachedCategory(Element element)
        {
            var categoryId = element.Category?.Id;
            if (categoryId == null)
                return "Прочее";

            if (!_categoryCache.TryGetValue(categoryId, out string category))
            {
                category = element.Category.Name;
                _categoryCache[categoryId] = category;
            }
            
            return category;
        }

        /// <summary>
        /// Получение типа элемента с кэшированием
        /// </summary>
        private Element GetCachedElementType(ElementId typeId)
        {
            if (!_elementTypeCache.TryGetValue(typeId, out Element elementType))
            {
                elementType = _document.GetElement(typeId);
                if (elementType != null)
                    _elementTypeCache[typeId] = elementType;
            }
            
            return elementType;
        }

        /// <summary>
        /// Получение имени элемента с кэшированием
        /// </summary>
        private string GetCachedElementName(Element element)
        {
            var elementId = element.Id;
            if (!_elementNameCache.TryGetValue(elementId, out string name))
            {
                name = element.Name ?? "Unnamed";
                _elementNameCache[elementId] = name;
            }
            
            return name;
        }

        /// <summary>
        /// Получение имени типа элемента с кэшированием
        /// </summary>
        private string GetCachedTypeName(ElementId typeId)
        {
            if (!_typeNameCache.TryGetValue(typeId, out string typeName))
            {
                var elementType = GetCachedElementType(typeId);
                typeName = elementType?.Name ?? "Неизвестный тип";
                _typeNameCache[typeId] = typeName;
            }
            
            return typeName;
        }

        /// <summary>
        /// Очистка всех кэшей
        /// </summary>
        private void ClearCaches()
        {
            _elementTypeCache.Clear();
            _categoryCache.Clear();
            _elementNameCache.Clear();
            _typeNameCache.Clear();
        }

        #endregion

        #region Специфичные методы обработки

        /// <summary>
        /// Получение только релевантных встроенных параметров
        /// </summary>
        private List<BuiltInParameter> GetRelevantBuiltInParameters()
        {
            return new List<BuiltInParameter>
            {
                BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                BuiltInParameter.ELEM_FAMILY_PARAM,
                BuiltInParameter.ELEM_TYPE_PARAM,
                BuiltInParameter.ELEM_TYPE_COMMENTS,
                BuiltInParameter.ELEM_DELETED_IN_VIEW,
                BuiltInParameter.ELEM_CREATED_BY,
                BuiltInParameter.ELEM_EDITED_BY,
                BuiltInParameter.ELEM_OWNED_BY_VIEW,
                BuiltInParameter.ELEM_VIEW_VISIBILITY,
                BuiltInParameter.ELEM_CATEGORY_PARAM,
                BuiltInParameter.ELEM_LEVEL_PARAM,
                BuiltInParameter.ELEM_ROOM_NUMBER,
                BuiltInParameter.ELEM_ROOM_NAME,
                BuiltInParameter.ELEM_PHASE_CREATED,
                BuiltInParameter.ELEM_PHASE_DEMOLISHED,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                BuiltInParameter.ALL_MODEL_TYPE_COMMENTS,
                BuiltInParameter.ALL_MODEL_MARK,
                BuiltInParameter.ALL_MODEL_DESCRIPTION,
                BuiltInParameter.ALL_MODEL_TYPE_MARK,
                BuiltInParameter.ALL_MODEL_TYPE_DESCRIPTION
            };
        }

        /// <summary>
        /// Проверка, является ли категория системной
        /// </summary>
        private bool IsSystemCategory(Category category)
        {
            if (category == null) return true;
            
            var categoryId = category.Id;
            return categoryId == BuiltInCategory.OST_Views ||
                   categoryId == BuiltInCategory.OST_Sheets ||
                   categoryId == BuiltInCategory.OST_Schedules ||
                   categoryId == BuiltInCategory.OST_Viewports ||
                   categoryId == BuiltInCategory.OST_ViewTemplates ||
                   categoryId == BuiltInCategory.OST_ProjectInformation ||
                   categoryId == BuiltInCategory.OST_Site ||
                   categoryId == BuiltInCategory.OST_Levels ||
                   categoryId == BuiltInCategory.OST_Grids;
        }

        /// <summary>
        /// Обработка геометрических свойств элемента
        /// </summary>
        private void ProcessElementGeometry(Element element, Dictionary<string, string> parameters)
        {
            try
            {
                var boundingBox = element.get_BoundingBox(null);
                if (boundingBox != null)
                {
                    var min = boundingBox.Min;
                    var max = boundingBox.Max;
                    
                    AddParameterOptimized(parameters, "BoundingBox_Min_X", min.X.ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Min_Y", min.Y.ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Min_Z", min.Z.ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Max_X", max.X.ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Max_Y", max.Y.ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Max_Z", max.Z.ToString("F3"));
                    
                    AddParameterOptimized(parameters, "BoundingBox_Width", (max.X - min.X).ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Height", (max.Y - min.Y).ToString("F3"));
                    AddParameterOptimized(parameters, "BoundingBox_Depth", (max.Z - min.Z).ToString("F3"));
                }
            }
            catch { }
        }

        /// <summary>
        /// Обработка параметров стены
        /// </summary>
        private void ProcessWallParameters(Wall wall, Dictionary<string, string> parameters)
        {
            try
            {
                var location = wall.Location as LocationCurve;
                if (location?.Curve != null)
                {
                    var curve = location.Curve;
                    AddParameterOptimized(parameters, "Wall_Length", curve.Length.ToString("F3"));
                    
                    var startPoint = curve.GetEndPoint(0);
                    var endPoint = curve.GetEndPoint(1);
                    AddParameterOptimized(parameters, "Wall_Start_X", startPoint.X.ToString("F3"));
                    AddParameterOptimized(parameters, "Wall_Start_Y", startPoint.Y.ToString("F3"));
                    AddParameterOptimized(parameters, "Wall_Start_Z", startPoint.Z.ToString("F3"));
                    AddParameterOptimized(parameters, "Wall_End_X", endPoint.X.ToString("F3"));
                    AddParameterOptimized(parameters, "Wall_End_Y", endPoint.Y.ToString("F3"));
                    AddParameterOptimized(parameters, "Wall_End_Z", endPoint.Z.ToString("F3"));
                }
            }
            catch { }
        }

        /// <summary>
        /// Обработка параметров семейства
        /// </summary>
        private void ProcessFamilyInstanceParameters(FamilyInstance familyInstance, Dictionary<string, string> parameters)
        {
            try
            {
                var host = familyInstance.Host;
                if (host != null)
                {
                    AddParameterOptimized(parameters, "Host_ElementId", host.Id.IntegerValue.ToString());
                    AddParameterOptimized(parameters, "Host_Category", host.Category?.Name ?? "Прочее");
                }
                
                var family = familyInstance.Symbol.Family;
                if (family != null)
                {
                    AddParameterOptimized(parameters, "Family_Name", family.Name);
                    AddParameterOptimized(parameters, "Family_FamilyCategory", family.FamilyCategory?.Name ?? "Прочее");
                }
            }
            catch { }
        }

        /// <summary>
        /// Обработка параметров пространственного элемента
        /// </summary>
        private void ProcessSpatialElementParameters(SpatialElement spatialElement, Dictionary<string, string> parameters)
        {
            try
            {
                AddParameterOptimized(parameters, "SpatialElement_Area", spatialElement.Area.ToString("F3"));
                AddParameterOptimized(parameters, "SpatialElement_Perimeter", spatialElement.Perimeter.ToString("F3"));
            }
            catch { }
        }

        /// <summary>
        /// Обработка параметров уровня
        /// </summary>
        private void ProcessLevelParameters(Level level, Dictionary<string, string> parameters)
        {
            try
            {
                AddParameterOptimized(parameters, "Level_Elevation", level.Elevation.ToString("F3"));
            }
            catch { }
        }

        #endregion
    }
}






