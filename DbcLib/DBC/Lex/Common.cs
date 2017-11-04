using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Lex
{
    static class Common
    {
        public static bool IsLetter(char ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
        }

        public static bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        public static bool IsIdentifierStart(char ch)
        {
            if (IsLetter(ch) || ch == '_')
                return true;

            return false;
        }

        public static bool IsIdentifierEnd(char ch)
        {
            if (IsIdentifierStart(ch) || IsDigit(ch))
                return true;

            return false;
        }

        public static bool IsIdentifier(string str)
        {
            if (!IsIdentifierStart(str[0]))
                return false;

            for (int i = 1; i < str.Length; ++i)
                if (!IsIdentifierEnd(str[i]))
                    return false;

            return true;
        }
    }
}
