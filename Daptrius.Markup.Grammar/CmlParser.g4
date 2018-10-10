parser grammar CmlParser;

options { tokenVocab=CmlLexer; }

cmlDocument : prologue elementContent EOF;
prologue : PROLOG S? tagContents LINE_END;
elementContent : (commentBlock|cdataBlock|elementBlock|textBlock|blankLine)+;

blankLine : (LINE_START LINE_END)+;

commentBlock : commentLine+;
commentLine : COMMENT_START LITERAL_TEXT LINE_END;

cdataBlock : cdataLine+;
cdataLine : CDATA_START LITERAL_TEXT LINE_END;

elementBlock : ELEMENT_START tagContents (COLON olTextLine? | COLON? LINE_END (INDENT elementContent (OUTDENT|EOF)) | LINE_END | EOF);

tagContents : QNAME S? shortAttribute* (S (attribute | shortAttribute* ))*;
shortAttribute : (DOT | HASH) QNAME;
attribute : QNAME (EQUALS (ATTRVAL_QUOTE textNode ATTRVAL_QUOTE|ATTRVAL_BARE))?;

text    : TEXT+;
newline : LINE_END LINE_START;

olTextLine       : (textNode | olInlineElement)* LINE_END;
olInlineElement : startTag (textNode | olInlineElement)+ endTag|selfClosingTag;

textBlock : textLine+;
textLine: LINE_START (textNode|inlineElement)+ LINE_END;
textNode : (text|entityRef)+;
inlineElement : startTag (textNode|inlineElement|newline)+ endTag|selfClosingTag;
startTag : TAG_START tagContents TAG_END;
selfClosingTag : TAG_START tagContents S? SLASH TAG_END;
endTag   : TAG_START SLASH QNAME? TAG_END;

entityRef : ENTITY_START ENTITY_NAME ENTITY_END;