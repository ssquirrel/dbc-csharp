using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbcLib.DBC.Parser
{
    using DBC.Lex;

    class Pattern
    {
        public List<Token> path = new List<Token>();
        public List<Pattern> branch = new List<Pattern>();
    }

    class TokenStream
    {
        private List<Token> tokens;
        private int pointer = 0;

        public bool EndOfStream
        {
            get { return tokens.Count == pointer; }
        }

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Token Curr()
        {
            return tokens[pointer];
        }

        public Token Consume()
        {
            return tokens[pointer++];
        }

        public bool Curr(Predicate<Token> p)
        {
            return !EndOfStream && p(Curr());
        }

        public Token[] Consume(Token[] pattern)
        {
            Token[] result = new Token[pattern.Length];

            for (int i = 0; !EndOfStream && i < pattern.Length; ++i)
            {
                Token t = pattern[i];

                if (t.Val != null)
                {
                    if (t.Val != Curr().Val)
                        break;
                }
                else if (t.Type == TokenType.SIGNED)
                {
                    if (!Curr().IsUnsigned())
                        break;
                }
                else if (t.Type == TokenType.DOUBLE)
                {
                    if (!Curr().IsDouble())
                        break;
                }
                else
                {
                    if (t.Type != Curr().Type)
                        break;
                }

                result[i] = Consume();
            }

            return result;
        }

        public bool Consume(Predicate<Token> p)
        {
            return !EndOfStream && p(Consume());
        }

        public bool ConsumeIf(Predicate<Token> p)
        {
            if (EndOfStream)
                return false;

            bool result = p(Curr());

            if (result)
                ++pointer;

            return result;
        }

        public void ConsumeIf(Predicate<Token> p, Action<Token> failure)
        {
            if (!ConsumeIf(p))
                failure(Curr());
        }

        public void ConsumeIf(Predicate<Token> p,
            Action<Token> success,
            Action<Token> failure)
        {
            if (ConsumeIf(p))
                success(tokens[pointer - 1]);
            else
                failure(Curr());
        }
    }
}
