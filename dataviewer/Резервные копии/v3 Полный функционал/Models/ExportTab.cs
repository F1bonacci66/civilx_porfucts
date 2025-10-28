using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitExporterAddin.Models
{
    public class ExportTab : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _folderPath = string.Empty;
        private string _revitVersion = "2023";
        private string _modelsFolder = string.Empty;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                OnPropertyChanged();
            }
        }

        public string RevitVersion
        {
            get { return _revitVersion; }
            set
            {
                _revitVersion = value;
                OnPropertyChanged();
            }
        }

        public string ModelsFolder
        {
            get { return _modelsFolder; }
            set
            {
                _modelsFolder = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

