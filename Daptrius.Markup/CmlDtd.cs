using System.Collections.Generic;
using System.Xml;

namespace Daptrius.Markup {
    public delegate CmlDtd CmlDtdFactory(string doctype);

    /// <summary>
    /// The settings required to expand a CML document into a DOM.
    /// </summary>
    public class CmlDtd {
        /// <summary>
        /// This namespace is never used for any real element. It serves to denote that an attribute should
        /// have the namespace of its containing element when used in <see cref="IdAttribute"/> or
        /// <see cref="ClassAttribute"/>.
        /// </summary>
        public const string UseElementNamespace = "https://ns.berigora.net/2018/daptrius/useElementNamespace";

        /// <summary>
        /// Name of implied root elements.
        /// </summary>
        public string DefaultRootElement { get; set; }

        /// <summary>
        /// Name of the element that text blocks are wrapped in.
        /// </summary>
        public string TextBlockElement { get; set; }

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
        public string PublicIdentifier { get; set; }

        /// <summary>
        /// System identifier on the DOCTYPE declaration.
        /// </summary>
        public string SystemIdentifier { get; set; }
    }
}