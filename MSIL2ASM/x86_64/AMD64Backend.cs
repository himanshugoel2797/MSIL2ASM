﻿using System;
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

        public int InstanceSize { get; set; }
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
                StaticSize = offset;
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

                int offset = 0;
                for (int i = 0; i < InstanceMembers.Count; i++)
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
                InstanceSize = offset;
            }

            {
                for (int i = 0; i < StaticMethods.Count; i++)
                {
                    //Generate the method name
                    StaticMethods[i].methodName = GetMethodName(StaticMethods[i].fakeInfo);
                }
            }

            {
                for (int i = 0; i < InstanceMethods.Count; i++)
                {
                    //Generate the method name
                    InstanceMethods[i].methodName = GetMethodName(InstanceMethods[i].fakeInfo);
                }
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