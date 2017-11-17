using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;

namespace DbcLib.Excel.Writer
{
    class Group
    {
        private ISheet sheet;
        private int start;

        public Group(ISheet sheet)
        {
            this.sheet = sheet;
        }

        public static Group Begin(ISheet sheet)
        {
            var g = new Group(sheet);
            g.Begin();

            return g;
        }

        public void Begin()
        {
            start = sheet.LastRowNum + 1;
        }

        public void End()
        {
            int end = sheet.LastRowNum;

            sheet.GroupRow(start, end);
        }
    }
}
