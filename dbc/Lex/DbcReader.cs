using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;

namespace DbcLib.DBC.Lex
{
    class DbcReader
    {
        private StreamReader reader;
        private int code = 0;

        public bool EndOfStream { get; private set; } = false;


        public DbcReader(StreamReader reader)
        {
            this.reader = reader;

            Advance();
        }

        public char Curr()
        {
            return (char)code;
        }

        public char Consume()
        {
            char old = Curr();

            Advance();

            return old;
        }

        public void Advance()
        {
            if (!EndOfStream)
            {
                code = reader.Read();
                EndOfStream = code == -1;
            }
        }

    }
}

