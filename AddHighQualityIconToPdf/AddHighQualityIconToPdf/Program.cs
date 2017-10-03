using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using iTextSharp.text.pdf;

namespace AddHighQualityIconToPdf
{
    class Program
    {
        static string _TempPDFPath = @"C:\Temp\Imza\3b6b0327882b42fb9cbeac0f82802fba";
        static string _TempImagePath = @"C:\Temp\Imza\testSon.pdf";

        static void Main(string[] args)
        {
            File.WriteAllBytes(@"C:\Temp\Imza\UzunlukTesti.png", CreateTextImage("Vural Güçlü Vural", "OFİS HİZMETLERİ PERSONELİ", "", true));
            byte[] pdfIn = File.ReadAllBytes(_TempPDFPath);
            byte[] pdfOut = null;
            List<KeyValuePair<string, string>> bookMCoord = new List<KeyValuePair<string, string>>();
            bookMCoord.Add(new KeyValuePair<string, string>("ImzaBilgileri1", "1;100;700"));
            bookMCoord.Add(new KeyValuePair<string, string>("ImzaBilgileri2", "1;300;700"));
            Dictionary<string, byte[]> sozluk = new Dictionary<string, byte[]>();
            //sozluk.Add("ImzaBilgileri1", );
            sozluk.Add("ImzaBilgileri2", CreateTextImage("Adı Soyadı", "Ünvanı", "", true));
            InsertAbsolutePositionIcon(pdfIn, out pdfOut, bookMCoord, sozluk);
            File.WriteAllBytes(_TempImagePath, pdfOut);
        }

        public static void InsertAbsolutePositionIcon(byte[] sourceArray, out byte[] destinationArray, List<KeyValuePair<string, string>> bookMCoord, Dictionary<string, byte[]> sozluk)//, bool sifrelenecekMi)
        {
            MemoryStream destinationStream = new MemoryStream();
            iTextSharp.text.pdf.PdfReader reader = null;
            iTextSharp.text.pdf.PdfStamper stamper = null;

            try
            {
                reader = new iTextSharp.text.pdf.PdfReader(sourceArray);
                stamper = new iTextSharp.text.pdf.PdfStamper(reader, destinationStream, '\0', true);
                int i = 0;
                foreach (KeyValuePair<string, string> kv in bookMCoord)
                {
                    if (!sozluk.ContainsKey(kv.Key))
                    {
                        continue;
                    }

                    string[] imzaCoord = kv.Value.Split(';');
                    int pageNumber = Convert.ToInt32(imzaCoord[0]);
                    int posX = Convert.ToInt32(imzaCoord[1]);
                    int posY = Convert.ToInt32(imzaCoord[2]);
                    //string stampName = "ImzaOnayKutusu" + kv.Key; //burada kv.Key hep 1 verdim ve unique olması lazım, gerçekte aktorId var bu nedenle alt satır açık bu commentli
                    string stampName = "ImzaciKutusu" + i.ToString() + kv.Key.ToString() + posX.ToString() + posY.ToString();

                    int x = 0;
                    int y = 25;
                    int h = 96;
                    int w = 192;

                    x = posX + 5;
                    y = posY - 5;

                    byte[] imzaPng = null;

                    if (sozluk.TryGetValue(kv.Key, out imzaPng))
                    {
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imzaPng);
                        image.SetAbsolutePosition(0, 0);
                        image.ScaleAbsoluteHeight(150);
                        image.ScaleAbsoluteWidth(300);
                        //PdfPTable table = new PdfPTable(1);
                        //table.SetTotalWidth(new float[] { 750f });
                        //table.AddCell(new PdfPCell(image));

                        iTextSharp.text.pdf.PdfTemplate template;
                        template = iTextSharp.text.pdf.PdfTemplate.CreateTemplate(stamper.Writer, image.Width, image.Height);
                        //table.WriteSelectedRows(0, -1, x, y, template);
                        template.AddImage(image);

                        iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(x, y, x + w, y - h);
                        iTextSharp.text.pdf.PdfAnnotation annotation = iTextSharp.text.pdf.PdfAnnotation.CreateStamp(stamper.Writer, rect, null, stampName);
                        annotation.SetAppearance(iTextSharp.text.pdf.PdfName.N, template);
                        annotation.Flags = iTextSharp.text.pdf.PdfAnnotation.FLAGS_PRINT;
                        stamper.AddAnnotation(annotation, pageNumber);
                    }

                    i++;
                }

                reader.Close();
                stamper.Close();
                destinationArray = destinationStream.ToArray();
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

        public static byte[] ExtractResource(String filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        private static byte[] CreateTextImage(string imzaciAd, string unvan, string vekilunvan, bool? eOnayMi)
        {
            int width = 800;
            int height = 400;
            PointF firstTextLocation = new PointF(0f, 10f);
            PointF secondTextLocation = new PointF(0f, 70f);
            PointF thirdTextLocation = new PointF(0f, 130f);
            Point iconLocation;

            if (!String.IsNullOrEmpty(vekilunvan))
            {
                iconLocation = new Point(150, 190);
            }
            else
            {
                iconLocation = new Point(150, 130);
            }

            int fontSize = 48;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap img = new Bitmap(width, height))
                {
                    img.SetResolution(300, 300);

                    using (Graphics drawing = Graphics.FromImage(img))
                    {
                        using (System.Drawing.Font textFont = new System.Drawing.Font("Times New Roman", fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            drawing.TextRenderingHint = TextRenderingHint.AntiAlias;
                            drawing.DrawString(imzaciAd, textFont, new SolidBrush(System.Drawing.Color.Black), firstTextLocation);

                            if (!String.IsNullOrEmpty(vekilunvan))
                            {
                                drawing.DrawString(vekilunvan, textFont, new SolidBrush(System.Drawing.Color.Black), secondTextLocation);
                                drawing.DrawString(unvan, textFont, new SolidBrush(System.Drawing.Color.Black), thirdTextLocation);
                            }
                            else
                            {
                                drawing.DrawString(unvan, textFont, new SolidBrush(System.Drawing.Color.Black), secondTextLocation);
                            }

                            string iconPath;

                            if (eOnayMi.HasValue)
                            {
                                if (eOnayMi.Value)
                                {
                                    iconPath = "Denemeler.icons.eonayicon.png";
                                }
                                else
                                {
                                    iconPath = "Denemeler.icons.eimzaicon.png";
                                }
                                using (MemoryStream msIcon = new MemoryStream(ExtractResource(iconPath)))
                                {
                                    drawing.DrawImage(Image.FromStream(msIcon), new Rectangle(iconLocation, new Size(150, 150)));
                                }
                            }
                        }
                        drawing.Save();
                    }

                    img.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

    }
}
