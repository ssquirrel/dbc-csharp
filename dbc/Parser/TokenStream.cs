﻿using System;
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
            get { return pointer < tokens.Count; }
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
            ++pointer;
            return Curr;
        }

        public Token[] Consume(Token[] pattern)
        {
            Token[] result = new Token[pattern.Length];

            for (int i = 0; !EndOfStream && i < pattern.Length; ++i)
            {
                Token curr = tokens[pointer];
                Token p = pattern[i];

                if (p.Val.Length != 0)
                {
                    if (p.Val != curr.Val)
                        break;
                }
                else if (p.Type == TokenType.SIGNED)
                {
                    if (!curr.IsSigned())
                        break;
                }
                else if (p.Type == TokenType.DOUBLE)
                {
                    if (!curr.IsDouble())
                        break;
                }
                else
                {
                    if (p.Type != curr.Type)
                        break;
                }

                result[i] = tokens[pointer++];
            }

            return result;
        }

        public bool ConsumeIf(bool pred)
        {
            if (pred)
                ++pointer;

            return pred;
        }
    }
}
