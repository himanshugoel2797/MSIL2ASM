using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    public static class OptimizationTokenParser
    {
        private static OptimizationInstruction ParseInstr(InstructionTypes t)
        {
            switch (t)
            {
                case InstructionTypes.LdArg:
                case InstructionTypes.LdArga:
                case InstructionTypes.Ldc:
                case InstructionTypes.Ldelem:
                case InstructionTypes.Ldelema:
                case InstructionTypes.Ldfld:
                case InstructionTypes.Ldflda:
                case InstructionTypes.LdInd:
                case InstructionTypes.Ldlen:
                case InstructionTypes.LdLoc:
                case InstructionTypes.LdLoca:
                case InstructionTypes.LdNull:
                case InstructionTypes.Ldsfld:
                case InstructionTypes.Ldsflda:
                case InstructionTypes.LdStr:
                case InstructionTypes.Ldtoken:
                    return OptimizationInstruction.Ld;

                case InstructionTypes.StArg:
                case InstructionTypes.Stelem:
                case InstructionTypes.Stfld:
                case InstructionTypes.Stind:
                case InstructionTypes.StLoc:
                case InstructionTypes.Stsfld:
                    return OptimizationInstruction.St;

                case InstructionTypes.Add:
                case InstructionTypes.AddCheckOverflow:
                case InstructionTypes.UAddCheckOverflow:
                    return OptimizationInstruction.Add;

                case InstructionTypes.And:
                    return OptimizationInstruction.And;

                case InstructionTypes.Beq:
                case InstructionTypes.Bge:
                case InstructionTypes.BgeUn:
                case InstructionTypes.Bgt:
                case InstructionTypes.BgtUn:
                case InstructionTypes.Ble:
                case InstructionTypes.BleUn:
                case InstructionTypes.Blt:
                case InstructionTypes.BltUn:
                case InstructionTypes.BneUn:
                case InstructionTypes.Br:
                case InstructionTypes.BrFalse:
                case InstructionTypes.BrTrue:
                    return OptimizationInstruction.Branch;

                case InstructionTypes.Call:
                case InstructionTypes.Calli:
                    return OptimizationInstruction.Call;

                case InstructionTypes.CallVirt:
                    return OptimizationInstruction.CallVirt;

                case InstructionTypes.Ceq:
                case InstructionTypes.Cgt:
                case InstructionTypes.CgtUn:
                case InstructionTypes.Clt:
                case InstructionTypes.CltUn:
                    return OptimizationInstruction.Compare;

                case InstructionTypes.Convert:
                    return OptimizationInstruction.Convert;

                case InstructionTypes.Divide:
                case InstructionTypes.UDivide:
                    return OptimizationInstruction.Div;

                case InstructionTypes.Dup:
                    return OptimizationInstruction.Dup;

                case InstructionTypes.Multiply:
                    return OptimizationInstruction.Mul;

                case InstructionTypes.Neg:
                    return OptimizationInstruction.Neg;

                case InstructionTypes.Not:
                    return OptimizationInstruction.Not;

                case InstructionTypes.Or:
                    return OptimizationInstruction.Or;

                case InstructionTypes.Pop:
                    return OptimizationInstruction.Pop;

                case InstructionTypes.Rem:
                case InstructionTypes.URem:
                    return OptimizationInstruction.Rem;

                case InstructionTypes.Ret:
                    return OptimizationInstruction.Ret;

                case InstructionTypes.Shl:
                    return OptimizationInstruction.Shl;

                case InstructionTypes.Shr:
                case InstructionTypes.ShrUn:
                    return OptimizationInstruction.Shr;

                case InstructionTypes.Subtract:
                case InstructionTypes.SubtractCheckOverflow:
                case InstructionTypes.USubtractCheckOverflow:
                    return OptimizationInstruction.Sub;

                case InstructionTypes.Switch:
                    return OptimizationInstruction.Switch;

                case InstructionTypes.Xor:
                    return OptimizationInstruction.Xor;
            }

            throw new Exception();
        }

        private static OptimizationInstructionSubType ParseInstrSubType(InstructionTypes t)
        {
            switch (t)
            {
                case InstructionTypes.LdArg:
                case InstructionTypes.StArg:
                    return OptimizationInstructionSubType.Arg;

                case InstructionTypes.LdArga:
                    return OptimizationInstructionSubType.ArgAddress;

                case InstructionTypes.Ldc:
                    return OptimizationInstructionSubType.Constant;

                case InstructionTypes.Ldelem:
                case InstructionTypes.Stelem:
                    return OptimizationInstructionSubType.Element;

                case InstructionTypes.Ldelema:
                    return OptimizationInstructionSubType.ElementAddress;

                case InstructionTypes.Ldfld:
                case InstructionTypes.Stfld:
                    return OptimizationInstructionSubType.Field;

                case InstructionTypes.Ldflda:
                    return OptimizationInstructionSubType.FieldAddress;

                case InstructionTypes.LdInd:
                case InstructionTypes.Stind:
                    return OptimizationInstructionSubType.Indirect;

                case InstructionTypes.Ldlen:
                    return OptimizationInstructionSubType.Length;

                case InstructionTypes.LdLoc:
                case InstructionTypes.StLoc:
                    return OptimizationInstructionSubType.Local;

                case InstructionTypes.LdLoca:
                    return OptimizationInstructionSubType.LocalAddress;

                case InstructionTypes.LdNull:
                    return OptimizationInstructionSubType.Null;

                case InstructionTypes.Ldsfld:
                case InstructionTypes.Stsfld:
                    return OptimizationInstructionSubType.StaticField;

                case InstructionTypes.Ldsflda:
                    return OptimizationInstructionSubType.StaticFieldAddress;

                case InstructionTypes.LdStr:
                    return OptimizationInstructionSubType.String;

                case InstructionTypes.Ldtoken:
                    return OptimizationInstructionSubType.Token;

                case InstructionTypes.Add:
                case InstructionTypes.And:
                case InstructionTypes.Divide:
                case InstructionTypes.Multiply:
                case InstructionTypes.Neg:
                case InstructionTypes.Not:
                case InstructionTypes.Or:
                case InstructionTypes.Rem:
                case InstructionTypes.Shl:
                case InstructionTypes.Shr:
                case InstructionTypes.Subtract:
                case InstructionTypes.Xor:
                    return OptimizationInstructionSubType.None;

                case InstructionTypes.AddCheckOverflow:
                case InstructionTypes.SubtractCheckOverflow:
                    return OptimizationInstructionSubType.CheckOverflow;

                case InstructionTypes.UDivide:
                case InstructionTypes.URem:
                case InstructionTypes.ShrUn:
                    return OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.UAddCheckOverflow:
                case InstructionTypes.USubtractCheckOverflow:
                    return OptimizationInstructionSubType.Unsigned | OptimizationInstructionSubType.CheckOverflow;

                case InstructionTypes.Ceq:
                case InstructionTypes.Beq:
                    return OptimizationInstructionSubType.Equal;

                case InstructionTypes.Bge:
                    return OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Greater;

                case InstructionTypes.Cgt:
                case InstructionTypes.Bgt:
                    return OptimizationInstructionSubType.Greater;

                case InstructionTypes.Ble:
                    return OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal;

                case InstructionTypes.Clt:
                case InstructionTypes.Blt:
                    return OptimizationInstructionSubType.Less;

                case InstructionTypes.Br:
                    return OptimizationInstructionSubType.None;

                case InstructionTypes.BrFalse:
                    return OptimizationInstructionSubType.False;

                case InstructionTypes.BrTrue:
                    return OptimizationInstructionSubType.True;

                case InstructionTypes.BgeUn:
                    return OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.CgtUn:
                case InstructionTypes.BgtUn:
                    return OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.BleUn:
                    return OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.CltUn:
                case InstructionTypes.BltUn:
                    return OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.BneUn:
                    return OptimizationInstructionSubType.NotEqual | OptimizationInstructionSubType.Unsigned;

                case InstructionTypes.Call:
                    return OptimizationInstructionSubType.None;

                case InstructionTypes.CallVirt:
                    return OptimizationInstructionSubType.None;

                case InstructionTypes.Calli:
                    return OptimizationInstructionSubType.Indirect;

                case InstructionTypes.Convert:
                    return OptimizationInstructionSubType.None;

                case InstructionTypes.ConvertCheckOverflow:
                    return OptimizationInstructionSubType.CheckOverflow;

                case InstructionTypes.Dup:
                case InstructionTypes.Pop:
                case InstructionTypes.Ret:
                case InstructionTypes.Switch:
                    return OptimizationInstructionSubType.None;
            }

            throw new Exception();
        }

        public static OptimizationToken Parse(SSAToken tkn)
        {
            OptimizationToken oTkn = new OptimizationToken()
            {
                ID = tkn.ID,
                Instruction = ParseInstr(tkn.Operation),
                SubType = ParseInstrSubType(tkn.Operation),
                Offset = tkn.InstructionOffset,
            };

            switch (tkn.Operation)
            {
                case InstructionTypes.Add:
                case InstructionTypes.And:
                case InstructionTypes.Divide:
                case InstructionTypes.Multiply:
                case InstructionTypes.Or:
                case InstructionTypes.Rem:
                case InstructionTypes.Shl:
                case InstructionTypes.Shr:
                case InstructionTypes.Subtract:
                case InstructionTypes.Xor:
                case InstructionTypes.AddCheckOverflow:
                case InstructionTypes.SubtractCheckOverflow:
                case InstructionTypes.UDivide:
                case InstructionTypes.URem:
                case InstructionTypes.ShrUn:
                case InstructionTypes.UAddCheckOverflow:
                case InstructionTypes.USubtractCheckOverflow:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[0],
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[1],
                            }
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown,
                                Size = 8,
                                Value = 0,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Neg:
                case InstructionTypes.Not:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[0],
                            },
                        };

                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown,
                                Size = 0,
                                Value = 0,
                            },
                        };
                    }
                    break;
                case InstructionTypes.Beq:
                case InstructionTypes.Bge:
                case InstructionTypes.BgeUn:
                case InstructionTypes.Bgt:
                case InstructionTypes.BgtUn:
                case InstructionTypes.Ble:
                case InstructionTypes.BleUn:
                case InstructionTypes.Blt:
                case InstructionTypes.BltUn:
                case InstructionTypes.BneUn:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[0]
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[1]
                            },
                        };

                        oTkn.Constants = new ulong[]
                        {
                            tkn.Constants[0]
                        };
                    }
                    break;
                case InstructionTypes.Ceq:
                case InstructionTypes.Cgt:
                case InstructionTypes.CgtUn:
                case InstructionTypes.Clt:
                case InstructionTypes.CltUn:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[0]
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[1]
                            },
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Integer,
                                Size = 4,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Br:
                    {
                        oTkn.Constants = new ulong[]
                        {
                            tkn.Constants[0]
                        };
                    }
                    break;
                case InstructionTypes.BrFalse:
                case InstructionTypes.BrTrue:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                Value = (ulong)tkn.Parameters[0],
                                ParameterType = OptimizationParameterType.Integer,
                                Size = 4
                            },
                        };

                        oTkn.Constants = new ulong[]
                        {
                            tkn.Constants[0]
                        };
                    }
                    break;
                case InstructionTypes.Call:
                case InstructionTypes.Calli:
                case InstructionTypes.CallVirt:
                case InstructionTypes.CallVirtConstrained:
                    {
                        oTkn.Strings = new string[] { tkn.String };
                        oTkn.Parameters = new OptimizationParameter[tkn.Parameters.Length];
                        for (int i = 0; i < oTkn.Parameters.Length; i++)
                        {
                            oTkn.Parameters[i] = new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[i]
                            };
                        }

                        if (tkn.RetValSz != 0)
                        {
                            oTkn.Results = new OptimizationParameter[]
                            {
                                new OptimizationParameter()
                                {
                                    ParameterLocation = OptimizationParameterLocation.Result,
                                    ParameterType = OptimizationParameterType.Unknown,
                                    Size = tkn.RetValSz,
                                }
                            };
                        }
                    }
                    break;
                case InstructionTypes.Convert:
                case InstructionTypes.ConvertCheckOverflow:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Integer,
                            }
                        };

                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                            case OperandTypes.U:
                            case OperandTypes.I8:
                            case OperandTypes.U8:
                                oTkn.Results[0].Size = 8;
                                break;
                            case OperandTypes.I1:
                            case OperandTypes.U1:
                                oTkn.Results[0].Size = 1;
                                break;
                            case OperandTypes.I2:
                            case OperandTypes.U2:
                                oTkn.Results[0].Size = 2;
                                break;
                            case OperandTypes.I4:
                            case OperandTypes.U4:
                                oTkn.Results[0].Size = 4;
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }

                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                            case OperandTypes.I1:
                            case OperandTypes.I2:
                            case OperandTypes.I4:
                            case OperandTypes.I8:
                                break;
                            case OperandTypes.U:
                            case OperandTypes.U1:
                            case OperandTypes.U2:
                            case OperandTypes.U4:
                            case OperandTypes.U8:
                                oTkn.SubType |= OptimizationInstructionSubType.Unsigned;
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                    break;
                //case InstructionTypes.Cpblk:
                case InstructionTypes.Dup:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown
                            }
                        };
                    }
                    break;
                //case InstructionTypes.EndFilter:
                //case InstructionTypes.EndFinally:
                //case InstructionTypes.InitBlk:
                //case InstructionTypes.Jmp:
                case InstructionTypes.LdArg:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown,
                                Size = MachineSpec.PointerSize,
                            }
                        };
                    }
                    break;
                case InstructionTypes.LdArga:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Ldftn:
                    {

                    }
                    break;
                case InstructionTypes.LdInd:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.ManagedPointer | OptimizationParameterType.UnmanagedPointer,
                                Value = (ulong)tkn.Parameters[0],
                                Size = MachineSpec.PointerSize,
                            },
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown
                            },
                        };

                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                            case OperandTypes.U:
                            case OperandTypes.I8:
                            case OperandTypes.U8:
                                oTkn.Results[0].Size = 8;
                                break;
                            case OperandTypes.I1:
                            case OperandTypes.U1:
                                oTkn.Results[0].Size = 1;
                                break;
                            case OperandTypes.I2:
                            case OperandTypes.U2:
                                oTkn.Results[0].Size = 2;
                                break;
                            case OperandTypes.I4:
                            case OperandTypes.U4:
                                oTkn.Results[0].Size = 4;
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                    break;
                case InstructionTypes.LdLoc:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Results = new OptimizationParameter[]
                           {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown
                            },
                           };
                    }
                    break;
                case InstructionTypes.LdLoca:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Results = new OptimizationParameter[]
                           {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.ManagedPointer
                            },
                           };
                    }
                    break;
                case InstructionTypes.LdNull:
                    {
                        oTkn.Results = new OptimizationParameter[]
                           {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Const,
                                ParameterType = OptimizationParameterType.Integer,
                                Value = 0,
                                Size = MachineSpec.PointerSize,
                            },
                           };
                    }
                    break;
                //case InstructionTypes.Leave:
                //case InstructionTypes.LocAlloc:
                case InstructionTypes.Pop:
                    {
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };
                    }
                    break;
                case InstructionTypes.Ret:
                    {
                        if (tkn.Parameters != null)
                            oTkn.Parameters = new OptimizationParameter[]
                            {
                                new OptimizationParameter()
                                {
                                    ParameterLocation = OptimizationParameterLocation.Index,
                                    ParameterType = OptimizationParameterType.Unknown,
                                    Value = (ulong)tkn.Parameters[0],
                                    Size = MachineSpec.PointerSize,
                                }
                            };
                    }
                    break;
                case InstructionTypes.StArg:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                                Size = MachineSpec.PointerSize,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Stind:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[1],
                            },
                        };

                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                            case OperandTypes.U:
                            case OperandTypes.I8:
                            case OperandTypes.U8:
                                oTkn.Parameters[0].Size = 8;
                                break;
                            case OperandTypes.I1:
                            case OperandTypes.U1:
                                oTkn.Parameters[0].Size = 1;
                                break;
                            case OperandTypes.I2:
                            case OperandTypes.U2:
                                oTkn.Parameters[0].Size = 2;
                                break;
                            case OperandTypes.I4:
                            case OperandTypes.U4:
                                oTkn.Parameters[0].Size = 4;
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                    }
                    break;
                case InstructionTypes.StLoc:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };
                    }
                    break;
                case InstructionTypes.Switch:
                    {
                        //TODO Implement switch
                    }
                    break;
                case InstructionTypes.LdStr:
                    {
                        oTkn.Strings = new string[] { tkn.String };
                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Const,
                                Value = 0,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                                Size = MachineSpec.PointerSize,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Ldc:
                    {
                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Const,
                                Value = tkn.Constants[1],
                                ParameterType = OptimizationParameterType.Integer,
                            }
                        };

                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                            case OperandTypes.U:
                            case OperandTypes.I8:
                            case OperandTypes.U8:
                                oTkn.Results[0].Size = 8;
                                break;
                            case OperandTypes.I1:
                            case OperandTypes.U1:
                                oTkn.Results[0].Size = 1;
                                break;
                            case OperandTypes.I2:
                            case OperandTypes.U2:
                                oTkn.Results[0].Size = 2;
                                break;
                            case OperandTypes.I4:
                            case OperandTypes.U4:
                                oTkn.Results[0].Size = 4;
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }
                        /*
                        //Sign extend values
                        switch ((OperandTypes)tkn.Constants[0])
                        {
                            case OperandTypes.I:
                                oTkn.Results[0].Value = unchecked((ulong)(long)oTkn.Results[0].Value);
                                break;
                            case OperandTypes.I1:
                                oTkn.Results[0].Value = unchecked((ulong)(long)(sbyte)oTkn.Results[0].Value);
                                break;
                            case OperandTypes.I2:
                                oTkn.Results[0].Value = unchecked((ulong)(long)(short)oTkn.Results[0].Value);
                                break;
                            case OperandTypes.I4:
                                oTkn.Results[0].Value = unchecked((ulong)(long)(int)oTkn.Results[0].Value);
                                break;
                            case OperandTypes.I8:
                            case OperandTypes.U:
                            case OperandTypes.U1:
                            case OperandTypes.U2:
                            case OperandTypes.U4:
                            case OperandTypes.U8:
                                break;
                            default:
                                throw new Exception("Unsupported type");
                        }*/
                    }
                    break;
                case InstructionTypes.Ldsfld:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Strings = new string[] { tkn.String };
                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown,
                                Size = tkn.RetValSz,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Stsfld:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Strings = new string[] { tkn.String };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                                Size = tkn.RetValSz
                            }
                        };
                    }
                    break;
                case InstructionTypes.Ldfld:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.Unknown,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Ldflda:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                                Value = (ulong)tkn.Parameters[0],
                            }
                        };

                        oTkn.Results = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Result,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                            }
                        };
                    }
                    break;
                case InstructionTypes.Stfld:
                    {
                        oTkn.Constants = new ulong[] { tkn.Constants[0] };
                        oTkn.Parameters = new OptimizationParameter[]
                        {
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.Unknown,
                                Value = (ulong)tkn.Parameters[0],
                            },
                            new OptimizationParameter()
                            {
                                ParameterLocation = OptimizationParameterLocation.Index,
                                ParameterType = OptimizationParameterType.ManagedPointer,
                                Value = (ulong)tkn.Parameters[0],
                            },
                        };
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (oTkn.Parameters == null)
                oTkn.Parameters = new OptimizationParameter[0];

            if (oTkn.Results == null)
                oTkn.Results = new OptimizationParameter[0];

            switch (tkn.Operation)
            {
                case InstructionTypes.Convert:
                case InstructionTypes.ConvertCheckOverflow:
                    {
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any | Assembly.AssemRegisters.Const,
                        };
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rax
                        };
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[0];
                    }
                    break;
                case InstructionTypes.UDivide:
                case InstructionTypes.Divide:
                    {
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any,
                            Assembly.AssemRegisters.Rax
                        };
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rax
                        };
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rdx
                        };
                    }
                    break;
                case InstructionTypes.URem:
                case InstructionTypes.Rem:
                    {
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any,
                            Assembly.AssemRegisters.Rax
                        };
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rdx
                        };
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rax
                        };
                    }
                    break;
                case InstructionTypes.Shl:
                case InstructionTypes.Shr:
                case InstructionTypes.ShrUn:
                    {
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Rcx | Assembly.AssemRegisters.Const8,
                            Assembly.AssemRegisters.Any,
                        };
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any
                        };
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[0];
                    }
                    break;
                case InstructionTypes.Add:
                case InstructionTypes.Xor:
                    {
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any | Assembly.AssemRegisters.Const8 | Assembly.AssemRegisters.Const16 | Assembly.AssemRegisters.Const32,
                            Assembly.AssemRegisters.Any,
                        };
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[]
                        {
                            Assembly.AssemRegisters.Any
                        };
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[0];
                    }
                    break;
                default:
                    {
                        oTkn.ThunkRegisters = new Assembly.AssemRegisters[0];
                        oTkn.ParameterRegisters = new Assembly.AssemRegisters[oTkn.Parameters.Length];
                        oTkn.ResultRegisters = new Assembly.AssemRegisters[oTkn.Results.Length];
                        for (int i = 0; i < oTkn.Parameters.Length; i++)
                            oTkn.ParameterRegisters[i] = Assembly.AssemRegisters.Any;

                        for (int i = 0; i < oTkn.Results.Length; i++)
                            oTkn.ResultRegisters[i] = Assembly.AssemRegisters.Any;
                    }
                    break;
            }

            oTkn.SaveParameterRegister = new bool[oTkn.ParameterRegisters.Length];

            return oTkn;
        }
    }
}
