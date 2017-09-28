using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DbcLib.DBC.Lex
{
    using static Common;

    class TokenStream : Peekable
    {
        public TokenStream(string filename) : base(filename) { }

        public Token Consume(string s)
        {
            if (Peek().Val == s)
                return Read();

            return Sentinel;
        }

        public Token Consume(TokenType t)
        {
            Token curr = Peek();

            if (curr.Assert(t))
                return Read();

            if (t.HasFlag(TokenType.STRING) && curr.Assert(TokenType.STRING))
            {
                switch (t & ~TokenType.STRING)
                {
                    case TokenType.IDENTIFIER:
                        if (IsIdentifier(curr.Val))
                            return Read();

                        break;
                }
            }


            return Sentinel;
        }

        public bool ConsumeIf(bool pred)
        {
            if (pred)
                Read();

            return pred;
        }
    }

    class Peekable : Lexer
    {
        private Token buffer;

        public Peekable(string filename) : base(filename) { }

        public Token Peek()
        {
            if (buffer == null)
                buffer = base.Read();

            return buffer;
        }

        public new Token Read()
        {
            if (buffer != null)
            {
                Token temp = buffer;
                buffer = null;

                return temp;
            }

            return base.Read();
        }
    }

    class Lexer : IDisposable
    {
        private StreamReader reader;

        public Lexer(string filename)
        {
            reader = new StreamReader(filename, Encoding.Default);
        }

        public bool EndOfStream => reader.EndOfStream;

        public static Token Sentinel = new Token();

        public void Dispose()
        {
            reader.Dispose();
        }

        public Token Read()
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
                    return LexNumber(new StringBuilder());
                }
                else if (ch == '-')
                {
                    return LexMinusSign();
                }
                else if (ch == '"')
                {
                    return LexCharString();
                }
                else if (IsIdentifierStart(ch))
                {
                    return LexIdentifier();
                }
                else
                {
                    reader.Read();
                    return new Token(ch.ToString());
                }
            }

            return Sentinel;
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

        //no longer collects '"'
        private Token LexCharString()
        {
            StringBuilder builder = new StringBuilder();

            reader.Read();

            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Read();

                if (ch == '"')
                    break;

                builder.Append(ch);
            }

            string str = builder.ToString();

            return new Token(str, TokenType.STRING);
        }

        private Token LexIdentifier()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append((char)reader.Read());

            while (!reader.EndOfStream)
            {
                char ch = (char)reader.Peek();

                if (!IsIdentifierEnd(ch))
                    break;

                builder.Append(ch);

                //Read() returns int not char!!!
                reader.Read();
            }

            return new Token(builder.ToString(), TokenType.IDENTIFIER);
        }
    }
}
