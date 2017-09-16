using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Excel.Reader;
using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;

namespace DbcLib.Excel
{
    class ParseError
    {
        public ParseError(int row, int col, string msg)
        {
            Row = row;
            Col = col;
            Msg = msg;
        }

        public int Row { get; private set; } = 0;
        public int Col { get; private set; } = 0;
        public string Msg { get; private set; } = "";
    }


    static class DbcTemplate
    {
        public static readonly string MsgSendType = "\"GenMsgSendType\"";
        public static readonly string MsgCycleTime = "\"GenMsgCycleTime\"";
        public static readonly string SigStartValue = "\"GenSigStartValue\"";

        public static
        AttributeValue NewMsgCycleTime(string id, string val)
        {
            return new AttributeValue
            {
                attributeName = MsgCycleTime,
                type = Keyword.MESSAGES,
                messageId = id,
                attributeValue = val
            }; ;
        }
    }

    class ExcelParser : IDisposable
    {
        private List<ParseError> errors = new List<ParseError>();

        private DbcWorkbook workbook;

        DBC.Model.DBC dbc;

        public ExcelParser(String file)
        {
            workbook = new DbcWorkbook(file);
        }

        public void Dispose()
        {
            workbook.Dispose();
        }

        DBC.Model.DBC Parse()
        {
            if (dbc != null)
                return null;

            dbc = new DBC.Model.DBC();

            DbcSheet sheet = workbook.Curr;

            while (sheet.EndOfStream)
            {
                Cell[] row = sheet.Consume();

                if (row[DbcExcelRow.MsgName] == DbcExcelRow.EmptyCell)
                    continue;

                ParseMessage();
            }

            return dbc;
        }

        private void ParseMessage()
        {
            DbcSheet sheet = workbook.Curr;
            Cell[] raw = sheet.Curr;

            Message msg = new Message
            {
                id = Get(raw, DbcExcelRow.MsgID),
                name = Get(raw, DbcExcelRow.MsgID),
                size = Get(raw, DbcExcelRow.MsgSize),
                transmitter = Get(raw, DbcExcelRow.Transmitter)
            };

            dbc.messages.Add(msg);



            if (raw[DbcExcelRow.MsgComment] != DbcExcelRow.EmptyCell)
            {
                dbc.comments.Add(new Comment
                {
                    type = Keyword.MESSAGES,
                    id = msg.id,
                    msg = Get(raw, DbcExcelRow.MsgComment)
                });
            }


            while (!sheet.EndOfStream)
            {
                Cell[] row = sheet.Curr;

                if (row[DbcExcelRow.SignalName] == DbcExcelRow.EmptyCell)
                    return;

                msg.signals.Add(ParseSignal(sheet.Consume()));
            }
        }

        private Signal ParseSignal(Cell[] raw)
        {
            Signal signal = new Signal
            {
                //name = raw.SignalName.Value

            };

            return signal;
        }

        private string Get(Cell[] raw, int idx)
        {
            switch (idx)
            {
                case DbcExcelRow.Transmitter:
                    return raw[idx].Value;
            }
            return null;
        }

    }
}
