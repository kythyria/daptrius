using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Daptrius.Markup
{
    public class UnparsedLine : IEquatable<UnparsedLine> {
        public UnparsedLine(int line, int indent, string text) {
            LineNumber = line;
            CurrentIndent = indent;
            Body = text;
        }

        public int LineNumber { get; private set; }
        public int CurrentIndent { get; private set; }
        public string Body { get; private set; }

        public override bool Equals(object obj) {
            return Equals(obj as UnparsedLine);
        }

        public bool Equals(UnparsedLine other) {
            return other != null &&
                   LineNumber == other.LineNumber &&
                   CurrentIndent == other.CurrentIndent &&
                   Body == other.Body;
        }

        public override int GetHashCode() {
            var hashCode = 447885035;
            hashCode = hashCode * -1521134295 + LineNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + CurrentIndent.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Body);
            return hashCode;
        }

        public override string ToString() {
            return String.Format("{{UnparsedLine:{0}:{1}:{2}}}", LineNumber, CurrentIndent, Body);
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
        /// <returns>
        /// The line read, or null at EOF.
        /// </returns>
        public UnparsedLine Read() {
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
                return new UnparsedLine(++LineNumber, CurrentIndent, "");
            }
            else if (indentPart == indents.Peek()) { // Same as last line
                return new UnparsedLine(++LineNumber, CurrentIndent, textPart);
            }
            else if (indentPart.StartsWith(indents.Peek())) { // Indented more
                indents.Push(indentPart);
                return new UnparsedLine(++LineNumber, ++CurrentIndent, textPart);
            }
            else { // Must be indented less or messed up indentation
                while(indents.Count > 1) {
                    indents.Pop();
                    CurrentIndent--;
                    if(indents.Peek() == indentPart) {
                        return new UnparsedLine(++LineNumber, CurrentIndent, textPart);
                    }
                }

                throw new ParseException(ParseErrorCode.BadIndent, Filename, LineNumber, 0);
            }
        }

        /// <summary>
        /// Try to read a line that's part of a literal block. 
        /// </summary>
        /// <remarks>
        /// If not in a literal block, expect the line read
        /// to be indented an additional level. If not, return null, if yes,
        /// set the new indentation level, set <see cref="InLiteralBlock"/>,
        /// and return the line. At the end of the block, return null.
        /// </remarks>
        /// <returns>The line, or null if the end of the block or a block
        /// could not be started.</returns>
        public UnparsedLine ReadLiteral() {
            throw new NotImplementedException();
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
