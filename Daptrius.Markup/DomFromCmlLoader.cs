using System;
using System.Linq;
using System.Xml;
using TextReader = System.IO.TextReader;

using Antlr4.Runtime;
using Daptrius.Markup.Grammar;

namespace Daptrius.Markup {

    class DomFromCmlLoader {
        private Func<string, CmlDtd> _dtdSource;
        private TextReader _cmlSource;
        private XmlDocument _doc;
        private XmlNode _parent;
        private XmlNamespaceManager _nsm;
        private CmlDtd _dtd;

        private readonly LiteralVisitor _lv = new LiteralVisitor();

        public DomFromCmlLoader(Func<string, CmlDtd> dtdSource, TextReader cmlSource, XmlNode parentNode = null) {
            _dtdSource = dtdSource;
            _cmlSource = cmlSource;
            _parent = parentNode ?? new XmlDocument();
            _doc = _parent as XmlDocument ?? _parent.OwnerDocument;
            _nsm = new XmlNamespaceManager(_doc.NameTable);
        }

        public XmlElement Load() {
            using (var pr = new CmlPrereader(_cmlSource)) {
                var istream = new AntlrInputStream(pr);
                var lexer = new CmlLexer(istream);
                var ts = new CommonTokenStream(lexer);
                var parser = new CmlParser(ts);

                var parseroot = parser.cmlDocument();
                var rootelement = MakeRootElement(parseroot);
                _parent.AppendChild(rootelement);
                ReadElementContent(parseroot.elementContent(), rootelement);

                return rootelement;
            }
        }

        private string LookupNamespace(string prefix) {
            var nsuri = _nsm.LookupNamespace(prefix ?? "");
            if (nsuri == null) {
                //TODO: Better error handling
                throw new Exception("Unspecified namespace!");
            }
            return nsuri;
        }

        private XmlElement MakeElement(CmlParser.TagContentsContext ctx, string name = null) {
            name = name ?? ctx.QNAME().GetText();
            var nameparts = name.Split(":", 2);
            var localname = nameparts.Length == 2 ? nameparts[1] : nameparts[0];
            var prefix = nameparts.Length == 2 ? nameparts[0] : string.Empty;

            var av = new AttributeVisitor(_doc, _dtd);
            av.Visit(ctx);

            foreach (var i in av.Attributes.Where(i => (i.Item1 ?? i.Item2) == "xmlns")) {
                var attprefix = i.Item1 == null ? "" : i.Item2;
                _nsm.AddNamespace(attprefix, i.Item3);
            }

            var nsuri = LookupNamespace(prefix);

            var el = _doc.CreateElement(prefix, localname, nsuri);

            foreach (var i in av.Attributes) {
                var attns = LookupNamespace(i.Item1);
                var att = _doc.CreateAttribute(i.Item1, i.Item2, attns);
                att.Value = i.Item3;
                el.Attributes.Append(att);
            }

            return el;
        }

        private XmlElement MakeRootElement(CmlParser.CmlDocumentContext ctx) {
            var prologue = ctx.prologue();
            if (prologue == null) {
                // TODO: Better error handling
                throw new Exception("Missing prologue");
            }

            var doctypename = ctx.prologue().tagContents().QNAME().GetText();
            // TODO: Is this a fatal error?
            _dtd = _dtdSource(doctypename) ?? CmlDtd.Default;
            foreach(var i in _dtd.DefaultPrefixes) {
                _nsm.AddNamespace(i.Key, i.Value);
            }

            var root = MakeElement(ctx.prologue().tagContents(), _dtd.DefaultRootElement);
            foreach (var i in _dtd.DefaultPrefixes) {
                var attns = i.Value;
                XmlAttribute att;
                if (i.Key == "") {
                    att = _doc.CreateAttribute("xmlns");
                }
                else {
                    att = _doc.CreateAttribute("xmlns:" + i.Key);
                }
                att.Value = i.Value;
                root.Attributes.Append(att);
            }
            return root;
        }

        private void ReadElementContent(CmlParser.ElementContentContext ctx, XmlNode parent) {
            for (int i = 0; i < ctx.ChildCount; i++) {
                var c = ctx.GetChild(i);
                if(c is CmlParser.CommentBlockContext cb) {
                    var contents = _lv.Visit(cb);
                    var cn = _doc.CreateComment(contents);
                    parent.AppendChild(cn);
                }
                else if(c is CmlParser.CdataBlockContext cd) {
                    var contents = _lv.Visit(cd);
                    var cn = _doc.CreateCDataSection(contents);

                    var nameparts = _dtd.LiteralBlockElement.Split(':', 2);
                    var prefix = nameparts.Length == 2 ? nameparts[0] : "";
                    var localpart = nameparts.Length == 2 ? nameparts[1] : nameparts[0];
                    // TODO: Better error handling (here or when building/loading a DTD?)
                    // Probably the latter, though it means we need a builder.
                    var nsuri = _dtd.DefaultPrefixes[prefix];
                    var contextualprefix = _nsm.LookupPrefix(nsuri);
                    var el = _doc.CreateElement(prefix, localpart, nsuri);

                    el.AppendChild(cn);
                    parent.AppendChild(el);
                }
                else if (c is CmlParser.ElementBlockContext eb) {

                }
                else if (c is CmlParser.BlankLineContext) {
                    // Don't need to do anything here
                }
                else {
                    throw new NotImplementedException();
                }
            }
        }
    }

}