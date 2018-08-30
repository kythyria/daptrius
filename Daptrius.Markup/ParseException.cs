using System;
using System.Collections.Generic;
using System.Text;

namespace Daptrius.Markup
{
    public sealed class ParseErrorCode {
        public string ShortCode { get; private set; }
        public string Text { get; private set; }
        private ParseErrorCode(string shortcode, string text) {
            ShortCode = shortcode;
            Text = text;
        }

        public static ParseErrorCode Generic = new ParseErrorCode("CML0", "Unspecified failure");
        public static ParseErrorCode BadIndent = new ParseErrorCode("CML1", "Mismatched indentation");
    }


    [Serializable]
    public class ParseException : Exception {
        /// <summary>
        /// File in which the error occurred
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Which error it is
        /// </summary>
        public ParseErrorCode Code { get; set; }

        /// <summary>
        /// Line number of the error
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Column of the error, if any.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Error message without the line/column/file
        /// </summary>
        public string ErrorText { get; set; }

        public ParseException() { }
        public ParseException(ParseErrorCode code, string file, int line, int col) :
            base(CreateMessage(code, file, line, col))
        {
            Code = code;
            Filename = file;
            Line = line;
            Column = col;
            ErrorText = code.Text;
        }

        public ParseException(ParseErrorCode code, string file, int line, int col, Exception inner) :
            base(CreateMessage(code, file, line, col), inner)
        {
            Code = code;
            Filename = file;
            Line = line;
            Column = col;
            ErrorText = code.Text;
        }

        protected ParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        // This method only exists because you can't call the base constructor except before the body
        // of the derived constructor.
        private static string CreateMessage(ParseErrorCode code, string file, int line, int col) {
            return string.Format("{0}({1},{2}) error {3}: {4}", file, line, col, code.ShortCode, code.Text);
        }
    }
}
