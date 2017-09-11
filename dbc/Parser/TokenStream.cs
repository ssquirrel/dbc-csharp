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
