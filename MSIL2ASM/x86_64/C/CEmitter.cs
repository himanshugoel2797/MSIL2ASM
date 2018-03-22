using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.C
{
    partial class CEmitter
    {
        private Dictionary<string, IAssemblyBackend> types;
        private List<string> Lines;
        private List<string> Signatures;

        public CEmitter(Dictionary<string, IAssemblyBackend> types)
        {
            Lines = new List<string>();
            Signatures = new List<string>();
            this.types = types;
        }

        private string GetTypeName(Type t)
        {
            return t.FullName.Replace('.', '_').Replace("[", "").Replace("]", "");
        }

        private string GetTypeRefName(Type t)
        {
            if (t.IsValueType)
                return GetTypeName(t);
            return GetTypeName(t) + "*";
        }

        private string GenerateMethodName(MethodInfo info)
        {
            string sig = "";
            sig += GetTypeName(info.DeclaringType);
            sig += "_" + info.Name;

            return sig;
        }

        private string GenerateSignature(MethodInfo info)
        {
            string sig = "";
            var retType = info.ReturnType;

            sig += GetTypeRefName(retType);
            sig += " ";
            sig += GenerateMethodName(info);
            sig += "(";

            var p = info.GetParameters();
            for (int i = 0; i < p.Length; i++)
            {
                var pi = p[i];

                sig += GetTypeRefName(pi.ParameterType);
                sig += " " + GenerateParameterName(i++);

                if (i < p.Length - 1)
                    sig += ",";
            }

            sig += ")";

            return sig;
        }

        private string GenerateVariableName(int id)
        {
            return "_var" + id.ToString() + "_";
        }

        private string GenerateParameterName(int id)
        {
            return "_par" + id.ToString() + "_";
        }

        public void Process(MethodInfo info, SSAToken[] tokens, string[] strtab)
        {
            var backend = types[info.ReflectedType.FullName] as AMD64Backend;

            //Generate the method entry stub
            var sig = GenerateSignature(info);
            Lines.Add(sig + "{");
            Signatures.Add(sig + ";");

            for (int i = 0; i < tokens.Length; i++)
            {
                var tkn = tokens[i];

                switch (tkn.Operation)
                {
                    case InstructionTypes.Newobj:
                        EmitNewobj(backend, tkn);
                        break;
                    case InstructionTypes.Ret:
                        EmitRet(tkn);
                        break;
                    case InstructionTypes.Call:
                        EmitCall(backend, tkn);
                        break;
                    case InstructionTypes.LdStr:
                        EmitLdStr(tkn, strtab);
                        break;
                    case InstructionTypes.Stsfld:
                        EmitStsfld(backend, tkn);
                        break;
                    case InstructionTypes.Nop:

                        break;
                    default:
                        throw new NotImplementedException(tkn.Operation.ToString());
                        break;
                }
            }
        }

        private void EmitStsfld(AMD64Backend backend, SSAToken tkn)
        {
            //Determine the field, and assign the variable to it
        }

        private void EmitCall(AMD64Backend backend, SSAToken tkn)
        {
            var mthd = backend.GetMethodInfo((int)tkn.Constants[0]);

            //CEmitCall(AMD64Backend.GetMethodName(mthd.DeclaringType, (int)tkn.Constants[0]), tkn.ID, mthd.ReturnType != typeof(void), tkn.Parameters);
        }

        private void EmitLdStr(SSAToken tkn, string[] strtab)
        {
            //Declare string variable with the given value
            CEmitInit(GetTypeName(typeof(string)), tkn.ID, "\"" + strtab[tkn.Constants[0] - 1] + "\"");
        }

        public void EmitRet(SSAToken tkn)
        {
            //Generate return code
            if (tkn.Parameters != null)
                CEmitRet(tkn.Parameters[0]);
            else
                CEmitRet(0);
        }

        public void EmitNewobj(AMD64Backend backend, SSAToken tkn)
        {
            var ctor = backend.GetCtorInfo((int)tkn.Constants[0]);

            //allocate object
            if (ctor.DeclaringType.IsValueType)
            {
                CEmitDecl(GetTypeRefName(ctor.DeclaringType), tkn.ID);
            }
            else
            {
                CEmitObjAlloc(GetTypeRefName(ctor.DeclaringType), GetTypeName(ctor.DeclaringType), tkn.ID);
            }

            //call the constructor for the object
            List<int> ps = new List<int>();
            ps.Add(tkn.ID);
            ps.AddRange(tkn.Parameters);

            //CEmitCall((types[ctor.ReflectedType.FullName] as AMD64Backend).GetMethodName((int)tkn.Constants[0]), tkn.ID, false, ps.ToArray());
        }
    }
}
