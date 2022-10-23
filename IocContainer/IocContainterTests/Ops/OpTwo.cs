using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IocContainterTests.Ops
{
    internal class OpTwo : OpBase
    {
        public Action firstAction;
        public string stringValue;
        public int intValue;
        public OpOne oneOp;

        public OpTwo(Action firstAction, string stringValue, int intValue, OpOne oneOp)
        {
            this.firstAction = firstAction;
            this.stringValue = stringValue;
            this.oneOp = oneOp;
        }
    }
}
