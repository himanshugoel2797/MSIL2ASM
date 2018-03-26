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
            var s = Encoding.UTF8.GetBytes(str);
            data.Add($"dd 0x{s.Length:X}");

            var line = prefix + "_str" + idx.ToString() + " : db ";
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] < 128)
                    line += $"'{(char)s[i]}'";
                else
                    line += $"0x{s[i]:X}";

                if (i < s.Length - 1)
                    line += ",";
            }
            data.Add(line);
        }

        public string GetStringLabel(int idx)
        {
            return prefix + "_str" + idx.ToString();
        }
    }
}
