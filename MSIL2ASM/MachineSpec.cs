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

        private static string GetMethodName(bool isStatic, bool isCtor, string parentName, string name, ParameterDef[] ps)
        {
            var str = "mthd_" + (isStatic ? "s_" : "") + parentName + "_" + name + "_";

            if (isStatic && isCtor)
                str = "cctor_" + parentName + "_";
            else if (isCtor)
                str = "ctor_" + parentName + "_";

            for (int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + ps[i].Name + (ps[i].IsOut ? "_o" : "") + (ps[i].IsIn ? "_i" : "") + (ps[i].IsRetVal ? "_r" : "") + "_";
            }

            return str;
        }

        public static string GetMethodName(MethodDef info)
        {
            return GetMethodName(info.IsStatic, info.IsConstructor, GetTypeName(info.ParentType), info.Name, info.Parameters);
        }

        public static string GetMethodName(MethodInfo info)
        {
            var p0 = ReflectionParser.ParseParams(info.GetParameters(), info.ReturnParameter, !info.IsStatic, false, info.ReflectedType.FullName, GetTypeName(info.ReflectedType));
            return GetMethodName(info.IsStatic, false, GetTypeName(info.ReflectedType), info.Name, p0);
        }

        public static string GetMethodName(ConstructorInfo info)
        {
            var p0 = ReflectionParser.ParseParams(info.GetParameters(), null, !info.IsStatic, true, info.ReflectedType.FullName, GetTypeName(info.ReflectedType));
            return GetMethodName(info.IsStatic, true, GetTypeName(info.ReflectedType), info.Name, p0);
        }

        public static string GetTypeName(TypeDef t)
        {
            var str = t.FullName.Replace('.', '_').Replace("`", "_$generic_").Replace("&", "_$addr").Replace("[]", "_$arr_");
            return str;
        }

        public static string GetTypeName(Type t)
        {
            if (t.FullName == null)
                return t.Name + "_" + t.DeclaringType.FullName.Replace('.', '_').Replace("`", "_$generic_").Replace("&", "_$addr").Replace("[]", "$_arr_");

            var str = t.FullName.Replace('.', '_').Replace("`", "_$generic_").Replace("&", "_$addr").Replace("[]", "_$arr_");
            return str;
        }
    }
}
