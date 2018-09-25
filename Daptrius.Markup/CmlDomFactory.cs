using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TextReader = System.IO.TextReader;
using StringReader = System.IO.StringReader;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Daptrius.Markup.Grammar;
using System.Linq;

namespace Daptrius.Markup
{
    /// <summary>
    /// Represents a place that the factory can load documents from.
    /// </summary>
    public interface ICmlLoader {
        /// <summary>
        /// Obtains a DTD via its id in the prologue.
        /// </summary>
        /// <param name="publicID">DTD to retrieve.</param>
        /// <returns>Parsed DTD, or null if the inbuilt default is to be used.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// When the DTD isn't found.
        /// </exception>
        /// <remarks>
        /// This will not be called when trying to read a DTD itself, as the DTD for that is
        /// already hardcoded as <see cref="CmlDtd.MetaDtd"/>.
        /// </remarks>
        CmlDtd LoadDtdByPublicId(string publicID);

        /// <summary>
        /// Open a file by virtual path, however the loader interprets that.
        /// </summary>
        /// <param name="path">Path to read from.</param>
        /// <returns><see cref="TextReader"/> pointing at the start of the document. Caller disposes.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// When no file exists at that path.
        /// </exception>
        /// <remarks>
        /// A caller that, eg, supports XInclude, may call this multiple times before disposing of any of
        /// the return values.
        /// </remarks>
        TextReader OpenFileByPath(string path);
    }

    public class CmlDomFactory {
        private ICmlLoader _loader;

        public CmlDomFactory(ICmlLoader loader) {
            _loader = loader;
        }

        public XmlDocument ParseFragment(string fragment) {
            using (var tr = new StringReader(fragment)) {
                return Parse(tr);
            }
        }

        public XmlDocument Parse(string path) {
            using (var tr = _loader.OpenFileByPath(path)) {
                return Parse(tr);
            }
        }

        public XmlDocument Parse(TextReader reader) {
            var doc = new XmlDocument();
            ParseInto(doc, reader);
            return doc;
        }

        public virtual void ParseInto(XmlNode parent, TextReader reader) {
            using (var pr = new CmlPrereader(reader)) {
                var istream = new AntlrInputStream(pr);
                var lexer = new Grammar.CmlLexer(istream);
                var ts = new CommonTokenStream(lexer);
                var parser = new Grammar.CmlParser(ts);

                var parseroot = parser.cmlDocument();
                var rootres = MakeRootElement(parent, parseroot);
                var rootelement = rootres.Item1;
                var dtd = rootres.Item2;

                parent.AppendChild(rootelement);
            }
        }

        protected virtual Tuple<XmlNode, CmlDtd> MakeRootElement(XmlNode parent, CmlParser.CmlDocumentContext ctx) {
            var doc = parent.OwnerDocument;
            var prologue = ctx.prologue();
            if (prologue == null) {
                // TODO: Better error handling
                throw new Exception("Missing prologue");
            }

            var doctypename = ctx.prologue().tagContents().QNAME().GetText();
            // TODO: Is this a fatal error?
            var dtd = _loader.LoadDtdByPublicId(doctypename) ?? CmlDtd.Default;
            var rootElement = doc.CreateElement(dtd.DefaultRootElement);
            foreach (var i in dtd.DefaultPrefixes) {
                rootElement.SetAttribute($"xmlns:{i.Key}", i.Value);
            }

            XmlDocumentType doctype = null;
            if (doc == parent
                && !(String.IsNullOrEmpty(dtd.PublicIdentifier) && String.IsNullOrEmpty(dtd.SystemIdentifier)))
            {
                doctype = doc.CreateDocumentType(dtd.DefaultRootElement, dtd.PublicIdentifier, dtd.SystemIdentifier, null);

            }

            var av = new AttributeVisitor(doc, dtd);
            av.AddAttributesTo(rootElement, ctx.prologue().tagContents());

            if(doctype != null) {
                var frag = doc.CreateDocumentFragment();
                frag.AppendChild(doctype);
                frag.AppendChild(rootElement);
                return Tuple.Create((XmlNode)frag, dtd);
            }
            else {
                return Tuple.Create((XmlNode)rootElement, dtd);
            }
        }
    }

    class AttributeVisitor : CmlParserBaseVisitor<XmlAttribute> {
        CmlDtd _dtd;
        XmlDocument _doc;
        List<XmlAttribute> _attlist = new List<XmlAttribute>();
        List<string> _classes = new List<string>();
        bool _idSeen = false;

        public AttributeVisitor(XmlDocument doc, CmlDtd dtd) {
            _doc = doc;
            _dtd = dtd;
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

            CreateAttribute(attname, attval);
            return null;
        }

        private void CreateAttribute(string attname, string attval) {
            var att = _doc.CreateAttribute(attname);
            att.Value = attval;
            _attlist.Add(att);
        }

