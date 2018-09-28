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
            var loader = new DomFromCmlLoader(_loader.LoadDtdByPublicId, reader, parent);
            loader.Load();
        }
    }
}
