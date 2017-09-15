using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Lex
{
    [Flags]
    public enum TokenType
    {
        None = 0x0,
        TOKEN = 0x1,
        UNSIGNED = 0x2,
        SIGNED = 0x4 | UNSIGNED,
        DOUBLE = 0X8 | SIGNED,
        STRING = 0x10,
        IDENTIFIER = 0x20,
        KEYWORD = 0x40,
    }

    public class Token
    {
        public string Val { get; private set; } = "";
        public TokenType Type { get; private set; } = TokenType.None;

        //creates a sentinel token
        public Token()
        {

        }

        public Token(string v)
        {
            Val = v;
            Type = TokenType.TOKEN;
        }

        public Token(TokenType t)
        {
            Type = t;
        }

        public Token(string v, TokenType t)
        {
            Val = v;
            Type = t;
        }

        public bool IsUnsigned()
        {
            return Type == TokenType.UNSIGNED;
        }

        public bool IsSigned()
        {
            return Type == TokenType.UNSIGNED || Type == TokenType.SIGNED;
        }

        public bool IsDouble()
        {
            switch (Type)
            {
                case TokenType.UNSIGNED:
                case TokenType.SIGNED:
                case TokenType.DOUBLE:
                    return true;
            }

            return false;
        }

        public bool IsString()
        {
            return Type == TokenType.STRING;
        }

        public bool IsIdentifier()
        {
            return Type == TokenType.IDENTIFIER;
        }

        public bool IsKeyword()
        {
            return Type == TokenType.KEYWORD;
        }
    }
}
