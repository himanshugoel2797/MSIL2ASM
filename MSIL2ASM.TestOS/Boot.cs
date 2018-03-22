using System;

namespace MSIL2ASM.TestOS
{
    public class Boot
    {
        private static IDT IDT;
        private static GDT GDT;
        static int a = 0;

        public static void Main(string[] args)
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
            */
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
            //IDT = new IDT();
            //GDT = new GDT();
        }
        
    }
}
