using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.Builtins
{
    public static class MemoryManager
    {
        public static ulong Block0_OthrBlock;
        public static ulong Block0_CurBlock;
        public static ulong Block0_CurPointer;
        public static ulong Block0_Size;

        public static ulong Block1_OtherBlock;
        public static ulong Block1_CurBlock;
        public static ulong Block1_CurPointer;
        public static ulong Block1_Size;

        public static int Block0_CollectionCount = 0;

        static MemoryManager()
        {

        }

        private static void TriggerCollectionBlock0()
        {

            if (Block0_CollectionCount++ == 64)
                TriggerCollectionBlock1();
        }

        private static void TriggerCollectionBlock1()
        {

        }

        private static ulong Alloc(uint net_sz)
        {
            if (Block0_CurPointer + net_sz > Block0_CurBlock + Block0_Size)
            {
                //Trigger collection
                TriggerCollectionBlock0();
            }

            var curPtr = Block0_CurPointer;
            Block0_CurPointer += net_sz;

            unsafe
            {
                uint* curPtr_p = (uint*)curPtr;
                curPtr_p[3] = net_sz;
            }

            return curPtr;
        }

        /*
         * [-16] = Additional info
         * [-8] = Collection info
         * [-4] = Size of allocation
         * [0] = Start of object
         */

        public static ulong AllocateArray(int unit_sz, int len)
        {
            uint net_sz = (uint)(unit_sz * len) + 16;
            return Alloc(net_sz) + 16;
        }

        public static ulong AllocateMemory(int len)
        {
            return Alloc((uint)(len + 16)) + 16;
        }
    }
}
