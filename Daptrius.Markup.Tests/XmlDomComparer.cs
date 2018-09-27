using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace Daptrius.Markup.Tests
{
    /// <summary>
    /// Compares XML documents, in a namespace aware manner.
    /// </summary>
    /// <remarks>
    /// Insignificant whitespace counts. Namespace declarations and prefixes don't.
    /// Entity references aren't expanded, and their replacement text isn't counted.
    /// Nothing inside an internal or external subset counts. Unprefixed attributes
    /// are treated as having the same namespace as their element, despite the spec.
    /// </remarks>
    class XmlDomComparer
    {
        public IEnumerable<XmlNode> Children(XmlNode parent) {
            return parent.ChildNodes.Cast<XmlNode>();
        }

        public bool Equals(XmlNode x, XmlNode y) {
            if (x.GetType() != y.GetType()) {
                return false;
            }
            else if (x is XmlCharacterData xcd) {
                return Equals(xcd, y as XmlCharacterData);
            }
            else if (x is XmlDeclaration xd) {
                return Equals(xd, y as XmlDeclaration);
            }
            else if (x is XmlDocumentType xdt) {
                return Equals(xdt, y as XmlDocumentType);
            }
            else if (x is XmlProcessingInstruction xpi) {
                return Equals(xpi, y as XmlProcessingInstruction);
            }
            else if (x is XmlEntityReference xer) {
                return Equals(xer, y as XmlEntityReference);
            }
            else if (x is XmlElement xe) {
                return Equals(xe, y as XmlElement);
            }
            else if (x is XmlDocument xdoc) {
                return Equals(xdoc, y as XmlDocument);
            }
            else if (x is XmlDocumentFragment xdf) {
                return Equals(xdf, y as XmlDocumentFragment);
            }
            else {
                throw new ArgumentException("Unknown XmlNode subclass");
            }
        }

        public bool Equals(XmlCharacterData x, XmlCharacterData y) {
            return x.Data == y.Data;
        }

        public bool Equals(XmlDeclaration x, XmlDeclaration y) {
            return x.Encoding == y.Encoding && x.Version == y.Version && x.Standalone == y.Standalone;
        }

        public bool Equals(XmlDocumentType x, XmlDocumentType y) {
            return x.Name == y.Name && x.PublicId == y.PublicId && x.SystemId == y.SystemId;
        }

        public bool Equals(XmlProcessingInstruction x, XmlProcessingInstruction y) {
            return x.Target == y.Target && x.Data == y.Data;
        }

        public bool Equals(XmlEntityReference x, XmlEntityReference y) {
            return x.Name == y.Name;
        }

        public bool Equals(XmlElement x, XmlElement y) {
            if (x.LocalName != y.LocalName) { return false; }
            if (x.NamespaceURI != y.NamespaceURI) { return false; }

            var xattrs = GetComparableAttributes(x);
            var yattrs = GetComparableAttributes(y);

            if (yattrs.Count() == xattrs.Count()) {
                if (xattrs.Count() > 0) {
                    if (!xattrs.Zip(yattrs, (i, j) => i.CompareTo(j)).All(i => i == 0)) { return false; }
                }
            }
            else { return false; }

            return ChildrenEquals(x, y);
        }

        public bool Equals(XmlDocument x, XmlDocument y) => ChildrenEquals(x, y);
        public bool Equals(XmlDocumentFragment x, XmlDocumentFragment y) => ChildrenEquals(x, y);

        private bool AttrEqual(XmlAttribute x, XmlAttribute y) {
            return x.NamespaceURI == y.NamespaceURI && x.LocalName == y.LocalName && x.Value == y.Value;
        }

        private bool ChildrenEquals(XmlNode x, XmlNode y) {
            if (x.ChildNodes.Count != y.ChildNodes.Count) {
                return false;
            }

            if (x.ChildNodes.Count == 0) { return true; }

            var sames = x.ChildNodes.Cast<XmlNode>().Zip(y.Cast<XmlNode>(), Equals);

            return sames.All(i => i);
        }

        private bool IsNsDecl(XmlAttribute attr) {
            return attr.Prefix == "xmlns" || (attr.Prefix == "" && attr.LocalName == "xmlns");
        }

        private IEnumerable<(string, string, string)> GetComparableAttributes(XmlNode node) {
            var result = new List<(string, string, string)>();
            foreach (XmlAttribute i in node.Attributes) {
                if (i.Prefix == "xmlns") { continue; }
                if (i.Name == "xmlns") { continue; }
                result.Add((i.NamespaceURI, i.LocalName, i.Value));
            }
            result.Sort();
            return result;
        }
    }
}
