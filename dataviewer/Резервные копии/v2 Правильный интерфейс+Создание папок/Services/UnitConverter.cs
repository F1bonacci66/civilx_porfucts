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
            _document = document;
            _units = document.GetUnits();
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
                // Для числовых значений используем конвертацию без единиц измерения
                if (parameter.StorageType == StorageType.Double)
                {
                    return ConvertNumericValue(value);
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
        /// Простая конвертация числовых значений (только числа без единиц измерения)
        /// </summary>
        private string ConvertNumericValue(double valueInInternalUnits)
        {
            try
            {
                // Пытаемся конвертировать в метры (стандартная метрическая единица)
                var valueInMeters = UnitUtils.ConvertFromInternalUnits(valueInInternalUnits, UnitTypeId.Meters);
                
                // Возвращаем только числовое значение без единиц измерения
                return valueInMeters.ToString("F3");
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
