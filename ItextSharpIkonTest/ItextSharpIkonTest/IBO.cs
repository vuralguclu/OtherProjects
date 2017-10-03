using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ItextSharpIkonTest
{
    [Serializable]
    public class PdfUpdateParameters
    {
        /// <summary>
        /// The actual file to be update
        /// </summary>
        public byte[] PDFContent { get; set; }

        /// <summary>
        /// Name of the document updated
        /// </summary>
        public string PDFName { get; set; }

        /// <summary>
        /// information whether all pages will be printed
        /// </summary>
        public bool ToAllPages { get; set; }

        /// <summary>
        /// information of which pages will be printed
        /// </summary>
        public List<int> PageNumbers { get; set; }

        /// <summary>
        /// information to be printed
        /// </summary>
        public InfoToBePrinted Info { get; set; }


    }

    [Serializable]
    public class InfoToBePrinted
    {
        /// <summary>
        /// Confirmation Info
        /// </summary>
        public string ConfirmationInfo { get; set; }

        /// <summary>
        /// Font Name
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// Font Size
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// HTML Color
        /// </summary>
        public string HTMLColor { get; set; }

        /// <summary>
        /// xCoordinate
        /// </summary>
        public int PDF_X { get; set; }

        /// <summary>
        /// yCoordinate
        /// </summary>
        public int PDF_Y { get; set; }

        /// <summary>
        /// width
        /// </summary>
        public int TextWidth { get; set; }
    }

    [Serializable]
    public class PdfUpdateOperationResult
    {
        /// <summary>
        /// Property to set success or failure message
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Size of the downloaded document
        /// </summary>
        public string NewDocumentSize { get; set; }

        /// <summary>
        /// Content of the downloaded document
        /// </summary>
        public byte[] NewDocumentFile { get; set; }

    }

    public class IBO
    {
        static void Main(string[] args)
        {
            ServiceReference1.FileManagerServiceSoapClient client = new ServiceReference1.FileManagerServiceSoapClient();

            ServiceReference1.PdfUpdateParameters pdfParameters = new ServiceReference1.PdfUpdateParameters();
            pdfParameters.PageNumbers = new ServiceReference1.ArrayOfInt();
            pdfParameters.PageNumbers.Add(1);
            pdfParameters.PageNumbers.Add(3);
            pdfParameters.PageNumbers.Add(5);
            pdfParameters.PDFName = "PdfTest.pdf";
            pdfParameters.PDFContent = File.ReadAllBytes(@"C:\PdfTest.pdf");
            pdfParameters.ToAllPages = true;
            pdfParameters.Info = new ServiceReference1.InfoToBePrinted();
            pdfParameters.Info.ConfirmationInfo = "hop hop";
            pdfParameters.Info.FontName = "arial";
            pdfParameters.Info.FontSize = 10;
            pdfParameters.Info.HTMLColor = "#FF0000";
            pdfParameters.Info.PDF_X = 100;
            pdfParameters.Info.PDF_Y = 100;
            ServiceReference1.PdfUpdateOperationResult result = client.PdfUpdate(pdfParameters);
            File.WriteAllBytes(@"C:\newnew.pdf", result.NewDocumentFile);
            client.Close();
        }

        public static PdfUpdateOperationResult PdfUpdate(PdfUpdateParameters updateDocParameters)
        {
            PdfUpdateOperationResult result = new PdfUpdateOperationResult();
            System.Drawing.Font font = CreateFont(updateDocParameters.Info.FontName,updateDocParameters.Info.FontSize);
            System.Drawing.Color color = CreateColor(updateDocParameters.Info.HTMLColor);
            
            byte[] png = DrawTextToPng(updateDocParameters.Info.ConfirmationInfo, font, color, updateDocParameters.Info.TextWidth);
            byte[] newPdf = CreatePdf(updateDocParameters.PDFContent, png, updateDocParameters.Info.PDF_X, updateDocParameters.Info.PDF_Y, updateDocParameters.ToAllPages, updateDocParameters.PageNumbers);

            return result;
        }

        private static System.Drawing.Color CreateColor(string colorCode)
        {
            bool existsColor = System.Text.RegularExpressions.Regex.IsMatch(colorCode, @"^#(?:[0-9a-fA-F]{3}){1,2}$");

            if (existsColor)
            {
                return System.Drawing.ColorTranslator.FromHtml(colorCode);
            }
            else
            {
                return System.Drawing.ColorTranslator.FromHtml("#000000");
            }
        }

        private static System.Drawing.Font CreateFont(string fontName, int fontSize)
        {
            System.Drawing.Font font = new System.Drawing.Font(fontName, fontSize);

            if (font.Name.ToLower() != fontName.ToLower())
            {
                if (fontSize <= 0)
                {
                    font = new System.Drawing.Font("arial", 10);
                }
                else
                {
                    font = new System.Drawing.Font("arial", fontSize);
                }
            }

            return font;
        }

        private static bool CheckPageNumbers(bool toAllPages, List<int> pageNumbers, int numberOfPages)
        {
            if (toAllPages)
            {
                if (numberOfPages > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                for (int index = 0; index < pageNumbers.Count; index++)
                {
                    if (pageNumbers[index] > numberOfPages || pageNumbers[index] < 1)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static byte[] DrawTextToPng(String text, System.Drawing.Font font, System.Drawing.Color textColor, int maxWidth)
        {
            System.Drawing.Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            SizeF textSize;

            if (maxWidth <= 0)
            {
                textSize = drawing.MeasureString(text, font);
            }
            else
            {
                textSize = drawing.MeasureString(text, font, maxWidth);
            }
            
            StringFormat sf = new StringFormat();
            Brush textBrush = new SolidBrush(textColor);

            sf.Trimming = StringTrimming.Word;
            img.Dispose();
            drawing.Dispose();
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);
            drawing.CompositingQuality = CompositingQuality.HighQuality;
            drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
            drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
            drawing.SmoothingMode = SmoothingMode.HighQuality;
            drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            drawing.Clear(System.Drawing.Color.Transparent);
            drawing.DrawString(text, font, textBrush, new RectangleF(0, 0, textSize.Width, textSize.Height), sf);
            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();
            
            MemoryStream pngStream = new MemoryStream();
            img.Save(pngStream, ImageFormat.Png);
            img.Dispose();
            return pngStream.ToArray();
        }

        private static byte[] CreatePdf(byte[] pdfIn, byte[] pngIn, int xCoordinate, int yCoordinate, bool toAllPages, List<int> pageNumbers)
        {
            MemoryStream destinationStream = new MemoryStream();
            iTextSharp.text.pdf.PdfReader reader = null;
            iTextSharp.text.pdf.PdfStamper stamper = null;

            try
            {
                reader = new iTextSharp.text.pdf.PdfReader(pdfIn);
                int numberOfPages = reader.NumberOfPages;

                if (CheckPageNumbers(toAllPages, pageNumbers, numberOfPages) == false)
                {
                    return null;//HATA - geçersiz sayfa sayısı
                }
                
                if (toAllPages)
                {
                    pageNumbers = new List<int>();

                    for (int index = 1; index <= numberOfPages; index++)
                    {
                        pageNumbers.Add(index);
                    }
                }

                stamper = new iTextSharp.text.pdf.PdfStamper(reader, destinationStream, '\0', true);

                foreach (int pageNumber in pageNumbers)
                {
                    int posX = Convert.ToInt32(xCoordinate);
                    int posY = Convert.ToInt32(yCoordinate);
                    string stampName = "Page" + pageNumber.ToString();

                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(pngIn);
                    image.SetAbsolutePosition(0, 0);

                    iTextSharp.text.pdf.PdfTemplate template = iTextSharp.text.pdf.PdfTemplate.CreateTemplate(stamper.Writer, image.Width, image.Height);
                    template.AddImage(image);

                    iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(xCoordinate, yCoordinate, xCoordinate + image.Width, yCoordinate - image.Height);
                    iTextSharp.text.pdf.PdfAnnotation annotation = iTextSharp.text.pdf.PdfAnnotation.CreateStamp(stamper.Writer, rect, null, stampName);
                    annotation.SetAppearance(iTextSharp.text.pdf.PdfName.N, template);
                    annotation.Flags = iTextSharp.text.pdf.PdfAnnotation.FLAGS_PRINT;
                    stamper.AddAnnotation(annotation, pageNumber);
                }

                reader.Close();
                stamper.Close();
                return destinationStream.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (destinationStream != null)
                    destinationStream.Close();
            }
        }
    }
}
