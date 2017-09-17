using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using DbcLib.Excel.Reader;
using DbcLib.DBC.Model;
using DbcLib.DBC.Lex;

namespace DbcLib.Excel
{
    class ParseError
    {
        public ParseError(string msg, Cell cell)
        {
            Msg = msg;
            Src = cell;
        }

        public Cell Src { get; private set; }
        public string Msg { get; private set; }
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

        private DBC.Model.DBC dbc;


        public ExcelParser(String file)
        {
            workbook = new DbcWorkbook(file);
        }

        public void Dispose()
        {
            workbook.Dispose();
        }


        public DBC.Model.DBC Parse()
        {
            if (dbc != null)
                return null;

            dbc = new DBC.Model.DBC();

            DbcSheet sheet = workbook.Curr;

            while (!sheet.EndOfStream)
            {
                DbcExcelRow row = sheet.Consume();

                if (row.MsgName == DbcExcelRow.EmptyCell)
                    continue;

                Message msg = ParseMessage(row);
                dbc.messages.Add(msg);

                while (!sheet.EndOfStream)
                {
                    row = sheet.Curr;

                    if (row.SignalName == DbcExcelRow.EmptyCell)
                        break;

                    msg.signals.Add(ParseSignal(sheet.Consume()));
                }
            }

            return dbc;
        }

        private Message ParseMessage(DbcExcelRow row)
        {
            Message msg = new Message
            {
                id = ExpectHex(row.MsgID),
                name = ExpectId(row.MsgName),
                size = ExpectDecimal(row.MsgSize, CellType.UNSIGNED),
                transmitter = ExpectTransmitter(row.Transmitter)
            };

            if (row.MsgComment != DbcExcelRow.EmptyCell)
            {
                dbc.comments.Add(new Comment
                {
                    type = Keyword.MESSAGES,
                    id = msg.id,
                    msg = "\"" + row.MsgComment.Value + "\""
                });
            }

            return msg;
        }

        private Signal ParseSignal(DbcExcelRow row)
        {
            Signal signal = new Signal
            {
                name = ExpectId(row.SignalName),
                startBit = ExpectStartBit(row.BitPos),
                signalSize = ExpectDecimal(row.SignalSize, CellType.UNSIGNED),
                byteOrder = "0", //Motorola, big endian 
                valueType = "+",
                factor = ExpectDecimal(row.Factor, CellType.DOUBLE),
                offset = ExpectDecimal(row.Factor, CellType.DOUBLE),
                min = ExpectDecimal(row.PhysicalMin, CellType.DOUBLE),
                max = ExpectDecimal(row.PhysicalMax, CellType.DOUBLE),
                unit = ExpectString(row.Unit),
                receivers = ExpectReceivers(row.Receiver)
            };

            return signal;
        }

        private string ExpectHex(Cell cell)
        {
            string hexstring = cell.Value;

            if (hexstring.StartsWith("0x"))
            {
                hexstring = hexstring.Substring(2, hexstring.Length - 2);

                if (int.TryParse(hexstring, NumberStyles.AllowHexSpecifier,
                    null, out int hex))
                {
                    return hex.ToString();
                }
            }

            errors.Add(new ParseError("", cell));

            return cell.Value;
        }

        private string ExpectDecimal(Cell cell, CellType expected)
        {
            if (cell.Assert(expected))
                return cell.Value;

            if (cell.Type == CellType.STRING &&
                double.TryParse(cell.Value, out double val))
            {
                if (expected == CellType.DOUBLE)
                    return val.ToString();

                if ((int)val == val)
                {
                    if (expected == CellType.SIGNED)
                        return val.ToString();

                    if (val >= 0)
                        return val.ToString();
                }
            }

            errors.Add(new ParseError("", cell));

            return cell.Value;
        }

        private string ExpectId(Cell cell)
        {
            if (cell == DbcExcelRow.EmptyCell ||
                !Lexer.IsIdentifier(cell.Value))
            {
                errors.Add(new ParseError("", cell));
            }

            return cell.Value;
        }

        private string ExpectTransmitter(Cell cell)
        {
            if (cell == DbcExcelRow.EmptyCell)
                return "Vector__XXX";

            if (!Lexer.IsIdentifier(cell.Value))
            {
                errors.Add(new ParseError("", cell));
            }

            return cell.Value;
        }

        private string ExpectStartBit(Cell cell)
        {
            StringBuilder builder = new StringBuilder();

            foreach (char ch in cell.Value)
            {
                if (ch < '0' || ch > '9')
                    break;

                builder.Append(ch);
            }

            if (int.TryParse(builder.ToString(), out int val))
            {
                return val.ToString();
            }

            errors.Add(new ParseError("", cell));

            return cell.Value;
        }

        private string ExpectString(Cell cell)
        {
            if (cell == DbcExcelRow.EmptyCell)
                errors.Add(new ParseError("", cell));

            return "\"" + cell.Value.Trim() + "\"";
        }

        private List<string> ExpectReceivers(Cell cell)
        {
            if (cell == DbcExcelRow.EmptyCell)
            {
                errors.Add(new ParseError("", cell));

                return new List<string> { "Vector__XXX" };
            }


            string raw = cell.Value.Trim();

            return raw.Split(null).ToList();
        }
    }
}
