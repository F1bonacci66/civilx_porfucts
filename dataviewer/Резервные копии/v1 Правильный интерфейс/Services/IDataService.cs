using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RevitExporterAddin.Models;

namespace RevitExporterAddin.Services
{
    public interface IDataService
    {
        List<Project> LoadProjectsAsync();
        void SaveProjectsAsync(List<Project> projects);
        Project CreateProjectAsync(string name);
        Export CreateExportAsync(string name);
        ExportTab CreateExportTabAsync(string name);
    }
}

