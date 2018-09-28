using Daptrius.Markup.Grammar;
using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

namespace Daptrius.Markup {
    class TextVisitor : CmlParserBaseVisitor<string> {
        CmlDtd _dtd;

        public TextVisitor(CmlDtd dtd) {
            _dtd = dtd;
        }

        public override string VisitText([NotNull] CmlParser.TextContext context) {
            return context.GetText();
        }

        public override string VisitEntityRef([NotNull] CmlParser.EntityRefContext context) {
            return _dtd.Entities[context.ENTITY_NAME().GetText()];
        }

        public override string VisitNewline([NotNull] CmlParser.NewlineContext context) {
            return "\n";
        }

        protected override string AggregateResult(string aggregate, string nextResult) {
            return aggregate + nextResult;
        }

        protected override string DefaultResult => "";
    }
}
