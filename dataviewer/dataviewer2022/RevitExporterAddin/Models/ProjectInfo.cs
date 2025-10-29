using System.Collections.Generic;

namespace RevitExporterAddin.Models
{
    public class ProjectInfo
    {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public List<ExportInfo> Exports { get; set; } = new List<ExportInfo>();
    }

    public class ExportInfo
    {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public List<TabInfo> Tabs { get; set; } = new List<TabInfo>();
    }

    public class TabInfo
    {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string RevitVersion { get; set; } = "2023";
    }
}

