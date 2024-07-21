// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrintHeaderFooter.cs" company="SemanticArchitecture">
//   http://www.SemanticArchitecture.net pkalkie@gmail.com
// </copyright>
// <summary>
//   This class represents the standard header and footer for a PDF print.
//   application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Common.ReportManagement
{
    using System;

    using iTextSharp.text;
    using iTextSharp.text.pdf;
    

    /// <summary>
    /// This class represents the standard header and footer for a PDF print.
    /// application.
    /// </summary>
    public class PrintHeaderFooter : PdfPageEventHelper
    {
        private PdfContentByte pdfContent;
        private PdfTemplate pageNumberTemplate;
        private BaseFont baseFont;
        private DateTime printTime;
        private Image HeaderImage;

        public string Title { get; set; }

        public string[] FooterText { get; set; }

        public string[] HeaderText { get; set; }

        public string path { get; set; }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            printTime = DateTime.Now;
            baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            pdfContent = writer.DirectContent;
            pageNumberTemplate = pdfContent.CreateTemplate(50, 50);
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);

            Rectangle pageSize = document.PageSize;
            // HeaderImage = Image.GetInstance(path);
            //HeaderImage.ScaleToFit(180, 150);
            //HeaderImage.SetAbsolutePosition(pageSize.GetRight(200), pageSize.GetTop(100));

            // HeaderImage.Border = Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER;
            //HeaderImage.BorderWidth = 1f;

            //if (path != string.Empty)
            //{
            //    pdfContent.BeginText();
            //    pdfContent.SetFontAndSize(baseFont, 11);
            //    pdfContent.SetRGBColorFill(0, 0, 0);
            //    pdfContent.SetTextMatrix(pageSize.GetRight(200), pageSize.GetTop(10));
            //    pdfContent.AddImage(HeaderImage);
            //    pdfContent.EndText();
            //}

            if (Title != string.Empty)
            {
                pdfContent.BeginText();
                pdfContent.SetFontAndSize(baseFont, 11);
                pdfContent.SetRGBColorFill(0, 0, 0);
                pdfContent.SetTextMatrix(pageSize.GetLeft(40), pageSize.GetTop(40));
                pdfContent.ShowText(Title);
                pdfContent.EndText();
            }

            if (HeaderText.Length > 0)
            {
                for (int i = 0; i < HeaderText.Length; i++)
                {
                    pdfContent.BeginText();
                    pdfContent.SetFontAndSize(baseFont, 8);
                    pdfContent.SetRGBColorFill(0, 0, 0);
                    pdfContent.ShowTextAligned(PdfContentByte.ALIGN_LEFT, HeaderText[i], pageSize.Width - 200, pageSize.GetTop(100 - i * 20), 0);
                    pdfContent.EndText();
                }
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            int pageN = writer.PageNumber;
            string text = pageN + " - ";
            float len = baseFont.GetWidthPoint(text, 8);

            Rectangle pageSize = document.PageSize;
            pdfContent = writer.DirectContent;
            pdfContent.SetRGBColorFill(100, 100, 100);

            if (FooterText.Length > 0)
            {
                for (int i = 0; i < FooterText.Length; i++)
                {
                    pdfContent.BeginText();
                    pdfContent.SetFontAndSize(baseFont, 8);
                    pdfContent.SetRGBColorFill(0, 0, 0);
                    pdfContent.ShowTextAligned(PdfContentByte.ALIGN_CENTER, FooterText[i], pageSize.Width / 2, pageSize.GetBottom(80 - i * 10), 0);
                    pdfContent.EndText();
                }
            }

            pdfContent.BeginText();
            pdfContent.SetFontAndSize(baseFont, 8);
            pdfContent.SetTextMatrix(pageSize.Width / 2, pageSize.GetBottom(30));
            pdfContent.ShowText(text);
            pdfContent.EndText();

            pdfContent.AddTemplate(pageNumberTemplate, (pageSize.Width / 2) + len, pageSize.GetBottom(30));

            pdfContent.BeginText();
            pdfContent.SetFontAndSize(baseFont, 8);
            pdfContent.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, printTime.ToString(), pageSize.GetRight(40), pageSize.GetBottom(30), 0);
            pdfContent.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            pageNumberTemplate.BeginText();
            pageNumberTemplate.SetFontAndSize(baseFont, 8);
            pageNumberTemplate.SetTextMatrix(0, 0);
            pageNumberTemplate.ShowText(string.Empty + (writer.PageNumber - 1));
            pageNumberTemplate.EndText();
        }
    }
}