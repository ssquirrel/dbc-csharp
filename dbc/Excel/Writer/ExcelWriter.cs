using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using NPOI.SS.UserModel;
using DbcLib.Model;
using DbcLib.Model.PropTree;
using System.Threading;
using DbcLib.Excel.Sheet;

namespace DbcLib.Excel.Writer
{
    class ExcelTemplate : IDisposable
    {
        public ExcelTemplate(string fn)
        {
            Name = fn;

            using (var stream = new FileStream("template.xlsx", FileMode.Open))
            {
                Workbook = WorkbookFactory.Create(stream);
            }

            CenterText = Workbook.CreateCellStyle();
            CenterText.Alignment = HorizontalAlignment.Center;

            TextWrap = Workbook.CreateCellStyle();
            TextWrap.WrapText = true;
        }

        public string Name { get; }

        public IWorkbook Workbook { get; }

        public ICellStyle CenterText { get; }
        public ICellStyle TextWrap { get; }

        public void Dispose()
        {
            Workbook.Close();
        }

        public void Save()
        {
            using (var stream = new FileStream(Name, FileMode.Create))
            {
                Workbook.Write(stream);
            }
        }
    }

    public class ExcelWriter : IDisposable
    {
        private SemanticAnalysis analysis;

        private Tree tree;

        ExcelTemplate excel;

        private IWorkbook workbook;


        public ExcelWriter(string fn)
        {
            excel = new ExcelTemplate(fn);

            workbook = excel.Workbook;
        }

        public void Add(string sn, Model.DBC dbc)
        {
            tree = new Tree(dbc);

            analysis = new SemanticAnalysis(tree);

            ISheet sheet = AllocateSheet(sn);

            var groups = dbc.Messages.ToLookup(msg => msg.Transmitter);

            foreach (var group in groups)
            {
                {
                    var row = AllocateRow(sheet);
                    row.Transmitter.Set(group.Key);
                }

                var topLevel = Group.Begin(sheet);
                foreach (var msg in group)
                {
                    {
                        var row = AllocateRow(sheet);

                        Messages(row, msg);
                        MsgRowStyle(row);
                    }

                    var subLevel = Group.Begin(sheet);
                    foreach (var signal in msg.Signals)
                    {
                        var row = AllocateRow(sheet);

                        Signals(row, signal, msg.MsgID);
                        SigRowStyle(row);
                    }
                    subLevel.End();
                }

                AllocateRow(sheet);

                topLevel.End();
            }

        }

        public void Write()
        {
            excel.Save();
        }

        public void Dispose()
        {
            excel.Dispose();
        }

        private ISheet AllocateSheet(string name)
        {
            workbook.SetSheetName(0, name);
            return workbook.GetSheetAt(0);
        }

        private WritingRow AllocateRow(ISheet sheet)
        {
            var raw = sheet.CreateRow(sheet.LastRowNum + 1);
            return new WritingRow(raw);
        }

        private void Messages(WritingRow row, Message msg)
        {
            row.MsgID.SetHex(msg.MsgID);
            row.MsgName.Set(msg.Name);
            row.MsgSize.Set(msg.Size);
            row.Transmitter.Set(msg.Transmitter);

            var prop = tree.MsgProp(msg.MsgID);

            if (prop.Comment.Length > 0)
                row.MsgComment.Set(prop.Comment);

            var sendType = prop.Attribute(MsgSendType.AttributeName,
                analysis.MsgSendTypeDefault);
            var type = analysis.GetSendType(sendType);

            switch (type)
            {
                case MsgSendTypeEnum.Cyclic:
                    var ct = prop.Attribute(MsgCycleTime.AttributeName,
                        analysis.MsgCycleTimeDefault);

                    if (ct != null)
                        row.MsgCycleTime.Set(ct.Num);
                    break;

                case MsgSendTypeEnum.IfActive:
                    row.Event.Set("x");
                    break;

                case MsgSendTypeEnum.CyclicEvent:
                    row.PeriodicEvent.Set("x");
                    break;
            }

        }

        private void MsgRowStyle(WritingRow row)
        {
            row.MsgID.Style = excel.CenterText;
            row.MsgCycleTime.Style = excel.CenterText;
            row.Event.Style = excel.CenterText;
            row.PeriodicEvent.Style = excel.CenterText;
            row.MsgSize.Style = excel.CenterText;
            row.MsgComment.Style = excel.TextWrap;
        }

        private void Signals(WritingRow row, Signal signal, long id)
        {
            row.SignalName.Set(signal.Name);
            row.SizeInBits.Set(signal.SignalSize);
            row.StartBit.Set(signal.StartBit);
            row.Unit.Set(signal.Unit);
            row.Factor.Set(signal.Factor);
            row.Offset.Set(signal.Offset);
            row.PhysicalMin.Set(signal.Min);
            row.PhysicalMax.Set(signal.Max);
            row.Receiver.SetReceiver(signal.Receivers);

            row.ByteOrder.SetByteOrder(signal.ByteOrder);
            row.ValueType.SetValueType(signal.ValueType);

            var prop = tree.SignalProp(id, signal.Name);

            row.SigComment.Set(prop.Comment);
            row.ValueDescs.SetValueDescs(prop.Descs);

            var startVal = prop.Attribute(SigStartValue.AttributeName,
                analysis.SigStartValDefault);
            if (startVal != null)
            {
                row.SigStartValue.Set(startVal.Num);
            }

            /*styling
            int descHeight = prop.Descs.Count;
            int height = Math.Max(descHeight, signal.Receivers.Count);

            if (height > 1)
            {
                row.NRow.HeightInPoints = height * row.NRow.HeightInPoints;
            }

            var state = row.ValueDescs.NCell;
            state.CellStyle = stackedTextStyle;

            var receiver = row.Receiver.NCell;
            receiver.CellStyle = stackedTextStyle;
            */
        }

        private void SigRowStyle(WritingRow row)
        {
            row.SizeInBits.Style = excel.CenterText;
            row.StartBit.Style = excel.CenterText;
            row.ByteOrder.Style = excel.CenterText;
            row.ValueType.Style = excel.CenterText;
            row.ValueDescs.Style = excel.TextWrap;
            row.SigComment.Style = excel.TextWrap;
            row.Receiver.Style = excel.TextWrap;
        }
    }
}
