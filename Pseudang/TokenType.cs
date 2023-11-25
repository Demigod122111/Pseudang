using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudang
{
    public enum TokenType
    {
        BEGIN,
        END,

        DECLARE,

        IDENTIFIER,
        NUMBER,
        STRING,

        TRUE,
        FALSE,

        LEFT_BRACKET, // [
        RIGHT_BRACKET, // ]
        LEFT_BRACE, // (
        RIGHT_BRACE, // )


        OPERATOR, // + - / *

        PRINT,

        READ,

        COMMA, // ,

        EQUAL, // =

        AT, // @


        IF,
        THEN,
        ENDIF,
        ELSE,

        WHILE,
        DO,
        ENDWHILE,

        FOR,
        TO,
        ENDFOR,

        LABEL,
        GOTO,

        RAND,
        ROUND,

        EOF
    }
}
