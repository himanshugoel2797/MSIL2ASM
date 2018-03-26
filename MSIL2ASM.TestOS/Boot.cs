using System;
using MSIL2ASM.Builtins;
using MSIL2ASM.CoreLib;

namespace MSIL2ASM.TestOS
{
    public class Boot
    {
        [Alias("_CSEntryPoint")]
        public static void Main(ulong multiboot_info, ulong magic, ulong kernel_start, ulong kernel_end)
        {
            //string str = "Hello World!";
            //for (int i = 0; i < str.Length; i++)
            //    x86_64.Out(0x3f8, str[i]);

            string hexTable = "0123456789ABCDEF";
            ulong v = kernel_end;

            for (int i = 0; i < 16; i++)
            {
                byte v0 = (byte)((v & 0xf000000000000000) >> 60);
                x86_64.Out(0x3f8, hexTable[v0]);

                v = v << 4;
            }

            x86_64.Halt();
        }
    }
}

    /*
    public class Test<A, B>
    {
        A a;
        B b;
        int r;

        public void TestA(A a) { this.a = a; }
        public void TestB(B b) { this.b = b; }
        public void TestAB(A a, B b) { this.a = a; this.b = b; }
        public void TestABCD<C, D>(A a, B b, C c, D d) { this.a = a; this.b = b; r = c.GetHashCode() * d.GetHashCode(); }
    }
    */
