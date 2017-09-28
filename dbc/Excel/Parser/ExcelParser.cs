using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using DbcLib.Excel.Reader;
using DbcLib.Model;
using DbcLib.DBC.Lex;
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
            Model.DBC dbc = DbcTemplate.NewDbcObject();

            Message parent = null;
            foreach (var row in sheet)
            {
                var parsed = parser.Parse(row);

                if (Errors.Count > 0)
                    continue;

                switch (row.RowType)
                {
                    case RowType.Msg:
                        parent = NewMessage(parsed, dbc);
                        break;
                    case RowType.Signal:
                        NewSignal(parsed, parent, dbc);
                        break;
                }
            }

            return dbc;
        }

        private Message NewMessage(ParsedRow row, Model.DBC dbc)
        {
            Message msg = new Message
            {
                MsgID = row.MsgID,
                Name = row.MsgName,
                Size = row.MsgSize,
                Transmitter = row.Transmitter,
                Signals = new List<Signal>()
            };
            dbc.Messages.Add(msg);

            var st = DbcTemplate.NewMsgSendType(msg.MsgID, row.MsgSendType);

            if (st != null)
            {
                dbc.AttributeValues.Add(st);
            }
            else
            {
                var ct = DbcTemplate.NewMsgCycleTime(msg.MsgID, row.MsgCycleTime);

                if (ct != null)
                    dbc.AttributeValues.Add(ct);
            }


            if (row.MsgComment.Length > 0)
                dbc.Comments.Add(new Comment
                {
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Val = row.MsgComment
                });

            return msg;
        }

        private static void
        NewSignal(ParsedRow row, Message msg, Model.DBC dbc)
        {
            msg.Signals.Add(new Signal
            {
                Name = row.SignalName,
                StartBit = row.StartBit,
                SignalSize = row.SignalSize,
                ByteOrder = row.ByteOrder,
                ValueType = row.ValueType,
                Factor = row.Factor,
                Offset = row.Offset,
                Min = row.Min,
                Max = row.Max,
                Unit = row.Unit,
                Receivers = row.Receiver
            });

            if (row.SignalComment.Length > 0)
                dbc.Comments.Add(new Comment
                {
                    Type = Keyword.SIGNAL,
                    MsgID = msg.MsgID,
                    SignalName = row.SignalName,
                    Val = row.SignalComment
                });

            if (row.SignalValDescs.Count > 0)
                dbc.ValueDescriptions.Add(new SignalValueDescription
                {
                    MsgID = msg.MsgID,
                    Name = row.SignalName,
                    Descs = row.SignalValDescs
                });
        }
    }
}
