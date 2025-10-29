using System;
using System.Threading.Tasks;
using RevitExporter.Models;

namespace RevitExporter.Services
{
    public interface IRevitExporter
    {
        Task ExportToCsvAsync(ExportTab exportTab, IProgress<string> progress = null);
    }
}
