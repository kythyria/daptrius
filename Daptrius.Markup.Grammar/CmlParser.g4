parser grammar CmlParser;

options { tokenVocab=CmlLexer; }

cmlDocument : prologue elementContent EOF;
prologue : PROLOG S? QNAME LINE_END;
elementContent : (commentBlock|cdataBlock|elementBlock|textBlock|blankLine)+;

blankLine : LINE_START LINE_END;

commentBlock : commentLine+;
commentLine : COMMENT_START LITERAL_TEXT LINE_END;

cdataBlock : cdataLine+;
cdataLine : CDATA_START LITERAL_TEXT LINE_END;

elementBlock : ELEMENT_START tagContents (COLON textLine? | LINE_END) (INDENT elementContent (OUTDENT|EOF))?;

tagContents : QNAME S? shortAttribute* (S (attribute | shortAttribute* ))*;
shortAttribute : (DOT | HASH) QNAME;
attribute : QNAME (EQUALS (ATTRVAL_QUOTE attrVal ATTRVAL_QUOTE|ATTRVAL_BARE))?;
attrVal : (TEXT | entityRef)*;

textBlock : textLine+;
textLine : LINE_START (TEXT|entityRef|startTag|endTag)+ LINE_END;
entityRef : ENTITY_START ENTITY_NAME ENTITY_END;
startTag : TAG_START tagContents SLASH? TAG_END;
endTag   : TAG_START SLASH QNAME? TAG_END;