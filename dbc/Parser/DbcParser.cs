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
        private static readonly Token[] MESSAGES_PATTERN = new Token[] {
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER),
            new Token(":"),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER)
        };

        private static readonly Token[] SIGNAL_PATTERN1 = new Token[] {
            new Token(":"),
            new Token(TokenType.UNSIGNED),
            new Token("|"),
            new Token(TokenType.UNSIGNED),
            new Token("@")};

        private static readonly Token[] SIGNAL_PATTERN2 = new Token[] {
            new Token("("),
            new Token(TokenType.DOUBLE),
            new Token(","),
            new Token(TokenType.DOUBLE),
            new Token(")"),
            new Token("["),
            new Token(TokenType.DOUBLE),
            new Token("|"),
            new Token(TokenType.DOUBLE),
            new Token("]"),
            new Token(TokenType.STRING),
            new Token(TokenType.IDENTIFIER)
        };



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
            while (stream.ConsumeIf(p => p.Val == Keyword.MESSAGES))
            {
                Token[] parsed = stream.Consume(MESSAGES_PATTERN);

                if (parsed.Length == 0)
                    break;

                if (parsed.Length != MESSAGES_PATTERN.Length)
                    throw new Exception();

                Message msg = new Message
                {
                    id = parsed[0].Val,
                    name = parsed[1].Val,
                    size = parsed[3].Val,
                    transmitter = parsed[4].Val
                };

                while (stream.ConsumeIf(t => t.Val == Keyword.SIGNAL))
                    msg.signals.Add(Signal());

                dbc.messages.Add(msg);
            }

            if (dbc.messages.Count == 0)
                throw new Exception();
        }

        //signal = 'SG_' signal_name multiplexer_indicator ':' start_bit '|'
        //signal_size '@' byte_order value_type '(' factor ',' offset ')'
        //       '[' minimum '|' maximum ']' unit receiver {',' receiver} ;
        //signal_name = C_identifier ;
        //multiplexer_indicator = ' ' | 'M' | 'm' multiplexer_switch_value ;
        //multiplexer_switch_value = unsigned_integer;
        //start_bit = unsigned_integer ;
        //signal_size = unsigned_integer ;
        //byte_order = '0' | '1' ; (* 0=little endian, 1=big endian *)
        //value_type = '+' | '-' ; (* +=unsigned, -=signed *)
        //factor = double ;
        //offset = double ;
        //minimum = double ;
        //maximum = double ;
        //unit = char_string ;
        //receiver = node_name | 'Vector__XXX' ;
        private Signal Signal()
        {
            Signal signal = new Signal();

            if (stream.Curr(t => t.IsIdentifier()))
                signal.name = stream.Consume().Val;
            else
                throw new Exception();

            if (stream.ConsumeIf(t => t.Val == "M"))
            {
                signal.multiplexerIndicator = "M";
            }
            else if (stream.ConsumeIf(t => t.Val == "m"))
            {
                if (stream.Curr(t => t.IsUnsigned()))
                    signal.multiplexerIndicator = stream.Consume().Val;
                else
                    throw new Exception();
            }

            {
                Token[] parsed = stream.Consume(SIGNAL_PATTERN1);

                if (parsed.Length != SIGNAL_PATTERN1.Length)
                    throw new Exception();

                signal.startBit = parsed[1].Val;
                signal.signalSize = parsed[3].Val;
            }

            if (stream.Curr(t => t.Val == "0" || t.Val == "1"))
                signal.byteOrder = stream.Consume().Val;
            else
                throw new Exception();

            if (stream.Curr(t => t.Val == "+" || t.Val == "-"))
                signal.valueType = stream.Consume().Val;
            else
                throw new Exception();

            {
                Token[] parsed = stream.Consume(SIGNAL_PATTERN2);

                if (parsed.Length != SIGNAL_PATTERN2.Length)
                    throw new Exception();

                signal.factor = parsed[1].Val;
                signal.offset = parsed[3].Val;
                signal.min = parsed[6].Val;
                signal.max = parsed[8].Val;
                signal.unit = parsed[10].Val;
                signal.receivers.Add(parsed[11].Val);
            }

            while (stream.ConsumeIf(t => t.Val == ","))
            {
                if (stream.Curr(t => t.IsIdentifier()))
                    signal.receivers.Add(stream.Consume().Val);
                else
                    throw new Exception();
            }

            return signal;
        }
    }
}
