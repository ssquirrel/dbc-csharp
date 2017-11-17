using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using DbcLib.Model;
using DbcLib.Excel.Sheet;
using NPOI.SS.UserModel;

namespace DbcLib.Excel.Parser
{
    class ParseError
    {
        private ReadingCell cell;

        public ParseError(string msg, ReadingCell cell)
        {
            this.cell = cell;
        }
    }

    public class ExcelDBC : IDisposable
    {
        internal ExcelDBC(IWorkbook wb, Model.DBC dbc)
        {
            Workbook = wb;
            DBC = dbc;
        }

        internal ExcelDBC(IWorkbook wb, IReadOnlyList<ParseError> err)
        {
            Workbook = wb;
            Errors = err;
        }

        public Model.DBC DBC { get; }

        internal IWorkbook Workbook { get; }
        internal IReadOnlyList<ParseError> Errors { get; }

        public void Dispose()
        {
            Workbook.Close();
        }
    }

    public class ExcelParser
    {
        private DbcBuilder builder = new DbcBuilder();

        private IWorkbook workbook;
        private ISheet sheet;

        private List<ParseError> errors = new List<ParseError>();

        private ExcelParser(string fn, string sn)
        {
            FileStream stream = File.Open(fn,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            workbook = WorkbookFactory.Create(stream);

            sheet = workbook.GetSheet(sn);
            if (sheet == null)
            {
                throw new ArgumentException(fn + " does not contain a sheet named " + sn);
            }
        }

        public static ExcelDBC Parse(string fn, string sheet)
        {
            ExcelParser parser = new ExcelParser(fn, sheet);

            return parser.Parse();
        }

        public ExcelDBC Parse()
        {
            var rows = (new EnumerableSheet(sheet)).Skip(2);

            return ParseImpl(rows);
        }

        private ExcelDBC ParseImpl(IEnumerable<IRow> rows)
        {
            Message parent = null;
            foreach (IRow raw in rows)
            {
                ReadingRow row = new ReadingRow(raw);

                if (!row.MsgName.IsEmpty)
                {
                    parent = NewMessage(row);
                }
                else if (!row.SignalName.IsEmpty)
                {
                    NewSignal(row, parent);
                }
                else
                {
                    //errors.Add(new ParseError(""))
                }
            }

            if (errors.Count > 0)
                return new ExcelDBC(workbook, errors);
            else
                return new ExcelDBC(workbook, builder.DBC);
        }

        private bool Sweep(ReadingRow row)
        {
            foreach (var cell in row)
            {
                if (cell.State != 0)
                    errors.Add(new ParseError("", cell));
            }

            return errors.Count != 0;
        }

        private MsgSendTypeEnum SendType(ReadingRow row)
        {
            bool cyclic = row.MsgCycleTime.IsEmpty;
            bool ifActive = row.Event.IsEmpty;
            bool cyclicEvent = row.PeriodicEvent.IsEmpty;

            if (!cyclic && ifActive && cyclicEvent)
                return MsgSendTypeEnum.Cyclic;

            if (!ifActive && cyclic && cyclicEvent)
                return MsgSendTypeEnum.IfActive;

            if (!cyclicEvent && cyclic && ifActive)
                return MsgSendTypeEnum.CyclicEvent;

            return MsgSendTypeEnum.NoMsgSendType;
        }

        private Message NewMessage(ReadingRow row)
        {
            Message msg = new Message();
            msg.MsgID = row.MsgID.Hex();
            msg.Name = row.MsgName.Identifier();
            msg.Size = row.MsgSize.Int();
            msg.Transmitter = row.Transmitter.Identifier();
            msg.Signals = new List<Signal>();

            string comment = row.MsgComment.CharString();
            var type = SendType(row);
            var time = row.MsgCycleTime.Int(type == MsgSendTypeEnum.Cyclic);

            if (Sweep(row))
                return msg;

            builder.NewMessage(msg)
                .Comment(comment)
                .SendType(type)
                .CycleTime(time);

            return msg;
        }


        private void NewSignal(ReadingRow row, Message msg)
        {
            var sig = new Signal();
            sig.Name = row.SignalName.Identifier();
            sig.StartBit = row.StartBit.Int();
            sig.SignalSize = row.SizeInBits.Int();
            sig.ByteOrder = row.ByteOrder.ByteOrder();
            sig.ValueType = row.ValueType.ValueType();
            sig.Unit = row.Unit.CharString();
            sig.Factor = row.Factor.Double();
            sig.Offset = row.Offset.Double();
            sig.Min = row.PhysicalMin.Double();
            sig.Max = row.PhysicalMax.Double();
            sig.Receivers = row.Receiver.Receivers();

            var comment = row.SigComment.CharString();
            var descs = row.ValueDescs.ValueDescs();
            var startVal = row.SigStartValue.Int(!row.SigStartValue.IsEmpty);

            if (msg == null)
            {
                errors.Add(new ParseError("a sig without parent", row.Transmitter));
            }

            if (Sweep(row))
                return;

            builder.NewSignal(msg, sig)
                .Comment(comment)
                .ValueDescs(descs)
                .StartVal(startVal);

        }
    }


    class EnumerableSheet : IEnumerable<IRow>
    {
        private ISheet sheet;

        public EnumerableSheet(ISheet sheet)
        {
            this.sheet = sheet;
        }

        public IEnumerator<IRow> GetEnumerator()
        {
            foreach (IRow row in sheet)
                yield return row;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
