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
    class ExcelTemplate
    {
        public ExcelTemplate(string fn)
        {
            File.Copy("template.xlsx", fn, true);

            FileStream stream = new FileStream(fn, FileMode.Open);

            Workbook = WorkbookFactory.Create(stream);

            StackedTextStyle = Workbook.CreateCellStyle();
            StackedTextStyle.Alignment = HorizontalAlignment.Left;
            StackedTextStyle.VerticalAlignment = VerticalAlignment.Bottom;
            StackedTextStyle.WrapText = true;
        }

        public IWorkbook Workbook { get; }

        public ICellStyle StackedTextStyle { get; }

        public static IWorkbook LoadTemplate(string fn)
        {
            File.Copy("template.xlsx", fn, true);

            FileStream stream = new FileStream(fn, FileMode.Open);

            return WorkbookFactory.Create(stream);
        }
    }

    public class ExcelWriter : IDisposable
    {
        private SemanticAnalysis analysis;

        private Tree tree;

        private string filename;
        private IWorkbook workbook;


        public ExcelWriter(string fn)
        {
            filename = fn;

            workbook = ExcelTemplate.LoadTemplate(fn);
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

        }
    }
}
