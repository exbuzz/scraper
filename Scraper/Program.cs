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
using System.Web.Script.Serialization;

namespace Scraper
{
    class Program
    {
        public static string BaseUrl = "https://www.italian-verbs.com/italian-verbs/conjugation.php";
        public static string GoogleTranslateUrl = "https://translation.googleapis.com/language/translate/v2?key=AIzaSyABjcSkcaGLlIT1iEG2KMO8p58rLpAzonk";

        public static string[] Moods = {
                "INDICATIVO",
                "CONGIUNTIVO",
                "CONDIZIONALE",
                "IMPERATIVO",
                "INFINITO",
                "PARTICIPIO",
                "GERUNDIO"
        };


        public static string[] Tenses = {
            "PRESENTE",
            "IMPERFETTO",
            "PASSATO REMOTO",
            "FUTURO SEMPLICE",
            "PASSATO PROSSIMO",
            "TRAPASSATO PROSSIMO",
            "TRAPASSATO REMOTO",
            "FUTURO ANTERIORE",
            "PASSATO",
            "TRAPASSATO"
        };

        static string[] getInfinitives()
        {
            string file = Path.Combine(Environment.CurrentDirectory,  "words.txt");
            return File.ReadAllLines(file);
        }

        static void Main(string[] args)
        {
            string[] infinitives = getInfinitives();
            string outFile = Path.Combine(Environment.CurrentDirectory, "out.csv");
            initializeOutfile(outFile);

            foreach (string infinitive in infinitives)
            {

                Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@");
                Console.WriteLine($"Starting new infinitive '{infinitive}'.");
                List<string> lines = getDataForInfinitive(infinitive);
                Console.WriteLine($"Finished processing all forms for '{infinitive}'.");


                lines.RemoveAt(0);
                lines.RemoveAt(0);

                lines.RemoveAt(lines.Count - 1);
                lines.RemoveAt(lines.Count - 1);

                StreamWriter fileWriter = new StreamWriter(File.Open(outFile,FileMode.Append));
                foreach (string line in lines)
                {
                    fileWriter.WriteLine(line);
                }
                fileWriter.Flush();
                fileWriter.Close();
                Console.WriteLine($"Finished '{infinitive}'.");
            }
            Console.WriteLine("Done.");
        }

