using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Daptrius.Markup
{
    public class UnparsedLine : IEquatable<UnparsedLine> {
        public UnparsedLine(string file, int line, int indentlevel, string indent, string text) {
            Filename = file;
            LineNumber = line;
            IndentLevel = indentlevel;
            Indent = indent;
            Body = text;
        }

        /// <summary>
        /// Part of the line that is indentation
        /// </summary>
        public string Indent { get; private set; }

        /// <summary>
        /// File the line came from
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// 1-indexed number of the line within the file
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// How many indentation levels the current line is indented by
        /// </summary>
        public int IndentLevel { get; private set; }

        /// <summary>
        /// Non-indentation text on the line
        /// </summary>
        public string Body { get; private set; }

        public override bool Equals(object obj) {
            return Equals(obj as UnparsedLine);
        }

        public bool Equals(UnparsedLine other) {
            return other != null &&
                Filename == other.Filename &&
                LineNumber == other.LineNumber &&
                IndentLevel == other.IndentLevel &&
                Body == other.Body;
        }

        public override int GetHashCode() {
            var hashCode = 447885035;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Filename);
            hashCode = hashCode * -1521134295 + LineNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + IndentLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Body);
            return hashCode;
        }

        public override string ToString() {
            return String.Format("{{UnparsedLine:{3}:{0}:{1}:{2}}}", LineNumber, IndentLevel, Body, Filename);
        }

        public static bool operator ==(UnparsedLine line1, UnparsedLine line2) {
            return EqualityComparer<UnparsedLine>.Default.Equals(line1, line2);
        }

        public static bool operator !=(UnparsedLine line1, UnparsedLine line2) {
            return !(line1 == line2);
        }
    }

    /// <summary>
    /// Breaks a <see cref="TextReader"/>'s contents into lines, reporting the amount
    /// each one was indented, and checks the indentation is consistent throughout
    /// the stream.
    /// </summary>
    /// <remarks>
    /// It would be considerably simpler to omit the consistency check and just count
    /// the number of whitespace characters, but the files that permits which this
    /// class rejects are either ambiguous, or later stages of parsing can inhibit
    /// rejection by use of the literal block function.
    /// </remarks>
    public class LineReader : IDisposable {
        /// <summary>
        /// Regex matching a line consisting of whitespace followed by anything.
        /// </summary>
        private static readonly Regex IndentPattern = new Regex(@"^(\W*)(.*)?$");

        private Stack<string> indents;
        private TextReader reader;

        public string Filename { get; private set; }

        /// <summary>
        /// 1-indexed number of the last line read
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// How many indentation levels is the last line read indented by.
        /// </summary>
        public int CurrentIndent { get; private set; }

        /// <summary>
        /// If set true, the current indentation level will not increase.
        /// Additional whitespace is treated literally.
        /// </summary>
        public bool InLiteralBlock { get; private set; }

        public LineReader(TextReader reader, string tuname) {
            this.reader = reader;
            indents = new Stack<string>();
            indents.Push("");
            LineNumber = 0;
            CurrentIndent = 0;
            InLiteralBlock = false;
        }

        /// <summary>
        /// Read a line.
        /// </summary>
        /// <param name="beginLiteralBlock">
        /// If true, the line read might be the first of a literal block.
        /// </param>
        /// <returns>
        /// The line read, or null at EOF.
        /// </returns>
        /// <remarks>
        /// Beginning a literal block involves the line being read having an increase of indent.
        /// Thereafter the indent does not increase until the end of the block, it's treated
        /// literally.
        /// </remarks>
        public UnparsedLine Read(bool beginLiteralBlock = false) {
            var str = reader.ReadLine();
            
            if (str == null) { // EOF
                return null;
            }

            // Identify how much of the string is indent.
            var m = IndentPattern.Match(str);
            var indentPart = m.Groups[1].Value;
            var textPart = m.Groups[2].Value;

            if (textPart.Length == 0) { // Effectively blank line.
                // Blank lines have the same indentation as the previous line.
                return new UnparsedLine(Filename, ++LineNumber, CurrentIndent, indentPart, "");
            }
            else if (indentPart == indents.Peek()) { // Same as last line
                return new UnparsedLine(Filename, ++LineNumber, CurrentIndent, indentPart, textPart);
            }
            else if (indentPart.StartsWith(indents.Peek())) { // Indented more
                if(beginLiteralBlock) {
                    InLiteralBlock = true;
                }
                else if (InLiteralBlock) {
                    var prefixlen = indents.Peek().Length;
                    return new UnparsedLine(Filename, ++LineNumber, CurrentIndent, indentPart, str.Substring(prefixlen));
                }
                indents.Push(indentPart);
                return new UnparsedLine(Filename, ++LineNumber, ++CurrentIndent, indentPart, textPart);
            }
            else { // Must be indented less or messed up indentation
                while(indents.Count > 1) {
                    indents.Pop();
                    CurrentIndent--;
                    if(indents.Peek() == indentPart) {
                        InLiteralBlock = false;
                        return new UnparsedLine(Filename, ++LineNumber, CurrentIndent, indentPart, textPart);
                    }
                }

                throw new ParseException(ParseErrorCode.BadIndent, Filename, LineNumber, 0);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    reader.Dispose();
                }

                indents = null;
                reader = null;
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }
}
