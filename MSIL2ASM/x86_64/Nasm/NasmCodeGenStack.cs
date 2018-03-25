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
                    Emitter.MovRelativeAddressMultToRegisterSize(AssemRegisters.Rsp, AssemRegisters.Rsp, 0, LocalTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize, tkn.ResultRegisters[0], Locals[(int)tkn.Constants[0]].TypeSize);
                    break;
                case OptimizationInstructionSubType.Arg:
                    Emitter.MovRelativeAddressMultToRegisterSize(AssemRegisters.Rsp, AssemRegisters.Rsp, 0, ArgumentTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize, tkn.ResultRegisters[0], MachineSpec.PointerSize);
                    break;
            }
        }

        private void StInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.Local:
                    Locals[(int)tkn.Constants[0]].TypeSize = tkn.Parameters[0].Size;
                    if ((tkn.Parameters[0].ParameterLocation & OptimizationParameterLocation.Const) != 0)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    Emitter.MovRegisterToRegisterRelativeAddressMultSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, AssemRegisters.Rsp, AssemRegisters.Rsp, 0, LocalTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize);
                    break;
                case OptimizationInstructionSubType.Arg:
                    if ((tkn.Parameters[0].ParameterLocation & OptimizationParameterLocation.Const) != 0)
                        Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ParameterRegisters[0], tkn.Parameters[0].Size);

                    Emitter.MovRegisterToRegisterRelativeAddressMultSize(tkn.ParameterRegisters[0], tkn.Parameters[0].Size, AssemRegisters.Rsp, AssemRegisters.Rsp, 0, ArgumentTopOffset + (int)tkn.Constants[0] * MachineSpec.PointerSize);
                    break;
            }
        }

        private void AddInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                            Emitter.AddConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                            Emitter.Add(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
            }
        }

        private void AndInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.And(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
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
            }
        }

        private void MulInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Multiply(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
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
            }
        }

        private void OrInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Or(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
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
            }
        }

        private void ShlInstr(OptimizationToken tkn)
        {

        }

        private void ShrInstr(OptimizationToken tkn)
        {

        }

        private void SubInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        Emitter.Sub(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
            }
        }

        private void XorInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.None:
                    {
                        if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
                            Emitter.XorConst(tkn.Parameters[0].Value, tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                        else
                            Emitter.Xor(tkn.ParameterRegisters[0], tkn.ParameterRegisters[1], tkn.ResultRegisters[0]);
                    }
                    break;
            }
        }

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
                    Emitter.JmpEqRelativeLabel((int)tkn.Constants[0]);
                    break;

                case OptimizationInstructionSubType.False:
                    Emitter.TestBool(tkn.ParameterRegisters[0]);
                    Emitter.JmpNeRelativeLabel((int)tkn.Constants[0]);
                    break;
            }
        }

        private void CallInstr(OptimizationToken tkn)
        {

        }

        private void CallVirtInstr(OptimizationToken tkn)
        {

        }

        private void CompareInstr(OptimizationToken tkn)
        {
            switch (tkn.SubType)
            {
                case OptimizationInstructionSubType.Greater:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGtRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Less:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLtRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGeRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLeRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;

                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGtUnRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLtUnRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Greater | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpGeUnRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
                case OptimizationInstructionSubType.Less | OptimizationInstructionSubType.Equal | OptimizationInstructionSubType.Unsigned:
                    Emitter.Compare(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpLeUnRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;

                case OptimizationInstructionSubType.Equal:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpEqRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;

                case OptimizationInstructionSubType.NotEqual | OptimizationInstructionSubType.Unsigned:
                    Emitter.Test(tkn.ParameterRegisters[1], tkn.ParameterRegisters[0]);
                    Emitter.CmpNeRelativeLabel(tkn.ResultRegisters[0], 4);
                    break;
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
            }

            if (tkn.Parameters[0].ParameterLocation == OptimizationParameterLocation.Const)
            {
                Emitter.MovConstantToRegisterSize(tkn.Parameters[0].Value, tkn.ResultRegisters[0], tkn.Results[0].Size);
            }
            else
            {
                Emitter.MovRegisterToRegisterSignSize(tkn.ParameterRegisters[0], tkn.Results[0].Size, tkn.ResultRegisters[0], tkn.Results[0].Size, !usigned);
            }

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
            Emitter.AddRegConst(AssemRegisters.Rsp, LocalTopOffset);
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

        public void GenerateCode(Graph<GraphNode<OptimizationToken>> graph)
        {
            var tkns = graph.Nodes.Values.ToArray();

            for (int i = 0; i < tkns.Length; i++)
            {
                var tkn = tkns[i].Node.Token;

                Emitter.MakeLineLabel(tkn.Offset);
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
                        CallInstr(tkn);
                        break;
                    case OptimizationInstruction.CallVirt:
                        CallVirtInstr(tkn);
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
                }
            }
        }
    }
}