        public override XmlAttribute VisitShortAttribute([NotNull] CmlParser.ShortAttributeContext context) {
            if (context.HASH() != null) {
                if (!_idSeen) {
                    CreateAttribute(_dtd.IdAttribute, context.QNAME().GetText());
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

            if (_classes.Count > 0) {
                CreateAttribute(_dtd.ClassAttribute, string.Join(' ', _classes));
            }

            foreach(var i in _attlist) {
                el.Attributes.Append(i);
            }

            _attlist = new List<XmlAttribute>();
            _classes = new List<string>();
            _idSeen = false;
        }
    }

    class TextVisitor : CmlParserBaseVisitor<string> {
        CmlDtd _dtd;

        public TextVisitor(CmlDtd dtd) {
            _dtd = dtd;
        }

        public override string VisitText([NotNull] CmlParser.TextContext context) {
            return context.TEXT().GetText();
        }

        public override string VisitEntityRef([NotNull] CmlParser.EntityRefContext context) {
            return _dtd.Entities[context.ENTITY_NAME().GetText()];
        }

        public override string VisitNewline([NotNull] CmlParser.NewlineContext context) {
            return "\n";
        }

        protected override string AggregateResult(string aggregate, string nextResult) {
            return aggregate + nextResult;
        }

        protected override string DefaultResult => "";
    }

    #region oldvisitors
    class ElementBlockVisitor : CmlParserBaseVisitor<XmlDocumentFragment> {
        XmlDocument _doc;
        XmlElement _ctx;

        public override XmlDocumentFragment VisitCommentBlock([NotNull] CmlParser.CommentBlockContext context) {
            var frag = _doc.CreateDocumentFragment();
            var commentText = new LiteralBlockVisitor().Visit(context);
            var comment = _doc.CreateComment(commentText);
            frag.AppendChild(comment);
            return frag;
        }

        public override XmlDocumentFragment VisitCdataBlock([NotNull] CmlParser.CdataBlockContext context) {
            var frag = _doc.CreateDocumentFragment();
            var cdataText = new LiteralBlockVisitor().Visit(context);
            var cdata = _doc.CreateCDataSection(cdataText);
            frag.AppendChild(cdata);
            return frag;
        }

        public override XmlDocumentFragment VisitTextBlock([NotNull] CmlParser.TextBlockContext context) {
            var frag = _doc.CreateDocumentFragment();
            throw new NotImplementedException();
        }

        protected override XmlDocumentFragment DefaultResult => _doc.CreateDocumentFragment();

        protected override XmlDocumentFragment AggregateResult(XmlDocumentFragment aggregate, XmlDocumentFragment nextResult) {
            //TODO: Can we avoid the copy?
            var newagg = _doc.CreateDocumentFragment();
            newagg.AppendChild(aggregate);
            newagg.AppendChild(nextResult);
            return newagg;
        }
    }

    class LiteralBlockVisitor : CmlParserBaseVisitor<string> {

        public override string VisitCommentLine([NotNull] CmlParser.CommentLineContext context) {
            return context.LITERAL_TEXT().GetText();
        }

        public override string VisitCdataLine([NotNull] CmlParser.CdataLineContext context) {
            return context.LITERAL_TEXT().GetText();
        }

        protected override string AggregateResult(string aggregate, string nextResult) {
            return aggregate + nextResult;
        }

        protected override string DefaultResult => "";
    }

    class AttributeVisitorOld : CmlParserBaseVisitor<XmlAttribute> {
        CmlDtd _dtd;
        XmlElement _ctxElement;

        public string Id { get; private set; }
        public string Classes { get; private set; }
        public IEnumerable<XmlAttribute> OtherAttributes { get; private set; }

        public override XmlAttribute VisitAttribute([NotNull] CmlParser.AttributeContext context) {
            var attname = context.QNAME().GetText();
            var attval = context.ATTRVAL_BARE()?.GetText();

            if(attval == null) {
                var visitor = new TextVisitor(_dtd);
                attval = visitor.Visit(context.olTextNode());
            }

            return CreateAttribute(attname, attval);
        }

        public override XmlAttribute VisitShortAttribute([NotNull] CmlParser.ShortAttributeContext context) {
            string attname;

            if (context.HASH() != null) {
                attname = _dtd.IdAttribute;
            }
            else if (context.DOT() != null) {
                attname = _dtd.ClassAttribute;
            }
            else {
                // TODO: Better exceptions.
                throw new Exception("Malformed short attribute");
            }

            return CreateAttribute(attname, context.QNAME().GetText());
        }

        private XmlAttribute CreateAttribute(string attname, string attval) {
            var nameparts = attname.Split(':', 2);
            var localpart = nameparts[nameparts.Length - 1];
            string uri;
            if (nameparts.Length == 2) {
                uri = _ctxElement.GetNamespaceOfPrefix(nameparts[0]);
            }
            else {
                uri = _ctxElement.NamespaceURI;
            }
            var attr = _ctxElement.OwnerDocument.CreateAttribute(_ctxElement.GetPrefixOfNamespace(uri), localpart, uri);
            attr.Value = attval;
            return attr;
        }
    }

    #endregion
}
