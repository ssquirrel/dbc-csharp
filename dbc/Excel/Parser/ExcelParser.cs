using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using DbcLib.Excel.Reader;
using DbcLib.Model;
using DbcLib.DBC.Parser;
using System.Collections;
using NPOI.SS.UserModel;

namespace DbcLib.Excel.Parser
{
    public class ExcelParser
    {
        private RowParser parser = new RowParser();

        public ExcelParser()
        {
            //workbook = new DbcWorkbook(fn);
        }

        public IReadOnlyCollection<ParseError> Errors => parser.Errors;

        public Model.DBC Parse(DbcSheet sheet)
        {
            Model.DBC dbc = DbcTemplate.LoadTemplate("Template.dbc");

            Message parent = null;
            foreach (var row in sheet)
            {
                if (row.RowType == RowType.Msg)
                {
                    parent = NewMessage(row, dbc);
                }
                else if (row.RowType == RowType.Signal)
                {
                    NewSignal(row, parent, dbc);
                }

            }

            return dbc;
        }

        private Message NewMessage(DbcRow row, Model.DBC dbc)
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
                cycleTime = parser.Unsigned(row.FixedPeriodic) : 0;

            if (parser.Errors.Any())
                return null;

            dbc.Messages.Add(msg);

            if (sendType == DbcTemplate.MsgSendTypeDefault)
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
                    AttributeName = DbcTemplate.Attr_MsgSendType,
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

        private void NewSignal(DbcRow row, Message msg, Model.DBC dbc)
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

            var comment = parser.String(row.SignalComment);
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
