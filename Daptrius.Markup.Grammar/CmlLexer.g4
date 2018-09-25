lexer grammar CmlLexer;

// Basic fragments

//Antlr 4.6 doesn't support non-bmp characters directly.
fragment AstralChar : [\uD800-\uDBBF][\uDC00-\uDCFF];

fragment NameStartChar : 'A'..'Z' | 'a'.. 'z' | '_'
                       | '\u00C0'..'\u00D6' | '\u00D8'..'\u00F6'
                       | '\u00F8'..'\u02FF' | '\u0370'..'\u037D'
                       | '\u037F'..'\u1FFF' | '\u200C'..'\u200D'
                       | '\u2070'..'\u218F' | '\u2C00'..'\u2FEF'
                       | '\u3001'..'\uD7FF' | '\uF900'..'\uFDCF'
                       | '\uFDF0'..'\uFFFD' | AstralChar;

fragment NameChar : '-' | '0'..'9' | '\u00B7' | '\u0300'..'\u036F'
                  |'\u203F'..'\u2040' | NameStartChar;

fragment NcName_ed : NameStartChar (NameChar |'\\.') *;
fragment NcName    : NameStartChar (NameChar |'.') *;

// Control (non)characters. Convert changes in indentation into INDENT and OUTDENT,
// and bracket the nonblank part of the line with LINE_START and LINE_END.
INDENT     : '\uFDD0';
OUTDENT    : '\uFDD1';
LINE_START : '\uFDD2';
LINE_END   : '\n';

// For debugging purposes
//INDENT     : '{';
//OUTDENT    : '}';
//LINE_START : '(';
//LINE_END   : '\n';

// Things that start a line
ELEMENT_START : LINE_START '%'   -> pushMode(tagContent);
COMMENT_START : LINE_START '!#'  -> pushMode(literal);
CDATA_START   : LINE_START '!>'  -> pushMode(literal);
PROLOG        : LINE_START '!!!' -> pushMode(tagContent);

// Things that occur in text
ENTITY_START  : '&' -> pushMode(entref);
TAG_START     : '<' -> pushMode(tagContent);

TEXT : (~[(&<\n])+;

mode literal;
LITERAL_TEXT     : (~'\n')+;
LITERAL_LINE_END : LINE_END -> type(LINE_END), popMode;

mode tagContent;
QNAME         : (NcName_ed ':')? NcName_ed;
EQUALS        : '='      -> pushMode(attrVal);
DOT           : '.';
HASH          : '#';
TAG_LINE_END  : LINE_END -> type(LINE_END), popMode;
TAG_END       : '>'      -> popMode;
SLASH         : '/';
COLON         : ':'      -> popMode;
S             : [ \t]+;

mode attrVal;
ATTRVAL_BARE   : NameChar+;
ATTRVAL_QUOTE : '\''       -> pushMode(attrVal_squote);
ATTRVAL_DQUOTE : '"'       -> type(ATTRVAL_QUOTE), pushMode(attrVal_dquote);

mode attrVal_squote;
ATTRVAL_SQUOTE_TERM : '\''      -> type(ATTRVAL_QUOTE), popMode, popMode;
ATTRVAL_SQUOTE_TEXT : (~[\n'])+ -> type(TEXT);
ATTRVAL_SQUOTE_ENTR : '&'       -> type(ENTITY_START), pushMode(entref);

mode attrVal_dquote;
ATTRVAL_DQUOTE_TERM : '"'       -> type(ATTRVAL_QUOTE), popMode, popMode;
ATTRVAL_DQUOTE_TEXT : (~[\n"])+ -> type(TEXT);
ATTRVAL_DQUOTE_ENTR : '&'       -> type(ENTITY_START), pushMode(entref);

mode entref;
ENTITY_END  : ';' -> popMode;
ENTITY_NAME : NcName;