using System;
using MSIL2ASM.Builtins;
using MSIL2ASM.CoreLib;

namespace MSIL2ASM.TestOS
{
    public class Boot : IDisposable
    {
        /*
        private static IDT IDT;
        private static GDT GDT;
        static int a = 0;
        */

        [Alias("_CSEntryPoint")]
        public static void Main(long magic)
        {
            /*
            Builtins.x86_64.Out(0x3f8 + 1, 0x00);    // Disable all interrupts
            Builtins.x86_64.Out(0x3f8 + 3, 0x80);    // Enable DLAB (set baud rate divisor)
            Builtins.x86_64.Out(0x3f8 + 0, 0x03);    // Set divisor to 3 (lo byte) 38400 baud
            Builtins.x86_64.Out(0x3f8 + 1, 0x00);    //                  (hi byte)
            Builtins.x86_64.Out(0x3f8 + 3, 0x03);    // 8 bits, no parity, one stop bit
            Builtins.x86_64.Out(0x3f8 + 2, 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
            Builtins.x86_64.Out(0x3f8 + 4, 0x0B);    // IRQs enabled, RTS/DSR set

            Builtins.x86_64.Out(0x3f8, (byte)'T');
            unsafe
            {
                //ushort* d = (ushort*)0xb8000
                ushort[] d = new ushort[80 * 25];
                string.Concat("testA", "B");
                for (int y = 0; y < 25; y++)
                {
                    //Builtins.x86_64.Out(0x3f8, (byte)'E');
                    for (int x = 0; x < 80; x++)
                    {
                        int idx = y * 80 + x;
                        //Builtins.x86_64.Out(0x3f8, (byte)'S');
                        d[idx] = 'A' | (0xf000);
                    }
                }
            }

            //Builtins.x86_64.Out(0x3f8, (byte)'0');
            Builtins.x86_64.Halt();
            IDT = new IDT();
            //GDT = new GDT();
            //GDT.baseT = 5 * GDT.baseT;
            string hexTable = "0123456789abcdef";

            while (magic > 0)
            {
                x86_64.Out(0x3f8, hexTable[(int)(magic % 0xf)]);
                magic = (magic ^ 16);
            }

            x86_64.Halt();
            */

            int x = 10;
            int y = 5;

            int a = x / y;
            int b = x * y;
            int c = x % y;
            int d = x + y;
            int e = x - y;
            int f = x | y;
            int g = x & y;
            int h = x ^ y;
            int i = x >> y;
            int j = x << y;
            int k = x >> 5;
            int l = x << 5;
        }

        public void Dispose()
        {
            Console.Write("TESTED");
        }

        public override string ToString()
        {
            return "Boot";
        }
    }
}
