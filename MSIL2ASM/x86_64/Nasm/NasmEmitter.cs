using MSIL2ASM.x86_64.Nasm.Assembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    partial class NasmEmitter
    {
        class LocalAllocs
        {
            public int sz;
            public int id;
            public int TypeSize;
            public bool ValueKnown;
            public ulong Value;
        }

        private Dictionary<string, IAssemblyBackend> types;
        private List<string> Lines;
        private List<string> data;
        private List<string> bss;
        private List<string> externals;
        private List<string> stringTable;
        private List<string> static_ctors;

        private string prefix;
        private InstrEmitter Emitter;
        private int ArgumentTopOffset = 0;
        private int LocalTopOffset = 0;
        private int SpillTopOffset = 0;
        private int SpillCurOffset = 0;
        private List<LocalAllocs> Locals;

        public NasmEmitter(Dictionary<string, IAssemblyBackend> types)
        {
            Lines = new List<string>();
            data = new List<string>();
            bss = new List<string>();
            stringTable = new List<string>();
            externals = new List<string>();
            static_ctors = new List<string>();

            this.types = types;
        }

        private string GetTypeName(Type t)
        {
            return AMD64Backend.GetTypeName(t);
        }

        private string GetTypeRefName(Type t)
        {
            if (t.IsValueType)
                return GetTypeName(t);
            return GetTypeName(t) + "*";
        }

        private string GenerateMethodName(MethodInfo info)
        {
            return AMD64Backend.GetMethodName(info);
        }

        private string GenerateMethodName(ConstructorInfo info)
        {
            return AMD64Backend.GetMethodName(info);
        }

        private string GenerateVariableName(int id)
        {
            return ".var" + id.ToString() + "_";
        }

        private string GenerateParameterName(int id)
        {
            return "_par" + id.ToString() + "_";
        }

        public string GetFile()
        {
            string file = "[BITS 64]\n";

            file += "\n";
            file += "section .data\n";
            foreach (string s in data)
                file += s + "\n";

            file += "\n";
            file += "section .bss\n";
            foreach (string s in bss)
                file += s + "\n";

            file += "\n";
            file += "section .static_ctors\n";
            foreach (string s in static_ctors)
                file += "dq " + s + "\n";

            file += "\n";
            file += "section .text\n";
            foreach (string s in externals)
                file += "extern " + s + "\n";

            foreach (string s in Lines)
                file += s + "\n";

            //TODO emit a section for static ctors

            return file;
        }

        public void Process(ConstructorInfo fakeInfo, ConstructorInfo realInfo, SSAToken[] tokens, string[] strtab)
        {
            var backend = types[fakeInfo.ReflectedType.FullName] as AMD64Backend;
            prefix = GetTypeName(fakeInfo.ReflectedType);
            Emitter = new InstrEmitter();

            //Initialize method state
            Locals = new List<LocalAllocs>();
            EvalStack = new StackQueue<StackAllocs>();
            Registers = new Stack<int>();
            LocalTopOffset = 0;
            SpillTopOffset = 0;
            ArgumentTopOffset = -2 * AMD64Backend.PointerSize;  //Top of stack is currently return address, and below that, a single return slot
            InitRegisters();

            //Generate the method entry stub
            var sig = GenerateMethodName(fakeInfo);
            Emitter.MakeGlobalFunction(sig);
            Emitter.MakeComment(fakeInfo.Name);

            if (fakeInfo.IsStatic)
            {
                //Called on module load
                static_ctors.Add(sig);
            }

            //Allocate stack space for the local variables and build the index table for offsets
            int locals_sz = 0;

            var mthdBody = (TypeMapper.ResolveMember(realInfo.DeclaringType, realInfo.MetadataToken) as ConstructorInfo).GetMethodBody();
            
            var locals = mthdBody.LocalVariables;
            foreach (LocalVariableInfo local in locals)
            {
                int sz = AMD64Backend.PointerSize;

                if (local.LocalType.IsValueType)
                    sz = Marshal.SizeOf(local.LocalType);

                locals_sz += sz;
                Locals.Add(new LocalAllocs()
                {
                    sz = sz,
                    id = local.LocalIndex
                });
            }
            if (locals_sz != 0) Emitter.SubRegConst((int)AssemRegisters.Rsp, locals_sz);

            LocalTopOffset = 0;
            ArgumentTopOffset -= locals_sz;

            //Calculate spill stack space
            var stack_sz = (mthdBody.MaxStackSize - 15) * AMD64Backend.PointerSize;
            if (stack_sz > 0)
            {
                SpillTopOffset = 0;
                LocalTopOffset -= stack_sz;
                ArgumentTopOffset -= stack_sz;

                Emitter.SubRegConst((int)AssemRegisters.Rsp, stack_sz);
            }
            
            //Setup vtables
            //Ldarg0
            //if(arg0 == null){
            // Install vtables
            //}

            ProcessTokens(backend, realInfo.DeclaringType, true, tokens, strtab);
        }

        public void Process(MethodInfo info, SSAToken[] tokens, string[] strtab)
        {
            Process(info, info, tokens, strtab);
        }

        //TODO cleanup this type translation mess before proceeding
        public void Process(MethodInfo fakeInfo, MethodInfo realInfo, SSAToken[] tokens, string[] strtab)
        {
            var backend = types[fakeInfo.ReflectedType.FullName] as AMD64Backend;
            prefix = GetTypeName(fakeInfo.ReflectedType);
            Emitter = new InstrEmitter();

            //Initialize method state
            Locals = new List<LocalAllocs>();
            EvalStack = new StackQueue<StackAllocs>();
            Registers = new Stack<int>();
            LocalTopOffset = 0;
            SpillTopOffset = 0;
            ArgumentTopOffset = -2 * AMD64Backend.PointerSize;  //Top of stack is currently return address, and below that, a single return slot
            InitRegisters();

            //Generate the method entry stub
            var sig = GenerateMethodName(fakeInfo);
            Emitter.MakeGlobalFunction(sig);
            Emitter.MakeComment(fakeInfo.Name);

            //Allocate stack space for the local variables and build the index table for offsets
            int locals_sz = 0;
            var mthdBody = (TypeMapper.ResolveMember(realInfo.DeclaringType, realInfo.MetadataToken) as MethodInfo).GetMethodBody();


            var locals = mthdBody.LocalVariables;
            foreach (LocalVariableInfo local in locals)
            {
                int sz = AMD64Backend.PointerSize;

                if (local.LocalType.IsValueType)
                    sz = Marshal.SizeOf(local.LocalType);

                locals_sz += sz;
                Locals.Add(new LocalAllocs()
                {
                    sz = sz,
                    id = local.LocalIndex
                });
            }
            if (locals_sz != 0) Emitter.SubRegConst((int)AssemRegisters.Rsp, locals_sz);

            LocalTopOffset = 0;
            ArgumentTopOffset -= locals_sz;

            //Calculate spill stack space
            var stack_sz = (mthdBody.MaxStackSize - 15) * AMD64Backend.PointerSize;
            if (stack_sz > 0)
            {
                SpillTopOffset = 0;
                LocalTopOffset -= stack_sz;
                ArgumentTopOffset -= stack_sz;

                Emitter.SubRegConst((int)AssemRegisters.Rsp, stack_sz);
            }
            ProcessTokens(backend, realInfo.DeclaringType, false, tokens, strtab);
        }

        private void ProcessTokens(AMD64Backend backend, Type resType, bool isCtor, SSAToken[] tokens, string[] strtab)
        {
            //Interpret the tokens
            for (int i = 0; i < tokens.Length; i++)
            {
                var tkn = tokens[i];

                Emitter.MakeLineLabel(tkn.InstructionOffset);

#if DEBUG
                Console.WriteLine(tkn.Operation);
#endif

                switch (tkn.Operation)
                {
                    case InstructionTypes.LdArg:
                        EmitLdArg(tkn);
                        break;
                    case InstructionTypes.StLoc:
                        EmitStLoc(tkn);
                        break;
                    case InstructionTypes.LdLoc:
                        EmitLdLoc(tkn);
                        break;
                    case InstructionTypes.Ldc:
                        EmitLdc(tkn);
                        break;
                    case InstructionTypes.Convert:
                    case InstructionTypes.ConvertCheckOverflow:
                        EmitConvert(tkn);
                        break;
                    case InstructionTypes.Multiply:
                    case InstructionTypes.Divide:
                    case InstructionTypes.UDivide:
                    case InstructionTypes.Add:
                    case InstructionTypes.UAddCheckOverflow:
                    case InstructionTypes.AddCheckOverflow:
                    case InstructionTypes.Subtract:
                    case InstructionTypes.USubtractCheckOverflow:
                    case InstructionTypes.SubtractCheckOverflow:
                    case InstructionTypes.Rem:
                    case InstructionTypes.URem:
                    case InstructionTypes.And:
                    case InstructionTypes.Or:
                    case InstructionTypes.Xor:
                    case InstructionTypes.Shl:
                    case InstructionTypes.Shr:
                    case InstructionTypes.ShrUn:
                    case InstructionTypes.Neg:
                    case InstructionTypes.Not:
                        EmitMath(tkn);
                        break;
                    case InstructionTypes.BrFalse:
                    case InstructionTypes.BrTrue:
                    case InstructionTypes.Br:
                    case InstructionTypes.Beq:
                    case InstructionTypes.BneUn:
                    case InstructionTypes.Bgt:
                    case InstructionTypes.BgtUn:
                    case InstructionTypes.Blt:
                    case InstructionTypes.BltUn:
                    case InstructionTypes.BleUn:
                    case InstructionTypes.BgeUn:
                    case InstructionTypes.Ble:
                    case InstructionTypes.Bge:
                        EmitBranch(tkn);
                        break;
                    case InstructionTypes.Ret:
                        EmitRet(isCtor, tkn);
                        break;
                    case InstructionTypes.LdStr:
                        EmitLdStr(tkn, strtab);
                        break;
                    case InstructionTypes.LdNull:
                        EmitLdNull(tkn);
                        break;
                    case InstructionTypes.Call:
                        EmitCall(resType, tkn);
                        break;
                    case InstructionTypes.CallVirt:
                        EmitCallVirt(tkn);
                        break;
                    case InstructionTypes.LdLoca:
                        EmitLdLoca(tkn);
                        break;
                    case InstructionTypes.Newobj:
                        EmitNewobj(resType, tkn);
                        break;
                    case InstructionTypes.Newarr:
                        EmitNewarr(resType, tkn);
                        break;
                    case InstructionTypes.Stsfld:
                        EmitStsfld(resType, tkn);
                        break;
                    case InstructionTypes.Stfld:
                        EmitStfld(resType, tkn);
                        break;
                    case InstructionTypes.Ldfld:
                        EmitLdfld(resType, tkn);
                        break;
                    case InstructionTypes.Ldsfld:
                        EmitLdsfld(resType, tkn);
                        break;
                    case InstructionTypes.Ldsflda:
                        EmitLdsflda(resType, tkn);
                        break;
                    case InstructionTypes.Ldflda:
                        EmitLdflda(resType, tkn);
                        break;
                    case InstructionTypes.Ceq:
                    case InstructionTypes.Cgt:
                    case InstructionTypes.CgtUn:
                    case InstructionTypes.Clt:
                    case InstructionTypes.CltUn:
                        EmitCmp(tkn);
                        break;
                    case InstructionTypes.Pop:
                        EmitPop(tkn);
                        break;
                    case InstructionTypes.Ldelema:
                        EmitLdelema(tkn);
                        break;
                    case InstructionTypes.Ldelem:
                        EmitLdelem(tkn);
                        break;
                    case InstructionTypes.Ldlen:
                        EmitLdlen(tkn);
                        break;
                    case InstructionTypes.Stelem:
                        EmitStelem(tkn);
                        break;
                    case InstructionTypes.Stind:
                        EmitStind(tkn);
                        break;
                    case InstructionTypes.Nop:

                        break;
                    case InstructionTypes.Dup:
                        EmitDup(tkn);
                        break;
                    case InstructionTypes.Throw:

                        break;
                    case InstructionTypes.Switch:
                        //Generate static jump tables, emit a single jump instruction
                        break;
                    default:
                        throw new NotImplementedException(tkn.Operation.ToString());
                        break;
                }
            }

            //TODO For exception handling, generate the catch and finally blocks as 'sub-functions'
            Lines.AddRange(Emitter.GetLines());
        }

        private void EmitNewobj(Type backend, SSAToken tkn)
        {
            var ctor = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as ConstructorInfo;
            var bEnd = types[ctor.DeclaringType.FullName] as AMD64Backend;

            //Request memory
            Emitter.MovConstantToRegisterSize((ulong)bEnd.InstanceSize, AllocEvalStack(4), 4);
            EmitCall(backend, typeof(Builtins.MemoryManager).GetMethod(nameof(Builtins.MemoryManager.AllocateMemory)), 1);

            //Call constructor
            EmitCall(backend, ctor, ctor.GetParameters().Length + 1);

        }

        private void EmitNewarr(Type backend, SSAToken tkn)
        {
            //-8 bytes is GC information
            //-4 bytes is array length
            //Emit a call to the rt_malloc_array alias
            int sz = 0;
            var tInfo = backend.Module.ResolveType((int)tkn.Constants[0]);
            if (tInfo.IsValueType)
            {
                sz = Marshal.SizeOf(tInfo);
            }
            else
            {
                sz = AMD64Backend.PointerSize;
            }

            Emitter.MovConstantToRegisterSize((ulong)sz, AllocEvalStack(4), 4);
            EmitCall(backend, typeof(Builtins.MemoryManager).GetMethod(nameof(Builtins.MemoryManager.AllocateArray)), 2);
        }

        private void EmitLdelema(SSAToken tkn)
        {
            var idx = PopEvalStack(out int idx_sz);
            var arr = PopEvalStack(out int arr_sz);
            //TODO Emit bounds check
            Emitter.MovRelativeAddressMultToRegisterSize(arr, idx, AMD64Backend.PointerSize, 0, AllocEvalStack(AMD64Backend.PointerSize), AMD64Backend.PointerSize);
        }

        private void EmitLdelem(SSAToken tkn)
        {
            int itm_sz = 0;
            //determine itm_sz based on instruction
            switch ((OperandTypes)tkn.Constants[0])
            {
                case OperandTypes.I:
                case OperandTypes.U:
                case OperandTypes.I4:
                case OperandTypes.U4:
                case OperandTypes.R4:
                    itm_sz = 4;
                    break;
                case OperandTypes.I1:
                case OperandTypes.U1:
                    itm_sz = 1;
                    break;
                case OperandTypes.I2:
                case OperandTypes.U2:
                    itm_sz = 2;
                    break;
                case OperandTypes.I8:
                case OperandTypes.U8:
                case OperandTypes.R8:
                case OperandTypes.Object:
                    itm_sz = 8;
                    break;
            }

            var idx = PopEvalStack(out int idx_sz);
            var arr = PopEvalStack(out int arr_sz);
            //TODO Emit bounds check
            Emitter.MovRelativeAddressMultToRegisterSize(arr, idx, AMD64Backend.PointerSize, 0, AllocEvalStack(itm_sz), itm_sz);
        }

        private void EmitLdlen(SSAToken tkn)
        {
            Emitter.MovRelativeAddressToRegister(PopEvalStack(out int ign0), -4, AllocEvalStack(4));
        }

        private void EmitStelem(SSAToken tkn)
        {
            int itm_sz = 0;
            //determine itm_sz based on instruction
            switch ((OperandTypes)tkn.Constants[0])
            {
                case OperandTypes.I:
                case OperandTypes.U:
                case OperandTypes.I4:
                case OperandTypes.U4:
                case OperandTypes.R4:
                    itm_sz = 4;
                    break;
                case OperandTypes.I1:
                case OperandTypes.U1:
                    itm_sz = 1;
                    break;
                case OperandTypes.I2:
                case OperandTypes.U2:
                    itm_sz = 2;
                    break;
                case OperandTypes.I8:
                case OperandTypes.U8:
                case OperandTypes.R8:
                case OperandTypes.Object:
                    itm_sz = 8;
                    break;
            }

            var val = PopEvalStack(out int val_sz);
            var idx = PopEvalStack(out int idx_sz);
            var arr = PopEvalStack(out int arr_sz);
            //TODO Emit bounds check
            Emitter.MovRegisterToRegisterRelativeAddressMultSize(val, itm_sz, arr, idx, AMD64Backend.PointerSize, 0);
        }

        private void EmitStfld(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;

            if (fInfo.DeclaringType.IsValueType)
            {
                var offset = (int)Marshal.OffsetOf(fInfo.DeclaringType, fInfo.Name);
                Emitter.MovRegisterToRegisterRelativeAddressMultSize(PopEvalStack(out int par0), par0, PopEvalStack(out int par1), 0, 0, offset);
            }
        }

        private void EmitLdfld(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;

            if (fInfo.DeclaringType.IsValueType)
            {
                var offset = (int)Marshal.OffsetOf(fInfo.DeclaringType, fInfo.Name);
                var par1 = (int)Marshal.SizeOf(fInfo.FieldType);
                Emitter.MovRelativeAddressMultToRegisterSize(PopEvalStack(out int par0), 0, 0, offset, AllocEvalStack(par1), par1);
            }
        }

        private void EmitLdflda(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;

            if (fInfo.DeclaringType.IsValueType)
            {
                var offset = (int)Marshal.OffsetOf(fInfo.DeclaringType, fInfo.Name);
                Emitter.MovLabelRelativeConstantToRegisterSize(GetTypeName(fInfo.ReflectedType) + "_static", offset, AllocEvalStack(AMD64Backend.PointerSize), AMD64Backend.PointerSize);
            }
        }

        private void EmitStsfld(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;
            (types[fInfo.ReflectedType.FullName] as AMD64Backend).GetFieldDesc(fInfo.MetadataToken, out int off, out int sz);
            Emitter.MovRegisterToLabelRelativeAddressSize(PopEvalStack(out int arg0_sz), arg0_sz, GetTypeName(fInfo.ReflectedType) + "_static", off);
        }

        private void EmitLdsfld(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;
            (types[fInfo.ReflectedType.FullName] as AMD64Backend).GetFieldDesc(fInfo.MetadataToken, out int off, out int sz);
            Emitter.MovLabelRelativeAddressToRegisterSize(GetTypeName(fInfo.ReflectedType) + "_static", off, AllocEvalStack(sz), sz);
        }

        private void EmitLdsflda(Type backend, SSAToken tkn)
        {
            var fInfo = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]) as FieldInfo;
            (types[fInfo.ReflectedType.FullName] as AMD64Backend).GetFieldDesc(fInfo.MetadataToken, out int off, out int sz);
            Emitter.MovLabelRelativeConstantToRegisterSize(GetTypeName(fInfo.ReflectedType) + "_static", off, AllocEvalStack(AMD64Backend.PointerSize), AMD64Backend.PointerSize);
        }

        private void EmitStind(SSAToken tkn)
        {
            var val = PopEvalStackFull();
            var addr = PopEvalStackFull();

            int itm_sz = 0;
            //determine itm_sz based on instruction
            switch ((OperandTypes)tkn.Constants[0])
            {
                case OperandTypes.I:
                case OperandTypes.U:
                case OperandTypes.I4:
                case OperandTypes.U4:
                case OperandTypes.R4:
                    itm_sz = 4;
                    break;
                case OperandTypes.I1:
                case OperandTypes.U1:
                    itm_sz = 1;
                    break;
                case OperandTypes.I2:
                case OperandTypes.U2:
                    itm_sz = 2;
                    break;
                case OperandTypes.I8:
                case OperandTypes.U8:
                case OperandTypes.R8:
                case OperandTypes.Object:
                    itm_sz = 8;
                    break;
            }

            Emitter.MovRegisterToRegisterAddressSize(val.Position, addr.Position, itm_sz);
        }

        private void EmitCmp(SSAToken tkn)
        {
            //compare
            int reg = AllocEvalStack(AMD64Backend.PointerSize);
            Emitter.Compare(PopEvalStack(out int ign0), PopEvalStack(out int ing1));

            //Emit branch skipping one instruction
            switch (tkn.Operation)
            {
                case InstructionTypes.Cgt:
                    Emitter.JmpGtRelativeLocalLabel(1);
                    break;
                case InstructionTypes.CgtUn:
                    Emitter.JmpGtUnRelativeLocalLabel(1);
                    break;
                case InstructionTypes.Clt:
                    Emitter.JmpLtRelativeLocalLabel(1);
                    break;
                case InstructionTypes.CltUn:
                    Emitter.JmpLtUnRelativeLocalLabel(1);
                    break;
                case InstructionTypes.Ceq:
                    Emitter.JmpEqRelativeLocalLabel(1);
                    break;
            }

            Emitter.MovConstantToRegister(0, reg);
            Emitter.JmpRelativeLocalLabel(2);

            Emitter.MakeLocalLineLabel(1);
            Emitter.MovConstantToRegister(1, reg);

            Emitter.MakeLocalLineLabel(2);
        }

        private void EmitLdLoca(SSAToken tkn)
        {
            Emitter.LoadEffectiveAddress((int)AssemRegisters.Rsp, LocalTopOffset + (int)tkn.Constants[0] * AMD64Backend.PointerSize, AllocEvalStack(Locals[(int)tkn.Constants[0]].TypeSize, Locals[(int)tkn.Constants[0]].ValueKnown, Locals[(int)tkn.Constants[0]].Value));
        }

        private void EmitLdArg(SSAToken tkn)
        {
            Emitter.MovRelativeAddressToRegister((int)AssemRegisters.Rsp, ArgumentTopOffset + (int)tkn.Constants[0] * AMD64Backend.PointerSize, AllocEvalStack(AMD64Backend.PointerSize));
        }

        private void EmitConvert(SSAToken tkn)
        {
            if (tkn.Operation == InstructionTypes.ConvertCheckOverflow)
            {
                //Emit overflow check
            }

            switch ((OperandTypes)tkn.Constants[0])
            {
                case OperandTypes.I:
                case OperandTypes.U:
                    {
                        Emitter.MovRegisterToRegisterSize(PopEvalStack(out int ign0), ign0, AllocEvalStack(4), 4);
                    }
                    break;
                case OperandTypes.I1:
                case OperandTypes.U1:
                    {
                        Emitter.MovRegisterToRegisterSize(PopEvalStack(out int ign0), ign0, AllocEvalStack(1), 1);
                    }
                    break;
                case OperandTypes.I2:
                case OperandTypes.U2:
                    {
                        Emitter.MovRegisterToRegisterSize(PopEvalStack(out int ign0), ign0, AllocEvalStack(2), 2);
                    }
                    break;
                case OperandTypes.I4:
                case OperandTypes.U4:
                    {
                        Emitter.MovRegisterToRegisterSize(PopEvalStack(out int ign0), ign0, AllocEvalStack(4), 4);
                    }
                    break;
                case OperandTypes.I8:
                case OperandTypes.U8:
                    {
                        Emitter.MovRegisterToRegisterSize(PopEvalStack(out int ign0), ign0, AllocEvalStack(8), 8);
                    }
                    break;
                case OperandTypes.R_U:
                case OperandTypes.R4:
                case OperandTypes.R8:
                    throw new NotImplementedException("Floating point support unimplemented!");
            }
        }

        private void EmitLdNull(SSAToken tkn)
        {
            Emitter.MovConstantToRegister(0, AllocEvalStack(AMD64Backend.PointerSize, true, 0));
        }

        private void EmitDup(SSAToken tkn)
        {
            var r = PeekEvalStackFull();
            //TODO: If pointer, we can't just duplicate the value?
            Emitter.MovRegisterToRegister(r.Position, AllocEvalStack(r.TypeSize, r.ValueKnown, r.Value));
        }

        private void EmitPop(SSAToken tkn)
        {
            PopEvalStack(out int ign);
        }

        private void EmitMath(SSAToken tkn)
        {
            int ign1 = 0;
            //TODO optimize operations for when operands are known, allowing assembler to remove dead stores
            switch (tkn.Operation)
            {
                case InstructionTypes.Add:
                    {
                        Emitter.Add(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.UAddCheckOverflow:
                case InstructionTypes.AddCheckOverflow:
                    {
                        Emitter.Add(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                        Emitter.CheckOverflow(ign1, "ovf_error");
                    }
                    break;
                case InstructionTypes.Multiply:
                    {
                        Emitter.Multiply(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Divide:
                    {
                        Emitter.Divide(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Rem:
                    {
                        Emitter.Remainder(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.UDivide:
                    {
                        Emitter.UDivide(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.URem:
                    {
                        Emitter.URemainder(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Subtract:
                    {
                        Emitter.Sub(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.USubtractCheckOverflow:
                case InstructionTypes.SubtractCheckOverflow:
                    {
                        Emitter.Sub(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                        Emitter.CheckOverflow(ign1, "ovf_error");
                    }
                    break;
                case InstructionTypes.And:
                    {
                        Emitter.And(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Or:
                    {
                        Emitter.Or(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Xor:
                    {
                        Emitter.Xor(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Neg:
                    {
                        Emitter.Neg(PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Not:
                    {
                        Emitter.Not(PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Shl:
                    {
                        Emitter.ShiftLeft(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.Shr:
                    {
                        Emitter.ShiftRightUn(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
                case InstructionTypes.ShrUn:
                    {
                        Emitter.ShiftRightUn(PopEvalStack(out int ign0), PopEvalStack(out ign1));
                    }
                    break;
            }
            AllocEvalStack(ign1);
        }

        private void EmitLdLoc(SSAToken tkn)
        {
            Emitter.MovRelativeAddressToRegisterSize((int)AssemRegisters.Rsp, LocalTopOffset + (int)tkn.Constants[0] * AMD64Backend.PointerSize, AllocEvalStack(Locals[(int)tkn.Constants[0]].TypeSize, Locals[(int)tkn.Constants[0]].ValueKnown, Locals[(int)tkn.Constants[0]].Value), Locals[(int)tkn.Constants[0]].TypeSize);
        }

        private void EmitBranch(SSAToken tkn)
        {
            Console.WriteLine(tkn.Constants[0]);
            if (tkn.Operation != InstructionTypes.Br)
            {
                if (new InstructionTypes[] { InstructionTypes.Beq, InstructionTypes.Bge, InstructionTypes.BgeUn, InstructionTypes.Bgt, InstructionTypes.BgtUn, InstructionTypes.Ble, InstructionTypes.BleUn, InstructionTypes.Blt, InstructionTypes.BltUn, InstructionTypes.BneUn }.Contains(tkn.Operation))
                {
                    var p1 = PopEvalStack(out int ign0);
                    var p2 = PopEvalStack(out int ign1);
                    Emitter.Compare(p2, p1);
                }
                else if (new InstructionTypes[] { InstructionTypes.BrFalse, InstructionTypes.BrTrue }.Contains(tkn.Operation))
                    Emitter.TestBool(PopEvalStack(out int ign0));
            }
            switch (tkn.Operation)
            {
                case InstructionTypes.Br:
                    Emitter.JmpRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.Beq:
                    Emitter.JmpEqRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BneUn:
                    Emitter.JmpNeRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.Bge:
                    Emitter.JmpGeRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BgeUn:
                    Emitter.JmpGeUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.Bgt:
                    Emitter.JmpGtRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BgtUn:
                    Emitter.JmpGtUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.Ble:
                    Emitter.JmpLeRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BleUn:
                    Emitter.JmpLeUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.Blt:
                    Emitter.JmpLtRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BltUn:
                    Emitter.JmpLtUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BrTrue:
                    Emitter.JmpZeroRelativeLabel((int)tkn.Constants[0]);
                    break;
                case InstructionTypes.BrFalse:
                    Emitter.JmpNZeroRelativeLabel((int)tkn.Constants[0]);
                    break;
            }
        }

        private void EmitStLoc(SSAToken tkn)
        {
            var r = PopEvalStackFull();
            Emitter.MovRegisterToRegisterRelativeAddress(r.Position, (int)AssemRegisters.Rsp, LocalTopOffset + (int)tkn.Constants[0] * AMD64Backend.PointerSize);
            Locals[(int)tkn.Constants[0]].TypeSize = r.TypeSize;
            Locals[(int)tkn.Constants[0]].Value = r.Value;
            Locals[(int)tkn.Constants[0]].ValueKnown = r.ValueKnown;
        }

        private void EmitLdc(SSAToken tkn)
        {
            var reg = -1;
            switch ((OperandTypes)tkn.Constants[0])
            {
                case OperandTypes.I4:
                    reg = AllocEvalStack(4, true, tkn.Constants[1]);
                    Emitter.MovConstantToRegister(tkn.Constants[1], reg);
                    break;
                case OperandTypes.I8:
                    reg = AllocEvalStack(8, true, tkn.Constants[1]);
                    Emitter.MovConstantToRegister(tkn.Constants[1], reg);
                    break;
                case OperandTypes.R4:
                    reg = AllocEvalStack(4, true, tkn.Constants[1]);
                    Emitter.MovConstantToRegister((ulong)BitConverter.ToInt32(BitConverter.GetBytes((float)tkn.Constants[1]), 0), reg);
                    break;
                case OperandTypes.R8:
                    reg = AllocEvalStack(8, true, tkn.Constants[1]);
                    Emitter.MovConstantToRegister((ulong)BitConverter.DoubleToInt64Bits((double)tkn.Constants[1]), reg);
                    break;
            }
        }

        private void EmitCallVirt(SSAToken tkn)
        {
            //TODO Call from vtable in object reference
        }

        private void EmitCall(Type backend, SSAToken tkn)
        {
            var mthd = TypeMapper.ResolveMember(backend, (int)tkn.Constants[0]);
            if (mthd.MemberType == MemberTypes.Method) EmitCall(backend, mthd as MethodInfo, tkn.Parameters.Length);
            else EmitCall(backend, mthd as ConstructorInfo, tkn.Parameters.Length);
        }

        private void EmitCall(Type backend, MethodInfo mthd, int paramCnt)
        {
            if (mthd.ReflectedType.FullName == typeof(MSIL2ASM.Builtins.x86_64).FullName)
            {
                var builtin_type = typeof(MSIL2ASM.Builtins.x86_64);
                //Check this method for built-in calls for which to emit other assembly
                switch (mthd.Name)
                {
                    case nameof(Builtins.x86_64.Halt):
                        Emitter.Hlt();
                        break;
                    case nameof(Builtins.x86_64.Cli):
                        Emitter.Cli();
                        break;
                    case nameof(Builtins.x86_64.Sti):
                        Emitter.Sti();
                        break;
                    case nameof(Builtins.x86_64.Out):
                        {
                            var arg1 = PopEvalStackFull();
                            var arg0 = PopEvalStackFull();

                            var sz = Marshal.SizeOf(mthd.GetParameters()[1].ParameterType);

                            if (arg0.ValueKnown && arg0.Value <= byte.MaxValue)
                                Emitter.OutConst((int)arg0.Value, arg1.Position, sz);
                            else
                                Emitter.Out(arg0.Position, arg1.Position, 1);
                        }
                        break;
                    case nameof(Builtins.x86_64.In):
                        {
                            var arg1 = PopEvalStackFull();
                            var arg0 = PopEvalStackFull();

                            //TODO Fix this code, arg1 will end up being a pointer, thus simply emitting an 'in' instruction won't do
                            if (arg0.ValueKnown)
                                Emitter.InConst((int)arg0.Value, arg1.Position, arg1.TypeSize);
                            else
                                Emitter.In(arg0.Position, arg1.Position, arg1.TypeSize);
                        }
                        break;
                }

                return;
            }


            //Save currently in use registers on the stack
            for (int i = 0; i < EvalStack.Count - paramCnt; i++)
            {
                Emitter.Push(EvalStack[EvalStack.Count - 1 - i].Position);
            }

            //Push arguments onto the stack in reverse order, so the called function accesses them like an array
            for (int i = 0; i < paramCnt; i++)
            {
                int reg = PopEvalStack(out int ign0);
                Emitter.Push(reg);
            }

            //The instance is pushed automatically

            //Push an empty spot for the return value
            Emitter.Push((int)AssemRegisters.Rax);

            var tName = mthd.ReflectedType;
            var mthdName = AMD64Backend.GetMethodName(mthd);

            if (prefix != GetTypeName(mthd.DeclaringType))
            {
                externals.Add(mthdName);
            }

            Emitter.CallLabel(mthdName);

            //Read return value
            int retValSz = 0;
            if (new Type[] { typeof(uint), typeof(int), typeof(float) }.Contains(mthd.ReturnType))
            {
                retValSz = 4;
            }
            else if (new Type[] { typeof(ushort), typeof(short) }.Contains(mthd.ReturnType))
            {
                retValSz = 2;
            }
            else if (new Type[] { typeof(byte), typeof(sbyte) }.Contains(mthd.ReturnType))
            {
                retValSz = 1;
            }
            else if (mthd.ReturnType != typeof(void))
            {
                retValSz = 8;
            }

            if (retValSz != 0)
            {
                int retVal = AllocEvalStack(retValSz);
                Emitter.Pop(retVal);
            }

            //Remove arguments from stack
            Emitter.SubRegConst((int)AssemRegisters.Rsp, paramCnt * AMD64Backend.PointerSize + (retValSz == 0 ? AMD64Backend.PointerSize : 0));

            //Reload used registers from stack
            for (int i = 0; i < EvalStack.Count - paramCnt; i++)
            {
                Emitter.Pop(EvalStack[EvalStack.Count - 1 - i].Position);
            }
        }

        private void EmitCall(Type backend, ConstructorInfo ctor, int paramCnt)
        {
            //Save currently in use registers on the stack
            for (int i = 0; i < EvalStack.Count - paramCnt - 1; i++)
            {
                Emitter.Push(EvalStack[EvalStack.Count - 1 - i].Position);
            }

            //Push arguments onto the stack in reverse order, so the called function accesses them like an array
            int objReg = PopEvalStack(out int objReg0);
            for (int i = 0; i < paramCnt - 1; i++)
            {
                int reg = PopEvalStack(out int ign0);
                Emitter.Push(reg);
            }
            Emitter.Push(objReg);

            //The instance is pushed automatically

            //Push an empty spot for the return value
            Emitter.Push((int)AssemRegisters.Rax);

            var tName = ctor.ReflectedType;
            var mthdName = AMD64Backend.GetMethodName(ctor);

            if (prefix != GetTypeName(ctor.DeclaringType))
            {
                externals.Add(mthdName);
            }

            Emitter.CallLabel(mthdName);

            //Remove arguments from stack
            int retVal = AllocEvalStack(AMD64Backend.PointerSize);
            Emitter.Pop(retVal);

            Emitter.SubRegConst((int)AssemRegisters.Rsp, paramCnt * AMD64Backend.PointerSize);

            //Reload used registers from stack
            for (int i = 0; i < EvalStack.Count - paramCnt - 1; i++)
            {
                Emitter.Pop(EvalStack[EvalStack.Count - 1 - i].Position);
            }
        }

        private void EmitLdStr(SSAToken tkn, string[] strtab)
        {
            //Declare string variable with the given value
            var str = strtab[tkn.Constants[0] - 1];
            int id = 0;
            if (!stringTable.Contains(str))
            {
                stringTable.Add(str);
                id = stringTable.Count - 1;
                AddString(str, id);
            }
            else
            {
                id = stringTable.IndexOf(str);
            }

            var str_lbl = GetStringLabel(id);
            var reg_idx = AllocEvalStack(8);

            //Mov the address of the label into the register
            Emitter.MovLabelAddressToRegister(reg_idx, str_lbl);
        }

        public void EmitRet(bool isCtor, SSAToken tkn)
        {
            //Move rsp back to previous location
            if (ArgumentTopOffset != -16)
                Emitter.SubRegConst((int)AssemRegisters.Rsp, (ArgumentTopOffset + 16));

            //Write to (rsp - 8) the return value register
            if (StackSize == 1 | isCtor)
            {
                int retValReg = 0;
                if (isCtor)
                {
                    var arg0_reg = AllocEvalStack(AMD64Backend.PointerSize);
                    Emitter.MovRelativeAddressToRegisterSize((int)AssemRegisters.Rsp, ArgumentTopOffset, arg0_reg, AMD64Backend.PointerSize);
                }
                retValReg = PopEvalStack(out int ign0);

                Emitter.MovRegisterToRegisterRelativeAddress(retValReg, (int)AssemRegisters.Rsp, -8);
            }

            Emitter.Ret();
        }

        /*
        public void EmitNewobj(AMD64Backend backend, SSAToken tkn)
        {
            var ctor = backend.GetCtorInfo((int)tkn.Constants[0]);

            //allocate object
            int sz = 0;
            if (types.ContainsKey(ctor.ReflectedType.FullName))
                sz = types[ctor.ReflectedType.FullName].InstanceSize;
            else
                sz = Marshal.SizeOf(ctor.ReflectedType);

            if (ctor.ReflectedType.IsValueType)
            {
                //NasmEmitDecl(GetTypeRefName(ctor.ReflectedType), sz, tkn.ID);
            }
            else
            {
                //NasmEmitDecl(GetTypeRefName(ctor.ReflectedType), AMD64Backend.PointerSize, tkn.ID);

                //initialize the object
                //NasmEmitObjAlloc(GetTypeRefName(ctor.ReflectedType), sz, tkn.ID);
            }

            //call the constructor for the object
            List<int> ps = new List<int>();
            ps.Add(tkn.ID);
            ps.AddRange(tkn.Parameters);

            //CEmitCall((types[ctor.ReflectedType.FullName] as AMD64Backend).GetMethodName((int)tkn.Constants[0]), tkn.ID, false, ps.ToArray());
        }*/
    }
}
