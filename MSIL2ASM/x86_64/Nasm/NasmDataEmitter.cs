using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    partial class NasmEmitter
    {
        private int MethodStringBase = 0;

        public void EmitStaticStruct(string name, int sz)
        {
            bss.Add("global " + name);
            bss.Add(name + ": resb " + sz.ToString());
        }

        public void AddString(string str, int idx)
        {
            data.Add(prefix + "_str" + idx.ToString() + " : db '" + str + "'");
        }

        public string GetStringLabel(int idx)
        {
            return prefix + "_str" + idx.ToString();
        }
    }
}
