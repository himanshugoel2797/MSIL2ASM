using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.CoreLib
{
    public class CorlibMapping
    {
        public static List<KeyValuePair<Type, Type>> TypeMappings;

        static CorlibMapping()
        {
            TypeMappings = new List<KeyValuePair<Type, Type>>();
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Object), typeof(Object)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.ValueType), typeof(ValueType)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(Builtins.MemoryManager), typeof(Builtins.MemoryManager)));
        }
    }
}
