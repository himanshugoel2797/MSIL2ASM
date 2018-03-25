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

        private static ParameterDef[] ParseParams(ParameterInfo[] ps, bool isInst, bool isConstructor, string parentName)
        {
            List<ParameterDef> defs = new List<ParameterDef>();

            if (isConstructor)
            {
                defs.Add(new ParameterDef()
                {
                    IsRetVal = true,
                    Name = parentName,
                    //TODO handle ParameterType
                });
            }

            if (isInst)
            {
                defs.Add(new ParameterDef()
                {
                    IsIn = true,
                    Name = parentName,
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
                    Name = MachineSpec.GetTypeName(ps[i].ParameterType),
                    //TODO handle ParameterType
                });
            }

            return defs.ToArray();
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
                //TODO figure out how to handle FieldType
            };
        }

        private static MethodDef ParseMethod(MethodInfo fakeInst, MethodInfo realInst, TypeDef tDef)
        {

            var mInfo = new MethodDef()
            {
                Name = fakeInst.Name,
                IsConstructor = false,
                IsStatic = fakeInst.IsStatic,
                IsInternalCall = realInst.MethodImplementationFlags == MethodImplAttributes.InternalCall,
                IsIL = realInst.MethodImplementationFlags == MethodImplAttributes.IL,
                Parameters = ParseParams(fakeInst.GetParameters(), !fakeInst.IsStatic, false, MachineSpec.GetTypeName(tDef)),
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
                    //TODO figure out how to handle Locals TypeDef

                    //if (mBody.LocalVariables[j].LocalType.IsValueType)
                    //    mInfo.LocalsSize += Marshal.SizeOf(mBody.LocalVariables[j].LocalType);
                    //else
                    mInfo.LocalsSize += MachineSpec.PointerSize;
                }
            }

            return mInfo;
        }

        private static MethodDef ParseMethod(ConstructorInfo fakeInst, ConstructorInfo realInst, TypeDef tDef)
        {

            var mInfo = new MethodDef()
            {
                Name = fakeInst.Name,
                IsConstructor = true,
                IsStatic = fakeInst.IsStatic,
                IsInternalCall = realInst.MethodImplementationFlags == MethodImplAttributes.InternalCall,
                IsIL = realInst.MethodImplementationFlags == MethodImplAttributes.IL,
                Parameters = ParseParams(fakeInst.GetParameters(), true, true, MachineSpec.GetTypeName(tDef)),
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
                    //TODO figure out how to handle Locals TypeDef
                    mInfo.Locals[j] = new TypeDef()
                    {
                        Name = MachineSpec.GetTypeName(mBody.LocalVariables[j].LocalType),
                    };
                }
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
    }
}
