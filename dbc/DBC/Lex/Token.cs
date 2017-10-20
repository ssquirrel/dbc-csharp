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
        UNSIGNED = 0x2,
        SIGNED = 0x4 | UNSIGNED,
        DOUBLE = 0X8 | SIGNED,
        STRING = 0x10,
        IDENTIFIER = 0x20
    }

    class Token
    {
        public string Val { get; private set; }
        public TokenType Type { get; set; } = TokenType.None;

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
            if (Type == TokenType.None)
                return t == Type;

            return t.HasFlag(Type);
        }
    }
}
