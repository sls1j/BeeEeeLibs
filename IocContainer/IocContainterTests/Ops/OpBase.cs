using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IocContainterTests.Ops
{
    internal class OpBase
    {
        protected string value;
        public void SetValue(string value)
        {
            this.value = value;
        }
    }
}
