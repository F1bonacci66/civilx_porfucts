using System;
using System.Collections.Generic;

namespace RevitExporterAddin.TestWpfApp
{
    public class Project
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Export> Exports { get; set; } = new List<Export>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class Export
    {
        public string Name { get; set; }
        public string RevitFilePath { get; set; }
        public List<ExportTab> Tabs { get; set; } = new List<ExportTab>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class ExportTab
    {
        public string Name { get; set; }
        public string RevitFilePath { get; set; }
        public string OutputPath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
