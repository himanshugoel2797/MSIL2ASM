using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public static class TypeMapper
    {
        private static Dictionary<Type, Type> Mappings;

        public static void SetTypeMappings(Dictionary<Type, Type> mappings)
        {
            Mappings = mappings;
        }

        public static MemberInfo ResolveMember(Type t, int tkn)
        {
            if (Mappings.ContainsKey(t))
            {
                var req_mem = t.Module.ResolveMember(tkn);
                var actual_mems = Mappings[t].GetMember(req_mem.Name);

                foreach (MemberInfo a in actual_mems)
                {
                    if (a.MemberType == req_mem.MemberType)
                    {
                        if (new MemberTypes[] { MemberTypes.Constructor, MemberTypes.Method }.Contains(a.MemberType))
                        {
                            var ps = (a as MethodInfo) == null ? (a as ConstructorInfo).GetParameters() : (a as MethodInfo).GetParameters();
                            var ps_o = (req_mem as MethodInfo) == null ? (req_mem as ConstructorInfo).GetParameters() : (req_mem as MethodInfo).GetParameters();

                            if (ps.Length == ps_o.Length)
                            {
                                bool isMatch = true;
                                for (int i = 0; i < ps.Length; i++)
                                {
                                    if (ps[i].ParameterType != ps_o[i].ParameterType)
                                        isMatch = false;
                                }
                                if (isMatch)
                                    return a;
                            }
                        }
                        else
                            return a;
                    }
                }
            }

            return t.Module.ResolveMember(tkn);
        }

        public static Type ResolveType(Type t)
        {
            if (Mappings.ContainsKey(t))
                return Mappings[t];
            return t;
        }
    }
}
