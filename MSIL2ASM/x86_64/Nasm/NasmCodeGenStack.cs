using MSIL2ASM.x86_64.Nasm.Assembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    partial class NasmEmitter
    {
        int stringIdx = 0;
        private void LdInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.String:
                    AddString(tkn.Strings[0], stringIdx);
                    Emitter.MovLabelAddressToRegister(tkn.ResultRegisters[0], GetStringLabel(stringIdx++));
                    break;
                case OptimizationInstructionSubType.Local:
                    Emitter.MovRelativeAddressMultToRegisterSize(AssemRegisters.Rsp, AssemRegisters.Rsp, 0, LocalTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize, tkn.ResultRegisters[0], tkn.Results[0].Size);
                    break;
                case OptimizationInstructionSubType.Arg:
                    Emitter.MovRelativeAddressMultToRegisterSize(AssemRegisters.Rsp, AssemRegisters.Rsp, 0, ArgumentTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize, tkn.ResultRegisters[0], tkn.Results[0].Size);
                    break;
                case OptimizationInstructionSubType.StaticField:
                    Emitter.MovLabelRelativeAddressToRegisterSize(tkn.Strings[0] + "_static", (int)tkn.Constants[0], tkn.ResultRegisters[0], tkn.Results[0].Size);
                    break;
                case OptimizationInstructionSubType.FieldAddress:
                    Emitter.LoadEffectiveAddress(tkn.ParameterRegisters[0], (int)tkn.Constants[0], tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Indirect:
                    Emitter.MovRelativeAddressToRegisterSize(tkn.ParameterRegisters[0], 0, tkn.ResultRegisters[0], tkn.Results[0].Size);
                    break;
                case OptimizationInstructionSubType.Null:
                    Emitter.MovConstantToRegister(0, tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Function:
                    {
                        if (!externals.Contains(tkn.Strings[0]))
                            externals.Add(tkn.Strings[0]);

                        Emitter.MovLabelAddressToRegister(tkn.ResultRegisters[0], tkn.Strings[0]);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void StInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.Local:
                    if ((tkn.Parameters[0].ParameterLocation & OptimizationParameterLocation.Const) != 0)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    Emitter.MovRegisterToRegisterRelativeAddressMultSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, AssemRegisters.Rsp, AssemRegisters.Rsp, 0, LocalTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize);
                    break;
                case OptimizationInstructionSubType.Arg:
                    if ((tkn.Parameters[0].ParameterLocation & OptimizationParameterLocation.Const) != 0)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    Emitter.MovRegisterToRegisterRelativeAddressMultSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, AssemRegisters.Rsp, AssemRegisters.Rsp, 0, ArgumentTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize);
                    break;
                case OptimizationInstructionSubType.StaticField:
                    if ((tkn.Parameters[0].ParameterLocation & OptimizationParameterLocation.Const) != 0)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    Emitter.MovRegisterToLabelRelativeAddressSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, tkn.Strings[0] + "_static", (int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Indirect:
                    if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    if (tkn.Parameters[1].ParameterLocation == OptimizationParameterLocation.Const)
                        Emitter.MovRegisterToAddressSize(tkn.ParameterRegisters[0], tkn.Parameters[1].Value, tkn.Parameters[0].Size);
                    else
                        Emitter.MovRegisterToRegisterAddressSize(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.Parameters[0].Size);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #region Math
        private void AddInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.AddConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);
                            Emitter.Add(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, tkn.ParameterRegisters[1], tkn.Parameters[1].Size, tkn.ResultRegisters[0], tkn.Results[0].Size, true);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void AndInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.AndConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);
                            Emitter.And(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void DivInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Divide(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                case OptimizationInstructionSubType.Unsigned:
                    {
                        Emitter.UDivide(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void MulInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.MultiplyConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.Multiply(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void NegInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Neg(tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void NotInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Not(tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void OrInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.OrConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.Or(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void RemInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Remainder(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                case OptimizationInstructionSubType.Unsigned:
                    {
                        Emitter.URemainder(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ShlInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.ShiftLeft(tkn.ParameterRegisters[1], (int)tkn.Parameters[0].Value, tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.ShiftLeft(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ShrInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.ShiftRight(tkn.ParameterRegisters[1], (int)tkn.Parameters[0].Value, tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.ShiftRight(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                case OptimizationInstructionSubType.Unsigned:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.ShiftRightUn(tkn.ParameterRegisters[1], (int)tkn.Parameters[0].Value, tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.ShiftRightUn(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SubInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.SubConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.Sub(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void XorInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if ((tkn.ParameterRegisters[0] & AssemRegisters.Const) != 0)
                            Emitter.XorConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                        {
                            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                            Emitter.Xor(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        private void BranchInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    Emitter.JmpRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Greater:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpGtRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Less:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpLtRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpGeRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpLeRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpGtUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpLtUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpGeUnRelativeLabel((int)tkn.Constants[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpLeUnRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.Equal:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpEqRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.NotEqual | OptimizationInstructionSubType.Unsigned:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.JmpNeRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.True:
                    Emitter.TestBool(tkn.ParameterRegisters[0]);
                    Emitter.JmpNZeroRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.False:
                    Emitter.TestBool(tkn.ParameterRegisters[0]);
                    Emitter.JmpZeroRelativeLabel((int)tkn.Constants[0]);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void OutOp(OptimizationToken tkn, int sz)
        {
            if (tkn.ParameterRegisters[0] == AssemRegisters.Const8)
                Emitter.OutConst((byte)tkn.Parameters[0].Value, tkn.ParameterRegisters[1], sz);
            else
            {
                if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                    Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

                Emitter.Out(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], sz);
            }
        }

        private void InOp(OptimizationToken tkn)
        {
            //TODO: In operation needs to write to the argument address
            throw new NotImplementedException();
        }

        private void CallInstr(OptimizationToken[] tkns, int idx)
        {
            var tkn = tkns[idx];

            switch (tkn.Strings[0])
            {
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Halt_0System_Void_r_":
                    Emitter.Hlt();
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Sti_0System_Void_r_":
                    Emitter.Sti();
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Cli_0System_Void_r_":
                    Emitter.Cli();
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_UInt32_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_Int32_2System_Void_r_":
                    OutOp(tkn, 4);
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_UInt16_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_Int16_2System_Void_r_":
                    OutOp(tkn, 2);
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_Byte_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_Out_0System_UInt16_1System_SByte_2System_Void_r_":
                    OutOp(tkn, 1);
                    return;
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_UInt32_$addr_o_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_UInt16_$addr_o_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_Byte_$addr_o_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_Int32_$addr_o_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_Int16_$addr_o_2System_Void_r_":
                case "mthd_s_MSIL2ASM_Builtins_x86_64_In_0System_UInt16_1System_SByte_$addr_o_2System_Void_r_":
                    InOp(tkn);
                    return;
            }

            //Emit extern
            if (!tkn.Strings[0].Contains(prefix) && !externals.Contains(tkn.Strings[0]))
                externals.Add(tkn.Strings[0]);

            if (tkn.Strings[0] == "ctor_System_Object_0System_Object_i_1System_Object_r_" && !externals.Contains(tkn.Strings[0]))
                externals.Add(tkn.Strings[0]);

            //Determine active registers, save them
            var activeRegs = GetActiveRegistersInclusive(tkns, idx).ToList();

            if (tkn.ResultRegisters.Length == 1 && activeRegs.Contains(tkn.ResultRegisters[0]))
                activeRegs.Remove(tkn.ResultRegisters[0]);

            for (int i = 0; i < activeRegs.Count; i++)
                Emitter.Push(activeRegs[i]);

            //Push arguments onto stack
            for (int i = tkn.Parameters.Length - 1; i >= 0; i--)
            //for (int i = 0; i < tkn.Parameters.Length; i++)
            {
                if (tkn.Parameters[i].ParameterLocation == OptimizationParameterLocation.Const)
                    Emitter.MovConstantToRegisterSize(tkn.Parameters[i].Value, tkn.ParameterRegisters[i], tkn.Parameters[i].Size);

                Emitter.Push(tkn.ParameterRegisters[i]);
            }

            //Push return value onto stack
            Emitter.Push(AssemRegisters.R10);

            //Emit call instruction
            Emitter.CallLabel(tkn.Strings[0]);

            //Pop return value from stack
            if (tkn.ResultRegisters.Length == 1)
                Emitter.Pop(tkn.ResultRegisters[0]);

            //Remove arguments from stack
            Emitter.AddRegConst(AssemRegisters.Rsp, tkn.Parameters.Length * MachineSpec.PointerSize + ((tkn.ResultRegisters.Length == 1) ? 0 : MachineSpec.PointerSize));

            //Restore active registers
            for (int i = 0; i < activeRegs.Count; i++)
                Emitter.Pop(activeRegs[i]);
        }

        private void CallVirtInstr(OptimizationToken[] tkns, int idx)
        {
            CallInstr(tkns, idx);
        }

        private void CompareInstr(OptimizationToken tkn)
        {
            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                Emitter.MovConstantToRegister(tkn.Parameters[0].Value, tkn.ParameterRegisters[0]);

            if (tkn.Parameters[1].ParameterLocation == OptimizationParameterLocation.Const)
                Emitter.MovConstantToRegister(tkn.Parameters[1].Value, tkn.ParameterRegisters[1]);

            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.Greater:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGtRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Less:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLtRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGeRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLeRelativeLabel(tkn.ResultRegisters[0]);
                    break;

                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGtUnRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLtUnRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGeUnRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLeUnRelativeLabel(tkn.ResultRegisters[0]);
                    break;

                case OptimizationInstructionSubType.Equal:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpEqRelativeLabel(tkn.ResultRegisters[0]);
                    break;

                case OptimizationInstructionSubType.NotEqual | OptimizationInstructionSubType.Unsigned:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpNeRelativeLabel(tkn.ResultRegisters[0]);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ConvertInstr(OptimizationToken tkn)
        {
            bool usigned = false;
            bool check_ovf = false;
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    check_ovf = false;
                    usigned = false;
                    break;
                case OptimizationInstructionSubType.Unsigned:
                    check_ovf = false;
                    usigned = true;
                    break;
                case OptimizationInstructionSubType.CheckOverflow:
                    check_ovf = true;
                    usigned = false;
                    break;
                case OptimizationInstructionSubType.CheckOverflow | OptimizationInstructionSubType.Unsigned:
                    check_ovf = true;
                    usigned = true;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (check_ovf)
                throw new NotImplementedException();

            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

            if (tkn.Parameters[0].Size == 0)
            {
                Console.WriteLine("Warning: Unable to determine operand size, defaulting to MachineSpec.PointerSize.");
                tkn.Parameters[0].Size = MachineSpec.PointerSize;
            }

            Emitter.MovRegisterToRegisterSignSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, tkn.ResultRegisters[0], tkn.Results[0].Size, !usigned);

        }

        private void DupInstr(OptimizationToken tkn)
        {
            Emitter.MovRegisterToRegister(tkn.ParameterRegisters[0], tkn.ResultRegisters[0]);
            Emitter.MovRegisterToRegister(tkn.ParameterRegisters[0], tkn.ResultRegisters[1]);
        }

        private void PopInstr(OptimizationToken tkn)
        {
            Emitter.Pop(tkn.ParameterRegisters[0]);
        }

        private void RetInstr(OptimizationToken tkn)
        {
            Emitter.AddRegConst(AssemRegisters.Rsp, LocalTopSize);
            if (tkn.Parameters.Length == 1)
            {
                Emitter.MovRegisterToRegisterRelativeAddress(tkn.ParameterRegisters[0], AssemRegisters.Rsp, MachineSpec.PointerSize);
            }
            else if (tkn.Parameters.Length != 0)
                throw new Exception("Unexpected parameter count.");

            Emitter.Ret();
        }

        private void SwitchInstr(OptimizationToken tkn)
        {

        }

        private void NewInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.Array:
                    {

                    }
                    break;
            }
        }

        public void GenerateCode(Graph<GraphNode<OptimizationToken>> graph)
        {
            var tkns = graph.Nodes.Values.Select(a => a.Node.Token).ToArray();

            for (int i = 0; i < tkns.Length; i++)
            {
                var tkn = tkns[i];

                while (InstructionOffsets.Count > 0 && InstructionOffsets.Peek() <= tkn.Offset)
                    Emitter.MakeLineLabel(InstructionOffsets.Dequeue());

                if (i > 0)
                {
                    for (int j = 0; j < tkn.Parameters.Length; j++)
                    {
                        if (tkn.Parameters[j].ParameterLocation == OptimizationParameterLocation.Index)
                        {
                            int reg = graph.Nodes[(int)tkn.Parameters[j].Value].Node.Token.GetResultIdx();

                            if (graph.Nodes[(int)tkn.Parameters[j].Value].Node.Token.ResultRegisters[reg] != tkn.ParameterRegisters[j])
                            {
                                //Emit a mov
                                Emitter.MovRegisterToRegister(graph.Nodes[(int)tkn.Parameters[j].Value].Node.Token.ResultRegisters[reg], tkn.ParameterRegisters[j]);
                            }
                        }
                    }
                }

                switch (tkn.Instruction)
                {
                    case OptimizationInstruction.Ld:
                        LdInstr(tkn);
                        break;
                    case OptimizationInstruction.St:
                        StInstr(tkn);
                        break;
                    case OptimizationInstruction.Add:
                        AddInstr(tkn);
                        break;
                    case OptimizationInstruction.And:
                        AndInstr(tkn);
                        break;
                    case OptimizationInstruction.Div:
                        DivInstr(tkn);
                        break;
                    case OptimizationInstruction.Mul:
                        MulInstr(tkn);
                        break;
                    case OptimizationInstruction.Neg:
                        NegInstr(tkn);
                        break;
                    case OptimizationInstruction.Not:
                        NotInstr(tkn);
                        break;
                    case OptimizationInstruction.Or:
                        OrInstr(tkn);
                        break;
                    case OptimizationInstruction.Rem:
                        RemInstr(tkn);
                        break;
                    case OptimizationInstruction.Shl:
                        ShlInstr(tkn);
                        break;
                    case OptimizationInstruction.Shr:
                        ShrInstr(tkn);
                        break;
                    case OptimizationInstruction.Sub:
                        SubInstr(tkn);
                        break;
                    case OptimizationInstruction.Xor:
                        XorInstr(tkn);
                        break;
                    case OptimizationInstruction.Branch:
                        BranchInstr(tkn);
                        break;
                    case OptimizationInstruction.Call:
                        CallInstr(tkns, i);
                        break;
                    case OptimizationInstruction.CallVirt:
                        CallVirtInstr(tkns, i);
                        break;
                    case OptimizationInstruction.Compare:
                        CompareInstr(tkn);
                        break;
                    case OptimizationInstruction.Convert:
                        ConvertInstr(tkn);
                        break;
                    case OptimizationInstruction.Dup:
                        DupInstr(tkn);
                        break;
                    case OptimizationInstruction.Pop:
                        PopInstr(tkn);
                        break;
                    case OptimizationInstruction.Ret:
                        RetInstr(tkn);
                        break;
                    case OptimizationInstruction.Switch:
                        SwitchInstr(tkn);
                        break;
                    case OptimizationInstruction.New:
                        NewInstr(tkn);
                        break;
                }
            }
        }
    }
}
