using System.Collections.Generic;
using System.Xml;

namespace Daptrius.Markup {
    public delegate CmlDtd CmlDtdFactory(string doctype);

    /// <summary>
    /// The settings required to expand a CML document into a DOM.
    /// </summary>
    public class CmlDtd {

        /// <summary>
        /// Name of implied root elements.
        /// </summary>
        public string DefaultRootElement { get; set; }

        /// <summary>
        /// Name of the element that text blocks are wrapped in.
        /// </summary>
        public string TextBlockElement { get; set; }

        ///<summary>
        ///Name of the element that literal blocks are wrapped in.
        ///</summary>
        public string LiteralBlockElement { get; set; }

        /// <summary>
        /// Attribute to use for the Id shorthand.
        /// </summary>
        public string IdAttribute { get; set; }

        /// <summary>
        /// Attribute to use for the Class shorthand.
        /// </summary>
        public string ClassAttribute { get; set; }

        /// <summary>
        /// Value to be inserted when attributes have no value specified
        /// </summary>
        public string AttributeTruthyValue { get; set; }

        /// <summary>
        /// Mapping of namespace prefixes to namespaces that is placed on the
        /// root element.
        /// </summary>
        public Dictionary<string, string> DefaultPrefixes { get; set; }

        /// <summary>
        /// Names to replacement text of all valid entities. All characters in the
        /// replacement text are literal.
        /// </summary>
        public Dictionary<string, string> Entities { get; set; }

        /// <summary>
        /// Public identifier on the DOCTYPE declaration.
        /// </summary>
        public string PublicIdentifier { get; set; } = "";

        /// <summary>
        /// System identifier on the DOCTYPE declaration.
        /// </summary>
        public string SystemIdentifier { get; set; } = "";


        /// <summary>
        /// DTD used when parsing DTD files.
        /// </summary>
        public static CmlDtd MetaDtd { get => metaDtd; }

        /// <summary>
        /// DTD used when no DTD is found.
        /// </summary>
        public static CmlDtd Default { get => defaultDtd; }
        public static Dictionary<string, string> DefaultEntities { get => defaultEntities; }

        private static CmlDtd metaDtd = new CmlDtd {
            DefaultRootElement = "cml-dtd",
            TextBlockElement = "block",
            LiteralBlockElement = "block",
            IdAttribute = "xml:id",
            ClassAttribute = "class",
            AttributeTruthyValue = "1",
            DefaultPrefixes = new Dictionary<string, string> {
                { "", "https://ns.berigora.net/2018/daptrius/dtd" }
            },
            Entities = DefaultEntities
        };

        private static CmlDtd defaultDtd = new CmlDtd {
            DefaultRootElement = "daptrius:root",
            TextBlockElement = "daptrius:text-block",
            LiteralBlockElement = "daptrius:literal-block",
            IdAttribute = "xml:id",
            ClassAttribute = "class",
            AttributeTruthyValue = "1",
            DefaultPrefixes = new Dictionary<string, string> {
                { "daptrius", "https://ns.berigora.net/2018/daptrius" }
            },
            Entities = DefaultEntities
        };

        private static Dictionary<string, string> defaultEntities = new Dictionary<string, string> {
            { "lt", "<" },
            { "gt", ">" },
            { "amp", "&" },
            { "apos", "'" },
            { "quot", "\"" }
        };
    }
}