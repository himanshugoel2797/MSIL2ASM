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

        public static List<Type> IgnoreTypes;

        static CorlibMapping()
        {
            IgnoreTypes = new List<Type>();

            TypeMappings = new List<KeyValuePair<Type, Type>>();
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Object), typeof(Object)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.ValueType), typeof(ValueType)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(Builtins.MemoryManager), typeof(Builtins.MemoryManager)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.String), typeof(CoreLib.String)));

            IgnoreTypes.Add(typeof(CoreLib.Attribute));
            IgnoreTypes.Add(typeof(CoreLib.AliasAttribute));
            IgnoreTypes.Add(typeof(CoreLib.CorlibMapping));
            IgnoreTypes.Add(typeof(Builtins.x86_64));
        }
    }
}
