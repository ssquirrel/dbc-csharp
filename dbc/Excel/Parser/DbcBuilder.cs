using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbcLib.Model;

namespace DbcLib.Excel.Parser
{
    class DbcBuilder
    {
        public class MsgBuilder
        {
            private DbcBuilder ctx;
            private Message msg;
            private MsgSendTypeEnum sendType = MsgSendTypeEnum.NoMsgSendType;

            public MsgBuilder(DbcBuilder ctx, Message msg)
            {
                this.ctx = ctx;
                this.msg = msg;

                ctx.DBC.Messages.Add(msg);
            }

            public MsgBuilder Comment(string cm)
            {
                if (cm.Length == 0)
                    return this;

                ctx.DBC.Comments.Add(new Comment
                {
                    Type = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Val = cm
                });

                return this;
            }

            public MsgBuilder SendType(MsgSendTypeEnum type)
            {
                if (type == MsgSendTypeEnum.NoMsgSendType)
                    return this;

                sendType = type;

                Model.DBC dbc = ctx.DBC;

                if (ctx.defs.Add(MsgSendType.AttributeName))
                {
                    dbc.AttributeDefinitions.Add(MsgSendType.Definition());
                    dbc.AttributeDefaults.Add(MsgSendType.Default());
                }

                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = MsgSendType.AttributeName,
                    ObjType = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Value = new AttributeValue
                    {
                        Num = (int)type
                    }
                });

                return this;
            }

            public MsgBuilder CycleTime(int time)
            {
                if (sendType != MsgSendTypeEnum.Cyclic || time == 0)
                    return this;

                Model.DBC dbc = ctx.DBC;

                if (ctx.defs.Add(MsgCycleTime.AttributeName))
                {
                    dbc.AttributeDefinitions.Add(MsgCycleTime.Definition());
                    dbc.AttributeDefaults.Add(MsgCycleTime.Default());
                }

                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = MsgCycleTime.AttributeName,
                    ObjType = Keyword.MESSAGES,
                    MsgID = msg.MsgID,
                    Value = new AttributeValue
                    {
                        Num = time
                    }
                });

                return this;
            }
        }

        public class SignalBuider
        {
            private DbcBuilder ctx;
            private Message msg;
            private Signal sig;

            public SignalBuider(DbcBuilder ctx, Message msg, Signal sig)
            {
                this.ctx = ctx;
                this.msg = msg;
                this.sig = sig;

                msg.Signals.Add(sig);
            }

            public SignalBuider Comment(string cm)
            {
                if (cm.Length == 0)
                    return this;

                ctx.DBC.Comments.Add(new Comment
                {
                    Type = Keyword.SIGNAL,
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Val = cm
                });

                return this;
            }

            public SignalBuider ValueDescs(IList<ValueDesc> descs)
            {
                if (descs.Count == 0)
                    return this;

                ctx.DBC.ValueDescriptions.Add(new SignalValueDescription
                {
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Descs = descs
                });

                return this;
            }

            public SignalBuider StartVal(int val)
            {
                if (val == 0)
                    return this;

                Model.DBC dbc = ctx.DBC;

                if (ctx.defs.Add(SigStartValue.AttributeName))
                {
                    dbc.AttributeDefinitions.Add(SigStartValue.Definition());
                    dbc.AttributeDefaults.Add(SigStartValue.Default());
                }

                dbc.AttributeValues.Add(new ObjAttributeValue
                {
                    AttributeName = SigStartValue.AttributeName,
                    ObjType = Keyword.SIGNAL,
                    MsgID = msg.MsgID,
                    Name = sig.Name,
                    Value = new AttributeValue
                    {
                        Num = val
                    }
                });

                return this;
            }
        }

        private HashSet<string> defs = new HashSet<string>();

        public MsgBuilder NewMessage(Message msg)
        {
            return new MsgBuilder(this, msg);
        }

        public Model.DBC DBC { get; } = new Model.DBC();

        public SignalBuider NewSignal(Message msg, Signal sig)
        {
            return new SignalBuider(this, msg, sig);
        }

    }

}
