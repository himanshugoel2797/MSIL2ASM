using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.CoreLib
{
    public class ValueType
    {
        public override string ToString()
        {
            return "Test";
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool Equals(object obj, object o)
        {
            return false;
        }
    }
}
