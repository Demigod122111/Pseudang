using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pseudang
{
    internal class Scanner
    {
        private readonly string sourceCode;
        private int currentPosition = 0;
        private int currentLine = 1;

        List<string> ifRelation = new List<string>();
        List<string> whileRelation = new List<string>();
        List<string> forRelation = new List<string>();

        public Scanner(string sourceCode)
        {
            this.sourceCode = sourceCode;
        }


        public List<Token> Scan()
        {
            List<Token> tokens = new List<Token>();

            while (currentPosition < sourceCode.Length)
            {
                char currentChar = sourceCode[currentPosition];

                // Skip whitespace
                if (char.IsWhiteSpace(currentChar))
                {
                    if (currentChar == '\n')
                    {
                        currentLine++;
                    }

                    currentPosition++;
                    continue;
                }


                // Check for keywords, identifiers, literals, etc.
                if (char.IsLetter(currentChar))
                {
                    tokens.Add(ScanIdentifier());
                    continue;
                }
                else if (char.IsDigit(currentChar))
                {
                    tokens.Add(ScanNumber());
                    continue;
                }
                else if((tokens.Last().Type != TokenType.NUMBER && tokens.Last().Type != TokenType.RIGHT_BRACE) && currentChar == '-' && currentPosition < sourceCode.Length - 2 && char.IsDigit(sourceCode[currentPosition + 1]))
                {
                    currentPosition++;
                    var nn = ScanNumber();
                    nn.Lexeme = $"-{nn.Lexeme}";
                    tokens.Add(nn);
                    continue;
                }
                else if (currentChar == '"')
                {
                    tokens.Add(ScanString());
                    continue;
                }
                else if (currentPosition < sourceCode.Length - 1)
                {
                    string dchar = currentChar.ToString() + sourceCode[currentPosition + 1].ToString();
                    if (dchar == "//")
                    {
                        while (sourceCode[currentPosition] != '\n' && currentPosition < sourceCode.Length - 1)
                        {
                            currentPosition++;
                        }
                        continue;
                    }
                    else if (tokensValues.ContainsKey(dchar.ToUpper()))
                    {
                        tokens.Add(new Token(tokensValues[dchar.ToUpper()], dchar, currentLine));
                        currentPosition += 2;
                        continue;
                    }
                    else if (tokensValues.ContainsKey(currentChar.ToString().ToUpper()))
                    {
                        tokens.Add(new Token(tokensValues[currentChar.ToString().ToUpper()], currentChar.ToString(), currentLine));
                        currentPosition++;
                        continue;
                    }
                }
                else if (tokensValues.ContainsKey(currentChar.ToString().ToUpper()))
                {
                    tokens.Add(new Token(tokensValues[currentChar.ToString().ToUpper()], currentChar.ToString(), currentLine));
                    currentPosition++;
                    continue;
                }

                var emsg = $"Syntax Error on Line #{currentLine} at \"{currentChar}\"";
                var col = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(emsg);
                Console.ForegroundColor = col;
                throw new Exception(emsg);
            }

            return tokens;
        }

        private Token ScanIdentifier()
        {
            // Scan and build an identifier token
            int start = currentPosition;
            while (currentPosition < sourceCode.Length && (char.IsLetterOrDigit(sourceCode[currentPosition]) || sourceCode[currentPosition] == '_'))
            {
                currentPosition++;
            }
            string identifier = sourceCode.Substring(start, currentPosition - start);

            if (identifier.ToUpper() == "IF")
            {
                ifRelation.Add(DateTime.Now.Ticks.ToString());
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: ifRelation.Last());
            }
            else if (identifier.ToUpper() == "ELSE")
            {
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: ifRelation.Last());
            }
            else if (identifier.ToUpper() == "ENDIF")
            {
                string id = ifRelation.Last();
                ifRelation.RemoveAt(ifRelation.Count - 1);
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: id);
            }
            else if (identifier.ToUpper() == "WHILE")
            {
                whileRelation.Add(DateTime.Now.Ticks.ToString());
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: whileRelation.Last());
            }
            else if (identifier.ToUpper() == "ENDWHILE")
            {
                string id = whileRelation.Last();
                whileRelation.RemoveAt(whileRelation.Count - 1);
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: id);
            }
            else if (identifier.ToUpper() == "FOR")
            {
                forRelation.Add(DateTime.Now.Ticks.ToString());
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: forRelation.Last());
            }
            else if (identifier.ToUpper() == "ENDFOR")
            {
                string id = forRelation.Last();
                forRelation.RemoveAt(forRelation.Count - 1);
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine, relationship: id);
            }
            else if (tokensValues.ContainsKey(identifier.ToUpper()))
            {
                return new Token(tokensValues[identifier.ToUpper()], identifier, currentLine);
            }
            return new Token(TokenType.IDENTIFIER, identifier, currentLine);
        }

        private Token ScanNumber()
        {
            // Scan and build a number token
            int start = currentPosition;
            bool firstPeriod = true;
            while (currentPosition < sourceCode.Length && (char.IsDigit(sourceCode[currentPosition]) || (sourceCode[currentPosition] == '.' && firstPeriod)))
            {
                if(sourceCode[currentPosition] == '.')
                {
                    firstPeriod = false;
                }

                currentPosition++;
            }
            string number = sourceCode.Substring(start, currentPosition - start);
            return new Token(TokenType.NUMBER, number, currentLine);
        }

        private Token ScanString()
        {
            // Scan and build a string token
            int start = currentPosition;
            currentPosition++; // Skip the opening double quote
            while (currentPosition < sourceCode.Length && sourceCode[currentPosition] != '"')
            {
                if (sourceCode[currentPosition] == '\n')
                {
                    // Handle newline character in the string (optional)
                    currentLine++;
                }
                currentPosition++;
            }

            if (currentPosition == sourceCode.Length)
            {
                throw new ParseException(currentLine, "Unterminated string literal.");
            }

            currentPosition++; // Skip the closing double quote
            string str = sourceCode.Substring(start + 1, currentPosition - start - 2); // Exclude the double quotes
            return new Token(TokenType.STRING, Regex.Unescape(str), currentLine);
        }

        static Dictionary<string, TokenType> tokensValues = new Dictionary<string, TokenType>()
        {
            { "BEGIN", TokenType.BEGIN },
            { "END", TokenType.END },

            { "DECLARE", TokenType.DECLARE },

            { "PRINT", TokenType.PRINT },

            { "READ", TokenType.READ },

            { "IF", TokenType.IF },
            { "THEN", TokenType.THEN },
            { "ENDIF", TokenType.ENDIF },
            { "ELSE", TokenType.ELSE },

            { "WHILE", TokenType.WHILE },
            { "DO", TokenType.DO },
            { "ENDWHILE", TokenType.ENDWHILE },

            { "FOR", TokenType.FOR },
            { "TO", TokenType.TO },
            { "ENDFOR", TokenType.ENDFOR },

            { "TRUE", TokenType.TRUE },
            { "FALSE", TokenType.FALSE },

            { "RAND", TokenType.RAND },
            { "ROUND", TokenType.ROUND },

            { "LABEL", TokenType.LABEL },
            { "GOTO", TokenType.GOTO },

            { "[", TokenType.LEFT_BRACKET },
            { "]", TokenType.RIGHT_BRACKET },

            { "(", TokenType.LEFT_BRACE },
            { ")", TokenType.RIGHT_BRACE },

            { "+", TokenType.OPERATOR },
            { "-", TokenType.OPERATOR },
            { "*", TokenType.OPERATOR },
            { "/", TokenType.OPERATOR },
            { "==", TokenType.OPERATOR },
            { "!=", TokenType.OPERATOR },
            { "<", TokenType.OPERATOR },
            { ">", TokenType.OPERATOR },
            { "<=", TokenType.OPERATOR },
            { ">=", TokenType.OPERATOR },

            { ",", TokenType.COMMA },

            { "=", TokenType.EQUAL },

            { "@", TokenType.AT },

        };

        public class ParseException : Exception
        {
            public int Line { get; }

            public ParseException(int line, string message) : base(message)
            {
                Line = line;
            }
        }
    }
}
