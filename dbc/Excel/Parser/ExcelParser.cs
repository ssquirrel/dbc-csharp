using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using DbcLib.Model;
using NPOI.SS.UserModel;
using System.Collections;

namespace DbcLib.Excel.Parser
{
    class ParseError
    {
        private DbcCell cell;

        public ParseError(string msg, DbcCell cell)
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
                DbcRow row = new DbcRow(raw);

                if (row.MsgName.Type != CellType.Blank)
                {
                    parent = NewMessage(row);
                }
                else if (row.SignalName.Type != CellType.Blank)
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
                return new ExcelDBC(workbook, builder.ToDBC());
        }

        private bool Sweep(DbcRow row)
        {
            foreach (var cell in row)
            {
                if (cell.State == 0)
                    errors.Add(new ParseError("", cell));
            }

            return errors.Count != 0;
        }

        private int SendType(DbcRow row)
        {
            bool cyclic = row.FixedPeriodic.Type != CellType.Blank;
            bool ifActive = row.Event.Type != CellType.Blank;
            bool cyclicEvent = row.PeriodicEvent.Type != CellType.Blank;

            if (cyclic && !ifActive && !cyclicEvent)
                return DbcStruct.MsgST_Cyclic;

            if (ifActive && !cyclic && !cyclicEvent)
                return DbcStruct.MsgST_IfActive;

            if (cyclicEvent && !cyclic && !ifActive)
                return DbcStruct.MsgST_CyclicEvent;

            return DbcStruct.MsgST_NoSendType;
        }

        private Message NewMessage(DbcRow row)
        {
            Message msg = new Message();
            msg.MsgID = row.MsgID.GetHex();
            msg.Name = row.MsgName.GetIdentifier();
            msg.Size = row.MsgSize.GetInt();
            msg.Transmitter = row.Transmitter.GetIdentifier();
            msg.Signals = new List<Signal>();

            string comment = row.MsgComment.GetCharString();
            int sendType = SendType(row);
            int cycleTime = sendType == DbcStruct.MsgST_Cyclic ?
                row.FixedPeriodic.GetInt() : 0;

            if (Sweep(row))
                return msg;

            builder.Messages.Add(msg);

            if (comment.Length > 0)
                builder.Comments.Add(new Comment
                {
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Val = comment
                });

            builder.SendTypes.Add(new ObjAttributeValue
            {
                AttributeName = DbcStruct.Attr_MsgCycleTime,
                ObjType = Keyword.MESSAGES,
                MsgID = msg.MsgID,
                Value = new AttributeValue
                {
                    Num = sendType
                }
            });

            if (cycleTime != 0)
                builder.SendTypes.Add(new ObjAttributeValue
                {
                    AttributeName = DbcStruct.Attr_MsgCycleTime,
                    ObjType = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Value = new AttributeValue
                    {
                        Num = cycleTime
                    }
                });


            return msg;
        }


        private void NewSignal(DbcRow row, Message msg)
        {
            var sig = new Signal();
            sig.Name = row.SignalName.GetIdentifier();
            sig.SignalSize = row.SignalSize.GetInt();
            sig.StartBit = row.BitPos.GetInt();

            sig.Unit = row.Unit.GetCharString();
            sig.Factor = row.Factor.GetDouble();
            sig.Offset = row.Offset.GetDouble();
            sig.Min = row.PhysicalMin.GetDouble();
            sig.Max = row.PhysicalMax.GetDouble();
            sig.Receivers = row.Receiver.GetReceiver();

            sig.ByteOrder = "0";
            sig.ValueType = "+";

            var comment = row.DetailedMeaning.GetCharString();
            var descs = row.State.GetValueDescs();

            if (msg == null)
            {
                errors.Add(new ParseError("a sig without parent", row.Transmitter));
            }

            if (Sweep(row))
                return;

            msg.Signals.Add(sig);

            if (comment.Length > 0)
                builder.Comments.Add(new Comment
                {
                    Type = Keyword.SIGNAL,
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Val = comment
                });

            if (descs.Count > 0)
                builder.ValueDescriptions.Add(new SignalValueDescription
                {
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Descs = descs
                });
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
