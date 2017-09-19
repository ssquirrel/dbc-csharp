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
            if (EndOfStream)
                return sentinel;

            Token old = Curr;

            ++pointer;

            return old;
        }

        public bool ConsumeIf(bool pred)
        {
            if (EndOfStream)
                return false;

            if (pred)
                ++pointer;

            return pred;
        }

        public Token Consume(string expected)
        {
            if (EndOfStream || tokens[pointer].Val != expected)
                throw new ParseException();

            return tokens[pointer++];
        }

        public Token Consume(TokenType expected)
        {
            if (EndOfStream || !tokens[pointer].Is(expected))
                throw new ParseException();

            return tokens[pointer++];
        }

        public Token Consume(params string[] args)
        {
            if (EndOfStream)
                throw new ParseException();

            Token curr = tokens[pointer];

            foreach (string expected in args)
            {
                if (curr.Val == expected)
                {
                    ++pointer;

                    return curr;
                }
            }

            throw new ParseException();
        }
    }
}
