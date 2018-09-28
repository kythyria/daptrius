using System;
using System.Collections.Generic;
using System.Xml;

using Daptrius.Markup.Grammar;
using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

namespace Daptrius.Markup
{
    class AttributeVisitor : CmlParserBaseVisitor<XmlAttribute> {
        CmlDtd _dtd;
        XmlDocument _doc;
        // (prefix, localname, value)
        List<(string, string, string)> _attlist = new List<(string, string, string)>();
        List<string> _classes = new List<string>();
        bool _idSeen = false;

        public List<(string, string, string)> Attributes { get => _attlist; set => _attlist = value; }

        public AttributeVisitor(XmlDocument doc, CmlDtd dtd) {
            _doc = doc;
            _dtd = dtd;
        }

        private void AddAttribute(string name, string value) {
            var parts = name.Split(":", 2);
            if (parts.Length == 1) {
                _attlist.Add((null, parts[0], value));
            }
            else {
                _attlist.Add((parts[0], parts[1], value));
            }
        }

        public override XmlAttribute VisitTagContents([NotNull] CmlParser.TagContentsContext context) {
            _attlist = new List<(string, string, string)>();
            _classes = new List<string>();
            _idSeen = false;

            var retval = base.VisitTagContents(context);

            if (_classes.Count > 0) {
                AddAttribute(_dtd.ClassAttribute, string.Join(' ', _classes));
            }

            return retval;
        }

        public override XmlAttribute VisitAttribute([NotNull] CmlParser.AttributeContext context) {
            var attname = context.QNAME().GetText();
            var attval = context.ATTRVAL_BARE()?.GetText();

            if (attval == null) {
                var visitor = new TextVisitor(_dtd);
                attval = visitor.Visit(context.olTextNode());
            }

            if (attval == null) {
                attval = _dtd.AttributeTruthyValue;
            }

            AddAttribute(attname, attval);
            return null;
        }

        public override XmlAttribute VisitShortAttribute([NotNull] CmlParser.ShortAttributeContext context) {
            if (context.HASH() != null) {
                if (!_idSeen) {
                    AddAttribute(_dtd.IdAttribute, context.QNAME().GetText());
                    _idSeen = true;
                }
                else {
                    // TODO: Better exceptions;
                    throw new Exception("Duplicate ID attribute");
                }
            }
            else if (context.DOT() != null) {
                _classes.Add(context.QNAME().GetText());
            }
            else {
                // TODO: Better exceptions.
                throw new Exception("Malformed short attribute");
            }
            return null;
        }

        public void AddAttributesTo(XmlElement el, CmlParser.TagContentsContext ctx) {
            Visit(ctx);
            foreach (var i in _attlist) {
                var att = _doc.CreateAttribute(i.Item1 + ":" + i.Item2);
                att.Value = i.Item3;
                el.SetAttributeNode(att);
            }
        }
    }
}
