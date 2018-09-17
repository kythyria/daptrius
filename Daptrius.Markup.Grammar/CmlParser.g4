parser grammar CmlParser;

options { tokenVocab=CmlLexer; }

cmlDocument : prologue elementContent EOF;
prologue : PROLOG S? QNAME LINE_END;
elementContent : (commentBlock|cdataBlock|elementBlock|textBlock|blankLine)+;

blankLine : (LINE_START LINE_END)+;

commentBlock : commentLine+;
commentLine : COMMENT_START LITERAL_TEXT LINE_END;

cdataBlock : cdataLine+;
cdataLine : CDATA_START LITERAL_TEXT LINE_END;

elementBlock : ELEMENT_START tagContents (COLON textLine? | COLON? LINE_END (INDENT elementContent (OUTDENT|EOF)))?;

tagContents : QNAME S? shortAttribute* (S (attribute | shortAttribute* ))*;
shortAttribute : (DOT | HASH) QNAME;
attribute : QNAME (EQUALS (ATTRVAL_QUOTE attrVal ATTRVAL_QUOTE|ATTRVAL_BARE))?;
attrVal : (text | entityRef)*;

text    : TEXT;
newline : LINE_END LINE_START;

textLine       : (ol_textNode | ol_inlineElement)* LINE_END;
ol_textNode    : (text | entityRef)+;
ol_inlineElement : startTag (ol_textNode | ol_inlineElement)+ endTag|selfClosingTag;

textBlock: LINE_START (textNode|inlineElement)+ LINE_END;
textNode : (text|entityRef|newline)+;
inlineElement : startTag (textNode|inlineElement)+ endTag|selfClosingTag;
startTag : TAG_START tagContents TAG_END;
selfClosingTag : TAG_START tagContents S? SLASH TAG_END;
endTag   : TAG_START SLASH QNAME? TAG_END;

entityRef : ENTITY_START ENTITY_NAME ENTITY_END;