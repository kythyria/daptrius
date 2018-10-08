using System;
using System.Collections.Generic;
using System.Xml;
using Antlr4.Runtime.Misc;
using Daptrius.Markup.Grammar;
using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

namespace Daptrius.Markup {
    class LiteralVisitor : CmlParserBaseVisitor<string> {
        public override string VisitCommentLine([NotNull] CmlParser.CommentLineContext context) {
            return context.LITERAL_TEXT().GetText();
        }

        public override string VisitCdataLine([NotNull] CmlParser.CdataLineContext context) {
            return context.LITERAL_TEXT().GetText();
        }

        protected override string AggregateResult(string aggregate, string nextResult) {
            if (aggregate == null) { return nextResult; }
            return aggregate + "\n" + nextResult;
        }
    }
}
