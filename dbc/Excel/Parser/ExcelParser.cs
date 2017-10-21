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
        private IWorkbook workbook;
        private ISheet sheet;

        private List<ParseError> errors = new List<ParseError>();

        private Model.DBC dbc = DbcTemplate.LoadTemplate("template.dbc");

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
                return new ExcelDBC(workbook, dbc);
        }

        private bool Sweep(DbcRow row)
        {
            foreach (var cell in row)
            {
                if (!cell.State)
                    errors.Add(new ParseError("", cell));
            }

            return errors.Count != 0;
        }

        private int SendType(DbcRow row)
        {
            bool cyclic = row.FixedPeriodic.Type != CellType.Blank;
            bool ifActive = row.Event.Type != CellType.Blank;
            bool TBD = row.PeriodicEvent.Type != CellType.Blank;

            if (cyclic && !ifActive && !TBD)
                return DbcTemplate.MsgSendType_Cyclic;

            if (ifActive && !cyclic && !TBD)
                return DbcTemplate.MsgSendType_IfActive;

            if (TBD && !cyclic && !ifActive)
                return 2; //?????

            errors.Add(new ParseError("", row.FixedPeriodic));

            return 0;
        }

        private Message NewMessage(DbcRow row)
        {
            Message msg = new Message();
            msg.MsgID = row.MsgID.GetHex();
            msg.Name = row.MsgName.GetIdentifier();
            msg.Size = row.MsgSize.GetUnsigned();
            msg.Transmitter = row.Transmitter.GetIdentifier();
            msg.Signals = new List<Signal>();

            string comment = row.MsgComment.GetCharString();
            int sendType = SendType(row);
            int cycleTime = sendType == DbcTemplate.MsgSendType_Cyclic ?
                row.FixedPeriodic.GetUnsigned() : 0;

            if (Sweep(row))
                return msg;

            dbc.Messages.Add(msg);

            if (sendType != DbcTemplate.MsgSendTypeDefault)
                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = DbcTemplate.Attr_MsgSendType,
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Value = new AttributeValue
                    {
                        Num = sendType
                    }
                });
            else if (cycleTime != DbcTemplate.MsgCycleTimeDefault)
                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = DbcTemplate.Attr_MsgCycleTime,
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Value = new AttributeValue
                    {
                        Num = cycleTime
                    }
                });

            if (comment.Length > 0)
                dbc.Comments.Add(new Comment
                {
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Val = comment
                });


            return msg;
        }


        private void NewSignal(DbcRow row, Message msg)
        {
            var sig = new Signal();
            sig.Name = row.SignalName.GetIdentifier();
            sig.SignalSize = row.SignalSize.GetUnsigned();
            sig.StartBit = row.BitPos.GetStartBit();

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
                dbc.Comments.Add(new Comment
                {
                    Type = Keyword.SIGNAL,
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Val = comment
                });

            if (descs.Count > 0)
                dbc.ValueDescriptions.Add(new SignalValueDescription
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
