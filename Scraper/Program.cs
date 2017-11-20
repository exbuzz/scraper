using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MarkupSanitizer;

namespace Scraper
{
    class Program
    {
        public static string BaseUrl = "https://www.italian-verbs.com/italian-verbs/conjugation.php";
        static void Main(string[] args)
        {
            string word = "avere";

            string html = getHtml(word);

            SanitizedMarkup markup = Sanitizer.SanitizeMarkup(html);

            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.DtdProcessing = DtdProcessing.Ignore;
            //using (XmlReader reader = XmlReader.Create(dataStream, settings))
            //{
            //    reader.MoveToContent();
            //    reader.ReadToDescendant("table");
            //    string x = reader.ReadElementContentAsString();
            //}
        }

        static string getHtml(string word)
        {
            
            WebRequest request = WebRequest.Create($"{BaseUrl}?parola={word}");
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

       
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();



            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
    }
}
