using IocContainterTests.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IocContainterTests.Objs
{
    internal class PropObj : ObjBase
    {
        public string stringValue { get; set; }
        public OpOne OpOne { get; set; }
        public Action firstAction { get; set; }
    }
}
