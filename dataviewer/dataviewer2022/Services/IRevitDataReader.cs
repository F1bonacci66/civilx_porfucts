using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RevitExporter.Models;

namespace RevitExporter.Services
{
    public interface IRevitDataReader
    {
        Task<List<RevitElementData>> ReadRevitFileAsync(string filePath, IProgress<string> progress = null);
        bool CanReadRevitFile(string filePath);
    }

    public class RevitElementData
    {
        public string ModelName { get; set; }
        public int ElementId { get; set; }
        public string Category { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
    }
}
