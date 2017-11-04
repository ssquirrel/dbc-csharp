using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NPOI.SS.UserModel;
using DbcLib.Model;
using System.Threading;

namespace DbcLib.Excel.Writer
{
    class ExcelTemplate
    {
        public static IWorkbook LoadTemplate(string fn)
        {
            File.Copy("template.xlsx", fn, true);

            FileStream stream = new FileStream(fn, FileMode.Open);

            return WorkbookFactory.Create(stream);
        }
    }

    public class ExcelWriter : IDisposable
    {
        private string filename;
        private IWorkbook workbook;
        private DbcQuery query;

        private ICellStyle stackedTextStyle;

        public ExcelWriter(string fn)
        {
            filename = fn;

            workbook = ExcelTemplate.LoadTemplate(fn);

            stackedTextStyle = workbook.CreateCellStyle();
            stackedTextStyle.Alignment = HorizontalAlignment.Left;
            stackedTextStyle.VerticalAlignment = VerticalAlignment.Bottom;
            stackedTextStyle.WrapText = true;
        }

        public void Add(string sn, Model.DBC dbc)
        {
            query = new DbcQuery(dbc);

            ISheet sheet = AllocateSheet(sn);

            foreach (var msg in dbc.Messages)
            {
                Messages(AllocateRow(sheet), msg);

                foreach (var signal in msg.Signals)
                {
                    Signals(AllocateRow(sheet), signal, msg.MsgID);
                }
            }
        }

        public void Write()
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                workbook.Write(stream);
            }

        }

        public void Dispose()
        {
            workbook.Close();
        }

        private ISheet AllocateSheet(string name)
        {
            workbook.SetSheetName(0, name);
            return workbook.GetSheetAt(0);
        }

        private DbcRow AllocateRow(ISheet sheet)
        {
            var raw = sheet.CreateRow(sheet.LastRowNum + 1);
            return new DbcRow(raw);
        }

        private void Messages(DbcRow row, Message msg)
        {
            var prop = query.GetMsgProp(msg.MsgID);

            row.MsgID.SetHex(msg.MsgID);
            row.MsgName.Set(msg.Name);
            row.MsgSize.Set(msg.Size);
            row.Transmitter.Set(msg.Transmitter);

            if (prop != null)
            {
                if (prop.CM != null)
                {
                    row.MsgComment.Set(prop.CM.Val);
                }

                var sendType = prop.GetAttribute(DbcTemplate.Attr_MsgSendType);
                var cycleTime = prop.GetAttribute(DbcTemplate.Attr_MsgCycleTime);

                if (sendType != null)
                {
                    if (sendType.Value.Num == DbcTemplate.MsgSendType_IfActive)
                    {
                        row.Event.Set("x");
                    }
                    else if (sendType.Value.Num == DbcTemplate.MsgSendType_CyclicEvent)
                    {
                        row.PeriodicEvent.Set("x");
                    }
                }
                else if (cycleTime != null)
                {
                    row.FixedPeriodic.Set(cycleTime.Value.Num);
                }
                else
                {
                    var t = query.GetAttributeDefault(DbcTemplate.Attr_MsgCycleTime);
                    row.FixedPeriodic.Set(t.Value.Num);
                }

            }

            row.Commit();
        }

        private void Signals(DbcRow row, Signal signal, long id)
        {
            var prop = query.GetSignalProp(id, signal.Name);

            row.SignalName.Set(signal.Name);
            row.SignalSize.Set(signal.SignalSize);
            row.BitPos.Set(signal.StartBit);
            row.Unit.Set(signal.Unit);
            row.Factor.Set(signal.Factor);
            row.Offset.Set(signal.Offset);
            row.PhysicalMin.Set(signal.Min);
            row.PhysicalMax.Set(signal.Max);
            row.Receiver.SetReceiver(signal.Receivers);

            if (prop.CM != null)
            {
                row.DetailedMeaning.Set(prop.CM.Val);
            }

            int descHeight = 0;

            if (prop.VD != null)
            {
                row.State.SetValueDescs(prop.VD.Descs);

                descHeight = prop.VD.Descs.Count;
            }

            row.Commit();

            int height = Math.Max(descHeight, signal.Receivers.Count);

            if (height > 1)
            {
                row.Raw.HeightInPoints = height * row.Raw.HeightInPoints;
            }

            if (row.State.Raw != null)
                row.State.Raw.CellStyle = stackedTextStyle;

            if (row.Receiver.Raw != null)
                row.Receiver.Raw.CellStyle = stackedTextStyle;
        }

    }
}
