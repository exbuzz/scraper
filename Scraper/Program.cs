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
            html = html.Replace("&nbsp;", string.Empty);


            html = Regex.Replace(html, @"<([^/]*?)\s+[^/>]+>", "<$1>",RegexOptions.Singleline);
            html = Regex.Replace(html, @"<!.*?>", string.Empty, RegexOptions.Singleline);

            SanitizedMarkup markup = Sanitizer.SanitizeMarkup(html);
            html = markup.MarkupText;

            html = html.Replace("--&gt;",string.Empty);
            html = html.Replace("&gt;", string.Empty);

            html = Regex.Replace(html, @"\&\#[0-9]+\;", string.Empty,RegexOptions.Singleline);

            html = Regex.Replace(html, @"<td>\s*</td>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<tr>\s*</tr>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<table>\s*</table>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<li>\s*</li>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<ul>\s*</ul>", string.Empty, RegexOptions.Singleline);

            html = html.Replace("<span>", String.Empty).Replace("</span>",string.Empty);
            html = html.Replace("<strong>", String.Empty).Replace("</strong>", string.Empty);
            html = html.Replace("<h3>", String.Empty).Replace("</h3>", string.Empty);
            html = html.Replace("<p>", String.Empty).Replace("</p>", string.Empty);

            html = "<html><body>" + html + "</body></html>";
            html = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + Environment.NewLine + html;

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(html);
            sw.Flush();
            ms.Position = 0;
            //StreamReader sr = new StreamReader(ms);
            //string html2 = sr.ReadToEnd();

            //XmlDocument xmlDocument = new XmlDocument();
            //xmlDocument.LoadXml(html);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            StringBuilder sb = new StringBuilder();
            using (XmlReader reader = XmlReader.Create(ms, settings))
            {
                while(reader.Read())
                {
                    if(reader.NodeType==XmlNodeType.Element && reader.Name=="td")
                    {
                        string x = reader.ReadElementContentAsString();
                        sb.Append(x);
                    }
                }
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
