using IocContainterTests.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IocContainterTests.Executors
{
    internal class ExecutorOne
    {
        public static (int,string) MakeThing(OpTwo two, Action firstAction)
        {
            firstAction();
            return (two.intValue, two.stringValue);
        }
    }
}
