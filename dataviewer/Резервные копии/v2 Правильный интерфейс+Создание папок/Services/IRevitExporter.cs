using System;
using System.Threading.Tasks;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public interface IRevitExporter
    {
        Task ExportToCsvAsync(ExportTab exportTab, IProgress<string> progress = null);
    }
}


