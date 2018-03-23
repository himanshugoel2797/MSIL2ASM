using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm.Assembly
{
    public enum AssemRegisters
    {
        Rax,
        Rbx,
        Rcx,
        Rdx,
        Rbp,
        Rsp,
        Rsi,
        Rdi,
        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15
    }

    public class InstrEmitter
    {
        private List<string> Lines;
        private int CurLine;

        private static string RegisterName(int idx, int size)
        {
            if (size == 8)
                switch (idx)
                {
                    case 0:
                        return "rax";
                    case 1:
                        return "rbx";
                    case 2:
                        return "rcx";
                    case 3:
                        return "rdx";
                    case 4:
                        return "rbp";
                    case 5:
                        return "rsp";
                    case 6:
                        return "rsi";
                    case 7:
                        return "rdi";
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return "r" + idx.ToString();
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 4)
                switch (idx)
                {
                    case 0:
                        return "eax";
                    case 1:
                        return "ebx";
                    case 2:
                        return "ecx";
                    case 3:
                        return "edx";
                    case 4:
                        return "ebp";
                    case 5:
                        return "esp";
                    case 6:
                        return "esi";
                    case 7:
                        return "edi";
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return "r" + idx.ToString() + "d";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 2)
                switch (idx)
                {
                    case 0:
                        return "ax";
                    case 1:
                        return "bx";
                    case 2:
                        return "cx";
                    case 3:
                        return "dx";
                    case 4:
                        return "bp";
                    case 5:
                        return "sp";
                    case 6:
                        return "si";
                    case 7:
                        return "di";
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return "r" + idx.ToString() + "w";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 1)
                switch (idx)
                {
                    case 0:
                        return "al";
                    case 1:
                        return "bl";
                    case 2:
                        return "cl";
                    case 3:
                        return "dl";
                    case 4:
                        return "bpl";
                    case 5:
                        return "spl";
                    case 6:
                        return "sil";
                    case 7:
                        return "dil";
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return "r" + idx.ToString() + "b";
                    default:
                        throw new Exception("Unexpected register index");
                }
            return null;
        }

        private static string RegisterName(int idx)
        {
            return RegisterName(idx, 8);
        }

        public InstrEmitter()
        {
            Lines = new List<string>();
        }

        private void LinesAdd(string ln)
        {
            Lines.Add("\t\t" + ln);
        }

        public void MakeComment(string ln)
        {
            LinesAdd(";" + ln);
        }

        #region Sub
        public void SubRegConst(int reg, int constV)
        {
            LinesAdd($"sub {RegisterName(reg)}, {constV.ToString()}");
        }
        #endregion

        #region Mov
        public void MovLabelAddressToRegister(int reg, string label)
        {
            LinesAdd($"mov {RegisterName(reg)}, [{label}]");
        }

        public void MovRegisterToRegisterRelativeAddress(int srcReg, int dstReg, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [{RegisterName(dstReg)} + {offset}], {RegisterName(srcReg)}");
            else
                LinesAdd($"mov [{RegisterName(dstReg)} - {-offset}], {RegisterName(srcReg)}");
        }

        public void MovRegisterToRegisterRelativeAddressMultSize(int srcReg, int src_sz, int dstReg, int dstMultReg, int multiplier, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [{RegisterName(dstReg)} + {RegisterName(dstMultReg)} * {multiplier} + {offset}], {RegisterName(srcReg, src_sz)}");
            else
                LinesAdd($"mov [{RegisterName(dstReg)} + {RegisterName(dstMultReg)} * {multiplier} - {-offset}], {RegisterName(srcReg, src_sz)}");
        }

        public void MovRegisterToLabelRelativeAddressSize(int srcReg, int src_sz, string lbl, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [rel {lbl} + {offset}], {RegisterName(srcReg, src_sz)}");
            else
                LinesAdd($"mov [rel {lbl} - {-offset}], {RegisterName(srcReg, src_sz)}");
        }

        public void MovLabelRelativeAddressToRegisterSize(string lbl, int offset, int dstReg, int dstSz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dstSz)}, [rel {lbl} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dstSz)}, [rel {lbl} - {-offset}]");
        }

        public void MovRelativeAddressToRegister(int srcReg, int offset, int dstReg)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg)}, [{RegisterName(srcReg)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg)}, [{RegisterName(srcReg)} - {-offset}]");
        }

        public void MovRelativeAddressToRegisterSize(int srcReg, int offset, int dstReg, int dst_sz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} - {-offset}]");
        }

        public void MovRelativeAddressMultToRegisterSize(int srcReg, int srcRegMult, int multiplier, int offset, int dstReg, int dst_sz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {multiplier} * {RegisterName(srcRegMult)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {multiplier} * {RegisterName(srcRegMult)} - {-offset}]");
        }

        public void MovConstantToRegister(ulong val, int dstReg)
        {
            if (val == 0)
                LinesAdd($"xor {RegisterName(dstReg, 4)}, {RegisterName(dstReg, 4)}");
            else
                LinesAdd($"mov {RegisterName(dstReg)}, {val}");
        }

        public void MovConstantToRegisterSize(ulong val, int dstReg, int dst_sz)
        {
            if (val == 0)
                LinesAdd($"xor {RegisterName(dstReg, 4)}, {RegisterName(dstReg, 4)}");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, {val}");
        }

        public void MovLabelRelativeConstantToRegisterSize(string lbl, int off, int dstReg, int dst_sz)
        {
            if (off >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, {lbl} + {off}");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, {lbl} - {-off}");
        }

        public void MovRegisterToRegister(int src, int dst)
        {
            if (src != dst)
                LinesAdd($"mov {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void MovRegisterToRegisterSize(int src, int src_size, int dst, int dst_size)
        {
            if (src_size > dst_size)
                src_size = dst_size;

            if (src != dst | src_size != dst_size)
            {
                LinesAdd($"mov {RegisterName(dst, dst_size)}, {RegisterName(src, src_size)}");
            }
        }

        public void MovRegisterToRegisterSignSize(int src, int src_size, int dst, int dst_size, bool signed)
        {
            if (src_size > dst_size)
                src_size = dst_size;

            if (src != dst | src_size != dst_size)
            {
                if (signed)
                    LinesAdd($"movsx {RegisterName(dst, dst_size)}, {RegisterName(src, src_size)}");
                else
                    LinesAdd($"movzx {RegisterName(dst, dst_size)}, {RegisterName(src, src_size)}");
            }
        }

        public void MovRegisterToRegisterSigned(int src, int src_sz, int dst, int dst_sz)
        {
            if (src != dst)
                LinesAdd($"movsx {RegisterName(dst, dst_sz)}, {RegisterName(src, src_sz)}");
        }

        public void MovRegisterToRegisterUnsigned(int src, int src_sz, int dst, int dst_sz)
        {
            if (src != dst)
                LinesAdd($"movzx {RegisterName(dst, dst_sz)}, {RegisterName(src, src_sz)}");
        }

        public void MovRegisterToRegisterAddressSize(int srcReg, int dstReg, int size)
        {
            LinesAdd($"mov [{RegisterName(dstReg)}], {RegisterName(srcReg, size)}");
        }

        #endregion

        #region Labels
        public void MakeLineLabel(int line)
        {
            Lines.Add($"\t.addr_{line}:");
            CurLine = line;
        }

        public void MakeGlobalLabel(string label)
        {
            Lines.Add($"global {label}");
            MakeLabel(label);
        }

        public void MakeGlobalFunction(string label)
        {
            Lines.Add($"global {label}:function");
            MakeLabel(label);
        }

        public void MakeLocalLineLabel(int idx)
        {
            Lines.Add($"\t\t.addr_{CurLine}_{idx}:");
        }

        public void MakeLabel(string label)
        {
            Lines.Add($"{label}:");
        }
        #endregion

        #region Math
        public void CheckOverflow(int sz, string target_lbl)
        {
            //TODO jmp to the label on overflow
        }


        public void AddRegConst(int reg, int constV)
        {
            LinesAdd($"add {RegisterName(reg)}, {constV.ToString()}");
        }

        public void Add(int src, int dst)
        {
            LinesAdd($"add {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void Sub(int src, int dst)
        {
            LinesAdd($"sub {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void Multiply(int src, int dst)
        {
            LinesAdd($"imul {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void And(int src, int dst)
        {
            LinesAdd($"and {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void Or(int src, int dst)
        {
            LinesAdd($"or {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void Xor(int src, int dst)
        {
            LinesAdd($"xor {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void Neg(int src)
        {
            LinesAdd($"neg {RegisterName(src)}");
        }

        public void Not(int src)
        {
            LinesAdd($"not {RegisterName(src)}");
        }

        #region Shift
        public void ShiftLeft(int reg, int amt_reg)
        {
            ShiftGeneric(reg, amt_reg, false, true);
        }

        public void ShiftRight(int reg, int amt_reg)
        {
            ShiftGeneric(reg, amt_reg, true, true);
        }

        public void ShiftRightUn(int reg, int amt_reg)
        {
            ShiftGeneric(reg, amt_reg, true, false);
        }

        private void ShiftGeneric(int reg, int amt_reg, bool right, bool signed)
        {
            if (amt_reg != (int)AssemRegisters.Rcx)
            {
                Push((int)AssemRegisters.Rcx);
                MovRegisterToRegister(amt_reg, (int)AssemRegisters.Rcx);
            }

            string inst = "";
            if (signed)
            {
                if (right)
                    inst = "sar";
                else
                    inst = "sal";
            }
            else
            {
                if (right)
                    inst = "shr";
                else
                    inst = "shl";
            }

            LinesAdd($"{inst} {RegisterName(reg)}, cl");

            if (amt_reg != (int)AssemRegisters.Rcx)
            {
                Pop((int)AssemRegisters.Rcx);
            }
        }
        #endregion

        #region Division
        public void Divide(int src, int divisor)
        {
            Divide(src, divisor, false, true);
        }

        public void Remainder(int src, int divisor)
        {
            Divide(src, divisor, true, true);
        }

        public void UDivide(int src, int divisor)
        {
            Divide(src, divisor, false, false);
        }

        public void URemainder(int src, int divisor)
        {
            Divide(src, divisor, true, false);
        }

        private void Divide(int src, int divisor, bool rem, bool signed)
        {
            if (src != (int)AssemRegisters.Rax)
            {
                Push((int)AssemRegisters.Rax);
                MovRegisterToRegister(src, (int)AssemRegisters.Rax);
            }
            if (divisor == (int)AssemRegisters.Rdx)
            {
                Push((int)AssemRegisters.Rbx);
                MovRegisterToRegister((int)AssemRegisters.Rdx, (int)AssemRegisters.Rbx);
            }

            Push((int)AssemRegisters.Rdx);
            MovConstantToRegister(0, (int)AssemRegisters.Rdx);

            if (signed)
                LinesAdd($"idiv {RegisterName(divisor)}");
            else
                LinesAdd($"div {RegisterName(divisor)}");

            if (!rem)
            {
                MovRegisterToRegister((int)AssemRegisters.Rax, src);    //Move quotient
            }
            else
            {
                MovRegisterToRegister((int)AssemRegisters.Rdx, src);    //Move remainder
            }

            Pop((int)AssemRegisters.Rdx);

            if (divisor == (int)AssemRegisters.Rdx)
            {
                Pop((int)AssemRegisters.Rbx);
            }
            if (src != (int)AssemRegisters.Rax)
            {
                Pop((int)AssemRegisters.Rax);
            }
        }
        #endregion
        #endregion

        public void LoadEffectiveAddress(int src_reg, int offset, int dst_reg)
        {
            if (offset >= 0)
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {offset} ");
            else
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} - {-offset} ");
        }

        public void LoadEffectiveMultAddress(int src_reg, int srcRegMult, int multiplier, int offset, int dst_reg)
        {
            if (offset >= 0)
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {RegisterName(srcRegMult)} * {multiplier} + {offset} ");
            else
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {RegisterName(srcRegMult)} * {multiplier}- {-offset} ");
        }

        public void JmpRelativeLabel(int line)
        {
            LinesAdd($"jmp .addr_{line}");
        }

        public void JmpRelativeLocalLabel(int idx)
        {
            LinesAdd($"jmp .addr_{CurLine}_{idx}");
        }

        public void CallLabel(string label)
        {
            LinesAdd($"call {label}");
        }

        public void Ret()
        {
            LinesAdd("ret");
        }

        public void Push(int reg)
        {
            LinesAdd($"push {RegisterName(reg)}");
        }

        public void Pop(int reg)
        {
            LinesAdd($"pop {RegisterName(reg)}");
        }

        public void Compare(int v1, int v2)
        {
            LinesAdd($"cmp {RegisterName(v1)}, {RegisterName(v2)}");
        }

        #region Builtins
        public void Hlt()
        {
            LinesAdd($"hlt");
        }

        public void Sti()
        {
            LinesAdd($"sti");
        }

        public void Cli()
        {
            LinesAdd($"cli");
        }

        #region Out
        public void Out(int srcReg, int src2Reg, int size)
        {
            if (srcReg == (int)AssemRegisters.Rax && src2Reg == (int)AssemRegisters.Rdx)
            {
                LinesAdd($"xchg rax, rdx");
            }
            else
            {
                if (srcReg == (int)AssemRegisters.Rax)
                {
                    Push((int)AssemRegisters.Rdx);
                    MovRegisterToRegisterSize(srcReg, 2, (int)AssemRegisters.Rdx, 2);
                }

                if (src2Reg != (int)AssemRegisters.Rax)
                {
                    Push((int)AssemRegisters.Rax);
                    MovRegisterToRegisterSize(src2Reg, size, (int)AssemRegisters.Rax, size);
                }

                if (srcReg != (int)AssemRegisters.Rdx && srcReg != (int)AssemRegisters.Rax)
                {
                    Push((int)AssemRegisters.Rdx);
                    MovRegisterToRegisterSize(srcReg, 2, (int)AssemRegisters.Rdx, 2);
                }
            }

            LinesAdd($"out {RegisterName((int)AssemRegisters.Rdx, 2)}, {RegisterName((int)AssemRegisters.Rax, size)}");


            if (srcReg == (int)AssemRegisters.Rax && src2Reg == (int)AssemRegisters.Rdx)
            {
                LinesAdd($"xchg rax, rdx");
            }
            else
            {
                if (srcReg != (int)AssemRegisters.Rdx && srcReg == (int)AssemRegisters.Rax)
                {
                    Pop((int)AssemRegisters.Rdx);
                }

                if (src2Reg != (int)AssemRegisters.Rax)
                {
                    Pop((int)AssemRegisters.Rax);
                }

                if (srcReg != (int)AssemRegisters.Rdx && srcReg != (int)AssemRegisters.Rax)
                {
                    Pop((int)AssemRegisters.Rdx);
                }
            }
        }

        public void OutConst(int src_addr, int srcReg, int size)
        {
            if (srcReg != (int)AssemRegisters.Rax)
            {
                Push((int)AssemRegisters.Rax);
                MovRegisterToRegisterSize(srcReg, size, (int)AssemRegisters.Rax, size);
            }

            LinesAdd($"out {src_addr}, {RegisterName((int)AssemRegisters.Rax, size)}");

            if (srcReg != (int)AssemRegisters.Rax)
            {
                Pop((int)AssemRegisters.Rax);
            }
        }
        #endregion

        #region In
        public void In(int srcReg, int dstReg, int size)
        {

        }

        public void InConst(int src_addr, int dstReg, int size)
        {

        }
        #endregion

        #endregion

        #region Conditional Jump
        public void JmpEqRelativeLabel(int line)
        {
            LinesAdd($"je .addr_{line}");
        }

        public void JmpNeRelativeLabel(int line)
        {
            LinesAdd($"jne .addr_{line}");
        }

        public void JmpLtRelativeLabel(int line)
        {
            LinesAdd($"jl .addr_{line}");
        }

        public void JmpGtRelativeLabel(int line)
        {
            LinesAdd($"jg .addr_{line}");
        }

        public void JmpLeRelativeLabel(int line)
        {
            LinesAdd($"jle .addr_{line}");
        }

        public void JmpGeRelativeLabel(int line)
        {
            LinesAdd($"jge .addr_{line}");
        }

        public void JmpLtUnRelativeLabel(int line)
        {
            LinesAdd($"jb .addr_{line}");
        }

        public void JmpGtUnRelativeLabel(int line)
        {
            LinesAdd($"ja .addr_{line}");
        }

        public void JmpLeUnRelativeLabel(int line)
        {
            LinesAdd($"jbe .addr_{line}");
        }

        public void JmpZeroRelativeLabel(int line)
        {
            LinesAdd($"jz .addr_{line}");
        }

        public void JmpNZeroRelativeLabel(int line)
        {
            LinesAdd($"jnz .addr_{line}");
        }

        public void JmpGeUnRelativeLabel(int line)
        {
            LinesAdd($"jae .addr_{line}");
        }

        public void JmpEqRelativeLocalLabel(int idx)
        {
            LinesAdd($"je .addr_{CurLine}_{idx}");
        }

        public void JmpLtRelativeLocalLabel(int idx)
        {
            LinesAdd($"jl .addr_{CurLine}_{idx}");
        }

        public void JmpGtRelativeLocalLabel(int idx)
        {
            LinesAdd($"jg .addr_{CurLine}_{idx}");
        }

        public void JmpLtUnRelativeLocalLabel(int idx)
        {
            LinesAdd($"jb .addr_{CurLine}_{idx}");
        }

        public void JmpGtUnRelativeLocalLabel(int idx)
        {
            LinesAdd($"ja .addr_{CurLine}_{idx}");
        }
        #endregion

        public void TestBool(int reg)
        {
            LinesAdd($"test {RegisterName(reg)}, {RegisterName(reg)}");
        }

        public string[] GetLines()
        {
            return Lines.ToArray();
        }

    }
}
