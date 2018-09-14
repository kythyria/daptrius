using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Daptrius.Markup
{
    /// <summary>
    /// Wraps a peekable TextReader to turn indentation-based blocks into delimited ones.
    /// </summary>
    /// <remarks>
    /// CR, LF, CRLF, LS, and PS are all supported as line breaks. Each line becomes
    /// wrapped in <see cref="LineStart"/> and <see cref="LineEnd"/>, and each
    /// valid change in indentation is represented as <see cref="Indent"/> or
    /// <see cref="Outdent"/>. Mismatched indentation throws. Don't use on texts
    /// you need to preserve noncharacters for, they'll become U+FFFD.
    /// </remarks>
    public class CmlPrereader : TextReader, IDisposable {
        // Special characters used in output
        public const char Indent = '\uFDD0';
        public const char Outdent = '\uFDD1';
        public const char LineStart = '\uFDD2';
        public const char LineEnd = '\uFDD3';
        public const char Replacement = '\uFFFD';

        // Line break characters
        public const char CR = '\r';
        public const char LF = '\n';
        public const char NEL = '\u0085';
        public const char LS = '\u2028';
        public const char PS = '\u2029';

        enum PeekState {
            MustRead
        }

        private TextReader _source;
        private IEnumerator<char> _translator;
        private bool _mustAdvance = true;

        public CmlPrereader(TextReader source) {
            _source = source;
            _translator = MakeTranslator(_source).GetEnumerator();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if(disposing) {
                _source.Dispose();
            }
        }

        public override void Close() {
            base.Close();
            _source.Close();
        }

        private bool IsNewlineChar(int curr) {
            return (curr == PS) || (curr == LS) || (curr == NEL) || (curr == LF) || (curr == CR);
        }

        private bool IsHorizontalWhitespace(int curr) {
            return char.IsWhiteSpace((char)curr) && !IsNewlineChar(curr);
        }

        private IEnumerable<char> MakeTranslator(TextReader rd) {
            var beginLine = true;
            var indents = new Stack<string>();
            var currentIndent = "";

            indents.Push("");

            int curr;
            while((curr = rd.Read()) != -1) {
                // Noncharacters
                if(curr >= 0xFDD0 && curr <= 0xFDEF
                    || (curr & 0xFFFF) == 0xFFFF
                    || (curr & 0xFFFF) == 0xFFFE) {
                    beginLine = false;
                    yield return Replacement;
                }

                // Absorb the CR of CRLF
                else if (curr == CR && rd.Peek() == LF) { continue; }

                // Line breaks
                else if ( IsNewlineChar(curr) ) {
                    if(beginLine) {
                        yield return LineStart;
                    }
                    yield return LineEnd;
                    beginLine = true;
                    currentIndent = "";
                }

                else if (beginLine && IsHorizontalWhitespace(curr)) {
                    currentIndent += (char)curr;
                    while(IsHorizontalWhitespace(rd.Peek())) {
                        curr = rd.Read();
                        currentIndent += (char)curr;
                    }

                    // The cursor is now on the *last* whitespace character.

                    // It's a blank line.
                    if (IsNewlineChar(rd.Peek())) {
                        yield return LineStart;
                        beginLine = false;
                    }
                    // It's not blank, so we have to check indents
                    else if(currentIndent == indents.Peek()) { // Same
                        yield return LineStart;
                        beginLine = false;
                    }
                    else if(currentIndent.StartsWith(indents.Peek())) { // Indented
                        indents.Push(currentIndent);
                        yield return Indent;
                        yield return LineStart;
                        beginLine = false;
                    }
                    else  { // Outdented or error
                        while (indents.Count > 0) {
                            if(indents.Peek() == currentIndent) {
                                yield return Outdent;
                                yield return LineStart;
                                beginLine = false;
                                break;
                            }
                            else {
                                indents.Pop();
                            }
                        }
                        if(indents.Count == 0) { // bad indents
                            //TODO: Nicer exception
                            throw new Exception("Bad indent!");
                        }
                    }
                }

                else {
                    if(beginLine) {
                        if (currentIndent == "") {
                            while (indents.Count > 1) {
                                indents.Pop();
                                yield return Outdent;
                            }
                        }
                        beginLine = false;
                        yield return LineStart;
                    }
                    yield return (char)curr;
                }
            }
            if(!beginLine) {
                yield return LineEnd;
            }
        }

        public override int Peek() {
            if (_mustAdvance) {
                if (!_translator.MoveNext()) {
                    return -1;
                }
                _mustAdvance = false;
            }
            return _translator.Current;
        }

        public override int Read() {
            if (_mustAdvance) {
                if (!_translator.MoveNext()) {
                    return -1;
                }
            }
            _mustAdvance = true;
            return _translator.Current;
        }
    }
}
