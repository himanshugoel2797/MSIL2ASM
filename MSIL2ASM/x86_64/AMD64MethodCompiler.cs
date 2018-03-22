using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64
{
    partial class AMD64Backend : IAssemblyBackend
    {
        private void CompileMethod(ref MethodDesc desc, Dictionary<string, IAssemblyBackend> types)
        {
            //Interface or abstract method
            if (desc.info.GetMethodBody() == null)
                return;

            if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.IL)
            {
                desc.stream = new SSAFormByteCode(desc.info);
                desc.stream.Initialize();

                //Generate an instruction stream
                CompileILMethod(ref desc, types);
            }
            else if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.InternalCall)
            {
                //TODO Is a special method implemented by the runtime
            }
            else if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.Native)
            {
                //TODO Is a native method, generate a stub to make the correct call
                throw new NotImplementedException();
            }
            else
                throw new Exception(desc.info.GetMethodImplementationFlags().ToString());
        }

        private void CompileMethod(ref CtorDesc desc, Dictionary<string, IAssemblyBackend> types)
        {
            //Interface or abstract method
            if (desc.info.GetMethodBody() == null)
                return;

            if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.IL)
            {
                desc.stream = new SSAFormByteCode(desc.info);
                desc.stream.Initialize();

                //Generate an instruction stream
                CompileILMethod(ref desc, types);
            }
            else if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.InternalCall)
            {
                //TODO Is a special method implemented by the runtime
            }
            else if (desc.info.GetMethodImplementationFlags() == MethodImplAttributes.Native)
            {
                //TODO Is a native method, generate a stub to make the correct call
                throw new NotImplementedException();
            }
            else
                throw new Exception(desc.info.GetMethodImplementationFlags().ToString());
        }

        public ConstructorInfo GetCtorInfo(int token)
        {
            return SearchType.Module.ResolveMethod(token) as ConstructorInfo;
        }

        public Type GetTypeInfo(int token)
        {
            return SearchType.Module.ResolveType(token);
        }

        public MethodInfo GetMethodInfo(int token)
        {
            return SearchType.Module.ResolveMethod(token) as MethodInfo;
        }

        public FieldInfo GetFieldInfo(int token)
        {
            return SearchType.Module.ResolveField(token) as FieldInfo;
        }

        public static string GetMethodName(MethodInfo info)
        {
            var str = "mthd_" + GetTypeName(info.ReflectedType) + "_" + info.Name + "_";

            var ps = info.GetParameters();
            for(int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + ps[i].ParameterType.Name[0].ToString() + (ps[i].IsOut ? "o" : "") + (ps[i].IsRetval ? "r" : "") + "_";
            }

            return str;
        }

        public static string GetMethodName(ConstructorInfo info)
        {
            var str = "ctor_" + GetTypeName(info.ReflectedType) + "_";

            var ps = info.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                str += i.ToString() + ps[i].ParameterType.Name[0].ToString() + (ps[i].IsOut ? "o" : "") + (ps[i].IsRetval ? "r" : "") + "_";
            }

            return str;
        }

        public static string GetTypeName(Type t)
        {
            var str = t.FullName.Replace('.', '_').Replace("[]", "_$arr_");
            return str;
        }

        private void CompileILMethod(ref MethodDesc desc, Dictionary<string, IAssemblyBackend> types)
        {
            //Generate a set of instructions for each SSAToken
            Emitter.Process(desc.fakeInfo, desc.info, desc.stream.GetTokens(), desc.stream.GetStrings());
        }

        private void CompileILMethod(ref CtorDesc desc, Dictionary<string, IAssemblyBackend> types)
        {
            //Generate a set of instructions for each SSAToken
            Emitter.Process(desc.fakeInfo, desc.info, desc.stream.GetTokens(), desc.stream.GetStrings());
        }
    }
}
