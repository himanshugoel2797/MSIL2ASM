﻿using System;
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
        private static Dictionary<Type, Type> RMappings;

        public static void SetTypeMappings(Dictionary<Type, Type> mappings)
        {
            Mappings = mappings;
            RMappings = new Dictionary<Type, Type>();

            foreach (KeyValuePair<Type, Type> k in Mappings)
            {
                RMappings[k.Value] = k.Key;
            }
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

        public static MemberInfo ReverseResolveMember(Type t, int tkn)
        {
            var req_mem = t.Module.ResolveMember(tkn);
            if (RMappings.ContainsKey(req_mem.DeclaringType))
            {
                var actual_mems = RMappings[req_mem.DeclaringType].GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

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
    }
}
