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

        private PropTree tree;

        private string filename;
        private IWorkbook workbook;

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
            analysis = new SemanticAnalysis(dbc);

            tree = new PropTree(dbc);

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
            row.MsgID.SetHex(msg.MsgID);
            row.MsgName.Set(msg.Name);
            row.MsgSize.Set(msg.Size);
            row.Transmitter.Set(msg.Transmitter);

            var prop = tree.ID(msg.MsgID).MsgProp;

            if (prop.Comment.Length > 0)
                row.MsgComment.Set(prop.CM.Val);

            var sendType = prop.Attributes[MsgSendType.AttributeName];
            var type = analysis.GetSendType(sendType);

            switch (type)
            {
                case MsgSendTypeEnum.Cyclic:
                    var ct = prop.Attributes[MsgCycleTime.AttributeName];
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

        private void Signals(DbcRow row, Signal signal, long id)
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

            var prop = tree.ID(id).Name(signal.Name);

            row.SigComment.Set(prop.Comment);
            row.ValueDescs.SetValueDescs(prop.Descs);

            var startVal = prop.Attributes[SigStartValue.AttributeName];
            if (startVal.Type == AttrValType.Number)
            {
                row.SigStartValue.Set(startVal.Num);
            }

            //styling
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
        }

    }
}
