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

        private ValueDescription ValueDescriptions()
        {
            ValueDescription vd = new ValueDescription
            {
                num = stream.Consume(TokenType.DOUBLE).Val,
                str = stream.Consume(TokenType.STRING).Val
            };

            return vd;
        }

        private void Version()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.VERSION))
                return;

            dbc.version = stream.Consume(TokenType.STRING).Val;
        }

        private void NewSymbols()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.NEW_SYMBOLS))
                return;

            stream.Consume(":");

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

            stream.Consume(":");

            while (stream.ConsumeIf(stream.Curr.Val != Keyword.NODES)) { }
        }

        //'BU_' ':' {node_name} ;
        //node_name = C_identifier
        private void Nodes()
        {
            if (!stream.ConsumeIf(stream.Curr.Val == Keyword.NODES))
                throw new Exception();

            stream.Consume(":");

            while (stream.Curr.IsIdentifier())
            {
                dbc.nodes.Add(stream.Consume().Val);
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

                vt.name = stream.Consume(TokenType.IDENTIFIER).Val;

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
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.MESSAGES))
            {
                Message msg = new Message();
                dbc.messages.Add(msg);

                msg.id = stream.Consume(TokenType.UNSIGNED).Val;
                msg.name = stream.Consume(TokenType.IDENTIFIER).Val;

                stream.Consume(":");

                msg.size = stream.Consume(TokenType.UNSIGNED).Val;
                msg.transmitter = stream.Consume(TokenType.IDENTIFIER).Val;

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

            signal.name = stream.Consume(TokenType.IDENTIFIER).Val;

            if (stream.Curr.Val == "M")
            {
                signal.multiplexerIndicator = stream.Consume().Val;
            }
            else if (stream.ConsumeIf(stream.Curr.Val == "m"))
            {
                signal.multiplexerIndicator =
                    stream.Consume(TokenType.UNSIGNED).Val;
            }

            stream.Consume(":");
            signal.startBit = stream.Consume(TokenType.UNSIGNED).Val;
            stream.Consume("|");
            signal.signalSize = stream.Consume(TokenType.UNSIGNED).Val;
            stream.Consume("@");
            signal.byteOrder = stream.Consume(new string[] { "0", "1" }).Val;
            signal.valueType = stream.Consume(new string[] { "+", "-" }).Val;

            stream.Consume("(");
            signal.factor = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume(",");
            signal.offset = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume(")");

            stream.Consume("[");
            signal.min = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume("|");
            signal.max = stream.Consume(TokenType.DOUBLE).Val;
            stream.Consume("]");

            signal.unit = stream.Consume(TokenType.STRING).Val;

            do
            {
                signal.receivers.Add(stream.Consume(TokenType.IDENTIFIER).Val);

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
                    dbc.comments.Add(new Comment
                    {
                        msg = stream.Consume().Val
                    });
                }
                else if (stream.Curr.Val == Keyword.NODES ||
                    stream.Curr.Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    dbc.comments.Add(new Comment
                    {
                        type = stream.Consume().Val,
                        name = stream.Consume(TokenType.IDENTIFIER).Val,
                        msg = stream.Consume(TokenType.STRING).Val
                    });
                }
                else if (stream.Curr.Val == Keyword.MESSAGES)
                {
                    dbc.comments.Add(new Comment
                    {
                        type = stream.Consume().Val,
                        id = stream.Consume(TokenType.UNSIGNED).Val,
                        msg = stream.Consume(TokenType.STRING).Val
                    });
                }
                else if (stream.Curr.Val == Keyword.SIGNAL)
                {
                    dbc.comments.Add(new Comment
                    {
                        type = stream.Consume().Val,
                        id = stream.Consume(TokenType.UNSIGNED).Val,
                        name = stream.Consume(TokenType.IDENTIFIER).Val,
                        msg = stream.Consume(TokenType.STRING).Val
                    });
                }
                else
                {
                    throw new Exception();
                }

                stream.Consume(";");
            }
        }

        private void AttributeDefinitions()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_DEFINITIONS))
            {
                AttributeDefinition ad = new AttributeDefinition();
                dbc.attributeDefinitions.Add(ad);

                switch (stream.Curr.Val)
                {
                    case Keyword.NODES:
                    case Keyword.MESSAGES:
                    case Keyword.SIGNAL:
                    case Keyword.ENVIRONMENT_VARIABLES:
                        ad.objectType = stream.Consume().Val;
                        break;
                }

                ad.attributeName = stream.Consume(TokenType.STRING).Val;

                if (stream.Curr.Val == "INT" ||
                    stream.Curr.Val == "HEX")
                {
                    ad.valueType = stream.Consume().Val;
                    ad.values.Add(stream.Consume(TokenType.SIGNED).Val);
                    ad.values.Add(stream.Consume(TokenType.SIGNED).Val);
                }
                else if (stream.Curr.Val == "FLOAT")
                {
                    ad.valueType = stream.Consume().Val;
                    ad.values.Add(stream.Consume(TokenType.DOUBLE).Val);
                    ad.values.Add(stream.Consume(TokenType.DOUBLE).Val);
                }
                else if (stream.Curr.Val == "STRING")
                {
                    ad.valueType = stream.Consume().Val;
                }
                else if (stream.Curr.Val == "ENUM")
                {
                    ad.valueType = stream.Consume().Val;

                    while (stream.Curr.Type == TokenType.STRING)
                    {
                        ad.values.Add(stream.Consume().Val);

                        if (!stream.ConsumeIf(stream.Curr.Val == ","))
                            break;
                    }
                }
                else
                {
                    throw new Exception();
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
                    attributeName = stream.Consume(TokenType.STRING).Val,
                    attributeValue = stream.Consume(TokenType.DOUBLE | TokenType.STRING).Val
                };

                if (!Lexer.IsIdentifier(ad.attributeName.Trim('"')))
                    throw new Exception();

                dbc.attributeDefaults.Add(ad);

                stream.Consume(";");
            }
        }

        private void AttributeValues()
        {
            while (stream.ConsumeIf(stream.Curr.Val == Keyword.ATTRIBUTE_VALUES))
            {
                AttributeValue av = new AttributeValue();
                dbc.attributeValues.Add(av);

                av.attributeName = stream.Consume(TokenType.STRING).Val;

                if (!Lexer.IsIdentifier(av.attributeName.Trim('"')))
                    throw new Exception();

                if (stream.Curr.Val == Keyword.NODES ||
                    stream.Curr.Val == Keyword.ENVIRONMENT_VARIABLES)
                {
                    av.type = stream.Consume().Val;
                    av.name = stream.Consume(TokenType.IDENTIFIER).Val;
                }
                else if (stream.Curr.Val == Keyword.MESSAGES)
                {
                    av.type = stream.Consume().Val;
                    av.messageId = stream.Consume(TokenType.UNSIGNED).Val;
                }
                else if (stream.Curr.Val == Keyword.SIGNAL)
                {
                    av.type = stream.Consume().Val;
                    av.messageId = stream.Consume(TokenType.UNSIGNED).Val;
                    av.name = stream.Consume(TokenType.IDENTIFIER).Val;
                }

                av.attributeValue = stream.Consume(TokenType.DOUBLE | TokenType.STRING).Val;

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
                SignalValueDescription vd = new SignalValueDescription();
                dbc.valueDescriptions.Add(vd);

                if (stream.Curr.IsUnsigned())
                    vd.messageId = stream.Consume().Val;

                vd.name = stream.Consume(TokenType.IDENTIFIER).Val;

                while (!stream.ConsumeIf(stream.Curr.Val == ";"))
                {
                    vd.descriptions.Add(ValueDescriptions());
                }
            }
        }

    }
}
