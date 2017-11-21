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

            //SanitizedMarkup markup = Sanitizer.SanitizeMarkup(html);
            //html = markup.MarkupText;



            html = removeElement("<script.*?>.*?</script>",html);
            html = removeElement("<head.*?>.*?</head>", html);
            html = removeElement("<img.*?>", html);
            html = removeElement("<br.*?>", html);
            html = removeElement("<hr.*?>", html);
            html = removeElement("<input.*?>", html);
            html = removeElement("<center.*?>.*?</center>", html);
            html = removeElement("<a.*?>.*?</a>", html);
            html = html.Replace("<tr></table>", "</tr></table>");
            html = html.Replace("<span class=\"br_italiano\">", string.Empty);
            html = html.Replace("<span class=\"br_italiano\" style=\"color:#FF0000; font-weight:bold;\">", string.Empty);
            html = html.Replace("<td align=\"left\" width=\"50%\"></span></td></tr>", "<td></td></tr>");


            html = html.Replace("<span class=\"br_italiano\">", string.Empty);
            html = html.Replace("<span lang=\"it\">", string.Empty);

             MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(html);
            ms.Position = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using (XmlReader reader = XmlReader.Create(ms, settings))
            {
                reader.MoveToContent();
                reader.ReadToDescendant("table");

                string x = reader.ReadElementContentAsString();
            }
        }

        static string removeElement(string pattern, string html)
        {
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            return  regex.Replace(html, string.Empty);
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
