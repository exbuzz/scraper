using System.Collections.Generic;

namespace MarkupSanitizer
{
    public class TagDefinition
    {
        internal static readonly string[] AllTags = new[] { "*" };
        internal static readonly string TagRoot = string.Empty;

        readonly string tagName;
        readonly bool isShortcut;
        readonly string[] requiredParents;
        readonly string[] requiredDirectParents;
        readonly string[] blockedParents;
        readonly string[] blockedDirectParents;
        readonly string[] allowedAttributes;
        readonly string[] legacyNames;
        readonly bool allowEmpty;
        readonly bool requiresTransitionalDoctype;

        public TagDefinition(string tagName, bool isShortcut)
            : this(tagName, isShortcut, null, null, null, null)
        {
        }

        public TagDefinition(string tagName, bool isShortcut, string[] requiredParents, string[] requiredDirectParents, string[] blockedParents, string[] blockedDirectParents)
            : this(tagName, isShortcut, requiredParents, requiredDirectParents, blockedParents, blockedDirectParents, null)
        {
        }

        public TagDefinition(string tagName, bool isShortcut, string[] requiredParents, string[] requiredDirectParents, string[] blockedParents, string[] blockedDirectParents, string[] allowedAttributes)
            : this(tagName, isShortcut, requiredParents, requiredDirectParents, blockedParents, blockedDirectParents, allowedAttributes, null, false, false)
        {
        }

        public TagDefinition(string tagName, bool isShortcut, string[] requiredParents, string[] requiredDirectParents, string[] blockedParents, string[] blockedDirectParents, string[] allowedAttributes, string[] legacyNames, bool allowEmpty, bool requiresTransitionalDoctype)
        {
            this.tagName = tagName;
            this.isShortcut = isShortcut;
            this.requiredParents = requiredParents ?? new string[] { };
            this.requiredDirectParents = requiredDirectParents ?? new string[] { };
            this.blockedParents = blockedParents ?? new string[] { };
            this.blockedDirectParents = blockedDirectParents ?? new string[] { };
            this.allowedAttributes = allowedAttributes ?? new string[] { };
            this.legacyNames = legacyNames ?? new string[] { };
            this.allowEmpty = allowEmpty;
            this.requiresTransitionalDoctype = requiresTransitionalDoctype;
        }

        public string TagName { get { return tagName; } }
        public bool IsShortcut { get { return isShortcut; } }
        public IEnumerable<string> RequiredParents { get { return requiredParents; } }
        public IEnumerable<string> RequiredDirectParents { get { return requiredDirectParents; } }
        public IEnumerable<string> BlockedParents { get { return blockedParents; } }
        public IEnumerable<string> BlockedDirectParents { get { return blockedDirectParents; } }
        public IEnumerable<string> AllowedAttributes { get { return allowedAttributes; } }
        public IEnumerable<string> LegacyNames { get { return legacyNames; } }
        public bool AllowEmpty { get { return allowEmpty; } }
        public bool RequiresTransitionalDoctype { get { return requiresTransitionalDoctype; } }
    }
}