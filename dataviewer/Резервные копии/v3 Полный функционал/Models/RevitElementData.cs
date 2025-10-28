using System;

namespace RevitExporterAddin.Models
{
    public class RevitElementData
    {
        public string ModelName { get; set; }
        public int ElementId { get; set; }
        public string Category { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
    }
}
