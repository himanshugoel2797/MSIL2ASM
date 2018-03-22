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
        private static Dictionary<string, IAssemblyBackend> backends;

        public static void SetTypeMappings(Dictionary<Type, Type> mappings)
        {
            Mappings = mappings;
        }

        public static MemberInfo ResolveMember(Type t, int tkn)
        {
            var req_mem = t.Module.ResolveMember(tkn);
            if (Mappings.ContainsKey(req_mem.DeclaringType))
            {
                var actual_mems = Mappings[req_mem.DeclaringType].GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (MemberInfo a in actual_mems)
                {
                    if (a.MemberType == req_mem.MemberType && a.Name == req_mem.Name)
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
                                    if (ps[i].IsOut != ps_o[i].IsOut)
                                        isMatch = false;
                                    if (ps[i].IsIn != ps_o[i].IsIn)
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

        public static IAssemblyBackend ResolveBackend(string name)
        {
            if (backends.ContainsKey(name))
                return backends[name];
            return null;
        }

        internal static void SetBackends(Dictionary<string, IAssemblyBackend> bs)
        {
            backends = bs;
        }
    }
}
