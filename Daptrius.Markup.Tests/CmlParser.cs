﻿using System;
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
        ICmlLoader _loader;
        Dictionary<string, string> _rhses = new Dictionary<string, string>();
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
            xnm.AddNamespace("t", "https://ns.berigora.net/2018/Daptrius/TestDefinitions");

            foreach (XmlNode i in doc.SelectNodes("/t:tests/t:test", xnm)) {
                var name = i.Attributes["name"].Value;
                var items = i.SelectNodes("t:item", xnm);

                loader.Documents[name] = items[0].InnerText;
                _rhses[name] = items[1]?.InnerText;
                _succeedOnEqual[name] = i.Attributes["result"].Value == "true";
            }
        }

        [TestMethod] public void SimplePrologue() => AssertDomMatch();
        [TestMethod] public void PrologueWithDtd() => AssertDomMatch();
        [TestMethod] public void PrologueWithAttribute() => AssertDomMatch();

        void AssertDomMatch([CallerMemberName] string caller = null) {
            var factory = new CmlDomFactory(_loader);
            var lhs = factory.Parse(caller);
            var rhs = new XmlDocument();
            rhs.LoadXml(_rhses[caller]);

            var result = _comparer.Equals(lhs, rhs);
            Assert.AreEqual(_succeedOnEqual[caller], result);
        }
    }
}