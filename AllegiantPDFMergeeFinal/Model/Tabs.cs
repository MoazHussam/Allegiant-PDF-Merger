using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AllegiantPDFMerger;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace AllegiantPDFMerger
{
    class Tabs
    {
        public int fileNumber { private set; get; }
        private List<Task> _tasks;
        public List<Task> tasks
        {
            get
            {
                if (_tasks == null) _tasks = new List<Task>();
                return _tasks;
            }
            set
            {
                _tasks = value;
            }
        }

        private ObservableCollection<ListedFiles> _listedFiles;
        public ObservableCollection<ListedFiles> listedFiles
        {
            get
            {
                if (_listedFiles == null)
                {
                    _listedFiles = new ObservableCollection<ListedFiles>();
                }
                return _listedFiles;
            }
            set
            {
                _listedFiles = value;
            }
        }

        //private ObservableCollection<string> _icons;
        //public ObservableCollection<string> icons
        //{
        //    get
        //    {
        //        if (_icons == null) _icons = new ObservableCollection<string>();
        //        foreach(Files file in listedFiles)
        //        {
        //            if (Path.GetExtension(file.fileName).ToLower() == ".pdf") _icons.Add(PDFICON);
        //            else if (Path.GetExtension(file.fileName).ToLower() == ".doc" || Path.GetExtension(file.fileName).ToLower() == ".docx") _icons.Add(DOCICON);
        //            else _icons.Add(DEFUALTICON);                  
        //        }
        //        return _icons;
        //    }
        //}

        public Tabs(int fileNumber)
        {
            this.fileNumber = fileNumber;
        }
    }
}
