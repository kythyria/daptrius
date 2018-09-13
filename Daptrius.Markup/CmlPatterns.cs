using System.Text.RegularExpressions;

namespace Daptrius.Markup { 
    static class CmlPatterns {
        public static readonly Regex CommentLine;
        public static readonly Regex PrologueLine;
        public static readonly Regex StartElementLine;
        public static readonly Regex ElementName;
        public static readonly Regex ClsIdShorthand;
        public static readonly Regex Attribute;
        public static readonly Regex EntityRef;
        public static readonly Regex EndElementLine;

        static CmlPatterns() {
            // Per the XML spec there should be a colon in here, but we heed the namespace spec
            // so we leave it out since that makes colons special and it's easier to pull apart
            // a qname this way. We leave out . as it will be subject to escaping, and the non-
            // BMP chars because of UTF-16.
            var ncc = "A-Z_a-z\u00C0-\u00D6\u00D8-\u00F6\u00F8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD";
            // snippet to match U+10000..U+EFFFF as surrogate pairs
            var astralchar = "[\uD800-\uDBBF][\uDC00-\uDCFF]";
            var nameStartCharClass = $"(?:[{ncc}]|{astralchar})";
            var nameCharClass = $"[-0-9\u00B7\u0300-\u036F\u203F-\u2040{ncc}]|{astralchar}";
            var escapeddot = @"\\\.";
            var ncName = $"{nameStartCharClass}(?:{escapeddot}|{nameCharClass})*";
            var shId = $"{nameStartCharClass}(?:{escapeddot}|{nameCharClass})*";
            var shClass = $"(?:{escapeddot}|{nameCharClass})*";

            CommentLine = new Regex(@"^-#(.*)$");
            PrologueLine = new Regex(@"^!!!\s*(\S+)");
            StartElementLine = new Regex(@"^%\s*");
            ElementName = new Regex($"\\G((?<prefix>{ncName}):)?(?<localpart>{ncName})");
            ClsIdShorthand = new Regex($"\\G\\s*(?:\\.(?<class>{shClass})|#(?<id>{shId}))");
            Attribute = new Regex($"\\G\\s+(?<name>(?:{ncName}:)?{ncName})(?:=(?:\"(?<value>[^\"]*)\"|\'(?<value>[^\']*)\'|(?<unquotedvalue>\\w*)))?");
            EntityRef = new Regex($"&({nameStartCharClass}(?:[\\.:]|{nameCharClass})*);");
            EndElementLine = new Regex(@"\s+(<|>|<>|><|!CDATA|:|$)");
        }

        public static readonly string TrimInside = "<";
        public static readonly string TrimOutside = ">";
        public static readonly string TrimBoth = "<>";
        public static readonly string TrimBothAlt = "><";
        public static readonly string TrimNone = ":";
        public static readonly string CDataBeginToken = "!CDATA";
    }
}
