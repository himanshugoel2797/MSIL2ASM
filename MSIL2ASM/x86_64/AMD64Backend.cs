using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64
{
    public class AMD64BackendProvider : IAssemblyBackendProvider
    {
        public IAssemblyBackend GetAssemblyBackend()
        {
            return new AMD64Backend();
        }
    }

    partial class AMD64Backend : IAssemblyBackend
    {
        class MemberDesc
        {
            public MemberInfo info;
            public int size;
            public int offset;
        }

        class MethodDesc
        {
            public string methodName;
            public MethodInfo info;
            public MethodInfo fakeInfo;
            public SSAFormByteCode stream;
            public int offset;
        }

        class CtorDesc
        {
            public string methodName;
            public ConstructorInfo info;
            public ConstructorInfo fakeInfo;
            public SSAFormByteCode stream;
        }

        public string TypeName { get; private set; }
        public Type TargetType { get; private set; }
        public Type SearchType { get; private set; }

        private List<MemberDesc> StaticMembers;
        private List<MemberDesc> InstanceMembers;

        private List<MethodDesc> StaticMethods;
        private List<MethodDesc> InstanceMethods;

        private List<CtorDesc> StaticCtors;
        private List<CtorDesc> InstanceCtors;

        private Nasm.NasmEmitter Emitter;

        public const int PointerSize = 8;
        public const int EnumSize = 4;

        private int PrivInstanceSize { get; set; }

        public int VTableSize { get; set; }
        public int InstanceSize
        {
            get
            {
                int rVal = PrivInstanceSize;

                if (!TargetType.IsValueType && TargetType.BaseType != null)
                {
                    rVal += (TypeMapper.ResolveBackend(TargetType.BaseType.FullName) as AMD64Backend).InstanceSize;
                }

                return rVal;
            }
        }
        public int StaticSize { get; set; }


        public void AddInstanceMember(MemberInfo m)
        {
            var m0 = m as FieldInfo;
            InstanceMembers.Add(new MemberDesc()
            {
                info = m,
                size = m0.FieldType.IsValueType ? (m0.FieldType.IsEnum ? EnumSize : Marshal.SizeOf((m as FieldInfo).FieldType)) : PointerSize
            });
        }

        public void AddInstanceMethod(MethodInfo m, MethodInfo fakeInfo)
        {
            InstanceMethods.Add(new MethodDesc()
            {
                info = m,
                fakeInfo = fakeInfo
            });
        }

        public void AddStaticMember(MemberInfo m)
        {
            var m0 = m as FieldInfo;
            StaticMembers.Add(new MemberDesc()
            {
                info = m,
                size = m0.FieldType.IsValueType ? (m0.FieldType.IsEnum ? EnumSize : Marshal.SizeOf((m as FieldInfo).FieldType)) : PointerSize
            });
        }

        public void AddStaticMethod(MethodInfo m, MethodInfo fakeInfo)
        {
            StaticMethods.Add(new MethodDesc()
            {
                info = m,
                fakeInfo = fakeInfo
            });
        }

        public void Compile(Dictionary<string, IAssemblyBackend> backends)
        {

            List<KeyValuePair<MethodInfo, MethodInfo>> BaseMethods = new List<KeyValuePair<MethodInfo, MethodInfo>>();
            List<MethodInfo> CurrentMethods = new List<MethodInfo>();

            //First make a vtable for the base class
            if (!TargetType.IsValueType)
                for (int i = 0; i < InstanceMethods.Count; i++)
                {
                    if (InstanceMethods[i].fakeInfo.IsAbstract | InstanceMethods[i].fakeInfo.IsVirtual)
                    {
                        var baseInfo = TargetType.BaseType?.GetMethod(InstanceMethods[i].fakeInfo.Name, InstanceMethods[i].fakeInfo.GetParameters().Select(a => a.ParameterType).ToArray());
                        if (baseInfo != null)
                        {
                            //Add to base class vtable
                            BaseMethods.Add(new KeyValuePair<MethodInfo, MethodInfo>(InstanceMethods[i].fakeInfo, baseInfo));//TypeMapper.ResolveMember(baseInfo.DeclaringType, baseInfo.MetadataToken) as MethodInfo));                                                                                   //Emitter.AddVtableEntry(GetMethodName(InstanceMethods[i].fakeInfo));
                        }
                        else
                        {
                            //Add to current class vtable
                            CurrentMethods.Add(InstanceMethods[i].fakeInfo);
                        }
                    }
                }


            if (TargetType.BaseType != null)
            {
                Emitter.EmitVtable(GetTypeName(TargetType) + "_super");
                var baseMethodsSorted = new MethodInfo[BaseMethods.Count];
                var baseType = backends[TargetType.BaseType.FullName] as AMD64Backend;
                for (int i = 0; i < BaseMethods.Count; i++)
                {
                    baseMethodsSorted[baseType.GetMethodOffset(BaseMethods[i].Value) / 8] = BaseMethods[i].Key;
                }

                for (int i = 0; i < baseMethodsSorted.Length; i++)
                {
                    Emitter.AddVtableEntry(GetMethodName(baseMethodsSorted[i]));
                }
            }


            Emitter.EmitVtable(GetTypeName(TargetType));
            var curMethodsSorted = new MethodInfo[CurrentMethods.Count];
            for (int i = 0; i < CurrentMethods.Count; i++)
            {
                curMethodsSorted[GetMethodOffset(CurrentMethods[i]) / 8] = CurrentMethods[i];
            }

            for (int i = 0; i < curMethodsSorted.Length; i++)
            {
                Emitter.AddVtableEntry(GetMethodName(curMethodsSorted[i]));
            }

            //Make vtables for the interfaces
            var interfaces = TargetType.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Emitter.EmitVtable(GetTypeName(TargetType) + "_" + GetTypeName(interfaces[i]) + "_ITable");
                var mapping = TargetType.GetInterfaceMap(interfaces[i]);

                for (int j = 0; j < mapping.InterfaceMethods.Length; j++)
                {
                    Emitter.AddVtableEntry(GetMethodName(mapping.TargetMethods[i]));
                }
            }

            //Add vtables to the class layout

            {
                for (int i = 0; i < StaticMethods.Count; i++)
                {
                    var m = StaticMethods[i];
                    CompileMethod(ref m, backends);
                    StaticMethods[i] = m;
                }
            }

            {
                for (int i = 0; i < InstanceMethods.Count; i++)
                {
                    var m = InstanceMethods[i];
                    CompileMethod(ref m, backends);
                    InstanceMethods[i] = m;
                }
            }

            {
                for (int i = 0; i < StaticCtors.Count; i++)
                {
                    var m = StaticCtors[i];
                    CompileMethod(ref m, backends);
                    StaticCtors[i] = m;
                }
            }

            {
                for (int i = 0; i < InstanceCtors.Count; i++)
                {
                    var m = InstanceCtors[i];
                    CompileMethod(ref m, backends);
                    InstanceCtors[i] = m;
                }
            }

            //Emit thunks for casting

        }

        public void Reset(Type t, Type searchType)
        {
            TypeName = t.FullName;
            TargetType = t;
            SearchType = searchType;
            StaticMembers = new List<MemberDesc>();
            StaticMethods = new List<MethodDesc>();

            InstanceMembers = new List<MemberDesc>();
            InstanceMethods = new List<MethodDesc>();

            InstanceCtors = new List<CtorDesc>();
            StaticCtors = new List<CtorDesc>();

        }

        public int GetMethodOffset(MethodInfo info)
        {
            for (int i = 0; i < InstanceMethods.Count; i++)
            {
                if (InstanceMethods[i].fakeInfo == info)
                    return InstanceMethods[i].offset;
            }
            throw new Exception();
        }

        public void Resolve(Dictionary<string, IAssemblyBackend> backends)
        {
            Emitter = new Nasm.NasmEmitter(backends);

            {
                int offset = 0;
                for (int i = 0; i < StaticMembers.Count; i++)
                {
                    while (StaticMembers[i].size <= PointerSize && offset % StaticMembers[i].size != 0)
                        offset++;

                    if (StaticMembers[i].size > PointerSize && offset % PointerSize != 0)
                        offset += PointerSize - (offset % PointerSize);

                    StaticMembers[i].offset = offset;
                    offset += StaticMembers[i].size;
                }
                Emitter.EmitStaticStruct(GetTypeName(TargetType) + "_static", StaticSize);
            }

            {
                bool packed = false;

                var attr = TargetType.GetCustomAttributesData();
                foreach (CustomAttributeData a in attr)
                {
                    if (a.AttributeType == typeof(StructLayoutAttribute))
                    {
                        if (a.NamedArguments.First(x => x.MemberName == "Pack").TypedValue.Value == (object)1)
                            packed = true;
                    }
                }

                int offset = AMD64Backend.PointerSize;
                for (int i = 0; i < InstanceMembers.Count; i++)
                {
                    if (TargetType.IsValueType)
                    {
                        InstanceMembers[i].offset = (int)Marshal.OffsetOf(TargetType, InstanceMembers[i].info.Name) + AMD64Backend.PointerSize;
                    }
                    else
                    {
                        if (!packed)
                        {
                            while (InstanceMembers[i].size <= PointerSize && offset % InstanceMembers[i].size != 0)
                                offset++;

                            if (InstanceMembers[i].size > PointerSize && offset % PointerSize != 0)
                                offset += PointerSize - (offset % PointerSize);
                        }

                        InstanceMembers[i].offset = offset;
                        offset += InstanceMembers[i].size;
                    }
                }
                if (TargetType.IsValueType)
                {
                    PrivInstanceSize = Marshal.SizeOf(TargetType);
                }
                else
                {
                    PrivInstanceSize = offset;
                }
            }

            {
                for (int i = 0; i < StaticMethods.Count; i++)
                {
                    //Generate the method name
                    StaticMethods[i].methodName = GetMethodName(StaticMethods[i].fakeInfo);
                }
            }

            {
                int offset = 0;
                for (int i = 0; i < InstanceMethods.Count; i++)
                {
                    //Generate the method name
                    InstanceMethods[i].methodName = GetMethodName(InstanceMethods[i].fakeInfo);

                    if (InstanceMethods[i].fakeInfo.IsAbstract | InstanceMethods[i].fakeInfo.IsVirtual)
                    {
                        InstanceMethods[i].offset = offset;
                        offset += PointerSize;
                    }
                }
                VTableSize = offset;
            }

            {
                for (int i = 0; i < StaticCtors.Count; i++)
                {
                    //Generate the method name
                    StaticCtors[i].methodName = GetMethodName(StaticCtors[i].fakeInfo);
                }
            }

            {
                for (int i = 0; i < InstanceCtors.Count; i++)
                {
                    //Generate the method name
                    InstanceCtors[i].methodName = GetMethodName(InstanceCtors[i].fakeInfo);
                }
            }
        }

        public void Save(string outputDir)
        {
            File.WriteAllText(Path.Combine(outputDir, TypeName.Replace('.', '_') + ".S"), Emitter.GetFile());
        }

        public void AddInstanceConstructor(ConstructorInfo m, ConstructorInfo fakeInfo)
        {
            InstanceCtors.Add(new CtorDesc()
            {
                info = m,
                fakeInfo = fakeInfo
            });
        }

        public void AddStaticConstructor(ConstructorInfo m, ConstructorInfo fakeInfo)
        {
            StaticCtors.Add(new CtorDesc()
            {
                info = m,
                fakeInfo = fakeInfo
            });
        }

        public void GetFieldDesc(int tkn, out int off, out int sz)
        {
            foreach (MemberDesc m in StaticMembers)
            {
                if (m.info.MetadataToken == tkn)
                {
                    off = m.offset;
                    sz = m.size;
                    return;
                }
            }

            foreach (MemberDesc m in InstanceMembers)
            {
                if (m.info.MetadataToken == tkn)
                {
                    off = m.offset;
                    sz = m.size;
                    return;
                }
            }

            throw new Exception("Field not found!");
        }
    }
}
