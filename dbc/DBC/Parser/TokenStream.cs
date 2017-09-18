using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbcLib.DBC.Parser
{
    using DBC.Lex;

    class TokenStream
    {
        private static Token sentinel = new Token();

        private List<Token> tokens;
        private int pointer = 0;

        public bool EndOfStream
        {
            get { return pointer >= tokens.Count; }
        }

        public Token Curr
        {
            get
            {
                if (EndOfStream)
                    return sentinel;
                else
                    return tokens[pointer];
            }
        }

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Token Consume()
        {
            Token old = Curr;

            ++pointer;

            return old;
        }

        public Token Consume(string expected)
        {
            if (expected.Length == 0)
                throw new ArgumentException();

            Token old = Curr;

            if (old.Val != expected)
                throw new Exception();

            ++pointer;

            return old;
        }

        public Token Consume(TokenType expected)
        {
            Token old = Curr;

            if (!old.Is(expected))
                throw new Exception();

            ++pointer;

            return old;
        }

        public Token Consume(params string[] args)
        {
            Token old = Curr;

            foreach (string expected in args)
            {
                if (expected.Length == 0)
                    throw new ArgumentException();

                if (old.Val == expected)
                {
                    ++pointer;

                    return old;
                }
            }

            throw new Exception();
        }

        public bool ConsumeIf(bool pred)
        {
            if (pred)
                ++pointer;

            return pred;
        }
    }
}
