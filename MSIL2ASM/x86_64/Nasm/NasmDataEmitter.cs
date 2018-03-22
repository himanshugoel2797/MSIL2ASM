using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    partial class NasmEmitter
    {
        public void EmitStaticStruct(string name, int sz)
        {
            bss.Add("global " + name);
            bss.Add("\t" + name + ": resb " + sz.ToString());
        }

        public void EmitVtable(string name)
        {
            data.Add(name + "_vtable:");
        }

        public void AddVtableEntry(string name)
        {
            data.Add("\tdq " + name);
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
