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
                new UnparsedLine(1, 0, "Foo"),
                new UnparsedLine(2, 0, "Bar"),
                new UnparsedLine(3, 0, "Baz"),
                new UnparsedLine(4, 0, "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void SimpleMixedLines() {
            var str = "Foo\nBar\r\nBaz\rBarrow";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(1, 0, "Foo"),
                new UnparsedLine(2, 0, "Bar"),
                new UnparsedLine(3, 0, "Baz"),
                new UnparsedLine(4, 0, "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void SimpleBlock() {
            var str = "Foo\n  Bar\n  Baz  \nBarrow";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(1, 0, "Foo"),
                new UnparsedLine(2, 1, "Bar"),
                new UnparsedLine(3, 1, "Baz  "),
                new UnparsedLine(4, 0, "Barrow")
            };
            var lines = CollectLines(str);
            AssertAreEqual(expected, lines);
        }

        [TestMethod]
        public void MoreNesting() {
            var str = "One\nTwo\n  Three\n  Four\n     Five\n     \tSix\n  Seven";
            var expected = new List<UnparsedLine> {
                new UnparsedLine(1, 0, "One"),
                new UnparsedLine(2, 0, "Two"),
                new UnparsedLine(3, 1, "Three"),
                new UnparsedLine(4, 1, "Four"),
                new UnparsedLine(5, 2, "Five"),
                new UnparsedLine(6, 3, "Six"),
                new UnparsedLine(7, 1, "Seven")
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
            using (var lr = new LineReader(sr, nameof(SimpleUnixLines))) {
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
