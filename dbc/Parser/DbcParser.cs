using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.DBC.Parser
{
    using DBC.Lex;
    using DBC.Model;

    public class DbcParser
    {
        private DBC dbc = new DBC();

        private TokenStream stream;

        public DbcParser(StreamReader reader)
        {
            Lexer lexer = new Lexer(reader);

            List<Token> tokens = lexer.Lex();

            stream = new TokenStream(tokens);
        }

        public DBC Parse()
        {
            //public string version = null;
            //public List<string> newSymbols = new List<string>();
            //BIT_TIMING
            //public List<string> nodes = new List<string>();
            //public List<ValueTable> valueTables = new List<ValueTable>();
            Messages();
            //MESSAGE_TRANSMITTERS
            //ENVIRONMENT_VARIABLES
            //ENVIRONMENT_VARIABLES_DATA
            //SIGNAL_TYPES
            //public List<Comment> comments = new List<Comment>();
            //public List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();
            //SIGTYPE_ATTR_LIST
            //public List<AttributeDefault> attributeDefaults = new List<AttributeDefault>();
            //public List<AttributeValue> attributeValues = new List<AttributeValue>();
            //public List<SignalValueDescription> valueDescriptions = new List<SignalValueDescription>();
            //CATEGORY_DEFINITIONS
            //CATEGORIES
            //FILTER
            //SIGNAL_TYPE_REFS
            //SIGNAL_GROUPS
            //SIGNAL_EXTENDED_VALUE_TYPE_LIST
            return dbc;
        }

        //messages = {message} ;
        //message = BO_ message_id message_name ':' message_size transmitter {signal} ;
        //message_id = unsigned_integer ;
        //message_name = C_identifier ;
        //message_size = unsigned_integer ;
        //transmitter = node_name | 'Vector__XXX' ;
        private void Messages()
        {
            while (stream.ConsumeIf(t => t.Val == Keyword.MESSAGES))
            {
                Message msg = new Message();

                dbc.messages.Add(msg);


                stream.ConsumeIf(t => t.IsUnsigned(),
                    t => msg.id = t.Val,
                    t => throw new Exception());

                stream.ConsumeIf(t => t.IsIdentifier(),
                    t => msg.name = t.Val,
                    t => throw new Exception());

                stream.ConsumeIf(t => t.Val == ":",
                   t => throw new Exception());

                stream.ConsumeIf(t => t.IsUnsigned(),
                 t => msg.size = t.Val,
                 t => throw new Exception());

                stream.ConsumeIf(t => t.IsIdentifier(),
                    t => msg.transmitter = t.Val,
                    t => throw new Exception());

                while (stream.ConsumeIf(t => t.Val == Keyword.SIGNAL))
                    msg.signals.Add(Signal());
            }

            if (dbc.messages.Count == 0)
                throw new Exception();
        }

        private Signal Signal()
        {
            Signal signal = new Signal();

            stream.ConsumeIf(t => t.IsIdentifier(),
                    t => signal.name = t.Val,
                    t => throw new Exception());

            if (stream.ConsumeIf(t => t.Val == "M"))
            {
                signal.multiplexerIndicator = "M";
            }
            else if (stream.ConsumeIf(t => t.Val == "m"))
            {
                stream.ConsumeIf(t => t.IsUnsigned(),
                    t => signal.multiplexerIndicator = t.Val,
                    t => throw new Exception());
            }

            stream.ConsumeIf(t => t.Val == ":",
                   t => throw new Exception());

            stream.ConsumeIf(t => t.IsUnsigned(),
                    t => signal.startBit = t.Val,
                    t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "|",
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsUnsigned(),
                    t => signal.signalSize = t.Val,
                    t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "@",
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "0" || t.Val == "1",
                    t => signal.byteOrder = t.Val,
                    t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "+" || t.Val == "-",
                    t => signal.valueType = t.Val,
                    t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "(",
                    t => throw new Exception());

            stream.ConsumeIf(t => t.IsDouble(),
                t => signal.factor = t.Val,
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == ",",
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsDouble(),
                t => signal.offset = t.Val,
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == ")",
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "[",
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsDouble(),
                t => signal.min = t.Val,
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "|",
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsDouble(),
                t => signal.max = t.Val,
                t => throw new Exception());

            stream.ConsumeIf(t => t.Val == "]",
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsString(),
                t => signal.unit = t.Val,
                t => throw new Exception());

            stream.ConsumeIf(t => t.IsIdentifier(),
                t => signal.receivers.Add(t.Val),
                t => throw new Exception());

            while (stream.ConsumeIf(t => t.Val == ","))
            {
                stream.ConsumeIf(t => t.IsIdentifier(),
                    t => signal.receivers.Add(t.Val),
                    t => throw new Exception());
            }

            return signal;
        }
    }
}
