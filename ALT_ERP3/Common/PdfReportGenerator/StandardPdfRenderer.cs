// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StandardPdfRenderer.cs" company="SemanticArchitecture">
//   http://www.SemanticArchitecture.net
// </copyright>
// <summary>
//   This class is responsible for rendering a html text string to a PDF document
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.ReportManagement
{
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.html.simpleparser;
    using iTextSharp.text.pdf;

    /// <summary>
    /// This class is responsible for rendering a html text string to a PDF document using the html renderer of iTextSharp.
    /// </summary>
    public class StandardPdfRenderer
    {
        private const int HorizontalMargin = 40;
        private const int VerticalMarginUp = 120;
        private const int VerticalMarginDown = 120;

        public byte[] Render(string htmlText, string pageTitle, string[] pageHeader, string[] pageFooter, string ImagePath)
        {
            byte[] renderedBuffer;

            using (var outputMemoryStream = new MemoryStream())
            {
                using (var pdfDocument = new Document(PageSize.A4, HorizontalMargin, HorizontalMargin, VerticalMarginUp, VerticalMarginDown))
                {
                    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument, outputMemoryStream);
                    pdfWriter.CloseStream = false;
                    pdfWriter.PageEvent = new PrintHeaderFooter { Title = pageTitle, FooterText = pageFooter, HeaderText = pageHeader, path = ImagePath };
                    pdfDocument.Open();
                    using (var htmlViewReader = new StringReader(htmlText))
                    {
                        using (var htmlWorker = new HTMLWorker(pdfDocument))
                        {
                            htmlWorker.Parse(htmlViewReader);
                        }
                    }
                }

                renderedBuffer = new byte[outputMemoryStream.Position];
                outputMemoryStream.Position = 0;
                outputMemoryStream.Read(renderedBuffer, 0, renderedBuffer.Length);
            }

            return renderedBuffer;
        }
    }
}
