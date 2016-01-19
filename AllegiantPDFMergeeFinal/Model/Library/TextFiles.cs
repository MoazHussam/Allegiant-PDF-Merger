using AllegiantPDFMerger;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllegiantPDFMerger
{
    class TextFiles :Files
    {
        public TextFiles(string filePath) 
            :base(filePath)
        {
            
        }

        private async Task<PDFFiles> convertToPDF(string outPath)
        {
            return await Task.Run(() =>
                {
                    Document newTextPDF = new Document(PageSize.A4);

                    PdfWriter.GetInstance(newTextPDF, new FileStream(outPath, FileMode.Create));

                    newTextPDF.Open();

                    newTextPDF.Add(new Paragraph(this.getText()));

                    newTextPDF.Close();

                    return new PDFFiles(outPath);
                });
        }

        public async Task<PDFFiles> convertToPdfAsync(string outPath)
        {
            return await this.convertToPDF(outPath);
        }

        public string getText()
        {
            return System.IO.File.ReadAllText(this.filePath);
        }
    }
}
