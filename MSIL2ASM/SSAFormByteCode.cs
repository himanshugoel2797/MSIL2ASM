using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public class SSAToken
    {
        public int ID;
        public int InstructionOffset;
        public int[] Parameters;
        public ulong[] Constants;
        public string String;
        public int RetValSz;
        public InstructionTypes Operation;

        public static List<SSAToken> Tokens;
        static SSAToken()
        {
            Tokens = new List<SSAToken>();
        }

        public SSAToken()
        {
            Tokens.Add(this);
            ID = Tokens.Count + 50; //Keep some room for later insertions
        }

        public override string ToString()
        {
            return ID.ToString() + " " + Operation.ToString();
        }
    }

    public enum InstructionTypes
    {
        LdArg,  //Load argument
        StLoc,  //Store local
        LdLoc,  //Load local
        Ldc,    //Load constant
        Convert,    //Convert integer
        ConvertCheckOverflow,   //Convert integer, checking for overflow
        Multiply,
        Divide,
        UDivide,
        Add,
        UAddCheckOverflow,
        AddCheckOverflow,
        Subtract,
        USubtractCheckOverflow,
        SubtractCheckOverflow,
        Rem,
        URem,
        And,
        Or,
        Xor,
        Shl,
        Shr,
        ShrUn,
        Neg,
        Not,
        BrFalse,    //Branch if zero
        BrTrue,     //Branch if not zero
        Br,         //Unconditional branch
        Beq,
        BneUn,
        Bgt,
        BgtUn,
        Blt,
        BltUn,
        BleUn,
        BgeUn,
        Ble,
        Bge,
        Ret,        //Return from function
        LdStr,
        LdNull,
        Call,
        CallVirt,
        LdLoca,
        Newobj,
        Newarr,
        Stsfld,
        Stfld,
        Ldfld,
        Ldsfld,
        Ldsflda,
        Ldflda,
        Ceq,
        Cgt,
        CgtUn,
        Clt,
        CltUn,
        CkFinite,
        Pop,
        Ldelema,
        Ldelem,
        Ldlen,
        Stelem,
        Stind,
        Nop,
        Dup,
        Throw,
        Switch,
        Ldtoken,
        Leave,
        CallVirtConstrained,
        EndFinally,
        StArg,
        LdInd,
        LdArga,
        Calli,
        Ldftn,
    }

    public enum OperandTypes
    {
        I,
        U,
        I1,
        I2,
        I4,
        I8,
        U1,
        U2,
        U4,
        U8,
        R4,
        R8,
        R_U,
        Object,
    }

    public class SSAFormByteCode
    {
        private MethodBody body;
        private Module module;

        private Stack<int> oStack;
        private List<SSAToken> Tokens { get; set; }
        private List<string> StringTable;

        private ulong ConstrainedCallVirt = 0;

        public SSAFormByteCode(MethodInfo mthd)
        {
            var tmp = (TypeMapper.ResolveMember(mthd.DeclaringType, mthd.MetadataToken) as MethodInfo);
            module = tmp.Module;
            body = tmp.GetMethodBody();
            oStack = new Stack<int>();
            StringTable = new List<string>();
        }

        public SSAFormByteCode(ConstructorInfo info1)
        {
            var tmp = (TypeMapper.ResolveMember(info1.DeclaringType, info1.MetadataToken) as ConstructorInfo);
            module = tmp.Module;
            body = tmp.GetMethodBody();

            oStack = new Stack<int>();
            StringTable = new List<string>();
        }

        private void ConvOpcode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            SSAToken tkn = new SSAToken()
            {
                Parameters = new int[] { oStack.Pop() },
                Operation = t,
                InstructionOffset = instructions.CurrentOffset
            };

            var parts = opc.Name.Split('.');
            if (parts.Length == 2)
            {
                switch (parts[1])
                {
                    case "i":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I };
                        break;
                    case "i1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I1 };
                        break;
                    case "i2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I2 };
                        break;
                    case "i4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I4 };
                        break;
                    case "i8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I8 };
                        break;
                    case "u":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U };
                        break;
                    case "u1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U1 };
                        break;
                    case "u2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U2 };
                        break;
                    case "u4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U4 };
                        break;
                    case "u8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U8 };
                        break;
                    case "r4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R4 };
                        break;
                    case "r8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R8 };
                        break;
                }
            }
            else
            {
                //conv.r.un
                tkn.Constants = new ulong[] { (ulong)OperandTypes.R_U };
            }

            oStack.Push(tkn.ID);
        }

        private void ExtnValOpcode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var tkn = new SSAToken()
            {
                Operation = t,
                InstructionOffset = instructions.CurrentOffset
            };

            var parts = opc.Name.Split('.');
            if (parts.Length == 2)
            {
                switch (parts[1])
                {
                    case "i4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I4, instructions.GetParameter(0) };
                        break;
                    case "i8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I8, instructions.GetParameter(0) };
                        break;
                    case "r4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R4, instructions.GetParameter(0) };
                        break;
                    case "r8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R8, instructions.GetParameter(0) };
                        break;
                }
            }
            else if (parts.Length == 3)
            {
                if (ulong.TryParse(parts[2], out ulong val))
                {
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I4, val };
                }
                else if (parts[2] == "M1")
                {
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I4, unchecked((ulong)-1) };
                }
                else if (parts[2] == "s")
                {
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I4, unchecked((ulong)(sbyte)instructions.GetParameter(0)) };
                }
            }
            oStack.Push(tkn.ID);
        }

        private void EncodedCountOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var p = opc.Name.Split('.')[1];
            SSAToken tkn = new SSAToken()
            {
                Parameters = null,
                Operation = t,
                InstructionOffset = instructions.CurrentOffset
            };

            if (ulong.TryParse(p, out ulong constant))
            {
                tkn.Constants = new ulong[] { constant };
            }
            else
            {
                tkn.Constants = new ulong[] { instructions.GetParameter(0) };
            }

            if (t == InstructionTypes.StLoc | t == InstructionTypes.StArg)
            {
                tkn.Parameters = new int[] { oStack.Pop() };
            }
            else
            {
                oStack.Push(tkn.ID);
            }
        }

        private void DualParamMathOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            SSAToken tkn = new SSAToken()
            {
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                Operation = t,
                InstructionOffset = instructions.CurrentOffset
            };

            oStack.Push(tkn.ID);
        }

        private void ConditionalBranchOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            long off = unchecked((long)instructions.GetParameter(0));
            if (instructions.GetParameterSize(0) == 1)
            {
                off = unchecked((sbyte)instructions.GetParameter(0));
            }
            if (instructions.GetParameterSize(0) == 2)
            {
                off = unchecked((short)instructions.GetParameter(0));
            }
            if (instructions.GetParameterSize(0) == 4)
            {
                off = unchecked((int)instructions.GetParameter(0));
            }

            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = new int[] { oStack.Pop() },
                Constants = new ulong[] { (ulong)unchecked(off + instructions.CurrentOffset + opc.Size + instructions.GetParameterSize(0)) },
                InstructionOffset = instructions.CurrentOffset
            };
        }

        private void CompConditionalBranchOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            long off = unchecked((long)instructions.GetParameter(0));
            if (instructions.GetParameterSize(0) == 1)
            {
                off = unchecked((sbyte)instructions.GetParameter(0));
            }
            if (instructions.GetParameterSize(0) == 2)
            {
                off = unchecked((short)instructions.GetParameter(0));
            }
            if (instructions.GetParameterSize(0) == 4)
            {
                off = unchecked((int)instructions.GetParameter(0));
            }

            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                Constants = new ulong[] { (ulong)unchecked(off + instructions.CurrentOffset + opc.Size + instructions.GetParameterSize(0)) },
                InstructionOffset = instructions.CurrentOffset
            };
        }

        private void BranchOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { (ulong)unchecked((long)instructions.GetParameter(0) + instructions.GetParameterSize(0) + instructions.CurrentOffset + opc.Size) }
            };
        }

        private void LeaveOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { (ulong)unchecked((long)instructions.GetParameter(0) + instructions.GetParameterSize(0) + instructions.CurrentOffset + opc.Size) }
            };
            oStack.Clear();
        }

        private void EndFinallyOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = null,
                Constants = null,
                InstructionOffset = instructions.CurrentOffset,
            };
            oStack.Clear();
        }

        private void RetOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = null,
                Constants = null,
                InstructionOffset = instructions.CurrentOffset,
            };

            if (oStack.Count > 0)
            {
                tkn.Parameters = new int[] { oStack.Pop() };
            }

            if (oStack.Count != 0)
                throw new Exception("Incorrect CIL! Evaluation stack should be empty on ret instruction.");
        }

        private void LdStrOpCode(ILStream instructions, OpCode opc, InstructionTypes t)
        {
            var str = module.ResolveString((int)instructions.GetParameter(0));

            var tkn = new SSAToken()
            {
                Operation = t,
                Parameters = null,
                Constants = new ulong[] { (ulong)StringTable.Count },
                InstructionOffset = instructions.CurrentOffset,
                String = str,
            };
            oStack.Push(tkn.ID);
        }

        private void CallOpCode(ILStream instructions, OpCode opc, InstructionTypes call)
        {
            int retSz = 0;
            Type retType = null;
            ParameterInfo[] @params = null;

            var mbase = module.ResolveMethod((int)instructions.GetParameter(0));
            @params = mbase.GetParameters();

            if (mbase is MethodInfo)
            {
                retType = (mbase as MethodInfo).ReturnType;

                if (retType == typeof(void))
                    retSz = 0;
                else if (retType.IsValueType)
                    retSz = Marshal.SizeOf(retType);
                else
                    retSz = MachineSpec.PointerSize;
            }
            else if (mbase is ConstructorInfo)
            {
                retType = typeof(void);
                retSz = MachineSpec.PointerSize;
            }

            var intP = new int[@params.Length + (mbase.IsStatic ? 0 : 1)];
            for (int i = 0; i < intP.Length; i++)
            {
                intP[intP.Length - 1 - i] = oStack.Pop();
            }

            if (call == InstructionTypes.CallVirt && ConstrainedCallVirt != 0)
            {
                call = InstructionTypes.CallVirtConstrained;
            }

            var tkn = new SSAToken()
            {
                Operation = call,
                Parameters = intP,
                InstructionOffset = instructions.CurrentOffset,
                String = (mbase is MethodInfo) ? MachineSpec.GetMethodName((MethodInfo)mbase) : MachineSpec.GetMethodName((ConstructorInfo)mbase),
                RetValSz = retSz,
            };

            if (call == InstructionTypes.CallVirtConstrained)
            {
                tkn.Constants = new ulong[] { instructions.GetParameter(0), ConstrainedCallVirt
    };
                ConstrainedCallVirt = 0;
            }
            else
            {
                tkn.Constants = new ulong[] { instructions.GetParameter(0) };
            }

            if (retType != typeof(void))
            {
                oStack.Push(tkn.ID);
            }
        }

        private void LdLocaOpCode(ILStream instructions, OpCode opc, InstructionTypes call)
        {
            var tkn = new SSAToken()
            {
                Operation = call,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };
            oStack.Push(tkn.ID);
        }

        private void NewobjOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            //allocate the object, call the constructor
            var mthd = (ConstructorInfo)module.ResolveMethod((int)instructions.GetParameter(0));
            var @params = mthd.GetParameters();

            var intP = new int[@params.Length];
            for (int i = 0; i < intP.Length; i++)
            {
                intP[intP.Length - 1 - i] = oStack.Pop();
            }

            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = intP,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };

            oStack.Push(tkn.ID);
        }

        private void NewarrOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            //allocate the array
            var type = module.ResolveType((int)instructions.GetParameter(0));

            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };

            oStack.Push(tkn.ID);
        }

        private void StsfldOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };
        }

        private void StfldOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };
        }

        private void StelemOpCode(ILStream instructions, OpCode opc, InstructionTypes stelem)
        {
            var tkn = new SSAToken()
            {
                Operation = stelem,
                Parameters = new int[] { oStack.Pop(), oStack.Pop(), oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };

            var parts = opc.Name.Split('.');
            if (parts.Length == 1)
            {
                tkn.Constants = new ulong[] { (ulong)OperandTypes.Object, instructions.GetParameter(0) };
            }
            else if (parts.Length == 2)
            {
                switch (parts[1])
                {
                    case "i":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I };
                        break;
                    case "i1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I1 };
                        break;
                    case "i2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I2 };
                        break;
                    case "i4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I4 };
                        break;
                    case "i8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I8 };
                        break;
                    case "u":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U };
                        break;
                    case "u1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U1 };
                        break;
                    case "u2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U2 };
                        break;
                    case "u4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U4 };
                        break;
                    case "u8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U8 };
                        break;
                    case "r4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R4 };
                        break;
                    case "r8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R8 };
                        break;
                    case "ref":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.Object };
                        break;
                }
            }
        }

        private void StindOpCode(ILStream instructions, OpCode opc, InstructionTypes stelem)
        {
            var tkn = new SSAToken()
            {
                Operation = stelem,
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };

            var parts = opc.Name.Split('.');

            switch (parts[1])
            {
                case "i":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I };
                    break;
                case "i1":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I1 };
                    break;
                case "i2":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I2 };
                    break;
                case "i4":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I4 };
                    break;
                case "i8":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I8 };
                    break;
                case "r4":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.R4 };
                    break;
                case "r8":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.R8 };
                    break;
                case "ref":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.Object };
                    break;
            }
        }

        private void LdindOpCode(ILStream instructions, OpCode opc, InstructionTypes stelem)
        {
            var tkn = new SSAToken()
            {
                Operation = stelem,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };

            var parts = opc.Name.Split('.');

            switch (parts[1])
            {
                case "i":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I };
                    break;
                case "i1":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I1 };
                    break;
                case "i2":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I2 };
                    break;
                case "i4":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I4 };
                    break;
                case "u1":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.U1 };
                    break;
                case "u2":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.U2 };
                    break;
                case "u4":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.U4 };
                    break;
                case "i8":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.I8 };
                    break;
                case "r4":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.R4 };
                    break;
                case "r8":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.R8 };
                    break;
                case "ref":
                    tkn.Constants = new ulong[] { (ulong)OperandTypes.Object };
                    break;
            }

            oStack.Push(tkn.ID);
        }

        private void LdfldOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };
            oStack.Push(tkn.ID);
        }

        private void LdsfldOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) },
            };
            oStack.Push(tkn.ID);
        }

        private void LdnullOpCode(ILStream instructions, OpCode opc, InstructionTypes newobj)
        {
            var tkn = new SSAToken()
            {
                Operation = newobj,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = null,
            };
            oStack.Push(tkn.ID);
        }

        private void LdelemOpCode(ILStream instructions, OpCode opc, InstructionTypes op)
        {
            var tkn = new SSAToken()
            {
                Operation = op,
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };

            var parts = opc.Name.Split('.');
            if (parts.Length == 1)
            {
                tkn.Constants = new ulong[] { (ulong)OperandTypes.Object, instructions.GetParameter(0) };
            }
            else if (parts.Length == 2)
            {
                switch (parts[1])
                {
                    case "i":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I };
                        break;
                    case "i1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I1 };
                        break;
                    case "i2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I2 };
                        break;
                    case "i4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I4 };
                        break;
                    case "i8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.I8 };
                        break;
                    case "u":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U };
                        break;
                    case "u1":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U1 };
                        break;
                    case "u2":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U2 };
                        break;
                    case "u4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U4 };
                        break;
                    case "u8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.U8 };
                        break;
                    case "r4":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R4 };
                        break;
                    case "r8":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.R8 };
                        break;
                    case "ref":
                        tkn.Constants = new ulong[] { (ulong)OperandTypes.Object };
                        break;
                }
            }

            oStack.Push(tkn.ID);
        }

        private void CompareOpCode(ILStream instructions, OpCode opc, InstructionTypes op)
        {
            var tkn = new SSAToken()
            {
                Operation = op,
                Parameters = new int[] { oStack.Pop(), oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };
            oStack.Push(tkn.ID);
        }

        private void PopOpCode(ILStream instructions, OpCode opc, InstructionTypes op)
        {
            var tkn = new SSAToken()
            {
                Operation = op,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };
        }

        private void DupOpCode(ILStream instructions, OpCode opc, InstructionTypes op)
        {
            var tkn = new SSAToken()
            {
                Operation = op,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };
            oStack.Push(tkn.ID);
            oStack.Push(tkn.ID);
        }

        private void NopOpCode(ILStream instructions, OpCode opc, InstructionTypes op)
        {
            var tkn = new SSAToken()
            {
                Operation = op,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };
        }

        private void LdlenOpCode(ILStream instructions, OpCode opc, InstructionTypes ldlen)
        {
            var tkn = new SSAToken()
            {
                Operation = ldlen,
                Parameters = new int[] { oStack.Pop() },
                InstructionOffset = instructions.CurrentOffset,
                Constants = null
            };
            oStack.Push(tkn.ID);
        }

        private void SwitchOpCode(ILStream instructions, OpCode opc, InstructionTypes @switch)
        {
            var tkn = new SSAToken()
            {
                Operation = @switch,
                InstructionOffset = instructions.CurrentOffset,
                Parameters = new int[] { oStack.Pop() }
            };

            ulong cnt = instructions.GetParameter(0);
            ulong[] parts = new ulong[cnt];
            for (uint i = 0; i < parts.Length; i++)
            {
                parts[i] = instructions.GetParameter(1 + i);
            }
            tkn.Constants = parts;

        }

        private void LdTokenOpCode(ILStream instructions, OpCode opc, InstructionTypes types)
        {
            var tkn = new SSAToken()
            {
                Operation = types,
                Parameters = null,
                InstructionOffset = instructions.CurrentOffset,
                Constants = new ulong[] { instructions.GetParameter(0) }
            };
            oStack.Push(tkn.ID);
        }

        public void Initialize()
        {
            //Parse the code and generate the SSA form instruction stream
            var instructions = new ILStream(body.GetILAsByteArray());

            do
            {
                var opc = instructions.GetCurrentOpCode();

                if (new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldarg, OpCodes.Ldarg_S }.Contains(opc))
                {
                    EncodedCountOpCode(instructions, opc, InstructionTypes.LdArg);
                }
                else if (new OpCode[] { OpCodes.Starg, OpCodes.Starg_S }.Contains(opc))
                {
                    EncodedCountOpCode(instructions, opc, InstructionTypes.StArg);
                }
                else if (new OpCode[] { OpCodes.Conv_I, OpCodes.Conv_I1, OpCodes.Conv_I2, OpCodes.Conv_I4, OpCodes.Conv_I8, OpCodes.Conv_R4, OpCodes.Conv_R8, OpCodes.Conv_U1, OpCodes.Conv_U2, OpCodes.Conv_U4, OpCodes.Conv_U8, OpCodes.Conv_U, OpCodes.Conv_R_Un }.Contains(opc))
                {
                    ConvOpcode(instructions, opc, InstructionTypes.Convert);
                }
                else if (new OpCode[] { OpCodes.Conv_Ovf_I, OpCodes.Conv_Ovf_I1, OpCodes.Conv_Ovf_I2, OpCodes.Conv_Ovf_I4, OpCodes.Conv_Ovf_I8, OpCodes.Conv_Ovf_U1, OpCodes.Conv_Ovf_U2, OpCodes.Conv_Ovf_U4, OpCodes.Conv_Ovf_U8, OpCodes.Conv_Ovf_U }.Contains(opc))
                {
                    ConvOpcode(instructions, opc, InstructionTypes.ConvertCheckOverflow);
                }
                else if (opc == OpCodes.Nop)
                {
                    NopOpCode(instructions, opc, InstructionTypes.Nop);
                }
                else if (opc == OpCodes.Dup)
                {
                    DupOpCode(instructions, opc, InstructionTypes.Dup);
                }
                else if (opc == OpCodes.Pop)
                {
                    PopOpCode(instructions, opc, InstructionTypes.Pop);
                }
                else if (opc == OpCodes.Throw)
                {
                    PopOpCode(instructions, opc, InstructionTypes.Throw);
                }
                else if (opc == OpCodes.Mul)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Multiply);
                }
                else if (opc == OpCodes.Div)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Divide);
                }
                else if (opc == OpCodes.Div_Un)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.UDivide);
                }
                else if (opc == OpCodes.Add)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Add);
                }
                else if (opc == OpCodes.Add_Ovf)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.AddCheckOverflow);
                }
                else if (opc == OpCodes.Add_Ovf_Un)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.UAddCheckOverflow);
                }
                else if (opc == OpCodes.Sub)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Subtract);
                }
                else if (opc == OpCodes.Sub_Ovf)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.SubtractCheckOverflow);
                }
                else if (opc == OpCodes.Sub_Ovf_Un)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.USubtractCheckOverflow);
                }
                else if (opc == OpCodes.Rem)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Rem);
                }
                else if (opc == OpCodes.Rem_Un)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.URem);
                }
                else if (opc == OpCodes.And)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.And);
                }
                else if (opc == OpCodes.Or)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Or);
                }
                else if (opc == OpCodes.Xor)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Xor);
                }
                else if (opc == OpCodes.Shl)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Shl);
                }
                else if (opc == OpCodes.Shr)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Shr);
                }
                else if (opc == OpCodes.Shr_Un)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.ShrUn);
                }
                else if (opc == OpCodes.Neg)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Neg);
                }
                else if (opc == OpCodes.Not)
                {
                    DualParamMathOpCode(instructions, opc, InstructionTypes.Not);
                }
                else if (opc == OpCodes.Ldstr)
                {
                    LdStrOpCode(instructions, opc, InstructionTypes.LdStr);
                }
                else if (opc == OpCodes.Ldnull)
                {
                    LdnullOpCode(instructions, opc, InstructionTypes.LdNull);
                }
                else if (new OpCode[] { OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Stloc_S, OpCodes.Stloc }.Contains(opc))
                {
                    EncodedCountOpCode(instructions, opc, InstructionTypes.StLoc);
                }
                else if (new OpCode[] { OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Ldloc_S, OpCodes.Ldloc }.Contains(opc))
                {
                    EncodedCountOpCode(instructions, opc, InstructionTypes.LdLoc);
                }
                else if (new OpCode[] { OpCodes.Ldc_R8, OpCodes.Ldc_R4, OpCodes.Ldc_I8, OpCodes.Ldc_I4, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, OpCodes.Ldc_I4_M1, OpCodes.Ldc_I4_S }.Contains(opc))
                {
                    ExtnValOpcode(instructions, opc, InstructionTypes.Ldc);
                }
                else if (new OpCode[] { OpCodes.Brfalse, OpCodes.Brfalse_S }.Contains(opc))
                {
                    ConditionalBranchOpCode(instructions, opc, InstructionTypes.BrFalse);
                }
                else if (new OpCode[] { OpCodes.Brtrue, OpCodes.Brtrue_S }.Contains(opc))
                {
                    ConditionalBranchOpCode(instructions, opc, InstructionTypes.BrTrue);
                }
                else if (new OpCode[] { OpCodes.Br, OpCodes.Br_S }.Contains(opc))
                {
                    BranchOpCode(instructions, opc, InstructionTypes.Br);
                }
                else if (opc == OpCodes.Ret)
                {
                    RetOpCode(instructions, opc, InstructionTypes.Ret);
                }
                else if (opc == OpCodes.Call)
                {
                    CallOpCode(instructions, opc, InstructionTypes.Call);
                }
                else if (opc == OpCodes.Callvirt)
                {
                    CallOpCode(instructions, opc, InstructionTypes.CallVirt);
                }
                else if (opc == OpCodes.Newobj)
                {
                    NewobjOpCode(instructions, opc, InstructionTypes.Newobj);
                }
                else if (opc == OpCodes.Newarr)
                {
                    NewarrOpCode(instructions, opc, InstructionTypes.Newarr);
                }
                else if (opc == OpCodes.Stsfld)
                {
                    StsfldOpCode(instructions, opc, InstructionTypes.Stsfld);
                }
                else if (opc == OpCodes.Stfld)
                {
                    StfldOpCode(instructions, opc, InstructionTypes.Stfld);
                }
                else if (opc == OpCodes.Ldfld)
                {
                    LdfldOpCode(instructions, opc, InstructionTypes.Ldfld);
                }
                else if (opc == OpCodes.Ldsfld)
                {
                    LdsfldOpCode(instructions, opc, InstructionTypes.Ldsfld);
                }
                else if (opc == OpCodes.Ldflda)
                {
                    LdfldOpCode(instructions, opc, InstructionTypes.Ldflda);
                }
                else if (opc == OpCodes.Ldsflda)
                {
                    LdsfldOpCode(instructions, opc, InstructionTypes.Ldsflda);
                }
                else if (opc == OpCodes.Ldelema)
                {
                    LdelemOpCode(instructions, opc, InstructionTypes.Ldelema);
                }
                else if (opc == OpCodes.Ldlen)
                {
                    LdlenOpCode(instructions, opc, InstructionTypes.Ldlen);
                }
                else if (opc == OpCodes.Ldelem)
                {
                    LdelemOpCode(instructions, opc, InstructionTypes.Ldelem);
                }
                else if (new OpCode[] { OpCodes.Ldelem_I, OpCodes.Ldelem_I1, OpCodes.Ldelem_I2, OpCodes.Ldelem_I4, OpCodes.Ldelem_I8, OpCodes.Ldelem_R4, OpCodes.Ldelem_R8, OpCodes.Ldelem_Ref, OpCodes.Ldelem_U1, OpCodes.Ldelem_U2, OpCodes.Ldelem_U4 }.Contains(opc))
                {
                    LdelemOpCode(instructions, opc, InstructionTypes.Ldelem);
                }
                else if (opc == OpCodes.Stelem)
                {
                    StelemOpCode(instructions, opc, InstructionTypes.Stelem);
                }
                else if (new OpCode[] { OpCodes.Stelem_I, OpCodes.Stelem_I1, OpCodes.Stelem_I2, OpCodes.Stelem_I4, OpCodes.Stelem_I8, OpCodes.Stelem_R4, OpCodes.Stelem_R8, OpCodes.Stelem_Ref }.Contains(opc))
                {
                    StelemOpCode(instructions, opc, InstructionTypes.Stelem);
                }
                else if (new OpCode[] { OpCodes.Stind_I, OpCodes.Stind_I1, OpCodes.Stind_I2, OpCodes.Stind_I4, OpCodes.Stind_I8, OpCodes.Stind_R4, OpCodes.Stind_R8, OpCodes.Stind_Ref }.Contains(opc))
                {
                    StindOpCode(instructions, opc, InstructionTypes.Stind);
                }
                else if (new OpCode[] { OpCodes.Ldind_I, OpCodes.Ldind_I1, OpCodes.Ldind_I2, OpCodes.Ldind_I4, OpCodes.Ldind_I8, OpCodes.Ldind_U1, OpCodes.Ldind_U2, OpCodes.Ldind_U4, OpCodes.Ldind_R4, OpCodes.Ldind_R8, OpCodes.Ldind_Ref }.Contains(opc))
                {
                    LdindOpCode(instructions, opc, InstructionTypes.LdInd);
                }
                else if (opc == OpCodes.Ceq)
                {
                    CompareOpCode(instructions, opc, InstructionTypes.Ceq);
                }
                else if (opc == OpCodes.Cgt)
                {
                    CompareOpCode(instructions, opc, InstructionTypes.Cgt);
                }
                else if (opc == OpCodes.Cgt_Un)
                {
                    CompareOpCode(instructions, opc, InstructionTypes.CgtUn);
                }
                else if (opc == OpCodes.Clt)
                {
                    CompareOpCode(instructions, opc, InstructionTypes.Clt);
                }
                else if (opc == OpCodes.Clt_Un)
                {
                    CompareOpCode(instructions, opc, InstructionTypes.CltUn);
                }
                else if (opc == OpCodes.Ckfinite)
                {
                    throw new Exception("Ckfinite");
                }
                else if (opc == OpCodes.Switch)
                {
                    SwitchOpCode(instructions, opc, InstructionTypes.Switch);
                }
                else if (new OpCode[] { OpCodes.Beq, OpCodes.Beq_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.Beq);
                }
                else if (new OpCode[] { OpCodes.Bne_Un, OpCodes.Bne_Un_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.BneUn);
                }
                else if (new OpCode[] { OpCodes.Bgt, OpCodes.Bgt_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.Bgt);
                }
                else if (new OpCode[] { OpCodes.Blt, OpCodes.Blt_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.Blt);
                }
                else if (new OpCode[] { OpCodes.Bgt_Un, OpCodes.Bgt_Un_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.BgtUn);
                }
                else if (new OpCode[] { OpCodes.Blt_Un, OpCodes.Blt_Un_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.BltUn);
                }
                else if (new OpCode[] { OpCodes.Bge, OpCodes.Bge_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.Bge);
                }
                else if (new OpCode[] { OpCodes.Ble, OpCodes.Ble_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.Ble);
                }
                else if (new OpCode[] { OpCodes.Bge_Un, OpCodes.Bge_Un_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.BgeUn);
                }
                else if (new OpCode[] { OpCodes.Ble_Un, OpCodes.Ble_Un_S }.Contains(opc))
                {
                    CompConditionalBranchOpCode(instructions, opc, InstructionTypes.BleUn);
                }
                else if (new OpCode[] { OpCodes.Leave, OpCodes.Leave_S }.Contains(opc))
                {
                    LeaveOpCode(instructions, opc, InstructionTypes.Leave);
                }
                else if (opc == OpCodes.Endfinally)
                {
                    EndFinallyOpCode(instructions, opc, InstructionTypes.EndFinally);
                }
                else if (opc == OpCodes.Ldtoken)
                {
                    LdTokenOpCode(instructions, opc, InstructionTypes.Ldtoken);
                }
                else if (new OpCode[] { OpCodes.Ldloca, OpCodes.Ldloca_S }.Contains(opc))
                {
                    LdLocaOpCode(instructions, opc, InstructionTypes.LdLoca);
                }
                else if (opc == OpCodes.Constrained)
                {
                    //Store this token for the next instruction, guaranteed to be a callvirt, which will be updated appropriately
                    ConstrainedCallVirt = instructions.GetParameter(0);
                }
                else
                    throw new Exception(opc.Name);

            }
            while (instructions.NextInstruction());

            //Optimize instruction sequences
            Tokens = new List<SSAToken>();
            for (int i = 0; i < SSAToken.Tokens.Count; i++)
            {
                var cur_tkn = SSAToken.Tokens[i];

                if (new InstructionTypes[] { }.Contains(cur_tkn.Operation))
                {

                }
                else
                {
                    Tokens.Add(cur_tkn);
                }
            }

            SSAToken.Tokens = new List<SSAToken>();
        }

        public void Reprocess(List<TypeDef> list)
        {
            for (int i = 0; i < Tokens.Count; i++)
            {
                switch (Tokens[i].Operation)
                {
                    case InstructionTypes.Ldsflda:
                    case InstructionTypes.Ldsfld:
                    case InstructionTypes.Stsfld:
                        {
                            for (int j = 0; j < list.Count; j++)
                                for (int k = 0; k < list[j].StaticFields.Length; k++)
                                {
                                    if (list[j].StaticFields[k].MetadataToken == (int)Tokens[i].Constants[0])
                                    {
                                        Tokens[i].String = MachineSpec.GetTypeName(list[j]);
                                        Tokens[i].Constants[0] = (ulong)list[j].StaticFields[k].Offset;
                                        Tokens[i].RetValSz = list[j].StaticFields[k].Size;
                                    }
                                }

                            if (string.IsNullOrEmpty(Tokens[i].String))
                                throw new Exception();
                        }
                        break;
                    case InstructionTypes.Ldflda:
                    case InstructionTypes.Ldfld:
                    case InstructionTypes.Stfld:
                        {
                            for (int j = 0; j < list.Count; j++)
                                for (int k = 0; k < list[j].InstanceFields.Length; k++)
                                {
                                    if (list[j].InstanceFields[k].MetadataToken == (int)Tokens[i].Constants[0])
                                    {
                                        Tokens[i].String = MachineSpec.GetTypeName(list[j]);
                                        Tokens[i].Constants[0] = (ulong)list[j].InstanceFields[k].Offset;
                                        Tokens[i].RetValSz = list[j].InstanceFields[k].Size;
                                    }
                                }

                            if (string.IsNullOrEmpty(Tokens[i].String))
                                throw new Exception();
                        }
                        break;
                }
            }
        }

        public SSAToken[] GetTokens()
        {
            return Tokens.ToArray();
        }

        public string[] GetStrings()
        {
            return StringTable.ToArray();
        }
    }
}
