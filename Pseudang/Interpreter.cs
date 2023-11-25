using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pseudang
{
    #pragma warning disable CS8600
    #pragma warning disable CS8601
    #pragma warning disable CS8602
    #pragma warning disable CS8603
    #pragma warning disable CS8604
    internal class Interpreter
    {
        private List<Token> tokens;
        private readonly Dictionary<string, object> variables = new Dictionary<string, object>();

        private readonly Dictionary<string, int> labels = new Dictionary<string, int>();

        public Interpreter(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public void Interpret()
        {
            try
            {
                Execute(tokens);
            }
            catch (Exception e)
            {
                PrintError(e.Message);
            }
        }

        public void Interpret(List<Token> t)
        {
            try
            {
                Execute(t, fullCode: false);
            }
            catch (Exception e)
            {
                PrintError(e.Message);
            }
        }

        void PrintError(string msg)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(msg);
            Console.ForegroundColor = col;
        }
        void Error(string msg, bool throwException = true)
        {
            //PrintError(msg);
            if (throwException) throw new Exception(msg);
        }

        List<int> whileConditioners = new List<int>();
        List<int> forConditioners = new List<int>();
        List<int> forTracker = new List<int>();

        void Execute(List<Token> tokenList, bool fullCode = true)
        {
            #region Begin
                {
                    bool begin = false;
                    try
                    {
                        begin = tokenList[0].Type == TokenType.BEGIN;
                    }
                    catch { }

                    if (!begin)
                    {
                        Error("Program needs to start with 'BEGIN'");
                        return;
                    }
                }
                #endregion

            #region End
                {
                    bool end = false;
                    try
                    {
                        end = tokenList.Last().Type == TokenType.END;
                    }
                    catch { }

                    if (!end)
                    {
                        Error("Program needs to end with 'END'");
                        return;
                    }
                }
                #endregion
            

            for (int i = 0; i < tokenList.Count - 1;)
            {
                //Console.WriteLine(i);
                Token token = tokenList[i];

                void AdvanceToken()
                {
                    token = tokenList[++i];
                    //Console.WriteLine("[Advance Token] " + token.ToString());
                }

                void RegressToken()
                {
                    token = tokenList[--i];
                    //Console.WriteLine("[Regress Token] " + token.ToString());
                }

                string Expected(TokenType type, string additionMsg = "")
                {
                    return $"Error at Token[{token.Type}] on line #{token.Line}, Expected {additionMsg}Token[{type}]";
                }


                int if_depth = 0;

                void ExecuteToken(bool managed = false)
                {
                    switch (token.Type)
                    {
                        case TokenType.DECLARE:
                            #region Declare
                            void declare()
                            {
                                if (token.Type == TokenType.IDENTIFIER)
                                {
                                    if (!variables.ContainsKey(token.Lexeme))
                                    {
                                        var variabeName = token.Lexeme;
                                        AdvanceToken();
                                        if (token.Type == TokenType.AT)
                                        {
                                            variables[variabeName] = new Dictionary<int, object>();
                                        }
                                        else
                                        {
                                            RegressToken();
                                            variables[token.Lexeme] = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Variable '{token.Lexeme}' cannot be declare more than once");
                                    }
                                }
                                else
                                {
                                    Error($"Invalid Token[{token.Type}] '{token.Lexeme}' on line #{token.Line}");
                                }
                            }

                            AdvanceToken();
                            declare();
                            AdvanceToken();
                            while (token.Type == TokenType.COMMA && i < tokenList.Count - 1)
                            {
                                AdvanceToken();
                                declare();
                                AdvanceToken();
                            }
                            #endregion
                            break;

                        case TokenType.PRINT:
                            #region Print
                            void print(bool first = false)
                            {
                                if (token.Type == TokenType.IDENTIFIER)
                                {
                                    if (variables.ContainsKey(token.Lexeme.Trim()))
                                    {
                                        var val = variables[token.Lexeme.Trim()];

                                        if (val is Dictionary<int, object>)
                                        {
                                            AdvanceToken();
                                            if (token.Type == TokenType.LEFT_BRACKET)
                                            {
                                                AdvanceToken();
                                                if (token.Type == TokenType.NUMBER)
                                                {
                                                    int index = 0;
                                                    if(int.TryParse(token.Lexeme, out index))
                                                    {
                                                        AdvanceToken();
                                                        if(token.Type == TokenType.RIGHT_BRACKET)
                                                        {
                                                            if (first)
                                                            {
                                                                Console.WriteLine($"{(val as Dictionary<int, object>)[index]}");
                                                            }
                                                            else
                                                            {
                                                                Console.Write($"{(val as Dictionary<int, object>)[index]}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                                        }
                                                    }
                                                    else{
                                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                                    }
                                                }
                                                else if (token.Type == TokenType.IDENTIFIER)
                                                {
                                                    int index = 0;
                                                    if (int.TryParse(variables[token.Lexeme].ToString(), out index))
                                                    {
                                                        AdvanceToken();
                                                        if (token.Type == TokenType.RIGHT_BRACKET)
                                                        {
                                                            if (first)
                                                            {
                                                                Console.WriteLine($"{(val as Dictionary<int, object>)[index]}");
                                                            }
                                                            else
                                                            {
                                                                Console.Write($"{(val as Dictionary<int, object>)[index]}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                                    }
                                                }
                                                else
                                                {
                                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Array Indexer Token[{TokenType.NUMBER}]");
                                                }
                                            }
                                            else {
                                                RegressToken();
                                                if (first)
                                                {
                                                    Console.WriteLine($"{token.Lexeme.Trim()} <Array>");
                                                }
                                                else
                                                {
                                                    Console.Write($"{token.Lexeme.Trim()} <Array>");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (first)
                                            {
                                                Console.WriteLine(val);
                                            }
                                            else
                                            {
                                                Console.Write(val);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{token.Lexeme}'");
                                    }
                                }
                                else if (token.Type == TokenType.STRING || token.Type == TokenType.NUMBER)
                                {
                                    Console.Write(token.Lexeme);
                                }
                                else if (token.Type == TokenType.TRUE)
                                {
                                    Console.Write(1);
                                }
                                else if (token.Type == TokenType.FALSE)
                                {
                                    Console.Write(0);
                                }
                                else
                                {
                                    Error($"Invalid Token[{token.Type}] '{token.Lexeme}' on line #{token.Line}");
                                }
                            }

                            AdvanceToken();
                            print(true);
                            AdvanceToken();
                            while (token.Type == TokenType.COMMA && i < tokenList.Count - 1)
                            {
                                AdvanceToken();
                                print();
                                AdvanceToken();

                            }
                            #endregion
                            break;

                        case TokenType.READ:
                            #region Read
                            void read()
                            {
                                if (token.Type == TokenType.IDENTIFIER)
                                {
                                    if (variables.ContainsKey(token.Lexeme.Trim()))
                                    {
                                        string? v = Console.ReadLine();
                                        variables[token.Lexeme.Trim()] = $"{v}";
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{token.Lexeme}'");
                                    }
                                }
                                else
                                {
                                    Error($"Invalid Token[{token.Type}] '{token.Lexeme}' on line #{token.Line}, can only read variables");
                                }
                            }

                            AdvanceToken();
                            read();
                            AdvanceToken();
                            while (token.Type == TokenType.COMMA && i < tokenList.Count - 1)
                            {
                                AdvanceToken();
                                read();
                                AdvanceToken();
                            }
                            #endregion
                            break;

                        case TokenType.IDENTIFIER:
                            #region Identifier
                            if (!variables.ContainsKey(token.Lexeme.Trim())) Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{token.Lexeme}'");
                            var vname = token.Lexeme.Trim();
                            AdvanceToken();

                            int index = 0;
                            bool indexed = false;

                            if (token.Type == TokenType.LEFT_BRACKET)
                            {
                                if (!(variables[vname] is Dictionary<int, object>)) Error($"Error at Token[{token.Type}] on line #{token.Line}, Cannot index a non-array variable");
                                
                                AdvanceToken();

                                if (token.Type == TokenType.NUMBER)
                                {
                                    if (int.TryParse(token.Lexeme, out index))
                                    {
                                        AdvanceToken();
                                        if (token.Type == TokenType.RIGHT_BRACKET)
                                        {
                                            indexed = true;
                                            AdvanceToken();
                                        }
                                        else
                                        {
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                    }
                                }
                                else if(token.Type == TokenType.IDENTIFIER)
                                {
                                    if (int.TryParse(variables[token.Lexeme].ToString(), out index))
                                    {
                                        AdvanceToken();
                                        if (token.Type == TokenType.RIGHT_BRACKET)
                                        {
                                            indexed = true;
                                            AdvanceToken();
                                        }
                                        else
                                        {
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                    }
                                }
                                else
                                {
                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Array Indexer Token[{TokenType.NUMBER}] or Token[{TokenType.IDENTIFIER}]");
                                }
                            }
                            

                            
                            if (token.Type == TokenType.EQUAL)
                            {
                                AdvanceToken();
                                object result = EvaluateExpression(); // Evaluate the right side expression

                                if (indexed)
                                {
                                    (variables[vname] as Dictionary<int, object>)[index] = result.ToString(); // Store the result in the variable
                                }
                                else
                                {
                                    variables[vname] = result.ToString(); // Store the result in the variable
                                }

                            }
                            else
                            {
                                Error($"Invalid Token[{token.Type}] '{token.Lexeme.Trim()}' on line #{token.Line} expected Token[{TokenType.EQUAL}]");
                            }
                            #endregion
                            break;

                        case TokenType.IF:
                            #region If
                            ExecuteIfBlock();
                            #endregion
                            break;

                        case TokenType.THEN:
                            #region Then
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'if' block.");
                            #endregion
                            break;

                        case TokenType.ELSE:
                            #region Else
                            if (!string.IsNullOrEmpty(token.Relationship))
                            {
                                AdvanceToken();
                                return;
                            }
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'if' block.");
                            #endregion
                            break;

                        case TokenType.ENDIF:
                            #region Endif
                            if (!string.IsNullOrEmpty(token.Relationship))
                            {
                                AdvanceToken();
                                return;
                            }
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'if' block.");
                            #endregion
                            break;

                        case TokenType.WHILE:
                            #region While
                            ExecuteWhileBlock();
                            #endregion
                            break;

                        case TokenType.DO:
                            #region Do
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'while' or 'for' block.");
                            #endregion
                            break;
                        case TokenType.ENDWHILE:
                            #region Endwhile
                            if (!string.IsNullOrEmpty(token.Relationship))
                            {
                                AdvanceToken();
                                return;
                            }
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'while' block.");
                            #endregion
                            break;

                        case TokenType.FOR:
                            #region For
                            ExecuteForBlock();
                            #endregion
                            break;
                        case TokenType.TO:
                            #region To
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'for' block.");
                            #endregion
                            break;
                        case TokenType.ENDFOR:
                            #region Endfor
                            if (!string.IsNullOrEmpty(token.Relationship))
                            {
                                AdvanceToken();
                                return;
                            }
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Unexpected '{token.Lexeme}' outside of 'for' block.");
                            #endregion
                            break;

                        case TokenType.ROUND:
                            #region Round
                            AdvanceToken();
                            if (token.Type == TokenType.IDENTIFIER)
                            {
                                if (variables.ContainsKey(token.Lexeme.Trim()))
                                {
                                    var val = variables[token.Lexeme.Trim()];

                                    if (val is Dictionary<int, object>)
                                    {
                                        AdvanceToken();
                                        if (token.Type == TokenType.LEFT_BRACKET)
                                        {
                                            AdvanceToken();
                                            if (token.Type == TokenType.NUMBER)
                                            {
                                                int round_index = 0;
                                                if (int.TryParse(token.Lexeme, out round_index))
                                                {
                                                    AdvanceToken();
                                                    if (token.Type == TokenType.RIGHT_BRACKET)
                                                    {
                                                        var res = (val as Dictionary<int, object>)[round_index].ToString();
                                                        double tres = 0;
                                                        if (double.TryParse(res, out tres))
                                                        {
                                                            (val as Dictionary<int, object>)[round_index] = Math.Round(tres);
                                                            AdvanceToken();
                                                        }
                                                        else
                                                        {
                                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Variable must contain an integer value");
                                                        }

                                                    }
                                                    else
                                                    {
                                                        Error(Expected(TokenType.RIGHT_BRACKET));
                                                    }
                                                }
                                                else
                                                {
                                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                                }
                                            }
                                            else if (token.Type == TokenType.IDENTIFIER)
                                            {
                                                int round_index = 0;
                                                if (int.TryParse(variables[token.Lexeme].ToString(), out index))
                                                {
                                                    AdvanceToken();
                                                    if (token.Type == TokenType.RIGHT_BRACKET)
                                                    {
                                                        var res = (val as Dictionary<int, object>)[round_index].ToString();
                                                        double tres = 0;
                                                        if (double.TryParse(res, out tres))
                                                        {
                                                            (val as Dictionary<int, object>)[round_index] = Math.Round(tres);
                                                            AdvanceToken();
                                                        }
                                                        else
                                                        {
                                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Variable must contain an integer value");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                                    }
                                                }
                                                else
                                                {
                                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                                }
                                            }
                                            else
                                            {
                                                Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Array Indexer Token[{TokenType.NUMBER}]");
                                            }
                                        }
                                        else
                                        {
                                            RegressToken();
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Variable must contain an integer value");
                                        }
                                    }
                                    else
                                    {
                                        var res = val.ToString();
                                        double tres = 0;
                                        if (double.TryParse(res, out tres))
                                        {
                                            variables[token.Lexeme] = Math.Round(tres);
                                            AdvanceToken();
                                        }
                                        else
                                        {
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Variable must contain an integer value");
                                        }
                                    }
                                }
                                else
                                {
                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{token.Lexeme}'");
                                }
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected variable Token[{TokenType.IDENTIFIER}]");
                            }
                            #endregion
                            break;

                        case TokenType.LABEL:
                            #region Label
                            AdvanceToken();
                            if(token.Type == TokenType.STRING)
                            {
                                labels[token.Lexeme] = i;
                                AdvanceToken();
                            }
                            else
                            {
                                Error(Expected(TokenType.STRING));
                            }
                            #endregion
                            break;
                        case TokenType.GOTO:
                            #region Goto
                            AdvanceToken();
                            if (token.Type == TokenType.STRING)
                            {
                                i = labels[token.Lexeme];
                                AdvanceToken();
                            }
                            else
                            {
                                Error(Expected(TokenType.STRING));
                            }
                            #endregion
                            break;

                        case TokenType.BEGIN:
                        case TokenType.END:
                        case TokenType.NUMBER:
                        case TokenType.STRING:
                        case TokenType.TRUE:
                        case TokenType.FALSE:
                        case TokenType.LEFT_BRACKET:
                        case TokenType.RIGHT_BRACKET:
                        case TokenType.LEFT_BRACE:
                        case TokenType.RIGHT_BRACE:
                        case TokenType.OPERATOR:
                        case TokenType.COMMA:
                        case TokenType.EQUAL:
                        case TokenType.AT:
                        case TokenType.EOF:
                            i++;
                            break;
                        default:
                            Error($"Invalid Token[{token.Type}] on Line #{token.Line}");
                            break;
                    }
                }

                ExecuteToken();

                #region FOR Statement
                void ExecuteForBlock()
                {
                    string forID = token.Relationship;
                    int forI = i;
                    bool first = true;

                    string variable = "";
                    int destination = 0;
                    int direction = 1;
                    int currentValue = 0;

                    if (forConditioners.Contains(forI))
                    {
                        first = false;
                        currentValue = forTracker.Last();
                        forTracker.RemoveAt(forTracker.Count - 1);
                    }
                    else
                    {
                        forConditioners.Add(forI);
                    }

                    AdvanceToken();

                    if(token.Type == TokenType.IDENTIFIER)
                    {
                        variable = token.Lexeme;
                        if (variables.ContainsKey(variable))
                        {
                            AdvanceToken();

                            if (token.Type == TokenType.EQUAL)
                            {
                                AdvanceToken();

                                if (token.Type == TokenType.NUMBER)
                                {
                                    var val = double.Parse(token.Lexeme);
                                    if((int) val == (double)val)
                                    {
                                        if (first)
                                        {
                                            variables[variable] = (int)val;
                                            currentValue = (int)val;
                                        }
                                        else
                                        {
                                            variables[variable] = currentValue;
                                        }

                                        AdvanceToken();

                                        if (token.Type == TokenType.TO)
                                        {
                                            AdvanceToken();

                                            if (token.Type == TokenType.NUMBER)
                                            {
                                                var eval = double.Parse(token.Lexeme);
                                                if ((int)eval == (double)eval)
                                                {
                                                    destination = (int) eval;
                                                    direction = eval > val ? 1 : (eval < val ? -1 : 0);

                                                    AdvanceToken();
                                                    if(token.Type == TokenType.DO)
                                                    {
                                                        AdvanceToken();
                                                    }
                                                    else
                                                    {
                                                        Error(Expected(TokenType.DO));
                                                    }
                                                }
                                                else
                                                {
                                                    Error($"Error at Token[{token.Type}] on Line #{token.Line}, Expected Integer value");
                                                }
                                            }
                                            else
                                            {
                                                Error(Expected(TokenType.NUMBER));
                                            }
                                        }
                                        else
                                        {
                                            Error(Expected(TokenType.TO));
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on Line #{token.Line}, Expected Integer value");
                                    }
                                }
                                else
                                {
                                    Error(Expected(TokenType.NUMBER));
                                }
                            }
                            else
                            {
                                Error(Expected(TokenType.EQUAL));
                            }
                        }
                        else
                        {
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{variable}'");
                        }
                    }
                    else
                    {
                        Error(Expected(TokenType.IDENTIFIER, "variable from "));
                    }

                    List<Token> forTokens = new List<Token>();
                    forTokens.Add(new Token(TokenType.BEGIN, "BEGIN", token.Line));

                    while ((token.Type != TokenType.ENDFOR || (token.Type == TokenType.ENDFOR && token.Relationship != forID)) && i < tokenList.Count - 1)
                    {
                        forTokens.Add(token);
                        AdvanceToken();
                    }

                    forTokens.Add(new Token(TokenType.END, "END", token.Line, forID));

                    if (direction != 0)
                    {
                        Interpret(forTokens);

                        int pcv = 0;
                        if (int.TryParse(variables[variable].ToString(), out pcv))
                        {
                            currentValue = pcv;
                        }

                        if (currentValue != destination)
                        {


                            if ((direction > 0 && currentValue < destination) || (direction < 0 && currentValue > destination))
                            {
                                i = forConditioners.Last() - 1;
                                currentValue += direction;
                                forTracker.Add(currentValue);
                            }
                            else
                            {
                                forConditioners.RemoveAt(forConditioners.Count - 1);
                            }
                        }
                        else
                        {
                            forConditioners.RemoveAt(forConditioners.Count - 1);
                        }
                    }
                    else
                    {
                        Interpret(forTokens);
                        forConditioners.RemoveAt(forConditioners.Count - 1);
                    }
                    
                    AdvanceToken();
                }
                #endregion

                #region WHILE Statement
                void ExecuteWhileBlock()
                {
                    int whileI = i;
                    string whileID = token.Relationship;

                    // Check if the condition is true
                    bool condition = EvaluateCondition();

                    // Execute the while block while the condition is true
                    if (condition)
                    {
                        whileConditioners.Add(whileI);
                        // Check to the next token after the condition
                        if (token.Type == TokenType.DO)
                        {
                            AdvanceToken();
                        }
                        else
                        {
                            Error(Expected(TokenType.DO));
                        }


                        List<Token> whileTokens = new List<Token>();
                        whileTokens.Add(new Token(TokenType.BEGIN, "BEGIN", token.Line));

                        while ((token.Type != TokenType.ENDWHILE || (token.Type == TokenType.ENDWHILE && token.Relationship != whileID)) && i < tokenList.Count - 1)
                        {
                            whileTokens.Add(token);
                            AdvanceToken();
                        }

                        whileTokens.Add(new Token(TokenType.END, "END", token.Line, whileID));

                        Interpret(whileTokens);

                        i = whileConditioners.Last() - 1;
                        whileConditioners.RemoveAt(whileConditioners.Count - 1);
                        AdvanceToken();
                    }
                    else
                    {
                        while ((token.Type != TokenType.ENDWHILE || (token.Type == TokenType.ENDWHILE && token.Relationship != whileID)) && i < tokenList.Count - 1)
                        {
                            AdvanceToken();
                        }
                        
                        if (token.Type == TokenType.ENDWHILE)
                        {
                            AdvanceToken();
                        }
                    }
                }
                #endregion

                #region IF Statement
                void ExecuteIfBlock()
                {
                    var ifID = token.Relationship;

                    // Check if the condition is true
                    bool condition = EvaluateCondition();

                    // Execute the if block if the condition is true
                    if (condition)
                    {
                        // Check to the next token after the condition
                        if (token.Type == TokenType.THEN)
                        {
                            if_depth++;
                            AdvanceToken();
                        }
                        else
                        {
                            Error($"Invalid Token[{token.Type}] '{token.Lexeme.Trim()}' on line #{token.Line} expected Token[{TokenType.THEN}]");
                        }

                        List<Token> ifTokens = new List<Token>();
                        ifTokens.Add(new Token(TokenType.BEGIN, "BEGIN", token.Line));

                        while ((token.Type != TokenType.ENDIF || (token.Type == TokenType.ENDIF && token.Relationship != ifID)) && i < tokenList.Count - 1)
                        {
                            if (token.Type == TokenType.ELSE && token.Relationship == ifID) break;
                            ifTokens.Add(token);
                            AdvanceToken();
                        }

                        ifTokens.Add(new Token(TokenType.END, "END", token.Line));

                        Interpret(ifTokens);

                        while ((token.Type != TokenType.ENDIF || (token.Type == TokenType.ENDIF && token.Relationship != ifID)) && i < tokenList.Count - 1)
                        {
                            AdvanceToken();
                        }

                        AdvanceToken();
                    }
                    else
                    {
                        while (token.Type != TokenType.ENDIF && token.Relationship != ifID && i < tokenList.Count - 1)
                        {
                            if (token.Type == TokenType.ELSE && token.Relationship == ifID) break;
                            AdvanceToken();
                        }

                        if (token.Type == TokenType.ELSE)
                        {
                            AdvanceToken();

                            List<Token> ifTokens = new List<Token>();
                            ifTokens.Add(new Token(TokenType.BEGIN, "BEGIN", token.Line));

                            while (token.Type != TokenType.ENDIF && token.Relationship != ifID && i < tokenList.Count - 1)
                            {
                                ifTokens.Add(token);
                                AdvanceToken();
                            }

                            ifTokens.Add(new Token(TokenType.END, "END", token.Line));

                            Interpret(ifTokens);
                        }

                        if (token.Type == TokenType.ENDIF)
                        {
                            AdvanceToken();
                        }
                        else
                        {
                            Error(Expected(TokenType.ENDIF));
                        }

                    }
                }
                #endregion

                #region Condition
                bool EvaluateCondition()
                {
                    AdvanceToken(); // Move to the next token after 'if'

                    // Implement your logic to evaluate the condition
                    object left = EvaluateExpression();

                    // Assuming that the condition is a comparison with a relational operator
                    if (token.Type == TokenType.OPERATOR)
                    {
                        string op = token.Lexeme;

                        if (op == "==")
                        {
                            AdvanceToken();
                            return left.Equals(EvaluateExpression());
                        }
                        else if (op == "!=")
                        {
                            AdvanceToken();
                            return !left.Equals(EvaluateExpression());
                        }
                        else if (op == "<")
                        {
                            AdvanceToken();
                            var res = EvaluateExpression();
                            if (double.TryParse(left.ToString(), out double bin) && double.TryParse(res.ToString(), out double bin2))
                            {
                                return double.Parse(left.ToString()) < double.Parse(res.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                            
                        }
                        else if (op == ">")
                        {
                            AdvanceToken();
                            var res = EvaluateExpression();
                            if (double.TryParse(left.ToString(), out double bin) && double.TryParse(res.ToString(), out double bin2))
                            {
                                return double.Parse(left.ToString()) > double.Parse(res.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                        else if (op == "<=")
                        {
                            AdvanceToken();
                            var res = EvaluateExpression();
                            if (double.TryParse(left.ToString(), out double bin) && double.TryParse(res.ToString(), out double bin2))
                            {
                                return double.Parse(left.ToString()) <= double.Parse(res.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                        else if (op == ">=")
                        {
                            AdvanceToken();
                            var res = EvaluateExpression();
                            if (double.TryParse(left.ToString(), out double bin) && double.TryParse(res.ToString(), out double bin2))
                            {
                                return double.Parse(left.ToString()) >= double.Parse(res.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                        else
                        {
                            Error($"Invalid operator '{op}' in condition on line #{token.Line}");
                        }
                    }
                    else
                    {
                        var res = EvaluateExpression();
                        if (double.TryParse(left.ToString(), out double bin) && double.TryParse(res.ToString(), out double bin2))
                        {
                            if (double.Parse(left.ToString()) != 0 && double.Parse(left.ToString()) != 1)
                            {
                                Error($"Invalid token type '{token.Type}' in condition on line #{token.Line}");
                            }
                            else
                            {
                                return double.Parse(left.ToString()) == 0 ? false : true;
                            }
                        }
                    }

                    return false;
                }
                #endregion

                #region Arithmetic
                object EvaluateExpression()
                {
                    object result = EvaluateTerm(); // Start with the first term

                    // Continue evaluating expressions while there are additive or subtractive operators
                    while (token.Type == TokenType.OPERATOR && (token.Lexeme == "+" || token.Lexeme == "-"))
                    {
                        var op = token.Lexeme;
                        AdvanceToken(); // Move to the next token

                        object right = EvaluateTerm(); // Evaluate the next term

                        // Apply the operation
                        if (op == "+")
                        {
                            if (double.TryParse(result.ToString(), out double bin) && double.TryParse(right.ToString(), out double bin2))
                            {
                                result = double.Parse(result.ToString()) + double.Parse(right.ToString());
                            }
                            else
                            {
                                result = result.ToString() + right;
                            }
                            
                        }
                        else if (op == "-")
                        {
                            if (double.TryParse(result.ToString(), out double bin) && double.TryParse(right.ToString(), out double bin2))
                            {
                                result = double.Parse(result.ToString()) - double.Parse(right.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                    }

                    return result;
                }

                object EvaluateTerm()
                {
                    object result = EvaluateFactor(); // Start with the first factor

                    // Continue evaluating terms while there are multiplicative or divisive operators
                    while (token.Type == TokenType.OPERATOR && (token.Lexeme == "*" || token.Lexeme == "/"))
                    {
                        var op = token.Lexeme;
                        AdvanceToken(); // Move to the next token

                        object right = EvaluateFactor(); // Evaluate the next factor

                        // Apply the operation
                        if (op == "*")
                        {
                            if (double.TryParse(result.ToString(), out double bin) && double.TryParse(right.ToString(), out double bin2))
                            {
                                result = double.Parse(result.ToString()) * double.Parse(right.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                        else if (op == "/")
                        {
                            if (double.TryParse(result.ToString(), out double bin) && double.TryParse(right.ToString(), out double bin2))
                            {
                                if (double.Parse(right.ToString()) == 0)
                                {
                                    Error($"Error at Token[{token.Type}] on line #{token.Line}, Division by zero.");
                                }

                                result = double.Parse(result.ToString()) / double.Parse(right.ToString());
                            }
                            else
                            {
                                Error($"Error at Token[{token.Type}] on line #{token.Line}, NaN");
                            }
                        }
                    }

                    return result;
                }

                object EvaluateFactor()
                {
                    object value = null;

                    if (token.Type == TokenType.NUMBER)
                    {
                        value = double.Parse(token.Lexeme);
                        AdvanceToken(); // Consume the number
                        return value;
                    }
                    else if (token.Type == TokenType.STRING)
                    {
                        value = token.Lexeme;
                        AdvanceToken(); // Consume the number
                        return value;
                    }
                    else if (token.Type == TokenType.IDENTIFIER)
                    {
                        string variableName = token.Lexeme.Trim();
                        if (variables.ContainsKey(variableName))
                        {
                            try
                            {
                                AdvanceToken();

                                int index = 0;
                                bool indexed = false;

                                if (token.Type == TokenType.LEFT_BRACKET)
                                {
                                    if (!(variables[variableName] is Dictionary<int, object>)) Error($"Error at Token[{token.Type}] on line #{token.Line}, Cannot index a non-array variable");

                                    AdvanceToken();

                                    if (token.Type == TokenType.NUMBER)
                                    {
                                        if (int.TryParse(token.Lexeme, out index))
                                        {
                                            AdvanceToken();
                                            if (token.Type == TokenType.RIGHT_BRACKET)
                                            {
                                                indexed = true;
                                                AdvanceToken();
                                            }
                                            else
                                            {
                                                Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                            }
                                        }
                                        else
                                        {
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                        }
                                    }
                                    else if (token.Type == TokenType.IDENTIFIER)
                                    {
                                        if (int.TryParse(variables[token.Lexeme].ToString(), out index))
                                        {
                                            AdvanceToken();
                                            if (token.Type == TokenType.RIGHT_BRACKET)
                                            {
                                                indexed = true;
                                                AdvanceToken();
                                            }
                                            else
                                            {
                                                Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Token[{TokenType.RIGHT_BRACKET}]");
                                            }
                                        }
                                        else
                                        {
                                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Array Indexer must be an integer value");
                                        }
                                    }
                                    else
                                    {
                                        Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected Array Indexer Token[{TokenType.NUMBER}] or Token[{TokenType.IDENTIFIER}]");
                                    }
                                }

                                if (indexed)
                                {
                                    value = (variables[variableName] as Dictionary<int, object>)[index];
                                }
                                else
                                {
                                    value = variables[variableName];
                                }
                                return value;
                            }
                            catch
                            {
                                return "";
                            }
                        }
                        else
                        {
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Undefined Variable '{variableName}'");
                        }
                    }
                    else if (token.Type == TokenType.TRUE)
                    {
                        AdvanceToken();
                        return 1;
                    }
                    else if (token.Type == TokenType.FALSE)
                    {
                        AdvanceToken();
                        return 0;
                    }
                    else if (token.Type == TokenType.RAND)
                    {
                        AdvanceToken();
                        return new Random().NextDouble();
                    }
                    else if (token.Type == TokenType.LEFT_BRACE)
                    {
                        AdvanceToken(); // Consume the opening parenthesis
                        object result = EvaluateExpression(); // Evaluate the expression within parentheses
                        if (token.Type == TokenType.RIGHT_BRACE)
                        {
                            AdvanceToken(); // Consume the closing parenthesis
                            return result;
                        }
                        else
                        {
                            Error($"Error at Token[{token.Type}] on line #{token.Line}, Expected ')' after expression within parentheses.");
                        }
                    }
                    else
                    {
                        Error($"Invalid Token[{token.Type}] '{token.Lexeme.Trim()}' on line #{token.Line}, Expected literal, identifier, or '('");
                    }

                    return 0;
                }
                #endregion
            }
        }
    }
}
