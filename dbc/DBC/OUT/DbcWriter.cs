using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;

namespace DbcLib.DBC.Out
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
            Version(dbc.version);
            NewSymbols(dbc.newSymbols);
            BitTiming();
            Nodes(dbc.nodes);
            ValueTables(dbc.valueTables);
            Messages(dbc.messages);
            //MessageTransmitter;
            //ENVIRONMENT_VARIABLES
            //ENVIRONMENT_VARIABLES_DATA
            //SIGNAL_TYPES
            Comments(dbc.comments);
            AttributeDefinitions(dbc.attributeDefinitions);
            //SIGTYPE_ATTR_LIST
            AttributeDefaults(dbc.attributeDefaults);
            AttributeValues(dbc.attributeValues);
            SignalValueDescriptions(dbc.valueDescriptions);
            //CATEGORY_DEFINITIONS
            //CATEGORIES
            //FILTER
            //SIGNAL_TYPE_REFS
            //SIGNAL_GROUPS
            //SIGNAL_EXTENDED_VALUE_TYPE_LIST
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
            writer.WriteLine(Keyword.VERSION + " " + version);
            writer.WriteLine();
            writer.WriteLine();
        }

        private void NewSymbols(List<string> newSymbols)
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

        private void Nodes(List<string> nodes)
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

        private void ValueTables(List<ValueTable> valueTables)
        {

        }

        private void Messages(List<Message> messages)
        {
            foreach (Message msg in messages)
            {
                writer.WriteLine(Keyword.MESSAGES + " " + msg.id + " " +
                    msg.name + ": " + msg.size + " " +
                    msg.transmitter);

                foreach (Signal signal in msg.signals)
                    Signal(signal);

                writer.WriteLine();
            }
        }

        private void
        Signal(Signal signal)
        {
            writer.Write(" " + Keyword.SIGNAL + " " + signal.name);

            if (signal.multiplexerIndicator == null)
                writer.Write(" ");
            else if (signal.multiplexerIndicator == "M")
                writer.Write(" M");
            else
                writer.Write(" m" + signal.multiplexerIndicator);

            writer.Write(": " + signal.startBit + "|" + signal.signalSize +
                "@" + signal.byteOrder + signal.valueType + " " +
                "(" + signal.factor + "," + signal.offset + ") [" +
                signal.min + "|" + signal.max + "]" + " " +
                signal.unit + " ");

            if (signal.receivers[0] == "Vector__XXX")
            {
                writer.Write("Vector__XXX");
            }
            else
            {
                WriteList(signal.receivers);
            }

            writer.WriteLine();
        }

        private void
        Comments(List<Comment> comments)
        {
            foreach (Comment cm in comments)
            {
                writer.Write(Keyword.COMMENTS + " ");

                if (cm.type != null)
                {
                    writer.Write(cm.type + " ");

                    if (cm.type == Keyword.MESSAGES ||
                        cm.type == Keyword.SIGNAL)
                    {

                        writer.Write(cm.id + " ");
                    }

                    if (cm.type == Keyword.SIGNAL ||
                        cm.type == Keyword.NODES ||
                        cm.type == Keyword.ENVIRONMENT_VARIABLES)
                    {

                        writer.Write(cm.name + " ");
                    }
                }

                writer.WriteLine(cm.msg + ";");
            }
        }

        private void
        AttributeDefinitions(List<AttributeDefinition> ads)
        {
            foreach (AttributeDefinition ad in ads)
            {
                writer.Write(Keyword.ATTRIBUTE_DEFINITIONS + " " +
                    (ad.objectType != null ? ad.objectType + "  " : " ") +
                    ad.attributeName + " " + ad.valueType + " ");

                if (ad.valueType == "ENUM")
                {
                    WriteList(ad.values);
                }
                else if(ad.valueType != "STRING")
                {
                    writer.Write(ad.values[0] + " " + ad.values[1]);
                }

                writer.WriteLine(";");
            }
        }

        private void
        AttributeDefaults(List<AttributeDefault> ads)
        {
            foreach (AttributeDefault ad in ads)
            {
                writer.WriteLine(Keyword.ATTRIBUTE_DEFAULTS + "  " +
                    ad.attributeName + " " + ad.attributeValue + ";");
            }
        }

        private void
        AttributeValues(List<AttributeValue> avs)
        {
            foreach (AttributeValue av in avs)
            {
                writer.Write(Keyword.ATTRIBUTE_VALUES + " " +
                    av.attributeName + " ");

                if (av.type != null)
                {
                    writer.Write(av.type + " ");

                    if (av.type == Keyword.MESSAGES ||
                            av.type == Keyword.SIGNAL)
                    {

                        writer.Write(av.messageId + " ");
                    }

                    if (av.type == Keyword.SIGNAL ||
                        av.type == Keyword.NODES ||
                        av.type == Keyword.ENVIRONMENT_VARIABLES)
                    {

                        writer.Write(av.name + " ");
                    }

                }

                writer.WriteLine(av.attributeValue + ";");
            }
        }


        private void
        SignalValueDescriptions(List<SignalValueDescription> ads)
        {
            foreach (SignalValueDescription ad in ads)
            {
                writer.Write(Keyword.VALUE_DESCRIPTIONS + " " +
                    (ad.messageId != null ? ad.messageId + " " : "") +
                    ad.name);

                foreach (ValueDescription vd in ad.descriptions)
                {
                    writer.Write(" " + vd.num + " " + vd.str);
                }

                writer.WriteLine(" ;");
            }
        }
    }
}
