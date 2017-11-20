namespace MarkupSanitizer
{
    interface IMarkupToken
    {
        void ProcessOutput(MarkupWriter writer);
    }
}