        static void initializeOutfile(string outFile)
        {
            StreamWriter fileWriter = File.CreateText(outFile);
            fileWriter.Flush();
            fileWriter.Close();
        }
        static List<string> getDataForInfinitive(string infinitive)
        {
            string englishInfinitive = getEnglish(infinitive);

            string html = getHtml(infinitive);
            html = sanitize(html);


            string currentMode = string.Empty;
            string currentTense = string.Empty;
            string currentPronoun = string.Empty;

            int imperativoCounter = 0;

            List<string> lines = new List<string>();
            using (XmlReader reader = getReader(html))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "td")
                    {
                        string currentCell = reader.ReadElementContentAsString();
                        currentCell = currentCell.Trim();
                        currentCell = currentCell.ToLower();

                        if (Moods.Contains(currentCell, StringComparer.InvariantCultureIgnoreCase))
                        {
                            currentMode = currentCell;
                            currentTense = string.Empty;
                            currentPronoun = string.Empty;
                            Console.WriteLine("#####################");
                            Console.WriteLine($"Starting mood '{currentMode}'...");
                            continue;
                        }
                        if (Tenses.Contains(currentCell, StringComparer.InvariantCultureIgnoreCase))
                        {
                            currentTense = currentCell;
                            currentPronoun = string.Empty;
                            Console.WriteLine("---------------------");
                            Console.WriteLine($"Starting tense '{currentTense}'...");
                            continue;
                        }

                        Match pronounMatch = Regex.Match(currentCell, "(io|tu|lui/lei|noi|voi|loro)");
                        if (pronounMatch.Success)
                        {
                            currentPronoun = pronounMatch.Value;
                        }

                        if (currentMode.Equals("IMPERATIVO", StringComparison.InvariantCultureIgnoreCase))
                        {
                            handleImperativo(ref imperativoCounter, ref currentPronoun);
                        }
                        if (currentCell.Contains(":"))
                        {
                            string[] parts = currentCell.Split(':');
                            currentTense = parts[0]?.Trim();
                            currentCell = parts[1]?.Trim();
                        }
                        if (currentCell.Contains("(") && currentCell.Contains(")"))
                        {
                            currentCell = Regex.Replace(currentCell, @"\([^\)]*\)", string.Empty);
                        }


                        string english = getEnglish(currentCell);

                        lines.Add($"{infinitive};{englishInfinitive};{currentMode};{currentTense};{currentPronoun};{currentCell};{english}");
                    }
                }
            }

            return lines;
        }

        static XmlReader getReader(string html)
        {

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(html);
            sw.Flush();
            ms.Position = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            XmlReader reader = XmlReader.Create(ms, settings);
            return reader;
        }

        static void handleImperativo(ref int counter, ref string currentPronoun)
        {
            counter++;
            if (counter == 1) currentPronoun = "tu";
            if (counter == 2) currentPronoun = "lui/lei";
            if (counter == 3) currentPronoun = "noi";
            if (counter == 4) currentPronoun = "voi";
            if (counter == 5)
            {
                currentPronoun = "loro";
                counter = 0;
            }
        }

        static string sanitize(string html)
        {
            html = removeElement("<script.*?>.*?</script>", html);
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


            html = Regex.Replace(html, @"<([^/]*?)\s+[^/>]+>", "<$1>", RegexOptions.Singleline);
            html = Regex.Replace(html, @"<!.*?>", string.Empty, RegexOptions.Singleline);

            SanitizedMarkup markup = Sanitizer.SanitizeMarkup(html);
            html = markup.MarkupText;

            html = html.Replace("--&gt;", string.Empty);
            html = html.Replace("&gt;", string.Empty);

            html = Regex.Replace(html, @"\&\#[0-9]+\;", string.Empty, RegexOptions.Singleline);

            html = Regex.Replace(html, @"<td>\s*</td>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<tr>\s*</tr>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<table>\s*</table>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<li>\s*</li>", string.Empty, RegexOptions.Singleline);
            html = Regex.Replace(html, @"<ul>\s*</ul>", string.Empty, RegexOptions.Singleline);

            html = html.Replace("<span>", String.Empty).Replace("</span>", string.Empty);
            html = html.Replace("<strong>", String.Empty).Replace("</strong>", string.Empty);
            html = html.Replace("<h3>", String.Empty).Replace("</h3>", string.Empty);
            html = html.Replace("<p>", String.Empty).Replace("</p>", string.Empty);

            html = "<html><body>" + html + "</body></html>";
            html = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + Environment.NewLine + html;
            return html;
        }

        static string removeElement(string pattern, string html)
        {
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            return regex.Replace(html, string.Empty);
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

        static string getEnglish(string input)
        {
            string translateText = input;

            if (translateText.Contains("lui/lei"))
            {
                translateText = translateText.Replace("lei", string.Empty);
                translateText = translateText.Replace("/", string.Empty);
            }
            Console.Write($"Translating '{translateText}'...");
            WebRequest request = WebRequest.Create($"{GoogleTranslateUrl}");
            request.Method = "POST";
            request.ContentType = "application/json";

            StreamWriter bodyWriter = new StreamWriter(request.GetRequestStream());
            var requestObject = new
            {
                q = new string[] { translateText },
                source = "it",
                target = "en"
            };
            string requestString = new JavaScriptSerializer().Serialize(requestObject);
            bodyWriter.Write(requestString);
            bodyWriter.Flush();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();


            reader.Close();
            dataStream.Close();
            response.Close();

            var responseObject = new JavaScriptSerializer().Deserialize<TranslationRoot>(responseFromServer);

            string ret = responseObject?.data?.translations?[0].translatedText;

            Console.WriteLine($"'{ret}'.");

            return ret;
        }
    }


    public class TranslationRoot
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Translation[] translations { get; set; }
    }

    public class Translation
    {
        public string translatedText { get; set; }
    }

}
