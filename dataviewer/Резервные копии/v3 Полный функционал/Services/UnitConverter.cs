using System;
using Autodesk.Revit.DB;

namespace RevitExporterAddin.Services
{
    public class UnitConverter
    {
        private readonly Document _document;
        private readonly Units _units;

        public UnitConverter(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document), "Документ Revit не может быть null");
            }

            _document = document;
            _units = document.GetUnits();
            
            if (_units == null)
            {
                throw new InvalidOperationException("Не удалось получить настройки единиц измерения из документа");
            }
        }

        /// <summary>
        /// Конвертирует значение из внутренних единиц Revit (дюймы) в единицы отображения документа
        /// Возвращает только числовые значения без единиц измерения
        /// </summary>
        public string ConvertToDisplayUnits(double value, Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue)
                return string.Empty;

            try
            {
                // Для числовых значений используем конвертацию в единицы документа
                if (parameter.StorageType == StorageType.Double)
                {
                    return ConvertToDocumentUnits(value, parameter);
                }

                // Для остальных типов возвращаем исходное значение
                return value.ToString("F2");
            }
            catch
            {
                // В случае ошибки возвращаем исходное значение
                return value.ToString("F2");
            }
        }

        /// <summary>
        /// Конвертирует значение в единицы измерения документа
        /// </summary>
        private string ConvertToDocumentUnits(double valueInInternalUnits, Parameter parameter)
        {
            try
            {
                // Получаем тип единиц измерения параметра
                var specTypeId = parameter.Definition.GetDataType();
                
                // Определяем единицы измерения для конвертации
                ForgeTypeId targetUnitId = null;
                
                if (specTypeId == SpecTypeId.Length)
                {
                    targetUnitId = _units.GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
                }
                else if (specTypeId == SpecTypeId.Area)
                {
                    targetUnitId = _units.GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
                }
                else if (specTypeId == SpecTypeId.Volume)
                {
                    targetUnitId = _units.GetFormatOptions(SpecTypeId.Volume).GetUnitTypeId();
                }
                else if (specTypeId == SpecTypeId.Angle)
                {
                    targetUnitId = _units.GetFormatOptions(SpecTypeId.Angle).GetUnitTypeId();
                }
                
                // Если единицы измерения определены, конвертируем
                if (targetUnitId != null)
                {
                    var convertedValue = UnitUtils.ConvertFromInternalUnits(valueInInternalUnits, targetUnitId);
                    return convertedValue.ToString("F3");
                }
                
                // Если единицы не определены, возвращаем исходное значение
                return valueInInternalUnits.ToString("F2");
            }
            catch
            {
                // В случае ошибки возвращаем исходное значение
                return valueInInternalUnits.ToString("F2");
            }
        }

        /// <summary>
        /// Получает информацию о текущих единицах измерения документа
        /// </summary>
        public string GetUnitsInfo()
        {
            try
            {
                var lengthUnit = _units.GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
                var areaUnit = _units.GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
                
                return string.Format("Длина: {0}, Площадь: {1}",
                    GetUnitDisplayName(lengthUnit),
                    GetUnitDisplayName(areaUnit));
            }
            catch
            {
                return "Информация о единицах измерения недоступна";
            }
        }

        private string GetUnitDisplayName(ForgeTypeId unitTypeId)
        {
            try
            {
                if (unitTypeId == UnitTypeId.Millimeters) return "мм";
                if (unitTypeId == UnitTypeId.Centimeters) return "см";
                if (unitTypeId == UnitTypeId.Meters) return "м";
                if (unitTypeId == UnitTypeId.Feet) return "фут";
                if (unitTypeId == UnitTypeId.Inches) return "дюйм";
                if (unitTypeId == UnitTypeId.SquareMillimeters) return "мм²";
                if (unitTypeId == UnitTypeId.SquareCentimeters) return "см²";
                if (unitTypeId == UnitTypeId.SquareMeters) return "м²";
                if (unitTypeId == UnitTypeId.SquareFeet) return "фут²";
                if (unitTypeId == UnitTypeId.SquareInches) return "дюйм²";
                if (unitTypeId == UnitTypeId.CubicMillimeters) return "мм³";
                if (unitTypeId == UnitTypeId.CubicCentimeters) return "см³";
                if (unitTypeId == UnitTypeId.CubicMeters) return "м³";
                if (unitTypeId == UnitTypeId.CubicFeet) return "фут³";
                if (unitTypeId == UnitTypeId.CubicInches) return "дюйм³";
                if (unitTypeId == UnitTypeId.Degrees) return "градусы";
                if (unitTypeId == UnitTypeId.Radians) return "радианы";
                
                return unitTypeId.ToString();
            }
            catch
            {
                return "неизвестно";
            }
        }
    }
}
