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
        private static readonly Token[] MESSAGES_PATTERN = new Token[]
        {
            new Token(Keyword.MESSAGES),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER),
            new Token(":"),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER)
        };

        private static readonly Token[] SIGNAL_PATTERN1 = new Token[]
        {
            new Token(":"),
            new Token(TokenType.UNSIGNED),
            new Token("|"),
            new Token(TokenType.UNSIGNED),
            new Token("@")};

        private static readonly Token[] SIGNAL_PATTERN2 = new Token[]
        {
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


        private static readonly Token[] CM_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token(";")
        };

        private static readonly Token[] CM_NODES_PATTERN = new Token[]
        {
            new Token(Keyword.NODES),
            new Token(TokenType.IDENTIFIER),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] CM_MESSAGES_PATTERN = new Token[]
        {
            new Token(Keyword.MESSAGES),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] CM_SIGNAL_PATTERN = new Token[]
        {
            new Token(Keyword.SIGNAL),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER),
            new Token(TokenType.STRING),
            new Token(";")};

        private static readonly Token[] CM_ENV_PATTERN = new Token[]
        {
            new Token(Keyword.SIGNAL),
            new Token(TokenType.IDENTIFIER),
            new Token(TokenType.STRING),
            new Token(";")
        };


        private static readonly Token[] AD_INT_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token("INT"),
            new Token(TokenType.SIGNED),
            new Token(TokenType.SIGNED),
            new Token(";")
        };

        private static readonly Token[] AD_HEX_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token("HEX"),
            new Token(TokenType.SIGNED),
            new Token(TokenType.SIGNED),
            new Token(";")
        };

        private static readonly Token[] AD_FLOAT_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token("FLOAT"),
            new Token(TokenType.DOUBLE),
            new Token(TokenType.DOUBLE),
            new Token(";")
        };

        private static readonly Token[] AD_STRING_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token("STRING"),
            new Token(";")
        };

        private static readonly Token[] AD_ENUM_PATTERN = new Token[]
        {
            new Token(TokenType.STRING),
            new Token("ENUM")
        };

        private static readonly Token[] AV_NODES_PATTERN = new Token[]
        {
            new Token(Keyword.NODES),
            new Token(TokenType.IDENTIFIER),
        };

        private static readonly Token[] AV_MESSAGES_PATTERN = new Token[]
        {
            new Token(Keyword.MESSAGES),
            new Token(TokenType.UNSIGNED),
        };

        private static readonly Token[] AV_SIGNAL_PATTERN = new Token[]
        {
            new Token(Keyword.SIGNAL),
            new Token(TokenType.UNSIGNED),
            new Token(TokenType.IDENTIFIER)
        };

        private static readonly Token[] AV_ENV_PATTERN = new Token[]
        {
            new Token(Keyword.ENVIRONMENT_VARIABLES),
            new Token(TokenType.IDENTIFIER)
        };

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

            using(Lexer lexer = new Lexer(filename))
            {
                List<Token> tokens = lexer.Lex();

                stream = new TokenStream(tokens);
            }

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
            JumpToComments();

            Comments();
            AttributeDefinitions();
            //SIGTYPE_ATTR_LIST
            AttributeDefaults();
            AttributeValues();
            SignalValueDescriptions();
            //CATEGORY_DEFINITIONS
            //CATEGORIES
            //FILTER
            //SIGNAL_TYPE_REFS
            //SIGNAL_GROUPS
            //SIGNAL_EXTENDED_VALUE_TYPE_LIST
            return dbc;
        }

        private Tuple<Token[], Token[]> TryParse(params Token[][] patterns)
        {
            foreach (Token[] pattern in patterns)
            {
                Token[] parsed = stream.ConsumeIf(pattern);

                if (parsed.Last() != null)
                    return new Tuple<Token[], Token[]>(pattern, parsed);
            }

            return new Tuple<Token[], Token[]>(null, null);
        }

        private ValueDescription ValueDescriptions()
        {
            ValueDescription vd = new ValueDescription();

            if (stream.Curr.IsDouble())
                vd.num = stream.Consume().Val;
            else
                throw new Exception();

            if (stream.Curr.IsString())
                vd.str = stream.Consume().Val;
            else
                throw new Exception();

            return vd;
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

                while (!stream.ConsumeIf(stream.Curr.Val == ";"))
                {
                    vt.descriptions.Add(ValueDescriptions());
                }
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
                Token[] parsed = stream.ConsumeIf(MESSAGES_PATTERN);

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
                Token[] parsed = stream.ConsumeIf(SIGNAL_PATTERN1);

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
                Token[] parsed = stream.ConsumeIf(SIGNAL_PATTERN2);

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

        private void JumpToComments()
        {
            while (stream.ConsumeIf(stream.Curr.Val != Keyword.COMMENTS)) { }
        }

        private void Comments()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.COMMENTS))
            {
                Tuple<Token[], Token[]> result = TryParse(
                    CM_PATTERN,
                    CM_NODES_PATTERN,
                    CM_MESSAGES_PATTERN,
                    CM_SIGNAL_PATTERN
                    );

                Token[] pattern = result.Item1;
                Token[] parsed = result.Item2;

                if (pattern == CM_PATTERN)
                {
                    dbc.comments.Add(new Comment
                    {
                        msg = parsed[0].Val
                    });
                }
                else if (pattern == CM_NODES_PATTERN ||
                    pattern == CM_ENV_PATTERN)
                {
                    dbc.comments.Add(new Comment
                    {
                        type = parsed[0].Val,
                        name = parsed[1].Val,
                        msg = parsed[2].Val
                    });
                }
                else if (pattern == CM_MESSAGES_PATTERN)
                {
                    dbc.comments.Add(new Comment
                    {
                        type = parsed[0].Val,
                        id = parsed[1].Val,
                        msg = parsed[2].Val
                    });
                }
                else if (pattern == CM_SIGNAL_PATTERN)
                {
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
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_DEFINITIONS))
            {
                AttributeDefinition ad = new AttributeDefinition();

                switch (stream.Curr.Val)
                {
                    case Keyword.NODES:
                    case Keyword.MESSAGES:
                    case Keyword.SIGNAL:
                    case Keyword.ENVIRONMENT_VARIABLES:
                        ad.objectType = stream.Consume().Val;
                        break;
                }

                Tuple<Token[], Token[]> result = TryParse(
                    AD_INT_PATTERN,
                    AD_HEX_PATTERN,
                    AD_FLOAT_PATTERN,
                    AD_STRING_PATTERN,
                    AD_ENUM_PATTERN
                    );

                Token[] pattern = result.Item1;
                Token[] parsed = result.Item2;

                if (pattern == null)
                {
                    throw new Exception();
                }

                ad.attributeName = parsed[0].Val;
                ad.valueType = parsed[1].Val;

                if (pattern == AD_INT_PATTERN ||
                    pattern == AD_HEX_PATTERN ||
                    pattern == AD_FLOAT_PATTERN)
                {
                    ad.values.Add(parsed[2].Val);
                    ad.values.Add(parsed[3].Val);
                }
                else if (pattern == AD_ENUM_PATTERN)
                {

                    while (!stream.ConsumeIf(stream.Curr.Val == ";"))
                    {
                        if (stream.ConsumeIf(stream.Curr.Val == ","))
                            continue;

                        if (stream.Curr.IsString())
                            ad.values.Add(stream.Consume().Val);
                        else
                            throw new Exception();
                    }
                }

            }
        }

        private void AttributeDefaults()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_DEFAULTS))
            {
                AttributeDefault ad = new AttributeDefault();
                dbc.attributeDefaults.Add(ad);

                if (stream.Curr.IsString())
                    ad.attributeName = stream.Consume().Val;
                else
                    throw new Exception();

                if (stream.Curr.IsDouble() || stream.Curr.IsString())
                    ad.attributeValue = stream.Consume().Val;
                else
                    throw new Exception();

                if (!stream.ConsumeIf(stream.Curr.Val == ";"))
                    throw new Exception();
            }
        }

        private void AttributeValues()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_VALUES))
            {
                AttributeValue av = new AttributeValue();
                dbc.attributeValues.Add(av);

                if (stream.Curr.IsString())
                    av.attributeName = stream.Consume().Val;
                else
                    throw new Exception();

                Tuple<Token[], Token[]> result = TryParse(
                   AV_NODES_PATTERN,
                   AV_MESSAGES_PATTERN,
                   AV_SIGNAL_PATTERN,
                   AV_ENV_PATTERN
                   );

                Token[] pattern = result.Item1;
                Token[] parsed = result.Item2;

                if (pattern != null)
                {
                    av.type = parsed[0].Val;

                    if (pattern == AV_NODES_PATTERN || pattern == AV_ENV_PATTERN)
                    {
                        av.name = parsed[1].Val;
                    }
                    else if (pattern == AV_MESSAGES_PATTERN)
                    {
                        av.messageId = parsed[1].Val;
                    }
                    else if (pattern == AV_SIGNAL_PATTERN)
                    {
                        av.messageId = parsed[1].Val;
                        av.name = parsed[2].Val;
                    }
                }

                if (stream.Curr.IsDouble() || stream.Curr.IsString())
                    av.attributeValue = stream.Consume().Val;
                else
                    throw new Exception();

                if (!stream.ConsumeIf(stream.Curr.Val == ";"))
                    throw new Exception();
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
                SignalValueDescription vd = new SignalValueDescription();
                dbc.valueDescriptions.Add(vd);

                if (stream.Curr.IsUnsigned())
                {
                    vd.messageId = stream.Consume().Val;
                }

                if (stream.Curr.IsIdentifier())
                {
                    vd.name = stream.Consume().Val;
                }
                else
                {
                    throw new Exception();
                }

                while (!stream.ConsumeIf(stream.Curr.Val == ";"))
                {
                    vd.descriptions.Add(ValueDescriptions());
                }
            }
        }

    }
}
