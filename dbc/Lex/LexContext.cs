using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Lex
{
    class LexContext
    {
        private StringBuilder builder = new StringBuilder();
        public TokenType Type { get; set; } = TokenType.TOKEN;

        public String Val
        {
            get { return builder.ToString(); }
        }

        public void Append(char c)
        {
            builder.Append(c);
        }

        public Token Finish()
        {
            Token t = new Token(builder.ToString(), Type);

            builder = null;

            return t;
        }
    }
}
