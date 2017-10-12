using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;
using NPOI.SS.UserModel;
using System.IO;

namespace DbcLib.Excel.Parser
{
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

        private CellParser parser = new CellParser();

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
            List<DbcRow> rows = new List<DbcRow>();

            int skip = 0;

            foreach (IRow raw in sheet)
            {
                if (skip < 2)
                {
                    ++skip;
                    continue;
                }

                rows.Add(new DbcRow(raw));
            }

            return ParseImpl(rows);
        }

        private ExcelDBC ParseImpl(List<DbcRow> rows)
        {
            Message parent = null;

            foreach (var row in rows)
            {
                if (row.Type == RowType.Msg)
                {
                    parent = NewMessage(row);
                }
                else if (row.Type == RowType.Signal)
                {
                    if (parent != null)
                        NewSignal(row, parent);
                }
            }

            if (parser.Errors.Count > 0)
                return new ExcelDBC(workbook, parser.Errors);
            else
                return new ExcelDBC(workbook, dbc);
        }

        private Message NewMessage(DbcRow row)
        {
            Message msg = new Message();
            msg.MsgID = parser.Hex(row.MsgID);
            msg.Name = parser.Identifier(row.MsgName);
            msg.Size = parser.Unsigned(row.MsgSize);
            msg.Transmitter = parser.Identifier(row.Transmitter);
            msg.Signals = new List<Signal>();

            string comment = parser.String(row.MsgComment);
            int sendType = parser.MsgSendType(row);
            int cycleTime = sendType == DbcTemplate.MsgSendType_Cyclic ?
                parser.Unsigned(row.FixedPeriodic) : 0;

            if (parser.Errors.Any())
                return null;

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
            sig.Name = parser.Identifier(row.SignalName);
            sig.SignalSize = parser.Unsigned(row.SignalSize);
            sig.StartBit = parser.StartBit(row.BitPos);

            sig.Unit = parser.String(row.Unit);
            sig.Factor = parser.Double(row.Factor);
            sig.Offset = parser.Double(row.Offset);
            sig.Min = parser.Double(row.PhysicalMin);
            sig.Max = parser.Double(row.PhysicalMax);
            sig.Receivers = parser.Receiver(row.Receiver);

            sig.ByteOrder = "0";
            sig.ValueType = "+";

            var comment = parser.String(row.DetailedMeaning);
            var descs = parser.SignalValDescs(row.State);

            if (parser.Errors.Any())
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



}
