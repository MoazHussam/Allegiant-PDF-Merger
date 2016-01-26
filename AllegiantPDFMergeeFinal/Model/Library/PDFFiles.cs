using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Word = NetOffice.WordApi;
//using Document = NetOffice.WordApi.Document;
//using NetOffice.WordApi.Enums;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;

namespace AllegiantPDFMerger
{
    class PDFFiles : Files
    {
        public PDFFiles(string filePath)
            : base(filePath)
        {

        }

        public static bool Merge(List<PDFFiles> InFiles, string OutFile)
        {
            try
            {

                InFiles.ForEach(file =>
                {
                    PdfReader reader = null;

                    reader = new PdfReader(file.filePath);
                    if (!reader.IsOpenedWithFullPermissions) throw new System.IO.FileLoadException("Cannot merge because \"" + file.fileName + "\" is Locked for editing");
                });
            }
            catch (System.IO.FileLoadException)
            {
                throw;
            }
            catch
            {
                return false;
            }

            try
            {
                using (FileStream stream = new FileStream(OutFile, FileMode.Create))
                using (Document doc = new Document(PageSize.A4))
                using (PdfCopy pdf = new PdfCopy(doc, stream))
                {
                    doc.Open();

                    PdfReader reader = null;
                    PdfImportedPage page = null;

                    //fixed typo
                    InFiles.ForEach(file =>
                    {
                        reader = new PdfReader(file.filePath);

                        for (int i = 0; i < reader.NumberOfPages; i++)
                        {
                            page = pdf.GetImportedPage(reader, i + 1);
                            //doc.SetPageSize(page.Width <= page.Height ? PageSize.A4 : PageSize.A4.Rotate());
                            pdf.AddPage(page);
                        }

                        pdf.FreeReader(reader);
                        reader.Close();
                    });
                }
            }
            catch
            {
                return false;
            }


            ScaleToA4(OutFile, OutFile);
            return true;
        }

        public static void ScaleToA4(string inPDF, string outPDF)
        {
            var reader = new PdfReader(new MemoryStream(File.ReadAllBytes(inPDF)));
            var document = new Document(PageSize.A4);
            var ms = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, ms);
            document.Open();
            var cb = writer.DirectContent;

            for (var pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
            {
                var page = writer.GetImportedPage(reader, pageNumber);
                document.SetPageSize(page.Width <= page.Height ?
                    PageSize.A4 : PageSize.A4.Rotate());
                document.NewPage();

                var widthFactor = document.PageSize.Width / page.Width;
                var heightFactor = document.PageSize.Height / page.Height;
                var factor = Math.Min(widthFactor, heightFactor);

                var offsetX = (document.PageSize.Width - (page.Width * factor)) / 2;
                var offsetY = (document.PageSize.Height - (page.Height * factor)) / 2;
                cb.AddTemplate(page, factor, 0, 0, factor, offsetX, offsetY);
            }
            document.Close();
            File.WriteAllBytes(outPDF, ms.GetBuffer());
        }
    }
}
