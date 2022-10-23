using IocContainterTests.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IocContainterTests.Objs
{
    internal class FieldObj : ObjBase
    {
        public string stringValue;
        public OpOne OpOne;
        public Action firstAction;
    }
}
