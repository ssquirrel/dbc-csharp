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
            new Token(Keyword.MESSAGES),
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

        private static readonly Token CM_TOKEN =
            new Token(Keyword.COMMENTS);

        private static readonly Token STRING_TOKEN =
            new Token(TokenType.STRING);

        private static readonly Token IDENTIFIER_TOKEN =
            new Token(TokenType.IDENTIFIER);

        private static readonly Token SEMI_COLON_TOKEN =
            new Token(";");

        private static readonly Token[] CM_PATTERN = new Token[] {
            CM_TOKEN,
            IDENTIFIER_TOKEN,
            STRING_TOKEN,
            SEMI_COLON_TOKEN};

        private static readonly Token[] CM_NODES_PATTERN = new Token[] {
            new Token(TokenType.IDENTIFIER),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] CM_MESSAGES_PATTERN = new Token[] {
            new Token(Keyword.MESSAGES),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] CM_SIGNAL_PATTERN = new Token[] {
            new Token(Keyword.SIGNAL),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] AD_INT_HEX_PATTERN = new Token[]
        {
            new Token(TokenType.SIGNED),
            new Token(TokenType.SIGNED),
            new Token(";")
        };

        private static readonly Token[] AD_FLOAT_PATTERN = new Token[]
        {
            new Token("FLOAT"),
            new Token(TokenType.DOUBLE),
            new Token(TokenType.DOUBLE),
            new Token(";")
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
            Version();
            NewSymbols();
            BitTiming();
            Nodes();
            ValueTables();
            Messages();
            //MessageTransmitters();
            //ENVIRONMENT_VARIABLES
            //ENVIRONMENT_VARIABLES_DATA
            //SIGNAL_TYPES
            Comments();
            AttributeDefinitions();
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

        private void Version()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.VERSION))
                return;

            if (stream.Curr.IsString())
                dbc.version = stream.Consume().Val;
            else
                throw new Exception();

        }

        private void NewSymbols()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.NEW_SYMBOLS))
                return;

            if (!stream.ConsumeIf(stream.Curr.Val == ":"))
                throw new Exception();


            while (stream.Curr.Val != Keyword.BIT_TIMING)
            {
                dbc.newSymbols.Add(stream.Consume().Val);
            }
        }


        //bit_timing = 'BS_:' [baudrate ':' BTR1 ',' BTR2 ] ;
        //This section is obsolete. skip to the next mandatory section
        private void BitTiming()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.BIT_TIMING))
                throw new Exception();

            if (!stream.ConsumeIf(stream.Curr.Val == ":"))
                throw new Exception();

            while (stream.ConsumeIf(stream.Curr.Val != Keyword.NODES)) { }
        }

        //'BU_' ':' {node_name} ;
        //node_name = C_identifier
        private void Nodes()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.NODES))
                throw new Exception();

            if (!stream.ConsumeIf(stream.Curr.Val == ":"))
                throw new Exception();

            while (stream.Curr.IsIdentifier())
            {
                dbc.newSymbols.Add(stream.Consume().Val);
            }
        }

        //value_tables = {value_table} ;
        //value_table = 'VAL_TABLE_' C_identifier {value_description} ';'
        //value_description = double char_string ;
        private void ValueTables()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.VALUE_TABLES))
            {
                ValueTable vt = new ValueTable();
                dbc.valueTables.Add(vt);

                if (stream.Curr.IsIdentifier())
                    vt.name = stream.Consume().Val;
                else
                    throw new Exception();

                while (stream.Curr.Val != ";")
                {
                    ValueDescription vd = new ValueDescription();
                    vt.descriptions.Add(vd);

                    if (stream.Curr.IsDouble())
                        vd.num = stream.Consume().Val;
                    else
                        throw new Exception();

                    if (stream.Curr.IsString())
                        vd.str = stream.Consume().Val;
                    else
                        throw new Exception();
                }

                if (!stream.ConsumeIf(stream.Curr.Val == ";"))
                    throw new Exception();
            }
        }

        //messages = {message} ;
        //message = BO_ message_id message_name ':' message_size transmitter {signal} ;
        //message_id = unsigned_integer ;
        //message_name = C_identifier ;
        //message_size = unsigned_integer ;
        //transmitter = node_name | 'Vector__XXX' ;
        private void Messages()
        {
            while (!stream.EndOfStream)
            {
                Token[] parsed = stream.Consume(MESSAGES_PATTERN);

                if (parsed.First() == null)
                    break;

                if (parsed.Last() == null)
                    throw new Exception();

                Message msg = new Message
                {
                    id = parsed[1].Val,
                    name = parsed[2].Val,
                    size = parsed[4].Val,
                    transmitter = parsed[5].Val
                };
                dbc.messages.Add(msg);

                while (stream.ConsumeIf(stream.Curr.Val == Keyword.SIGNAL))
                {
                    msg.signals.Add(Signal());
                }
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

            if (stream.Curr.IsIdentifier())
                signal.name = stream.Consume().Val;
            else
                throw new Exception();

            if (stream.Curr.Val == "M")
            {
                signal.multiplexerIndicator = stream.Consume().Val;
            }
            else if (stream.ConsumeIf(stream.Curr.Val == "m"))
            {
                if (stream.Curr.IsUnsigned())
                    signal.multiplexerIndicator = stream.Consume().Val;
                else
                    throw new Exception();
            }

            {
                Token[] parsed = stream.Consume(SIGNAL_PATTERN1);

                if (parsed.Last() == null)
                    throw new Exception();

                signal.startBit = parsed[1].Val;
                signal.signalSize = parsed[3].Val;
            }

            if (stream.Curr.Val == "0" || stream.Curr.Val == "1")
                signal.byteOrder = stream.Consume().Val;
            else
                throw new Exception();

            if (stream.Curr.Val == "+" || stream.Curr.Val == "-")
                signal.valueType = stream.Consume().Val;
            else
                throw new Exception();

            {
                Token[] parsed = stream.Consume(SIGNAL_PATTERN2);

                if (parsed.Last() == null)
                    throw new Exception();

                signal.factor = parsed[1].Val;
                signal.offset = parsed[3].Val;
                signal.min = parsed[6].Val;
                signal.max = parsed[8].Val;
                signal.unit = parsed[10].Val;
                signal.receivers.Add(parsed[11].Val);
            }

            while (stream.ConsumeIf(stream.Curr.Val == ","))
            {
                if (stream.Curr.IsIdentifier())
                    signal.receivers.Add(stream.Consume().Val);
                else
                    throw new Exception();
            }

            return signal;
        }

        private void MessageTransmitters()
        {

        }

        private void Comments()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.COMMENTS))
            {
                if (stream.Curr.IsString())
                {
                    dbc.comments.Add(new Comment
                    {
                        msg = stream.Consume().Val
                    });

                    if (!stream.ConsumeIf(stream.Curr.Val == ";"))
                        throw new Exception();
                }
                else if (stream.Curr.Val == Keyword.NODES ||
                    stream.Curr.Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    Comment cm = new Comment
                    {
                        type = stream.Consume().Val
                    };
                    dbc.comments.Add(cm);

                    Token[] parsed = stream.Consume(CM_NODES_PATTERN);

                    if (parsed.Last() == null)
                        throw new Exception();

                    cm.name = parsed[0].Val;
                    cm.msg = parsed[1].Val;
                }
                else if (stream.Curr.Val == Keyword.MESSAGES)
                {
                    Token[] parsed = stream.Consume(CM_MESSAGES_PATTERN);

                    if (parsed.Last() == null)
                        throw new Exception();

                    dbc.comments.Add(new Comment
                    {
                        type = parsed[0].Val,
                        id = parsed[1].Val,
                        msg = parsed[2].Val
                    });
                }
                else if (stream.Curr.Val == Keyword.SIGNAL)
                {
                    Token[] parsed = stream.Consume(CM_SIGNAL_PATTERN);

                    if (parsed.Last() == null)
                        throw new Exception();

                    dbc.comments.Add(new Comment
                    {
                        type = parsed[0].Val,
                        id = parsed[1].Val,
                        name = parsed[2].Val,
                        msg = parsed[3].Val
                    });
                }
                else
                {
                    throw new Exception();
                }

            }
        }

        private void AttributeDefinitions()
        {
            while (stream.ConsumeIf(
                stream.Curr.Val == Keyword.ATTRIBUTE_DEFINITIONS
                ))
            {

                if (!stream.Curr.IsString())
                {

                }

            }
        }


    }
}
