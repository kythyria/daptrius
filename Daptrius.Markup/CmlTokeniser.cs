using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Daptrius.Markup {

    public enum TokenType {
        Comment,
        Prolog,
        StartElement,
        EndElement,
        AttributeName,
        AttributeValue,
        Text
    }

    public class Token {
        public TokenType TokenType { get; private set; }
        public string Content { get; private set; }

        public string Filename { get; private set; }
        public int LineNumber { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, string content, string file, int line, int column) {
            TokenType = type;
            Content = content;
            Filename = file;
            LineNumber = line;
            Column = column;
        }
    }

    /// <summary>
    /// Breaks a file into tokens. Despite the interfaces, can only be enumerated once.
    /// </summary>
    public class CmlTokeniser {
        private CmlDtdFactory dtdFactory;

        private LineReader reader;
        private List<Token> outbuf = new List<Token>();
        private CmlDtd dtd;
        private UnparsedLine currentLine;
        private UnparsedLine prevLine;
        private int startIndent = 0;
        private int currentIndent = 0;
        private bool inParagraph = false;

        public CmlTokeniser(CmlDtdFactory dtdFactory) {
            this.dtdFactory = dtdFactory;
        }

        public void Tokenise(LineReader reader) {
            this.reader = reader;

            ReadPrologue();

            ReadBody();

            while (currentIndent > startIndent) {
                Out(TokenType.EndElement, "");
                currentIndent--;
            }

            this.reader = null;
        }

        private void ReadPrologue() {
            string comment = null;
            Match m;
            while (currentLine != null) {
                m = CmlPatterns.CommentLine.Match(currentLine.Body);
                if (m.Success) {
                    comment = (comment ?? "") + m.Groups[1].Value + "\n";
                    ReadNext();
                }
                else {
                    if (comment != null) {
                        Out(TokenType.Comment, comment, prevLine);
                        comment = null;
                    }

                    if (currentLine.Body == "") {
                        ReadNext();
                    }
                    else { break; }
                }
            }

            m = CmlPatterns.PrologueLine.Match(currentLine.Body);
            if(!m.Success) {
                throw new ParseException(ParseErrorCode.BadProlog, currentLine.Filename, currentLine.LineNumber, currentLine.Indent.Length);
            }

            // should now be parked at the prologue

            Out(TokenType.Prolog, m.Groups[1].Value);
            startIndent = currentLine.IndentLevel;
            dtd = dtdFactory(m.Groups[1].Value);
            Out(TokenType.StartElement, dtd.DefaultRootElement);
            ReadNext();
        }

        private void ReadBody() {
            while (currentLine != null) {
                TryReadElementLine();
                //TryReadCommentLine();
                //TryReadCdata();
                //TryReadBlankLine();
                //TryReadTextLine();
            }
        }

        private void TryReadElementLine() {
            if(currentLine == null) { return; }

            var m = CmlPatterns.StartElementLine.Match(currentLine.Body);
            if(!m.Success) { return; }

            int pos = m.Length;

            m = CmlPatterns.ElementName.Match(currentLine.Body, pos);
            if (!m.Success) {
                throw new ParseException(ParseErrorCode.BadQName, currentLine.Filename, currentLine.LineNumber, currentLine.Indent.Length + pos);
            }

            Out(TokenType.StartElement, m.Groups[1].Value, pos);
            pos += m.Length;

            // We don't check if any attributes occur twice here, because it needs 
            // awareness of namespaces, which this tokeniser doesn't have. For the
            // same reason we just blindly emit @class and #id each time we see it.

            while (true) {
                m = CmlPatterns.ClsIdShorthand.Match(currentLine.Body, pos);
                if (m.Success) {
                    if(m.Groups["class"].Success) {
                        Out(TokenType.AttributeName, dtd.ClassAttribute, m.Groups["class"].Index);
                        Out(TokenType.AttributeValue, m.Groups["class"].Value, m.Groups["class"].Index);
                    }
                    else if(m.Groups["id"].Success) {
                        Out(TokenType.AttributeName, dtd.IdAttribute, m.Groups["id"].Index);
                        Out(TokenType.AttributeValue, m.Groups["id"].Value, m.Groups["id"].Index);
                    }
                    else {
                        throw new ParseException(ParseErrorCode.Generic, currentLine.Filename, currentLine.LineNumber, currentLine.Indent.Length + pos);
                    }
                    pos += m.Length;
                    continue;
                }

                m = CmlPatterns.Attribute.Match(currentLine.Body, pos);
                if (m.Success) {
                    var name = m.Groups["name"];
                    var value = m.Groups["value"];
                    var unquotedvalue = m.Groups["unquotedvalue"];

                    Out(TokenType.AttributeName, m.Groups["name"].Value, m.Groups["name"].Index);

                    if (value.Success) {
                        var expanded = ExpandAttributes(value.Value, value.Index + pos);
                        Out(TokenType.AttributeValue, expanded, pos + value.Index);
                    }
                    else if (unquotedvalue.Success && unquotedvalue.Length > 0) {
                        Out(TokenType.AttributeValue, unquotedvalue.Value, value.Index + pos);
                    }
                    else if (unquotedvalue.Success && unquotedvalue.Length == 0) {
                        throw new ParseException(ParseErrorCode.NoAttrValue, currentLine.Filename, currentLine.LineNumber, pos + unquotedvalue.Index);
                    }
                    else {
                        Out(TokenType.AttributeValue, dtd.AttributeTruthyValue, pos + m.Length);
                    }
                    pos += m.Length;
                    continue;
                }

                m = CmlPatterns.EndElementLine.Match(currentLine.Body, pos);
                if(m.Success) { break; }
                else {
                    throw new ParseException(ParseErrorCode.MangledTag, currentLine.Filename, currentLine.LineNumber, pos);
                }
            }
            

        }

        private string ExpandAttributes(string text, int startcolumn) {
            return CmlPatterns.EntityRef.Replace(text, m => {
                string result;
                if (dtd.Entities.TryGetValue(m.Groups[1].Value, out result)) {
                    return result;
                }
                else {
                    throw new ParseException(ParseErrorCode.NoEntity, currentLine.Filename, currentLine.LineNumber, startcolumn + m.Index);
                }
            });
        }

        private void Out(TokenType type, string value) {
            Out(type, value, currentLine);
        }

        private void Out(TokenType type, string value, int column) {

            outbuf.Add(new Token(type, value, currentLine.Filename, currentLine.LineNumber, currentLine.Indent.Length + column));
        }

        private void Out(TokenType type, string value, int line, int column) {
            outbuf.Add(new Token(type, value, currentLine.Filename, line, column));
        }

        private void Out(TokenType type, string value, UnparsedLine locationsource) {
            Out(type, value, locationsource, locationsource.Indent.Length);
        }

        private void Out(TokenType type, string value, UnparsedLine locationsource, int column) {
            outbuf.Add(new Token(type, value, locationsource.Filename, locationsource.LineNumber, column));
        }

        private void ReadNext() {
            prevLine = currentLine;
            currentLine = reader.Read();
        }
    }
}