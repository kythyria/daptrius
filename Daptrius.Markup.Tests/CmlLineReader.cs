using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Daptrius.Markup;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Daptrius.Markup.Tests
{
    [TestClass]
    public class CmlLineParser
    {
        [TestMethod]
        public void SimpleUnixLines()
        {
            var str = "Foo\nBar\nBaz\nBarrow";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "Foo"),
                new UnparsedLine(nameof(CmlLineParser), 2, 0, "", "Bar"),
                new UnparsedLine(nameof(CmlLineParser), 3, 0, "", "Baz"),
                new UnparsedLine(nameof(CmlLineParser), 4, 0, "", "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void SimpleMixedLines() {
            var str = "Foo\nBar\r\nBaz\rBarrow";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "Foo"),
                new UnparsedLine(nameof(CmlLineParser), 2, 0, "", "Bar"),
                new UnparsedLine(nameof(CmlLineParser), 3, 0, "", "Baz"),
                new UnparsedLine(nameof(CmlLineParser), 4, 0, "", "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void SimpleBlock() {
            var str = "Foo\n  Bar\n  Baz  \nBarrow";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "Foo"),
                new UnparsedLine(nameof(CmlLineParser), 2, 1, "  ", "Bar"),
                new UnparsedLine(nameof(CmlLineParser), 3, 1, "  ", "Baz  "),
                new UnparsedLine(nameof(CmlLineParser), 4, 0, "", "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void MoreNesting() {
            var str = "One\nTwo\n  Three\n  Four\n     Five\n     \tSix\n  Seven";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "One"),
                new UnparsedLine(nameof(CmlLineParser), 2, 0, "", "Two"),
                new UnparsedLine(nameof(CmlLineParser), 3, 1, "  ", "Three"),
                new UnparsedLine(nameof(CmlLineParser), 4, 1, "  ", "Four"),
                new UnparsedLine(nameof(CmlLineParser), 5, 2, "     ", "Five"),
                new UnparsedLine(nameof(CmlLineParser), 6, 3, "     \t", "Six"),
                new UnparsedLine(nameof(CmlLineParser), 7, 1, "  ", "Seven")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void BadNesting() {
            var str = "One\n  Two\n    Three\n  \tFour";
            Assert.ThrowsException<ParseException>(() => {
                var lines = CollectLines(str);
            });
        }

        [TestMethod]
        public void LiteralBlock() {
            var str = "One\n  Two\n  Three\n\n    Four\n   Five\nSix";
            using (var sr = new StringReader(str))
            using (var lr = new LineReader(sr, nameof(SimpleUnixLines))) {
                UnparsedLine ul;
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "One"), ul);
                ul = lr.Read(true); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 2, 1, "Two"), ul);
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 3, 1, "Three"), ul);
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 4, 1, ""), ul);
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 5, 1, "  Four"), ul);
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 6, 1, " Five"), ul);
                ul = lr.Read(); Assert.AreEqual(new UnparsedLine(nameof(CmlLineParser), 7, 0, "", "Six"), ul);
            }
        }

        [TestMethod]
        public void BlankLines() {
            var str = "One\n\nTwo\n  Three\n\n  Four\n\nFive";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(nameof(CmlLineParser), 1, 0, "", "One"),
                new UnparsedLine(nameof(CmlLineParser), 2, 0, "", ""),
                new UnparsedLine(nameof(CmlLineParser), 3, 0, "", "Two"),
                new UnparsedLine(nameof(CmlLineParser), 4, 1, "Three"),
                new UnparsedLine(nameof(CmlLineParser), 5, 1, "", ""),
                new UnparsedLine(nameof(CmlLineParser), 6, 1, "Four"),
                new UnparsedLine(nameof(CmlLineParser), 7, 1, ""),
                new UnparsedLine(nameof(CmlLineParser), 8, 0, "", "Five")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        static List<UnparsedLine> CollectLines(LineReader lr) {
            var lines = new List<UnparsedLine>();
            while (true) {
                var l = lr.Read();
                if (l == null) break;
                lines.Add(l);
            }
            return lines;
        }

        static List<UnparsedLine> CollectLines(string str) {
            using (var sr = new StringReader(str))
            using (var lr = new LineReader(sr, nameof(CmlLineParser))) {
                return CollectLines(lr);
            }
        }

        static void AssertAreEqual<T>(List<T> expected, List<T> actual) {
            var z = expected.Zip(actual, (x, y) => Tuple.Create(x,y));
            
            foreach(var i in z) {
                Assert.AreEqual(i.Item1, i.Item2);
            }
        }
    }
}
