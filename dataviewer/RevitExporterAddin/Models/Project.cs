using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitExporterAddin.Models
{
    public class Project : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _folderPath = string.Empty;
        private ObservableCollection<Export> _exports = new ObservableCollection<Export>();

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

        public ObservableCollection<Export> Exports
        {
            get { return _exports; }
            set
            {
                _exports = value;
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

