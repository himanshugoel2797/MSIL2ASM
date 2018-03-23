using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.CoreLib
{
    public class AliasAttribute : System.Attribute
    {
        public string Name { get; private set; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}
