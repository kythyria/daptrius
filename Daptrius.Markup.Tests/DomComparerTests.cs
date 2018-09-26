using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Daptrius.Markup.Tests
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class FileDataAttribute : Attribute, ITestDataSource {

        // This is a positional argument
        public FileDataAttribute(string filename) {
            Filename = filename;
        }

        public string Filename { get; }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo) {
            var doc = new XmlDocument();

            var xrs = new XmlReaderSettings {
                IgnoreWhitespace = false,
            };
            using (var tr = new StreamReader(Filename))
            using (var xr = XmlReader.Create(tr)) {
                doc.Load(xr);
            }

            var xnm = new XmlNamespaceManager(doc.NameTable);
            xnm.AddNamespace("t", "https://ns.berigora.net/2018/Daptrius/TestDefinitions");

            Func<XmlNode, bool> SignificantNode = k => {
                if (k is XmlCharacterData kc) {
                    return string.IsNullOrWhiteSpace(kc.Data);
                }
                else { return true; }
            };

            foreach (XmlNode i in doc.SelectNodes("/t:test", xnm)) {
                var nr = new object[] { i.Attributes["name"].Value, i.Attributes["result"].Value };
                var items = i.SelectNodes("t:item", xnm).Cast<XmlElement>()
                    .Select(j => (object)j.ChildNodes
                        .Cast<XmlNode>()
                        .First(SignificantNode));
                yield return nr.Concat(items).ToArray();
            }
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data) {
            return data[0] as string;
        }
    }

    [TestClass]
    public class DomComparerTests
    {
        [TestMethod]
        [FileData("DomComparerTests.xml")]
        public void TestDom(string name, string result, XmlNode left, XmlNode right) {
            var ld = new XmlDocument();
            var rd = new XmlDocument();

            ld.LoadXml((left as XmlCharacterData).Data);
            rd.LoadXml((right as XmlCharacterData).Data);

            var comp = new XmlDomComparer();

            Assert.IsTrue(comp.Equals(ld, rd));
        }
    }
}
