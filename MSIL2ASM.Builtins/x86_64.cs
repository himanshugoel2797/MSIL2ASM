using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.Builtins
{
    public class x86_64
    {
        public static void Halt() { }
        public static void Cli() { }
        public static void Sti() { }

        public static void In(ushort addr, out int res) { res = 0; }
        public static void In(ushort addr, out short res) { res = 0; }
        public static void In(ushort addr, out byte res) { res = 0; }
        public static void In(ushort addr, out uint res) { res = 0; }
        public static void In(ushort addr, out ushort res) { res = 0; }
        public static void In(ushort addr, out sbyte res) { res = 0; }

        public static void Out(ushort addr, int v) { }
        public static void Out(ushort addr, short v) { }
        public static void Out(ushort addr, byte v) { }
        public static void Out(ushort addr, uint v) { }
        public static void Out(ushort addr, ushort v) { }
        public static void Out(ushort addr, sbyte v) { }
    }
}
