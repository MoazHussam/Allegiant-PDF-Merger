using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = NetOffice.WordApi;
using Document = NetOffice.WordApi.Document;
using NetOffice.WordApi.Enums;
using System.IO;

namespace AllegiantPDFMerger
{
    class DOCFiles : Files
    {
        string convertionErrorMsg = "";

        public DOCFiles(string filePath) : base(filePath)
        {

        }

        public async Task<PDFFiles> convertToPDF()
        {
            if (this.fileType == FileType.PDF) return await convertDocToPDF(this.filePath + " (1)", true);
            else return await convertDocToPDF(Path.ChangeExtension(this.filePath, ".pdf"), false);
        }

        public async Task<PDFFiles> convertToPDF(string outFile)
        {
            return await convertDocToPDF(outFile, false);
        }

        private async Task<PDFFiles> convertDocToPDF(string destinationFile, bool deleteOiginal)
        {
            return await Task.Run(() =>
            {
                if (!Directory.GetParent(destinationFile).Exists) return null;

                string filePath = this.filePath;
                FileInfo file = new FileInfo(filePath);
                if (this.IsFileLocked()) throw new Exception("File: \"" + this.fileName + "\" is open in another application and cannot be merged"); 

                //read doc
                Word.Application wordApp = null;
                Word.Document doc = null;

                try
                {
                    wordApp = new Word.Application();
                    wordApp.DisplayAlerts = NetOffice.WordApi.Enums.WdAlertLevel.wdAlertsNone;          //disable ms word alerts
                    wordApp.Visible = false;

                    doc = wordApp.Documents.Open(filePath, Type.Missing, Type.Missing, Type.Missing,
                        Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                        Type.Missing, Type.Missing, Type.Missing, false, Type.Missing,
                        Type.Missing, Type.Missing, Type.Missing);

                    doc.Protect(WdProtectionType.wdNoProtection);

                    doc.SaveAs(destinationFile, WdSaveFormat.wdFormatPDF);

                    return new PDFFiles(destinationFile);

                }
                catch (Exception ex)
                {
                    this.convertionErrorMsg = ex.Message;
                    throw ex;
                }
                catch
                {                    
                    throw;
                }
                finally
                {
                    doc.Close(saveChanges: false);
                    doc.Dispose();
                    wordApp.Quit();
                    wordApp.Dispose();
                    GC.Collect();
                    if (deleteOiginal) this.delete();
                }
            });

        }      

        //private bool fileLocked()
        //{
        //    int elapseTime = 0;
        //    int delay = 250; //wait 250 milliseconds each time
        //    int timeout = 5; //5 seconds
        //    FileInfo file = new FileInfo(this.filePath);

        //    while (IsFileLocked(file) && elapseTime / 1000 < timeout)
        //    {
        //        System.Threading.Thread.Sleep(delay);
        //        elapseTime += delay;
        //    }

        //    return IsFileLocked(file);
        //}

        //protected virtual bool IsFileLocked(FileInfo file)
        //{
        //    FileStream stream = null;

        //    try
        //    {
        //        stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        //    }
        //    catch (IOException)
        //    {
        //        //the file is unavailable because it is:
        //        //still being written to
        //        //or being processed by another thread
        //        //or does not exist (has already been processed)
        //        return true;
        //    }
        //    finally
        //    {
        //        if (stream != null)
        //            stream.Close();
        //    }

        //    //file is not locked
        //    return false;
        //}
    }
}
