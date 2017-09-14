using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DbcLib.Excel
{
    class ExcelReader : IDisposable
    {
        private IWorkbook workbook;
        private List<DbcExcelRow> result;

        public ExcelReader(string filename)
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
        }

        public void Dispose()
        {
            workbook.Close();
        }

        public List<DbcExcelRow> Read()
        {
            if (result != null)
                return result;

            ISheet sheet = workbook.GetSheet("Message_Detail");

            result = new List<DbcExcelRow>();

            foreach (IRow row in sheet)
            {
                if (row.RowNum <= 2)
                    continue;

                DbcExcelRow data = new DbcExcelRow();
                string[] raw = data.row;

                for (int i = 0; i < raw.Length; ++i)
                {
                    ICell cell = row.GetCell(i);

                    if (cell == null)
                        continue;

                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            raw[i] = cell.NumericCellValue.ToString();

                            break;
                        case CellType.String:
                            raw[i] = cell.StringCellValue;
                            break;
                    }
                }

                if (!raw.All(str => str == ""))
                    result.Add(data);
            }

            return result;
        }

    }
}
