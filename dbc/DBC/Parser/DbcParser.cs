using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using DbcLib.DBC.Lex;
using DbcLib.Model;

namespace DbcLib.DBC.Parser
{
    [Serializable]
    public class DbcParseException : Exception
    {
        public DbcParseException() : base("bad dbc file.")
        {
        }

        public DbcParseException(string message) : base(message)
        {
        }

        public DbcParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DbcParseException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class DbcParser : IDisposable
    {
        private Model.DBC dbc = new Model.DBC();
        private TokenStream stream;

        public static Model.DBC Parse(String fn)
        {
            using (DbcParser parser = new DbcParser(fn))
            {
                return parser.ParseImpl();
            }
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        private DbcParser(String fn)
        {
            stream = new TokenStream(fn);
        }

        private Model.DBC ParseImpl()
        {
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

        private Token EXPECT(string e)
        {
            Token t = stream.Consume(e);

            if (t == TokenStream.Sentinel)
                throw new DbcParseException();

            return t;
        }

        private Token EXPECT(TokenType e)
        {
            Token t = stream.Consume(e);

            if (t == TokenStream.Sentinel)
                throw new DbcParseException();

            return t;
        }

        private Token EXPECT(string e1, string e2)
        {
            {
                Token t = stream.Consume(e1);

                if (t != TokenStream.Sentinel)
                    return t;
            }

            {
                Token t = stream.Consume(e2);

                if (t != TokenStream.Sentinel)
                    return t;
            }

            throw new DbcParseException();
        }

        private void Version()
        {
            if (!stream.ConsumeIf(stream.Peek().Val == Keyword.VERSION))
                return;

            dbc.Version = EXPECT(TokenType.STRING).Val;
        }

        private void NewSymbols()
        {
            if (!stream.ConsumeIf(stream.Peek().Val == Keyword.NEW_SYMBOLS))
                return;

            EXPECT(":");

            while (stream.Peek().Val != Keyword.BIT_TIMING)
            {
                dbc.NewSymbols.Add(stream.Read().Val);
            }
        }


        //bit_timing = 'BS_:' [baudrate ':' BTR1 ',' BTR2 ] ;
        //This section is obsolete. skip to the next mandatory section
        private void BitTiming()
        {
            EXPECT(Keyword.BIT_TIMING);
            EXPECT(":");

            while (stream.ConsumeIf(stream.Peek().Val != Keyword.NODES)) { }
        }

        //'BU_' ':' {node_name} ;
        //node_name = C_identifier
        private void Nodes()
        {
            EXPECT(Keyword.NODES);
            EXPECT(":");

            while (stream.Peek().Val != Keyword.MESSAGES)
            {
                dbc.Nodes.Add(stream.Read().Val);
            }
        }

        private void SkipValueTables()
        {
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.VALUE_TABLES))
            {
                while (stream.ConsumeIf(stream.Peek().Val != ";")) { }

                stream.Read();
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
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.MESSAGES))
            {
                Message msg = new Message
                {
                    Signals = new List<Signal>()
                };
                dbc.Messages.Add(msg);

                msg.MsgID = EXPECT(TokenType.UNSIGNED).INT;
                msg.Name = EXPECT(TokenType.IDENTIFIER).Val;

                EXPECT(":");

                msg.Size = EXPECT(TokenType.UNSIGNED).INT;
                msg.Transmitter = EXPECT(TokenType.IDENTIFIER).Val;

                while (stream.ConsumeIf(stream.Peek().Val == Keyword.SIGNAL))
                {
                    msg.Signals.Add(Signal());
                }
            }

            if (dbc.Messages.Count == 0)
                throw new DbcParseException();
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
                Name = EXPECT(TokenType.IDENTIFIER).Val,
                Receivers = new List<string>()
            };

            EXPECT(":");
            signal.StartBit = EXPECT(TokenType.UNSIGNED).INT;
            EXPECT("|");
            signal.SignalSize = EXPECT(TokenType.UNSIGNED).INT;
            EXPECT("@");
            signal.ByteOrder = EXPECT("0", "1").Val;
            signal.ValueType = EXPECT("+", "-").Val;

            EXPECT("(");
            signal.Factor = EXPECT(TokenType.DOUBLE).DOUBLE;
            EXPECT(",");
            signal.Offset = EXPECT(TokenType.DOUBLE).DOUBLE;
            EXPECT(")");

            EXPECT("[");
            signal.Min = EXPECT(TokenType.DOUBLE).DOUBLE;
            EXPECT("|");
            signal.Max = EXPECT(TokenType.DOUBLE).DOUBLE;
            EXPECT("]");

            signal.Unit = EXPECT(TokenType.STRING).Val;

            do
            {
                signal.Receivers.Add(EXPECT(TokenType.IDENTIFIER).Val);

            } while (stream.ConsumeIf(stream.Peek().Val == ","));


            return signal;
        }

        private void JumpToComments()
        {
            while (stream.ConsumeIf(stream.Peek().Val != Keyword.COMMENTS)) { }
        }

