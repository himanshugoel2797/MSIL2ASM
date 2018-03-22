using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSIL2ASM
{
    public interface IAssemblyBackend
    {
        void Reset(Type t, Type s);
        void Save(string outputDir);
        void AddStaticMethod(MethodInfo m, MethodInfo f);
        void AddStaticMember(MemberInfo m);
        void AddInstanceMethod(MethodInfo m, MethodInfo f);
        void AddInstanceMember(MemberInfo m);
        void Compile(Dictionary<string, IAssemblyBackend> backends);
        void Resolve(Dictionary<string, IAssemblyBackend> backends);

        int InstanceSize { get; set; }
        int StaticSize { get; set; }

        void AddInstanceConstructor(ConstructorInfo m, ConstructorInfo fakeInfo);
        void AddStaticConstructor(ConstructorInfo m, ConstructorInfo f);
    }

    public interface IAssemblyBackendProvider
    {
        IAssemblyBackend GetAssemblyBackend();
    }
}