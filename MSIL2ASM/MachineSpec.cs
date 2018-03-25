using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public class MachineSpec
    {
        public const int PointerSize = 8;

        public static string GetMethodName(MethodDef info)
        {
            var str = "mthd_" + (info.IsStatic ? "s_" : "") + GetTypeName(info.ParentType) + "_" + info.Name + "_";

            var ps = info.Parameters;
            for (int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + ps[i].Name + (ps[i].IsOut ? "_o" : "") + (ps[i].IsIn ? "_i" : "") + (ps[i].IsRetVal ? "_r" : "") + "_";
            }

            return str;
        }

        public static string GetMethodName(MethodInfo info)
        {
            var str = "mthd_" + (info.IsStatic ? "s_" : "") + GetTypeName(info.ReflectedType) + "_" + info.Name + "_";
            
            var ps = info.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + GetTypeName(ps[i].ParameterType) + (ps[i].IsOut ? "_o" : "") + (ps[i].IsIn ? "_i" : "") + (ps[i].IsRetval ? "_r" : "") + "_";
            }

            return str;
        }

        public static string GetMethodName(ConstructorInfo info)
        {
            var str = "mthd_" + (info.IsStatic ? "s_" : "") + GetTypeName(info.ReflectedType) + "_" + info.Name + "_";

            if (info.IsStatic && info.IsConstructor)
                str = "cctor_" + GetTypeName(info.ReflectedType) + "_";
            else if (info.IsConstructor)
                str = "ctor_" + GetTypeName(info.ReflectedType) + "_";

            var ps = info.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + GetTypeName(ps[i].ParameterType) + (ps[i].IsOut ? "_o" : "") + (ps[i].IsIn ? "_i" : "") + (ps[i].IsRetval ? "_r" : "") + "_";
            }

            return str;
        }

        public static string GetTypeName(TypeDef t)
        {
            var str = t.FullName.Replace('.', '_').Replace("[]", "_$arr_");
            return str;
        }

        public static string GetTypeName(Type t)
        {
            var str = t.FullName.Replace('.', '_').Replace("[]", "_$arr_");
            return str;
        }
    }
}
