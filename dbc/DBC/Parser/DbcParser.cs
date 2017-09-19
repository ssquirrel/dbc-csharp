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

    public class ParseException : Exception
    {
        public ParseException() : base("Bad dbc file.")
        {

        }
    }

    public class DbcParser
    {
        private String filename;

        private DBC dbc;
        private TokenStream stream;

        public DbcParser(String fn)
        {
            filename = fn;
        }

        public DBC Parse()
        {
            if (dbc != null)
                return dbc;

            dbc = new DBC();

            using (Lexer lexer = new Lexer(filename))
            {
                List<Token> tokens = lexer.Lex();

                stream = new TokenStream(tokens);
            }

            Version();
            NewSymbols();
            BitTiming();
            Nodes();
            SkipValueTables();
            Messages();

            //MessageTransmitters();
            //ENVIRONMENT_VARIABLES
            //ENVIRONMENT_VARIABLES_DATA
            //SIGNAL_TYPES
            JumpToComments();

            Comments();
            AttributeDefinitions();
            //SIGTYPE_ATTR_LIST
            AttributeDefaults();
            ObjAttributeValues();
            SignalValueDescriptions();
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

            dbc.Version = stream.Consume(TokenType.STRING).Val;
        }

        private void NewSymbols()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.NEW_SYMBOLS))
                return;

            stream.Consume(":");

            while (stream.Curr.Val != Keyword.BIT_TIMING)
            {
                dbc.NewSymbols.Add(stream.Consume().Val);
            }
        }


        //bit_timing = 'BS_:' [baudrate ':' BTR1 ',' BTR2 ] ;
        //This section is obsolete. skip to the next mandatory section
        private void BitTiming()
        {
            stream.Consume(Keyword.BIT_TIMING);
            stream.Consume(":");

            while (stream.ConsumeIf(stream.Curr.Val != Keyword.NODES)) { }
        }

        //'BU_' ':' {node_name} ;
        //node_name = C_identifier
        private void Nodes()
        {
            stream.Consume(Keyword.NODES);
            stream.Consume(":");

            while (stream.Curr.Type == TokenType.IDENTIFIER)
            {
                dbc.Nodes.Add(stream.Consume().Val);
            }
        }

        private void SkipValueTables()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.VALUE_TABLES))
            {
                while (stream.ConsumeIf(stream.Curr.Val != ";")) { }

                stream.Consume();
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
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.MESSAGES))
            {
                Message msg = new Message
                {
                    Signals = new List<Signal>()
                };
                dbc.Messages.Add(msg);

                msg.MsgID = stream.Consume(TokenType.UNSIGNED).Val;
                msg.Name = stream.Consume(TokenType.IDENTIFIER).Val;

                stream.Consume(":");

                msg.Size = stream.Consume(TokenType.UNSIGNED).Val;
                msg.Transmitter = stream.Consume(TokenType.IDENTIFIER).Val;

                while (stream.ConsumeIf(stream.Curr.Val == Keyword.SIGNAL))
                {
                    msg.Signals.Add(Signal());
                }
            }

            if (dbc.Messages.Count == 0)
                throw new ParseException();
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
            Signal signal = new Signal
            {
                Name = stream.Consume(TokenType.IDENTIFIER).Val,
                Receivers = new List<string>()
            };

            stream.Consume(":");
            signal.StartBit = stream.Consume(TokenType.UNSIGNED).Val;
            stream.Consume("|");
            signal.SignalSize = stream.Consume(TokenType.UNSIGNED).Val;
            stream.Consume("@");
            signal.ByteOrder = stream.Consume("0", "1").Val;
            signal.ValueType = stream.Consume("+", "-").Val;

            stream.Consume("(");
            signal.Factor = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume(",");
            signal.Offset = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume(")");

            stream.Consume("[");
            signal.Min = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume("|");
            signal.Max = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume("]");

            signal.Unit = stream.Consume(TokenType.STRING).Val;

            do
            {
                signal.Receivers.Add(stream.Consume(TokenType.IDENTIFIER).Val);

            } while (stream.ConsumeIf(stream.Curr.Val == ","));


            return signal;
        }

        private void JumpToComments()
        {
            while (stream.ConsumeIf(stream.Curr.Val != Keyword.COMMENTS)) { }
        }

        private void Comments()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.COMMENTS))
            {
                if (stream.Curr.Type == TokenType.STRING)
                {
                    dbc.Comments.Add(new Comment
                    {
                        Str = stream.Consume().Val
                    });
                }
                else if (stream.Curr.Val == Keyword.NODES ||
                    stream.Curr.Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    dbc.Comments.Add(new Comment
                    {
                        Type = stream.Consume().Val,
                        Name = stream.Consume(TokenType.IDENTIFIER).Val,
                        Str = stream.Consume(TokenType.STRING).Val
                    });
                }
                else if (stream.Curr.Val == Keyword.MESSAGES)
                {
                    dbc.Comments.Add(new Comment
                    {
                        Type = stream.Consume().Val,
                        MsgID = stream.Consume(TokenType.UNSIGNED).Val,
                        Str = stream.Consume(TokenType.STRING).Val
                    });
                }
                else if (stream.Curr.Val == Keyword.SIGNAL)
                {
                    dbc.Comments.Add(new Comment
                    {
                        Type = stream.Consume().Val,
                        MsgID = stream.Consume(TokenType.UNSIGNED).Val,
                        Name = stream.Consume(TokenType.IDENTIFIER).Val,
                        Str = stream.Consume(TokenType.STRING).Val
                    });
                }
                else
                {
                    throw new ParseException();
                }

                stream.Consume(";");
            }
        }

        private void AttributeDefinitions()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_DEFINITIONS))
            {
                AttributeDefinition ad = new AttributeDefinition
                {
                    Values = new List<string>()
                };

                dbc.AttributeDefinitions.Add(ad);

                switch (stream.Curr.Val)
                {
                    case Keyword.NODES:
                    case Keyword.MESSAGES:
                    case Keyword.SIGNAL:
                    case Keyword.ENVIRONMENT_VARIABLES:
                        ad.ObjectType = stream.Consume().Val;
                        break;
                }

                ad.AttributeName = stream.Consume(TokenType.STRING).Val;

                if (stream.Curr.Val == "INT" ||
                    stream.Curr.Val == "HEX")
                {
                    ad.ValueType = stream.Consume().Val;
                    ad.Values.Add(stream.Consume(TokenType.SIGNED).Val);
                    ad.Values.Add(stream.Consume(TokenType.SIGNED).Val);
                }
                else if (stream.Curr.Val == "FLOAT")
                {
                    ad.ValueType = stream.Consume().Val;
                    ad.Values.Add(stream.Consume(TokenType.DOUBLE).Val);
                    ad.Values.Add(stream.Consume(TokenType.DOUBLE).Val);
                }
                else if (stream.Curr.Val == "STRING")
                {
                    ad.ValueType = stream.Consume().Val;
                }
                else if (stream.Curr.Val == "ENUM")
                {
                    ad.ValueType = stream.Consume().Val;

                    while (stream.Curr.Type == TokenType.STRING)
                    {
                        ad.Values.Add(stream.Consume().Val);

                        if (!stream.ConsumeIf(stream.Curr.Val == ","))
                            break;
                    }
                }
                else
                {
                    throw new ParseException();
                }

                stream.Consume(";");
            }
        }

        private void AttributeDefaults()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_DEFAULTS))
            {
                AttributeDefault ad = new AttributeDefault
                {
                    AttributeName = stream.Consume(TokenType.STRING).Val,
                    AttributeValue = AttributeValues()
                };

                if (!Lexer.IsIdentifier(ad.AttributeName))
                    throw new ParseException();

                dbc.AttributeDefaults.Add(ad);

                stream.Consume(";");
            }
        }

        private AttributeValue AttributeValues()
        {
            if (stream.Curr.Is(TokenType.DOUBLE))
            {
                return new AttributeValue { Num = stream.Consume().DOUBLE };
            }

            if (stream.Curr.Is(TokenType.STRING))
            {
                return new AttributeValue { Val = stream.Consume().Val };
            }

            throw new ParseException();
        }

        private void ObjAttributeValues()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_VALUES))
            {
                ObjAttributeValue av = new ObjAttributeValue();
                dbc.AttributeValues.Add(av);

                av.AttributeName = stream.Consume(TokenType.STRING).Val;

                if (!Lexer.IsIdentifier(av.AttributeName))
                    throw new ParseException();

                if (stream.Curr.Val == Keyword.NODES ||
                    stream.Curr.Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    av.Type = stream.Consume().Val;
                    av.Name = stream.Consume(TokenType.IDENTIFIER).Val;
                }
                else if (stream.Curr.Val == Keyword.MESSAGES)
                {
                    av.Type = stream.Consume().Val;
                    av.MsgID = stream.Consume(TokenType.UNSIGNED).Val;
                }
                else if (stream.Curr.Val == Keyword.SIGNAL)
                {
                    av.Type = stream.Consume().Val;
                    av.MsgID = stream.Consume(TokenType.UNSIGNED).Val;
                    av.Name = stream.Consume(TokenType.IDENTIFIER).Val;
                }

                av.AttributeValue = AttributeValues();

                stream.Consume(";");
            }
        }

        //value_descriptions = { value_descriptions_for_signal | value_descriptions_for_env_var } ;
        //value_descriptions_for_signal = 'VAL_' message_id signal_name { value_description } ';' ;
        //value_descriptions_for_env_var = 'VAL_' env_var_aname { value_description } ';'
        //message_id = unsigned_integer ;
        //signal_name = C_identifier ;
        //env_var_name = C_identifier ;
        //value_description = double char_string
        private void SignalValueDescriptions()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.VALUE_DESCRIPTIONS))
            {
                SignalValueDescription vd = new SignalValueDescription
                {
                    MsgID = stream.Consume(TokenType.UNSIGNED).Val,
                    Name = stream.Consume(TokenType.IDENTIFIER).Val,
                    Descs = new List<ValueDesc>()
                };
                dbc.ValueDescriptions.Add(vd);

                while (!stream.ConsumeIf(stream.Curr.Val == ";"))
                {
                    vd.Descs.Add(ValueDescriptions());
                }
            }
        }

        private ValueDesc ValueDescriptions()
        {
            ValueDesc vd = new ValueDesc
            {
                Num = stream.Consume(TokenType.DOUBLE).Val,
                Str = stream.Consume(TokenType.STRING).Val
            };

            return vd;
        }
    }
}
