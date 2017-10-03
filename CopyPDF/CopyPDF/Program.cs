using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CopyPDF
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@System.Configuration.ConfigurationManager.AppSettings["txtPath"].ToString());
                string destination = System.Configuration.ConfigurationManager.AppSettings["DestinationPath"].ToString();
                StringBuilder errorText = new StringBuilder();
                int index = 1;
                /*
                 * txt içine aşağıdaki gibi kayıt edilmelidir.
                 * du."Adres" || '\' || d."Adres" || '#' || d."DokumanAdi"
                 * du depolamaunitesi
                 * d dokuman
                */
                foreach (string line in lines)
                {
                    index++;
                    string path = @line.Split('#')[0];
                    string dokumanAdi = @line.Split('#')[1];

                    if (File.Exists(@path))
                    {
                        string fullDestination = @destination + index.ToString() + "_" + @dokumanAdi;

                        if (!File.Exists(fullDestination))
                        {
                            File.Copy(@path, @fullDestination);
                        }
                    }
                    else
                    {
                        errorText.AppendLine(@path);
                    }
                }

                if (!String.IsNullOrEmpty(errorText.ToString()))
                {
                    File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["ErrorPath"].ToString(), errorText.ToString());
                }

                Console.WriteLine("Bitti");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
