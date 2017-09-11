using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbcLib.DBC.Lex
{
    public class Lexer
    {
        private StreamReader reader;
        private List<Token> list = new List<Token>();

        public Lexer(StreamReader reader)
        {
            this.reader = reader;
        }

        public List<Token> Lex()
        {
            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Peek();

                if (char.IsWhiteSpace(ch))
                {
                    reader.Read();
                    continue;
                }


                if (IsDigit(ch))
                {
                    list.Add(LexNumber(new StringBuilder()));
                }
                else if (ch == '-')
                {
                    list.Add(LexMinusSign());
                }
                else if (ch == '"')
                {
                    list.Add(LexCharString());
                }
                else if (IsLetter(ch) || ch == '_')
                {
                    list.Add(LexIdentifier());
                }
                else
                {
                    list.Add(new Token(ch.ToString()));
                    reader.Read();
                }
            }

            return list;
        }

        private static bool IsLetter(char ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }

        private static bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        private Token LexNumber(StringBuilder builder)
        {
            TokenType type =
                builder.Length > 0 ? TokenType.SIGNED : TokenType.UNSIGNED;

            builder.Append((char)reader.Read());

            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Peek();

                if (!IsDigit(ch))
                {
                    if (ch == '.' && type != TokenType.DOUBLE)
                        type = TokenType.DOUBLE;
                    else
                        break;
                }

                builder.Append(ch);

                reader.Read();
            }

            return new Token(builder.ToString(), type);
        }

        private Token LexMinusSign()
        {
            reader.Read();

            if (!reader.EndOfStream && IsDigit((char)reader.Peek()))
            {
                return LexNumber(new StringBuilder("-"));
            }

            return new Token("-");
        }

        private Token LexCharString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append((char)reader.Read());

            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Read();
                builder.Append(ch);

                if (ch == '"')
                    break;
            }

            return new Token(builder.ToString(), TokenType.STRING);
        }

        private Token LexIdentifier()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append((char)reader.Read());

            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Peek();

                if (!IsDigit(ch) &&
                    !IsLetter(ch) &&
                    ch != '_')
                    break;

                builder.Append(ch);

                reader.Read();
            }


            if (Keyword.IsKeyword(builder.ToString()))
                return new Token(builder.ToString(), TokenType.KEYWORD);
            else
                return new Token(builder.ToString(), TokenType.IDENTIFIER);
        }
    }
}