        private void Comments()
        {
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.COMMENTS))
            {
                if (stream.Peek().Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    while (stream.ConsumeIf(stream.Peek().Val != ";")) { }
                    stream.Read();

                    continue;
                }

                Comment cm = new Comment { Type = "" };
                dbc.Comments.Add(cm);

                if (stream.Peek().Val == Keyword.NODES)
                {
                    cm.Type = stream.Read().Val;
                    cm.NodeName = EXPECT(TokenType.IDENTIFIER).Val;
                }
                else if (stream.Peek().Val == Keyword.MESSAGES)
                {
                    cm.Type = stream.Read().Val;
                    cm.MsgID = EXPECT(TokenType.UNSIGNED).INT;

                }
                else if (stream.Peek().Val == Keyword.SIGNAL)
                {
                    cm.Type = stream.Read().Val;
                    cm.MsgID = EXPECT(TokenType.UNSIGNED).INT;
                    cm.SignalName = EXPECT(TokenType.IDENTIFIER).Val;

                }
                else
                {
                    throw new DbcParseException();
                }

                cm.Val = EXPECT(TokenType.STRING).Val;

                EXPECT(";");
            }
        }

        private void AttributeDefinitions()
        {
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.ATTRIBUTE_DEFINITIONS))
            {
                if (stream.Peek().Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    while (stream.ConsumeIf(stream.Peek().Val != ";")) { }
                    stream.Read();

                    continue;
                }

                AttributeDefinition ad = new AttributeDefinition
                {
                    ObjectType = ""
                };


                if (stream.Peek().Val == Keyword.NODES ||
                    stream.Peek().Val == Keyword.MESSAGES ||
                    stream.Peek().Val == Keyword.SIGNAL)
                {
                    ad.ObjectType = stream.Read().Val;
                }

                ad.AttributeName = EXPECT(TokenType.STRING | TokenType.IDENTIFIER).Val;

                if (stream.Peek().Val == "INT" ||
                    stream.Peek().Val == "HEX")
                {
                    ad.ValueType = stream.Read().Val;
                    ad.Num1 = EXPECT(TokenType.SIGNED).INT;
                    ad.Num2 = EXPECT(TokenType.SIGNED).INT;
                }
                else if (stream.Peek().Val == "FLOAT")
                {
                    ad.ValueType = stream.Read().Val;
                    ad.Num1 = EXPECT(TokenType.DOUBLE).DOUBLE;
                    ad.Num2 = EXPECT(TokenType.DOUBLE).DOUBLE;
                }
                else if (stream.Peek().Val == "STRING")
                {
                    ad.ValueType = stream.Read().Val;
                }
                else if (stream.Peek().Val == "ENUM")
                {
                    ad.ValueType = stream.Read().Val;
                    ad.Values = new List<string>();

                    while (stream.Peek().Assert(TokenType.STRING))
                    {
                        ad.Values.Add(stream.Read().Val);

                        if (!stream.ConsumeIf(stream.Peek().Val == ","))
                            break;
                    }
                }
                else
                {
                    throw new DbcParseException();
                }

                dbc.AddAttrDefinition(ad);

                EXPECT(";");
            }
        }

        private AttributeValue AttributeValues(string name)
        {
            AttributeDefinition attr = dbc.GetAttrDefinition(name);

            if (attr == null)
                throw new DbcParseException();

            if (attr.ValueType == "ENUM" || attr.ValueType == "STRING")
                return new AttributeValue
                {
                    Val = EXPECT(TokenType.STRING).Val
                };

            return new AttributeValue
            {
                Num = EXPECT(TokenType.DOUBLE).DOUBLE
            };

        }

        private void AttributeDefaults()
        {
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.ATTRIBUTE_DEFAULTS))
            {
                string name = EXPECT(TokenType.STRING | TokenType.IDENTIFIER).Val;

                dbc.AttributeDefaults.Add(new AttributeDefault
                {
                    AttributeName = name,
                    Value = AttributeValues(name)
                });


                EXPECT(";");
            }
        }

        private void ObjAttributeValues()
        {
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.ATTRIBUTE_VALUES))
            {
                string name = EXPECT(TokenType.STRING | TokenType.IDENTIFIER).Val;

                if (stream.Peek().Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    while (stream.ConsumeIf(stream.Peek().Val != ";")) { }
                    stream.Read();

                    continue;
                }

                ObjAttributeValue oav = new ObjAttributeValue();
                dbc.AttributeValues.Add(oav);

                oav.AttributeName = name;
                oav.Type = "";

                if (stream.Peek().Val == Keyword.NODES)
                {
                    oav.Type = stream.Read().Val;
                    oav.NodeName = EXPECT(TokenType.IDENTIFIER).Val;

                }
                else if (stream.Peek().Val == Keyword.MESSAGES)
                {
                    oav.Type = stream.Read().Val;
                    oav.MsgID = EXPECT(TokenType.UNSIGNED).INT;

                }
                else if (stream.Peek().Val == Keyword.SIGNAL)
                {
                    oav.Type = stream.Read().Val;
                    oav.MsgID = EXPECT(TokenType.UNSIGNED).INT;
                    oav.SignalName = EXPECT(TokenType.IDENTIFIER).Val;

                }

                oav.Value = AttributeValues(name);

                EXPECT(";");
            }
        }

        private ValueDesc ValueDescriptions()
        {
            ValueDesc vd = new ValueDesc
            {
                Num = EXPECT(TokenType.DOUBLE).DOUBLE,
                Val = EXPECT(TokenType.STRING).Val
            };

            return vd;
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
            while (stream.ConsumeIf(stream.Peek().Val == Keyword.VALUE_DESCRIPTIONS))
            {
                SignalValueDescription vd = new SignalValueDescription
                {
                    MsgID = EXPECT(TokenType.UNSIGNED).INT,
                    Name = EXPECT(TokenType.IDENTIFIER).Val,
                    Descs = new List<ValueDesc>()
                };
                dbc.ValueDescriptions.Add(vd);

                while (!stream.ConsumeIf(stream.Peek().Val == ";"))
                {
                    vd.Descs.Add(ValueDescriptions());
                }
            }
        }

    }
}
