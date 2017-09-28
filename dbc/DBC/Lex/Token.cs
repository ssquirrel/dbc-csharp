using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Lex
{
    [Flags]
    enum TokenType
    {
        None = 0x0,
        TOKEN = 0x1,
        DOUBLE = 0x2,
        SIGNED = 0x4 | DOUBLE,
        UNSIGNED = 0X8 | SIGNED,
        STRING = 0x10,
        IDENTIFIER = 0x20
    }

    class Token
    {
        public string Val { get; private set; }
        private TokenType Type { get; set; } = TokenType.None;

        public double DOUBLE { get { return Double.Parse(Val); } }
        public int INT { get { return int.Parse(Val); } }

        //creates a sentinel token
        public Token()
        {

        }

        public Token(string v)
        {
            Val = v;
            Type = TokenType.TOKEN;
        }

        public Token(string v, TokenType t)
        {
            Val = v;
            Type = t;
        }

        public bool Assert(TokenType t)
        {
            if (t == TokenType.None)
                return t == Type;

            return Type.HasFlag(t);
        }
    }
}
