using System.Text.RegularExpressions;

using Microsoft.Security.Application;
using System.Diagnostics;

namespace MarkupSanitizer
{
    [DebuggerDisplay("Blob: {UnescapedText}")]
    class MarkupBlob : IMarkupToken
    {
        static readonly Regex NoiseRegex = new Regex(@"(&#13;|&#10;)");

        public string UnescapedText { get; private set; }

        public MarkupBlob(string unescapedText)
        {
            UnescapedText = unescapedText;
        }

        public void ProcessOutput(MarkupWriter writer)
        {
            var encodedText = AntiXss.HtmlEncode(UnescapedText);
            var cleanedText = NoiseRegex.Replace(encodedText, string.Empty);
            writer.Append(cleanedText);
        }
    }
}