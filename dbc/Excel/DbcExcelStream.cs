using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    class DbcSheet
    {
        private IEnumerator rowIter;

        public DbcSheet(ISheet sheet)
        {
            rowIter = sheet.GetEnumerator();

            EndOfStream = !rowIter.MoveNext();
        }

        public bool EndOfStream { get; private set; }

        public DbcExcelRow Consume()
        {
            if (EndOfStream)
                return null;

            IRow row = (IRow)rowIter.Current;

            DbcExcelRow data = new DbcExcelRow();
            Cell[] raw = data.row;

            for (int i = 0; i < raw.Length; ++i)
            {
                ICell cell = row.GetCell(i);

                if (cell == null)
                    continue;

                switch (cell.CellType)
                {
                    case NPOI.SS.UserModel.CellType.Numeric:
                        double num = cell.NumericCellValue;

                        if ((int)num != num)
                            raw[i] = new Cell(cell.StringCellValue, CellType.DOUBLE);
                        else if ((int)num < 0)
                            raw[i] = new Cell(cell.StringCellValue, CellType.SIGNED);
                        else
                            raw[i] = new Cell(cell.StringCellValue, CellType.UNSIGNED);

                        break;
                    case NPOI.SS.UserModel.CellType.String:
                        raw[i] = new Cell(cell.StringCellValue, CellType.STRING);
                        break;
                }
            }

            EndOfStream = !rowIter.MoveNext();

            return data;
        }
    }

    class DbcWorkbook : IDisposable
    {
        private IWorkbook workbook;
        private IEnumerator sheetIter;

        public DbcWorkbook(string filename)
        {
            if (!filename.EndsWith(".xlsx") &&
                !filename.EndsWith(".xls"))
                throw new ArgumentException("The file ext must be either .xlsx or xls");

            FileStream stream = File.Open(filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            if (filename.EndsWith(".xlsx"))
                workbook = new XSSFWorkbook(stream);
            else
                workbook = new HSSFWorkbook(stream);

            sheetIter = workbook.GetEnumerator();

            Curr = FindNext();

            EndOfStream = Curr == null;
        }

        public void Dispose()
        {
            workbook.Close();
        }


        public bool EndOfStream { get; private set; }
        public DbcSheet Curr { get; private set; }

        public DbcSheet Consume()
        {
            DbcSheet old = Curr;
            Curr = FindNext();

            EndOfStream = Curr == null;

            return old;
        }

        private DbcSheet FindNext()
        {
            while (sheetIter.MoveNext())
            {
                ISheet sheet = (ISheet)sheetIter.Current;

                if (!sheet.SheetName.StartsWith("Message_Detail"))
                    continue;

                DbcSheet dbcSheet = new DbcSheet(sheet);

                DbcExcelRow first = dbcSheet.Consume();
                DbcExcelRow sec = dbcSheet.Consume();

                if (first != null && sec != null)
                    return dbcSheet;
            }

            return null;
        }

    }
}
