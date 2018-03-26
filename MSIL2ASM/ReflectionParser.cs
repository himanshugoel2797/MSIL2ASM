using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public static class ReflectionParser
    {
        private static int StaticOffset = 0;
        private static int InstanceOffset = 0;

        public static ParameterDef[] ParseParams(ParameterInfo[] ps, ParameterInfo ret, bool isInst, bool isConstructor, string parentName, string name)
        {
            List<ParameterDef> defs = new List<ParameterDef>();

            if (isConstructor)
            {
                defs.Add(new ParameterDef()
                {
                    IsRetVal = true,
                    Name = name,
                    ParameterType = new TypeDef()
                    {
                        FullName = parentName,
                    }
                    //TODO handle ParameterType
                });
            }

            if (isInst)
            {
                defs.Add(new ParameterDef()
                {
                    IsIn = true,
                    Name = name,
                    Index = 0,
                    ParameterType = new TypeDef()
                    {
                        FullName = parentName
                    }
                    //TODO handle ParameterType
                });
            }

            for (int i = 0; i < ps.Length; i++)
            {
                defs.Add(new ParameterDef()
                {
                    IsIn = ps[i].IsIn,
                    IsOut = ps[i].IsOut,
                    IsRetVal = ps[i].IsRetval,
                    Index = ps[i].Position + (isInst ? 1 : 0),
                    Name = MachineSpec.GetTypeName(ps[i].ParameterType),
                    ParameterType = new TypeDef()
                    {
                        FullName = ps[i].ParameterType.FullName,
                        IsGenericParameter = ps[i].ParameterType.IsGenericParameter,
                        IsGenericType = ps[i].ParameterType.IsGenericType,
                    }
                    //TODO handle ParameterType
                });
            }

            if (ret != null)
                defs.Add(new ParameterDef()
                {
                    IsIn = ret.IsIn,
                    IsOut = ret.IsOut,
                    IsRetVal = true,
                    Index = defs.Count,
                    Name = MachineSpec.GetTypeName(ret.ParameterType),
                    ParameterType = new TypeDef()
                    {
                        FullName = ret.ParameterType.FullName,
                        IsGenericParameter = ret.ParameterType.IsGenericParameter,
                        IsGenericType = ret.ParameterType.IsGenericType,
                    }
                    //TODO handle ParameterType
                });

            if (isConstructor)
                defs[0].Index = defs.Count - 1;

            return defs.OrderBy(a =>
            {
                return a.Index + a.Name + (a.IsOut ? "_o" : "") + (a.IsIn ? "_i" : "") + (a.IsRetVal ? "_r" : "");
            }).ToArray();
        }

        private static FieldDef ParseField(FieldInfo fakeInst, FieldInfo realInst)
        {
            int sz = MachineSpec.PointerSize;
            if (realInst.FieldType.IsValueType)
                sz = Marshal.SizeOf(realInst.FieldType);

            int offset = 0;
            if (realInst.IsStatic)
            {
                offset = StaticOffset;
                StaticOffset += sz;
            }
            else
            {
                offset = InstanceOffset;
                InstanceOffset += sz;
            }

            return new FieldDef()
            {
                Name = fakeInst.Name,
                IsStatic = realInst.IsStatic,
                MetadataToken = fakeInst.MetadataToken,
                Size = sz,
                Offset = offset,
                FieldType = new TypeDef()
                {
                    FullName = realInst.FieldType.FullName,
                    IsGenericParameter = realInst.FieldType.IsGenericParameter,
                    IsGenericType = realInst.FieldType.IsGenericType,
                }
                //TODO figure out how to handle FieldType
            };
        }

        private static MethodDef ParseMethod(MethodInfo fakeInst, MethodInfo realInst, TypeDef tDef)
        {

            var mInfo = new MethodDef()
            {
                Name = fakeInst.Name,
                Aliases = new List<string>(),
                IsConstructor = false,
                IsStatic = fakeInst.IsStatic,
                IsInternalCall = realInst.MethodImplementationFlags == MethodImplAttributes.InternalCall,
                IsIL = realInst.MethodImplementationFlags == MethodImplAttributes.IL,
                Parameters = ParseParams(fakeInst.GetParameters(), realInst.ReturnParameter, !fakeInst.IsStatic, false, tDef.FullName, MachineSpec.GetTypeName(tDef)),
                ParentType = tDef,

                MetadataToken = fakeInst.MetadataToken,
            };

            if (realInst.MethodImplementationFlags == MethodImplAttributes.IL)
            {
                var bc = new SSAFormByteCode(realInst);
                bc.Initialize();
                mInfo.ByteCode = bc;

                var mBody = realInst.GetMethodBody();
                mInfo.StackSize = mBody.MaxStackSize;
                mInfo.Locals = new TypeDef[mBody.LocalVariables.Count];

                for (int j = 0; j < mBody.LocalVariables.Count; j++)
                {
                    mInfo.Locals[j] = new TypeDef()
                    {
                        Name = MachineSpec.GetTypeName(mBody.LocalVariables[j].LocalType),
                    };
                    mInfo.LocalsSize += MachineSpec.PointerSize;
                }
            }

            var aliasAttrs = realInst.GetCustomAttributes(typeof(CoreLib.AliasAttribute)).ToArray();
            for (int i = 0; i < aliasAttrs.Length; i++)
            {
                mInfo.Aliases.Add((aliasAttrs[i] as CoreLib.AliasAttribute).Name);
            }

            return mInfo;
        }

        private static MethodDef ParseMethod(ConstructorInfo fakeInst, ConstructorInfo realInst, TypeDef tDef)
        {

            var mInfo = new MethodDef()
            {
                Name = fakeInst.Name,
                Aliases = new List<string>(),
                IsConstructor = true,
                IsStatic = fakeInst.IsStatic,
                IsInternalCall = realInst.MethodImplementationFlags == MethodImplAttributes.InternalCall,
                IsIL = realInst.MethodImplementationFlags == MethodImplAttributes.IL,
                Parameters = ParseParams(fakeInst.GetParameters(), null, true, true, tDef.FullName, MachineSpec.GetTypeName(tDef)),
                ParentType = tDef,

                MetadataToken = fakeInst.MetadataToken,
            };

            if (realInst.MethodImplementationFlags == MethodImplAttributes.IL)
            {
                var bc = new SSAFormByteCode(realInst);
                bc.Initialize();
                mInfo.ByteCode = bc;

                var mBody = realInst.GetMethodBody();
                mInfo.StackSize = mBody.MaxStackSize;
                mInfo.Locals = new TypeDef[mBody.LocalVariables.Count];

                for (int j = 0; j < mBody.LocalVariables.Count; j++)
                {
                    mInfo.Locals[j] = new TypeDef()
                    {
                        Name = MachineSpec.GetTypeName(mBody.LocalVariables[j].LocalType),
                    };
                    mInfo.LocalsSize += MachineSpec.PointerSize;
                }
            }

            var aliasAttrs = realInst.GetCustomAttributes(typeof(CoreLib.AliasAttribute)).ToArray();
            for (int i = 0; i < aliasAttrs.Length; i++)
            {
                mInfo.Aliases.Add((aliasAttrs[i] as CoreLib.AliasAttribute).Name);
            }

            return mInfo;
        }

        public static TypeDef Parse(Type fakeType, Type realType)
        {
            TypeDef tDef = new TypeDef()
            {
                Name = fakeType.Name,
                FullName = fakeType.FullName,
                MetadataToken = fakeType.MetadataToken,
                IsValueType = fakeType.IsValueType,
                IsGenericParameter = fakeType.IsGenericParameter,
                IsGenericType = fakeType.IsGenericType,
            };

            StaticOffset = 0;
            InstanceOffset = 0;

            List<FieldDef> fieldList = new List<FieldDef>();
            List<MethodDef> mthdList = new List<MethodDef>();

            var instanceMembers = realType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < instanceMembers.Length; i++)
            {
                if (instanceMembers[i].MemberType == MemberTypes.Field)
                {
                    var inst = instanceMembers[i] as FieldInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetField(inst.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fm != null) fieldList.Add(ParseField(inst, fm));
                }
                else if (instanceMembers[i].MemberType == MemberTypes.Method)
                {
                    var inst = instanceMembers[i] as MethodInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetMethod(inst.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, inst.GetParameters().Select(a => a.ParameterType).ToArray(), null);
                    if (fm != null) mthdList.Add(ParseMethod(inst, fm, tDef));
                }
                else if (instanceMembers[i].MemberType == MemberTypes.Constructor)
                {
                    var inst = instanceMembers[i] as ConstructorInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, inst.GetParameters().Select(a => a.ParameterType).ToArray(), null);
                    if (fm != null) mthdList.Add(ParseMethod(inst, fm, tDef));
                }
            }

            tDef.InstanceMethods = mthdList.ToArray();
            tDef.InstanceFields = fieldList.ToArray();

            mthdList.Clear();
            fieldList.Clear();

            var staticMembers = realType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < staticMembers.Length; i++)
            {
                if (staticMembers[i].MemberType == MemberTypes.Field)
                {
                    var inst = staticMembers[i] as FieldInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetField(inst.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (fm != null) fieldList.Add(ParseField(inst, fm));
                }
                else if (staticMembers[i].MemberType == MemberTypes.Method)
                {
                    var inst = staticMembers[i] as MethodInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetMethod(inst.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, inst.GetParameters().Select(a => a.ParameterType).ToArray(), null);
                    if (fm != null) mthdList.Add(ParseMethod(inst, fm, tDef));
                }
                else if (staticMembers[i].MemberType == MemberTypes.Constructor)
                {
                    var inst = staticMembers[i] as ConstructorInfo;
                    var fm = TypeMapper.ResolveType(inst.DeclaringType).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, inst.GetParameters().Select(a => a.ParameterType).ToArray(), null);
                    if (fm != null) mthdList.Add(ParseMethod(inst, fm, tDef));
                }
            }

            tDef.StaticMethods = mthdList.ToArray();
            tDef.StaticFields = fieldList.ToArray();

            tDef.InstanceSize = InstanceOffset;
            tDef.StaticSize = StaticOffset;

            return tDef;
        }

        public static void Reinit(Dictionary<string, TypeDef> tDefs)
        {
            for (int i = 0; i < tDefs.Keys.Count; i++)
            {
                var t = tDefs.Keys.ElementAt(i);
                for (int j = 0; j < tDefs[t].InstanceFields.Length; j++)
                {
                    var key = tDefs[t].InstanceFields[j].FieldType;

                    if (!key.IsGenericParameter)
                        tDefs[t].InstanceFields[j].FieldType = tDefs[key.FullName];
                }

                for (int j = 0; j < tDefs[t].StaticFields.Length; j++)
                {
                    var key = tDefs[t].StaticFields[j].FieldType;

                    if (!key.IsGenericParameter)
                        tDefs[t].StaticFields[j].FieldType = tDefs[key.FullName];
                }

                for (int j = 0; j < tDefs[t].InstanceMethods.Length; j++)
                {
                    for(int k = 0; k < tDefs[t].InstanceMethods[j].Parameters.Length; k++)
                    {
                        var key = tDefs[t].InstanceMethods[j].Parameters[k].ParameterType;

                        if (!key.IsGenericParameter && tDefs.ContainsKey(key.FullName))
                            tDefs[t].InstanceMethods[j].Parameters[k].ParameterType = tDefs[key.FullName];
                    }
                }

                for (int j = 0; j < tDefs[t].StaticMethods.Length; j++)
                {
                    for (int k = 0; k < tDefs[t].StaticMethods[j].Parameters.Length; k++)
                    {
                        var key = tDefs[t].StaticMethods[j].Parameters[k].ParameterType;

                        if (!key.IsGenericParameter && tDefs.ContainsKey(key.FullName))
                            tDefs[t].StaticMethods[j].Parameters[k].ParameterType = tDefs[key.FullName];
                    }
                }
            }
        }
    }
}
