using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daptrius.Markup.Tests
{
    class DictionaryCmlLoader : ICmlLoader {
        public Dictionary<string, CmlDtd> Dtds;
        public Dictionary<string, string> Documents;

        public CmlDtd LoadDtdByPublicId(string publicID) {
            if (Dtds.TryGetValue(publicID, out CmlDtd ov)) {
                return ov;
            }
            return null;
        }

        public TextReader OpenFileByPath(string path) {
            if (Documents.TryGetValue(path, out string doc)) {
                return new StringReader(doc);
            }
            throw new FileNotFoundException("Loader not initialised with that path", path);
        }
    }

    [TestClass]
    [DeploymentItem("ParserTests.xml")]
    public class CmlParserTests {
        private const string TestDefinitions = "https://ns.berigora.net/2018/daptrius/testDefinitions";
        ICmlLoader _loader;
        Dictionary<string, XmlDocument> _rhses = new Dictionary<string, XmlDocument>();
        Dictionary<string, bool> _succeedOnEqual = new Dictionary<string, bool>();

        XmlDomComparer _comparer = new XmlDomComparer();

        public CmlParserTests() {
            var loader = new DictionaryCmlLoader {
                Dtds = new Dictionary<string, CmlDtd> {
                    { "test", new CmlDtd {
                        DefaultRootElement = "test-document",
                        TextBlockElement = "textblock",
                        LiteralBlockElement = "literalblock",
                        IdAttribute = "xml:id",
                        ClassAttribute = "class",
                        AttributeTruthyValue = "true",
                        DefaultPrefixes = new Dictionary<string, string> {
                            {"", "https://ns.berigora.net/2018/daptrius/testdtd" }
                        },
                        Entities = new Dictionary<string, string> {
                            { "lt", "<" },
                            { "gt", ">" },
                            { "amp", "&" },
                            { "apos", "'" },
                            { "quot", "\"" },
                            { "bird", "🐦" },
                            { "dragon", "🐉" }
                        }
                    }
                } },
                Documents = new Dictionary<string, string>()
            };

            _loader = loader;

            var doc = new XmlDocument();

            var xrs = new XmlReaderSettings {
                IgnoreWhitespace = false,
            };
            using (var tr = new StreamReader("ParserTests.xml"))
            using (var xr = XmlReader.Create(tr)) {
                doc.Load(xr);
            }

            var xnm = new XmlNamespaceManager(doc.NameTable);
            xnm.AddNamespace("t", TestDefinitions);

            foreach (XmlNode i in doc.SelectNodes("/t:tests/t:test", xnm)) {
                var name = i.Attributes["name"].Value;
                var items = i.SelectNodes("t:item", xnm);

                loader.Documents[name] = items[0].InnerText;
                var rhs = new XmlDocument();
                rhs.AppendChild(rhs.ImportNode(items[1].ChildNodes[0], true));
                _rhses[name] = rhs;
                _succeedOnEqual[name] = i.Attributes["result"].Value == "true";
            }
        }

        void AssertDomMatch([CallerMemberName] string caller = null) {
            var factory = new CmlDomFactory(_loader);
            var lhs = factory.Parse(caller);
            var rhs = _rhses[caller];

            var result = _comparer.Equals(lhs, rhs);
            Assert.AreEqual(_succeedOnEqual[caller], result);
        }

        [TestMethod] public void SimplePrologue() => AssertDomMatch();
        [TestMethod] public void PrologueWithAttribute() => AssertDomMatch();
        [TestMethod] public void PrologueWithMultiAttribute() => AssertDomMatch();
        [TestMethod] public void OneLineText() => AssertDomMatch();
        [TestMethod] public void ThreeLinesText() => AssertDomMatch();
        [TestMethod] public void TwoParagraphs() => AssertDomMatch();
        [TestMethod] public void NonDefaultDtd() => AssertDomMatch();
        [TestMethod] public void AstralPlaneEntities() => AssertDomMatch();
        [TestMethod] public void LiteralBlock() => AssertDomMatch();
        [TestMethod] public void LongLiteralBlock() => AssertDomMatch();
        [TestMethod] public void MixedTextAndLiterals() => AssertDomMatch();
        [TestMethod] public void NonRootElement() => AssertDomMatch();
        [TestMethod] public void MixedElementText() => AssertDomMatch();
        [TestMethod] public void NestedElements() => AssertDomMatch();
        [TestMethod] public void ElementWithTextLine() => AssertDomMatch();
        [TestMethod] public void ElementWithAttributes() => AssertDomMatch();
        [TestMethod] public void ElementWithId() => AssertDomMatch();
        [TestMethod] public void ElementWithClass() => AssertDomMatch();
        [TestMethod] public void ElementWithClassAndId() => AssertDomMatch();
        [TestMethod] public void ElementShorthandAndTextLine() => AssertDomMatch();
        [TestMethod] public void AttributeWithPrefix() => AssertDomMatch();
        [TestMethod] public void NoValueAttributeWithText() => AssertDomMatch();
        [TestMethod] public void MixedContentChildren() => AssertDomMatch();
        [TestMethod] public void CommentBlock() => AssertDomMatch();
        [TestMethod] public void LongComment() => AssertDomMatch();
        [TestMethod] public void MixedTextComments() => AssertDomMatch();
        [TestMethod] public void XmlnsCanPostcedeItsUse() => AssertDomMatch();
    }
}
