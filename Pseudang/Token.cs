using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudang
{
    public class Token
    {
        public TokenType Type;
        public string Lexeme;
        public int Line;
        public string Relationship;

        public Token(TokenType type, string lexeme, int line, string relationship = "")
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Relationship = relationship;
        }


        public override string ToString()
        {
            return $"{{Type: {Type}, Lexeme: {Lexeme}, Line #{Line}, Relationship: {Relationship}}}";
        }
    }
}
