using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.CoreLib
{
    public class String
    {
        public char m_firstChar;

        //[Alias("mthd_System_String_get_Chars_0System_Int32_")]
        [IndexerName("Chars")]
        public char this[int idx]
        {
            get
            {
                unsafe
                {
                    fixed (char* chars = &m_firstChar)
                    {
                        byte* b_chars = (byte*)chars;
                        return (char)b_chars[idx];
                    }
                }
            }
        }

        public int Length
        {
            get
            {
                unsafe
                {
                    fixed (char* chars = &m_firstChar)
                    {
                        int* len = (int*)chars;
                        return len[-1];
                    }
                }
            }
        }
    }
}
