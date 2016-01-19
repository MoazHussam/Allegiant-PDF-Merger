using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllegiantPDFMerger
{
    public enum FileType { PDF, Word, Excel, Html, Text, Other };

    class Files
    {
        private FileInfo _file;
        public FileType fileType
        {
            get
            {
                FileType _fileType;

                switch (this.extension)
                {
                    case ".doc":
                    case ".docx": _fileType = FileType.Word;
                        break;
                    case ".pdf": _fileType = FileType.PDF;
                        break;
                    case ".xlx":
                    case ".xlxs": _fileType = FileType.Excel;
                        break;
                    case ".html":
                    case ".htm": _fileType = FileType.Html;
                        break;
                    case ".txt": _fileType = FileType.Text;
                        break;
                    default: _fileType = FileType.Other;
                        break;
                }

                return _fileType;
            }
        }

        public string extension
        {
            get
            {
                return Path.GetExtension(this.fileName).ToLower();
            }
        }

        public string filePath
        {
            get
            {
                if (_file != null) return _file.FullName;
                else return "";
            }
            set
            {
                if (File.Exists(value))
                {
                    _file = new FileInfo(value);
                }
                else throw new FileNotFoundException();
            }
        }

        public string fileName
        {
            get
            {
                if (_file != null) return _file.Name;
                else return "";
            }
        }

        public Files(string filePath)
        {
            this.filePath = filePath;
        }

        public void delete()
        {
            try
            {
                _file.Delete();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void copy(string destinationDirectory)
        {
            try
            {
                _file.CopyTo(Path.Combine(destinationDirectory, _file.Name));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void move(string destinationDirectory, string fileName)
        {
            try
            {
                _file.MoveTo(Path.Combine(destinationDirectory, fileName));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void move(string destinationDirectory)
        {
            this.move(destinationDirectory, _file.Name);
        }

        public void rename(string newFileName) //test for overwrite and already existing file
        {
            try
            {
                this.move(_file.DirectoryName, newFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
