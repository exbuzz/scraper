namespace MarkupSanitizer
{
    public class SanitizedMarkup
    {
        readonly string markupText;
        readonly bool transitionalDoctypeRequired;

        public SanitizedMarkup(string markupText, bool transitionalDoctypeRequired)
        {
            this.markupText = markupText;
            this.transitionalDoctypeRequired = transitionalDoctypeRequired;
        }

        public string MarkupText { get { return markupText; } }
        public bool TransitionalDoctypeRequired { get { return transitionalDoctypeRequired; } }
    }
}