﻿using System;
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
        private Model.PropTree.PropTree tree;

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
            tree = new Model.PropTree.PropTree(dbc);
            tree.Def.TryInsert(new AttributeDefault
            {
                AttributeName = DbcTemplate.Attr_MsgSendType,
                Num = DbcTemplate.MsgSendTypeDefault
            });

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
            var prop = tree.ID(msg.MsgID).MsgProp;

            row.MsgID.SetHex(msg.MsgID);
            row.MsgName.Set(msg.Name);
            row.MsgSize.Set(msg.Size);
            row.Transmitter.Set(msg.Transmitter);

            row.MsgComment.Set(prop.CM.Val);

            var sendType = prop.Attribute[DbcTemplate.Attr_MsgSendType];
            var type = sendType.Num;

            if (type == DbcTemplate.MsgSendType_Cyclic)
            {
                var cycleTime = prop.Attribute[DbcTemplate.Attr_MsgCycleTime];

                if (cycleTime != null)
                    row.FixedPeriodic.Set(cycleTime.Num);
            }
            else if (type == DbcTemplate.MsgSendType_IfActive)
            {
                row.Event.Set("x");
            }
            else if (type == DbcTemplate.MsgSendType_CyclicEvent)
            {
                row.PeriodicEvent.Set("x");
            }

        }

        private void Signals(DbcRow row, Signal signal, long id)
        {
            var prop = tree.ID(id).Name(signal.Name);

            row.SignalName.Set(signal.Name);
            row.SignalSize.Set(signal.SignalSize);
            row.BitPos.Set(signal.StartBit);
            row.Unit.Set(signal.Unit);
            row.Factor.Set(signal.Factor);
            row.Offset.Set(signal.Offset);
            row.PhysicalMin.Set(signal.Min);
            row.PhysicalMax.Set(signal.Max);
            row.Receiver.SetReceiver(signal.Receivers);


            row.DetailedMeaning.Set(prop.CM.Val);
            row.State.SetValueDescs(prop.VD.Descs);

            int descHeight = prop.VD.Descs.Count;
            int height = Math.Max(descHeight, signal.Receivers.Count);

            if (height > 1)
            {
                row.NRow.HeightInPoints = height * row.NRow.HeightInPoints;
            }

            var state = row.State.NCell;
            if (state != null)
                state.CellStyle = stackedTextStyle;

            var receiver = row.Receiver.NCell;
            if (receiver != null)
                receiver.CellStyle = stackedTextStyle;
        }

    }
}
