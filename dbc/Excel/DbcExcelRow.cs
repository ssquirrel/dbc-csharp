using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbcLib.Excel
{
    class DbcExcelRow
    {
        /*
        Transmitter,
        MsgID,
        MsgName,
        FixedPeriodic,
        Event,
        PeriodicEvent,
        MsgSize,
        SignalName,
        SignalSize,
        BitPos,
        SignalComment,
        SignalValDef,
        Unit,
        Factor,
        Offset,
        LogicalMin,
        PhysicalMin,
        LogicalMax,
        PhysicalMax,
        DefaultVal,
        DefaultTimeout,
        Storage,
        Receiver,
        MsgComment
        */

        public string[] row = new string[24];

        public DbcExcelRow()
        {
            for (int i = 0; i < row.Length; ++i)
                row[i] = "";
        }

        public string Transmitter
        {
            get
            {
                return row[0];
            }
        }
        public string MsgID
        {
            get
            {
                return row[1];
            }
        }
        public string MsgName
        {
            get
            {
                return row[2];
            }
        }
        public string FixedPeriodic
        {
            get
            {
                return row[3];
            }
        }
        public string Event
        {
            get
            {
                return row[4];
            }
        }
        public string PeriodicEvent
        {
            get
            {
                return row[5];
            }
        }
        public string MsgSize
        {
            get
            {
                return row[6];
            }
        }
        public string SignalName
        {
            get
            {
                return row[7];
            }
        }
        public string SignalSize
        {
            get
            {
                return row[8];
            }
        }
        public string BitPos
        {
            get
            {
                return row[9];
            }
        }
        public string SignalComment
        {
            get
            {
                return row[10];
            }
        }
        public string SignalValDef
        {
            get
            {
                return row[11];
            }
        }
        public string Unit
        {
            get
            {
                return row[12];
            }
        }
        public string Factor
        {
            get
            {
                return row[13];
            }
        }
        public string Offset
        {
            get
            {
                return row[14];
            }
        }
        public string LogicalMin
        {
            get
            {
                return row[15];
            }
        }
        public string PhysicalMin
        {
            get
            {
                return row[16];
            }
        }
        public string LogicalMax
        {
            get
            {
                return row[17];
            }
        }
        public string PhysicalMax
        {
            get
            {
                return row[18];
            }
        }
        public string DefaultVal
        {
            get
            {
                return row[19];
            }
        }
        public string DefaultTimeout
        {
            get
            {
                return row[20];
            }
        }
        public string Storage
        {
            get
            {
                return row[21];
            }
        }
        public string Receiver
        {
            get
            {
                return row[22];
            }
        }
        public string MsgComment
        {
            get
            {
                return row[23];
            }
        }
    }
}
