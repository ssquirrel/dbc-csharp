using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


using DbcLib.Model;
using DbcLib.DBC.Lex;

namespace DbcLib.DBC.Writer
{
    public class DbcWriter : IDisposable
    {
        TextWriter writer;

        public DbcWriter(TextWriter w)
        {
            writer = w;
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public void Write(Model.DBC dbc)
        {
            Version(dbc.Version);
            NewSymbols(dbc.NewSymbols);
            BitTiming();
            Nodes(dbc.Nodes);
            //ValueTables;
            Messages(dbc.Messages);
            //MessageTransmitter;
            //ENVIRONMENT_VARIABLES
            //ENVIRONMENT_VARIABLES_DATA
            //SIGNAL_TYPES
            Comments(dbc.Comments);
            AttributeDefinitions(dbc.AttributeDefinitions);
            //SIGTYPE_ATTR_LIST
            AttributeDefaults(dbc.AttributeDefaults);
            AttributeValues(dbc.AttributeValues);
            SignalValueDescriptions(dbc.ValueDescriptions);
            //CATEGORY_DEFINITIONS
            //CATEGORIES
            //FILTER
            //SIGNAL_TYPE_REFS
            //SIGNAL_GROUPS
            //SIGNAL_EXTENDED_VALUE_TYPE_LIST

            writer.Flush();
        }

        private void WriteList(IEnumerable<string> enumerable)
        {
            var iter = enumerable.GetEnumerator();

            if (!iter.MoveNext())
                return;

            writer.Write(" " + iter.Current);

            while (iter.MoveNext())
                writer.Write("," + iter.Current);
        }

        private void Version(string version)
        {
            writer.WriteLine("{0} \"{1}\"",
                Keyword.VERSION,
                version);

            writer.WriteLine();
            writer.WriteLine();
        }

        private void NewSymbols(IEnumerable<string> newSymbols)
        {
            writer.WriteLine(Keyword.NEW_SYMBOLS + " : ");
            foreach (String symbol in newSymbols)
                writer.WriteLine("\t" + symbol);
            writer.WriteLine();
        }

        private void BitTiming()
        {
            writer.WriteLine(Keyword.BIT_TIMING + ":");
            writer.WriteLine();
        }

        private void Nodes(IEnumerable<string> nodes)
        {
            writer.Write(Keyword.NODES + ":");
            foreach (String node in nodes)
            {
                writer.Write(" " + node);
            }
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine();
        }

        private void Messages(IEnumerable<Message> messages)
        {
            foreach (Message msg in messages)
            {
                writer.WriteLine("{0} {1} {2}: {3} {4}",
                    Keyword.MESSAGES,
                    msg.MsgID,
                    msg.Name,
                    msg.Size,
                    msg.Transmitter);

                foreach (Signal signal in msg.Signals)
                    Signal(signal);

                writer.WriteLine();
            }
        }

        private void
        Signal(Signal signal)
        {
            writer.Write(" {0} {1} : {2}|{3}@{4}{5} ({6},{7}) [{8}|{9}] \"{10}\" ",
                 Keyword.SIGNAL,
                 signal.Name,
                 signal.StartBit,
                 signal.SignalSize,
                 signal.ByteOrder,
                 signal.ValueType,
                 signal.Factor,
                 signal.Offset,
                 signal.Min,
                 signal.Max,
                 signal.Unit);

            if (signal.Receivers[0] == "Vector__XXX")
            {
                writer.Write("Vector__XXX");
            }
            else
            {
                WriteList(signal.Receivers);
            }

            writer.WriteLine();
        }

        private void
        Comments(IEnumerable<Comment> comments)
        {
            foreach (Comment cm in comments)
            {

                writer.Write("{0} {1} ",
                    Keyword.COMMENTS,
                    cm.Type ?? "");

                switch (cm.Type)
                {
                    case Keyword.NODES:
                    case Keyword.ENVIRONMENT_VARIABLES:

                        writer.Write(cm.Name + " ");

                        break;

                    case Keyword.MESSAGES:

                        writer.Write(cm.MsgID + " ");

                        break;

                    case Keyword.SIGNAL:

                        writer.Write(cm.MsgID + " " + cm.Name + " ");

                        break;
                }

                writer.WriteLine("\"{0}\";", cm.Val);
            }
        }

        private void
        AttributeDefinitions(IEnumerable<AttributeDefinition> ads)
        {
            foreach (AttributeDefinition ad in ads)
            {
                writer.Write("{0} {1} \"{2}\" {3} ",
                    Keyword.ATTRIBUTE_DEFINITIONS,
                    ad.ObjectType == null ? "" : ad.ObjectType + " ",
                    ad.AttributeName,
                    ad.ValueType);

                if (ad.ValueType == "ENUM")
                {
                    //assert ad.Values.Count > 0
                    writer.Write(" \"" + ad.Values[0] + "\"");

                    for (int i = 1; i < ad.Values.Count; ++i)
                        writer.Write(",\"" + ad.Values[i] + "\"");
                }
                else if (ad.ValueType != "STRING")
                {
                    writer.Write(ad.Num1 + " " + ad.Num2);
                }

                writer.WriteLine(";");
            }
        }

        private void
        AttributeDefaults(IEnumerable<AttributeDefault> ads)
        {
            foreach (AttributeDefault ad in ads)
            {
                if (ad.Value.Type == AttrValType.String)
                {
                    writer.WriteLine("{0}  \"{1}\" \"{2}\";",
                        Keyword.ATTRIBUTE_DEFAULTS,
                        ad.AttributeName,
                        ad.Value.Val);
                }
                else
                {
                    writer.WriteLine("{0}  \"{1}\" {2};",
                       Keyword.ATTRIBUTE_DEFAULTS,
                       ad.AttributeName,
                       ad.Value.Num);
                }

            }
        }

        private void
        AttributeValues(IEnumerable<ObjAttributeValue> avs)
        {
            foreach (ObjAttributeValue av in avs)
            {
                writer.Write("{0} \"{1}\"",
                    Keyword.ATTRIBUTE_VALUES,
                    av.AttributeName);

                if (av.Type != null)
                    writer.Write(" " + av.Type);

                switch (av.Type)
                {
                    case Keyword.NODES:
                    case Keyword.ENVIRONMENT_VARIABLES:

                        writer.Write(" " + av.Name);

                        break;

                    case Keyword.MESSAGES:

                        writer.Write(" " + av.MsgID);

                        break;

                    case Keyword.SIGNAL:

                        writer.Write(" " + av.MsgID + " " + av.Name);

                        break;
                }

                if (av.Value.Type == AttrValType.String)
                {
                    writer.WriteLine(" \"{0}\";", av.Value.Val);

                }
                else
                {
                    writer.WriteLine(" {0};", av.Value.Num);

                }


            }
        }


        private void
        SignalValueDescriptions(IEnumerable<SignalValueDescription> ads)
        {
            foreach (SignalValueDescription ad in ads)
            {
                writer.Write(Keyword.VALUE_DESCRIPTIONS + " " +
                    ad.MsgID + " " + ad.Name);

                foreach (ValueDesc vd in ad.Descs)
                {
                    writer.Write(" {0} \"{1}\"", vd.Num, vd.Val);
                }

                writer.WriteLine(" ;");
            }
        }
    }
}
