using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbcLib.DBC.Lex
{
    public class Lexer
    {
        private DbcReader reader;
        private List<Token> list = new List<Token>();

        public Lexer(StreamReader reader)
        {
            this.reader = new DbcReader(reader);
        }

        public List<Token> Lex()
        {
            while (!reader.EndOfStream)
            {
                char ch = reader.Curr();

                if (char.IsWhiteSpace(ch))
                {
                    reader.Advance();
                    continue;
                }


                if (IsDigit(ch))
                {
                    list.Add(LexNumber(new LexContext()));
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
                    list.Add(new Token(reader.Consume().ToString()));
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

        private Token LexNumber(LexContext ctx)
        {
            ctx.Append(reader.Consume());

            while (!reader.EndOfStream)
            {
                if (!IsDigit(reader.Curr()))
                {
                    if (reader.Curr() == '.' && ctx.Type != TokenType.DOUBLE)
                        ctx.Type = TokenType.DOUBLE;
                    else
                        break;
                }

                ctx.Append(reader.Consume());
            }

            if (ctx.Type == TokenType.TOKEN)
                ctx.Type = TokenType.UNSIGNED;

            return ctx.Finish();
        }

        private Token LexMinusSign()
        {
            reader.Advance();

            if (!reader.EndOfStream && IsDigit(reader.Curr()))
            {
                LexContext ctx = new LexContext { Type = TokenType.SIGNED };

                ctx.Append('-');

                return LexNumber(ctx);
            }

            return new Token("-");
        }

        private Token LexCharString()
        {
            LexContext ctx = new LexContext { Type = TokenType.STRING };

            ctx.Append(reader.Consume());

            while (!reader.EndOfStream)
            {
                char last = reader.Consume();
                ctx.Append(last);

                if (last == '"')
                    break;
            }

            return ctx.Finish();
        }

        private Token LexIdentifier()
        {
            LexContext ctx = new LexContext();

            ctx.Append(reader.Consume());

            while (!reader.EndOfStream)
            {
                if (!IsDigit(reader.Curr()) &&
                    !IsLetter(reader.Curr()) &&
                    reader.Curr() != '_')
                    break;

                ctx.Append(reader.Consume());
            }


            if (Keyword.IsKeyword(ctx.Val))
                ctx.Type = TokenType.KEYWORD;
            else
                ctx.Type = TokenType.IDENTIFIER;

            return ctx.Finish();
        }
    }
}
