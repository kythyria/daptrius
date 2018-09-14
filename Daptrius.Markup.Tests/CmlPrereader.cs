using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Daptrius.Markup.Tests
{
    [TestClass]
    public class CmlPrereaderTests
    {
        public const char Indent = '\uFDD0';
        public const char Outdent = '\uFDD1';
        public const char LineStart = '\uFDD2';
        public const char LineEnd = '\uFDD3';
        public const char Replacement = '\uFFFD';

        [TestMethod]
        public void SingleLine() {
            AssertSimpleParse("test", "\uFDD2test\uFDD3");
        }

        [TestMethod]
        public void AllTheLineEndings() {
            AssertSimpleParse("1\r2\n3\r\n4\u0085g\u2028h\u2029i\r\rj\n\nk",
                LineStart + "1" + LineEnd
                + LineStart + "2" + LineEnd
                + LineStart + "3" + LineEnd
                + LineStart + "4" + LineEnd
                + LineStart + "g" + LineEnd
                + LineStart + "h" + LineEnd
                + LineStart + "i" + LineEnd
                + LineStart + LineEnd
                + LineStart + "j" + LineEnd
                + LineStart + LineEnd
                + LineStart + "k" + LineEnd);
        }

        [TestMethod]
        public void BlankLine() {
            AssertSimpleParse("z\n\nz", "\uFDD2z\uFDD3\uFDD2\uFDD3\uFDD2z\uFDD3");
        }

        [TestMethod]
        public void InitialIndent() {
            AssertSimpleParse("  1", $"{Indent}{LineStart}1{LineEnd}");
        }

        [TestMethod]
        public void IndentedLine() {
            AssertSimpleParse("1\n  2\n1", $"{LineStart}1{LineEnd}{Indent}{LineStart}2{LineEnd}{Outdent}{LineStart}1{LineEnd}");
        }

        [TestMethod]
        public void ComplexIndents() {
            AssertGraphicParse("1\n  2\n    3\n1", "〖1〗→〖2〗→〖3〗←←〖1〗");
            AssertGraphicParse("1\n  2\n\n  2", "〖1〗→〖2〗〖〗〖2〗");
            AssertGraphicParse("1\n  2\n\n    3\n1 \n    2", "〖1〗→〖2〗〖〗→〖3〗←←〖1 〗→〖2〗");
        }

        [TestMethod]
        public void BadIndent() {
            Assert.ThrowsException<Exception>(() => {
                var f = MapControls(Parse("1\n    2\n  1.5"));
                System.Diagnostics.Debug.Write(f);
            });
        }

        private string Parse(string test) {
            using (var sr = new StringReader(test))
            using (var pr = new CmlPrereader(sr)) {
                return pr.ReadToEnd();
            }
        }

        private void AssertSimpleParse(string test, string expected) {
            Assert.AreEqual(MapControls(expected), MapControls(Parse(test)));
        }

        private void AssertGraphicParse(string test, string expected) {
            Assert.AreEqual(expected, MapControls(Parse(test)));
        }

        private string MapControls(string src) {
            return src.Replace(LineStart, '〖')
                .Replace(LineEnd, '〗')
                .Replace(Indent, '→')
                .Replace(Outdent, '←');
        }
    }
}