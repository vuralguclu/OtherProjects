using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ItextSharpIkonTest
{
    /*EdiTT'teki kodun taklidi*/
    public class Bookmark
    {
        public string Name { get; private set; }
        public string Action { get; private set; }
        public string Page { get; private set; }
        public PageInfo Info { get; private set; }

        public Bookmark(string name, string action, string page)
        {
            this.Name = name;
            this.Action = action;
            this.Page = page;
            this.Info = new PageInfo(page);
        }
    }
    /*EdiTT'teki kodun taklidi*/
    public class PageInfo
    {
        public int PageId { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public PageInfo(string page)
        {
            try
            {
                string[] m_values = page.Split(' ');
                this.PageId = Convert.ToInt32(m_values[0].ToString());
                this.X = Convert.ToInt32(m_values[2].ToString());
                this.Y = Convert.ToInt32(m_values[3].ToString());
            }
            catch
            {

                throw;
            }
        }
    }
    /*EdiTT'teki kodun taklidi*/
    public class PdfBookmark
    {
        /*EdiTT'teki kodun taklidi*/
        public List<Bookmark> GetBookmark(byte[] pdfIn, byte[] ownerPassword)
        {
            List<Bookmark> m_bookmarks = new List<Bookmark>();
            myTextSharp.text.pdf.PdfReader m_reader = null;
            try
            {
                m_reader = new myTextSharp.text.pdf.PdfReader(pdfIn, ownerPassword);
                IList<Dictionary<string, object>> bookmarks = myTextSharp.text.pdf.SimpleBookmark.GetBookmark(m_reader);

                if (bookmarks != null)
                {
                    if (bookmarks.Count > 0)
                    {

                        foreach (Dictionary<string, object> m_bookmark in bookmarks)
                        {
                            string m_title = string.Empty;
                            string m_action = string.Empty;
                            string m_page = string.Empty;

                            foreach (var item in m_bookmark)
                            {
                                switch (item.Key)
                                {
                                    case "Title":
                                        m_title = item.Value.ToString();
                                        break;
                                    case "Action":
                                        m_action = item.Value.ToString();
                                        break;
                                    case "Page":
                                        m_page = item.Value.ToString();
                                        break;
                                }
                            }
                            m_bookmarks.Add(new Bookmark(m_title, m_action, m_page));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_bookmarks = null;
            }
            finally
            {
                if (m_reader != null)
                {
                    m_reader.Close();
                }
            }
            return (m_bookmarks);
        }

        /*EdiTT'teki kodun taklidi*/
        public List<Bookmark> GetBookmarkEx(iTextSharp.text.pdf.PdfReader m_reader, byte[] pdfIn)
        {
            List<Bookmark> m_bookmarks = new List<Bookmark>();
            try
            {
                ArrayList bookmarks = iTextSharp.text.pdf.SimpleBookmark.GetBookmark(m_reader);

                foreach (var item in bookmarks.ToArray())
                {
                    Hashtable map = item as Hashtable;
                    string m_title = string.Empty;
                    string m_action = string.Empty;
                    string m_page = string.Empty;
                    m_title = map["Title"].ToString();
                    m_action = map["Action"].ToString();
                    m_page = map["Page"].ToString();


                    m_bookmarks.Add(new Bookmark(m_title, m_action, m_page));

                }
            }
            catch (Exception ex)
            {
                m_bookmarks = null;
            }
            return (m_bookmarks);
        }

        /*BU YENİ METOT, verilen bookmarkların olduğu yere verilen imajı basıyor*/
        public void InsertAbsolutePositionIcon(byte[] pdfIn, byte[] iconIn, List<KeyValuePair<long, KeyValuePair<string, string>>> bookMCoord)
        {
            using (FileStream outputPdfStream = new FileStream(@"D:\Folder\NewPDF_" + DateTime.Now.Ticks.ToString() + ".pdf", FileMode.Create, FileAccess.Write))
            {
                PdfReader reader = null;
                PdfStamper stamper = null;
                try
                {
                    reader = new PdfReader(pdfIn);
                    stamper = new PdfStamper(reader, outputPdfStream, '\0', true);

                    foreach (KeyValuePair<long, KeyValuePair<string, string>> kv in bookMCoord)
                    {
                        string[] imzaCoord = kv.Value.Value.Split(';');
                        int pageNumber = Convert.ToInt32(imzaCoord[0]);
                        int posX = Convert.ToInt32(imzaCoord[1]);
                        int posY = Convert.ToInt32(imzaCoord[2]);
                        //string stampName = "ImzaOnayKutusu" + kv.Key; //burada kv.Key hep 1 verdim ve unique olması lazım, gerçekte aktorId var bu nedenle alt satır açık bu commentli
                        string stampName = "ImzaOnayKutusu" + kv.Key.ToString() + posX.ToString() + posY.ToString(); 

                        int x = 0;
                        int y = 25;
                        int h = 30;
                        int w = 30;

                        x = posX + 50;
                        y = posY - 5;

                        Image image = Image.GetInstance(iconIn);
                        image.SetAbsolutePosition(0, 0);

                        PdfTemplate template = PdfTemplate.CreateTemplate(stamper.Writer, image.Width, image.Height);
                        template.AddImage(image);

                        Rectangle rect = new Rectangle(x, y, x + w, y-h);
                        PdfAnnotation annotation = PdfAnnotation.CreateStamp(stamper.Writer, rect, null, stampName);
                        annotation.SetAppearance(PdfName.N, template);
                        annotation.Flags = PdfAnnotation.FLAGS_PRINT;
                        stamper.AddAnnotation(annotation, pageNumber);

                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    reader.Close();
                    stamper.Close();
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*BU KISIM new TaslakYonetimiBusiness().GetBookmarkCoordinate(item); kodunu taklit ediyor.*/
            byte[] pdfIn  = File.ReadAllBytes(@"D:\Folder\5081f98763b14fba922673b8ad624d7a.pdf");
            byte[] iconIn = ExtractResource(@"ItextSharpIkonTest.icons.onaylogo.jpg");

            List<KeyValuePair<long, KeyValuePair<string, string>>> bookMCoord = new List<KeyValuePair<long, KeyValuePair<string, string>>>();
            List<Bookmark> bookmarkList = new PdfBookmark().GetBookmark(pdfIn, null);

            if (bookmarkList != null && bookmarkList.Count > 0)
            {
                foreach (Bookmark bookm in bookmarkList)
                {
                    if (bookm.Name.StartsWith("bm_"))
                    {
                        string[] bookMArr = bookm.Name.Split('_');
                        KeyValuePair<string, string> tempPair = new KeyValuePair<string, string>(bookMArr[1], bookm.Info.PageId + ";" + bookm.Info.X + ";" + bookm.Info.Y);
                        bookMCoord.Add(new KeyValuePair<long, KeyValuePair<string, string>>(Convert.ToInt32("1"), tempPair));
                    }
                }
            }

            /*********************************************/

            /*ASIL İŞİ YAPAN YENİ METOT!!!!*/
            new PdfBookmark().InsertAbsolutePositionIcon(pdfIn, iconIn, bookMCoord);

            

        }

        /*Embedded Resource Okuma ÖRNEĞİ*/
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
    }
}
