using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.Mapping
{
    public class CorlibMapping
    {
        public static List<KeyValuePair<Type, Type>> TypeMappings;

        public static List<Type> IgnoreTypes;

        static CorlibMapping()
        {
            IgnoreTypes = new List<Type>();

            TypeMappings = new List<KeyValuePair<Type, Type>>();
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Object), typeof(CoreLib.Object)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.ValueType), typeof(CoreLib.ValueType)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(Builtins.MemoryManager), typeof(Builtins.MemoryManager)));

            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Delegate), typeof(CoreLib.Delegate)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Type), typeof(CoreLib.Type)));

            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(void), typeof(CoreLib.Void)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Boolean), typeof(CoreLib.Boolean)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.String), typeof(CoreLib.String)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Char), typeof(CoreLib.Char)));

            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Byte), typeof(CoreLib.Byte)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Int16), typeof(CoreLib.Int16)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Int32), typeof(CoreLib.Int32)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.Int64), typeof(CoreLib.Int64)));

            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.SByte), typeof(CoreLib.SByte)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.UInt16), typeof(CoreLib.UInt16)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.UInt32), typeof(CoreLib.UInt32)));
            TypeMappings.Add(new KeyValuePair<Type, Type>(typeof(System.UInt64), typeof(CoreLib.UInt64)));

            IgnoreTypes.Add(typeof(CoreLib.Attribute));
            IgnoreTypes.Add(typeof(CoreLib.AliasAttribute));
            IgnoreTypes.Add(typeof(MSIL2ASM.Mapping.CorlibMapping));
            IgnoreTypes.Add(typeof(Builtins.x86_64));
        }
    }
}
