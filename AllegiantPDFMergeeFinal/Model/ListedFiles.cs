using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllegiantPDFMerger;
using System.IO;
using BlackFox.Win32;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace AllegiantPDFMerger
{
    class ListedFiles : Files
    {
        private bool deleteNewFile = false;
        private bool deleteAfterFinish = false;
        private bool conversionFailed = false;
        //private Task waitingTask
        //{
        //    set
        //    {
        //        if (_waitingTask != null) _waitingTask.Dispose();
        //        _waitingTask = value;
        //    }
        //    get
        //    {
        //        return _waitingTask;
        //    }
        //}
        public string errorMsg
        {
            get
            {
                if (task.IsFaulted) return task.Exception.InnerException.Message;
                else return "";
            }
        }

        public Task<PDFFiles> task
        {
            set;
            get;
        }

        private PDFFiles _PDFFile;
        public PDFFiles PDFFile
        {
            get
            {
                try
                {
                    if (conversionFailed && this.fileType == FileType.Word)
                    {
                        this.convert();
                        return _PDFFile = getPDFFile().Result;
                    }
                    else if (this.fileType == FileType.Word || this.fileType == FileType.Html || this.fileType == FileType.Text) return _PDFFile = getPDFFile().Result;
                    else if (this.fileType == FileType.PDF) return _PDFFile;
                    else
                    {
                        conversionFailed = true;
                        return null;
                    }
                }
                catch
                {
                    conversionFailed = true;
                    return null;
                }
            }
            private set
            {
                _PDFFile = value;
            }
        }

        private async Task<PDFFiles> getPDFFile()
        {
            return await task;
        }

        public BitmapSource icon
        {
            get
            {
                //if (this.extension == ".pdf") return @"Resources/Pdf Icon.ico";
                //else if (this.extension == ".doc" || this.extension == ".docx") return @"Resources/Word Icon.ico";
                //else return "";

                //Bitmap bitmap = Icons.IconFromExtension(this.extension, Icons.SystemIconSize.Small).ToBitmap();
                Icon icon = Icons.IconFromExtension(this.extension, Icons.SystemIconSize.Small);
                //using (MemoryStream memory = new MemoryStream())
                //{
                //    bitmap.Save(memory, ImageFormat.Png);
                //    memory.Position = 0;
                //    BitmapImage bitmapImage = new BitmapImage();
                //    bitmapImage.BeginInit();
                //    bitmapImage.StreamSource = memory;
                //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //    bitmapImage.EndInit();
                //    return bitmapImage;
                //}

                //return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(16, 16));
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public ListedFiles(string filePath)
            : base(filePath)
        {
            this.convert();
        }

        public ListedFiles(string filePath, bool deleteAfterFinish)
            : base(filePath)
        {
            this.deleteAfterFinish = deleteAfterFinish;
            this.convert();
        }

        //public async void waitForLockedDocFile()
        //{
        //    _waitingTask = Task.Run(() =>
        //    {
        //        this.waitForLockedFile(300);

        //        return true;
        //    });
        //}

        public void convert()
        {
            conversionFailed = false;
            if (this.fileType == FileType.PDF) PDFFile = new PDFFiles(this.filePath);
            else if (this.fileType == FileType.Word || this.fileType == FileType.Html)
            {
                deleteNewFile = true;
                DOCFiles _docFile = new DOCFiles(this.filePath);
                string tempFile = "";
                try
                {
                    tempFile = Path.GetTempFileName();
                }
                catch (Exception ex)
                {
                    //System.Windows.Forms.MessageBox.Show("Exception Messege :" + ex.Message, "Just screenshot this error report, excution will continue as normal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    tempFile = Path.Combine(Path.GetTempPath(), "FFFFFFFFF");
                }
                //if (tempFile == "" || tempFile == null || !File.Exists(tempFile)) System.Windows.Forms.MessageBox.Show("Method name convert \nvar tempFile :" + tempFile, "Just screenshot this error report, excution will continue as normal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                task = _docFile.convertToPDF(tempFile);
            }
            else if (this.fileType == FileType.Text)
            {
                //deleteNewFile = true;
                TextFiles _textFile = new TextFiles(this.filePath);
                string tempFile = "";
                try
                {
                    tempFile = Path.GetTempFileName();
                }
                catch (Exception ex)
                {
                    tempFile = Path.Combine(Path.GetTempPath(), "FFFFFFFFF");
                }
                task = _textFile.convertToPdfAsync(tempFile);
            }

        }

        ~ListedFiles()
        {
            try
            {
                if (deleteNewFile) PDFFile.delete();
                if (deleteAfterFinish) this.delete();
            }
            catch { }
        }
    }
}
