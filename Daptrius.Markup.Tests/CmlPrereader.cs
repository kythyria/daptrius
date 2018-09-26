using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Daptrius.Markup.Tests
{

    /// <remarks>
    /// The tests in this section replace the actual special noncharacters with printing but
    /// otherwise not used characters for easier visualisation. See <see cref="MapControls(string)"/>
    /// for the mapping.
    /// </remarks>
    [TestClass]
    public class CmlPrereaderTests
    {
        public const char Indent = '\uFDD0';
        public const char Outdent = '\uFDD1';
        public const char LineStart = '\uFDD2';
        public const char LineEnd = '\n';
        public const char Replacement = '\uFFFD';

        [TestMethod]
        public void SingleLine() {
            AssertGraphicParse("test", "〖test〗");
        }

        [TestMethod]
        public void AllTheLineEndings() {
            AssertGraphicParse("1\r2\n3\r\n4\u0085g\u2028h\u2029i\r\rj\n\nk",
                  "〖1〗〖2〗〖3〗〖4〗〖g〗〖h〗〖i〗〖〗〖j〗〖〗〖k〗");
        }

        [TestMethod]
        public void BlankLine() {
            AssertGraphicParse("z\n\nz", "〖z〗〖〗〖z〗");
        }

        [TestMethod]
        public void InitialIndent() {
            AssertGraphicParse("  1", "→〖1〗");
        }

        [TestMethod]
        public void IndentedLine() {
            AssertGraphicParse("1\n  2\n1", $"〖1〗→〖2〗←〖1〗");
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