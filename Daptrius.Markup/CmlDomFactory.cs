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

        private XmlDocument _doc;
        private XmlNamespaceManager _nsm;
        private CmlDtd _dtd;

        public virtual void ParseInto(XmlNode parent, TextReader reader) {
            // This way around so we still get a NPE if there's something else where OwnerDocument is null.
            _doc = parent as XmlDocument ?? parent.OwnerDocument;
            _nsm = new XmlNamespaceManager(_doc.NameTable);

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
            // This way around so we still get a NPE if there's something else where OwnerDocument is null.
            var doc = parent as XmlDocument ?? parent.OwnerDocument;
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
            if (doc == parent)
                //&& !(String.IsNullOrEmpty(dtd.PublicIdentifier) && String.IsNullOrEmpty(dtd.SystemIdentifier)))
            {
                doctype = doc.CreateDocumentType(dtd.DefaultRootElement, dtd.PublicIdentifier, dtd.SystemIdentifier, null);

            }

            var av = new AttributeVisitor(doc, dtd);
            av.AddAttributesTo(rootElement, ctx.prologue().tagContents());
            
            var frag = doc.CreateDocumentFragment();
            if(doctype != null) {
                frag.AppendChild(doctype);
            }
            frag.AppendChild(rootElement);
            return Tuple.Create((XmlNode)frag, dtd);
        }
    }
}
