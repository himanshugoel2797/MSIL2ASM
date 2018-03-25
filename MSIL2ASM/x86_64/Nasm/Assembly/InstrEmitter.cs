using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm.Assembly
{
    [Flags]
    public enum AssemRegisters
    {
        None = 0,
        Rax = (1 << 1),
        Rbx = (1 << 2),
        Rcx = (1 << 3),
        Rdx = (1 << 4),
        Rbp = (1 << 5),
        Rsp = (1 << 6),
        Rsi = (1 << 7),
        Rdi = (1 << 8),
        R8 = (1 << 9),
        R9 = (1 << 10),
        R10 = (1 << 11),
        R11 = (1 << 12),
        R12 = (1 << 13),
        R13 = (1 << 14),
        R14 = (1 << 15),
        R15 = (1 << 16),
        Any = (1 << 17),
        Const8 = (1 << 28),
        Const16 = (1 << 29),
        Const32 = (1 << 30),
        Const64 = (1 << 31),
        Const = (Const8 | Const16 | Const32 | Const64),
    }

    public class InstrEmitter
    {
        private List<string> Lines;
        private int CurLine;

        private static string RegisterName(AssemRegisters idx, int size)
        {
            if (size == 8)
                switch (idx)
                {
                    case AssemRegisters.Rax:
                        return "rax";
                    case AssemRegisters.Rbx:
                        return "rbx";
                    case AssemRegisters.Rcx:
                        return "rcx";
                    case AssemRegisters.Rdx:
                        return "rdx";
                    case AssemRegisters.Rbp:
                        return "rbp";
                    case AssemRegisters.Rsp:
                        return "rsp";
                    case AssemRegisters.Rsi:
                        return "rsi";
                    case AssemRegisters.Rdi:
                        return "rdi";
                    case AssemRegisters.R8:
                        return "r8";
                    case AssemRegisters.R9:
                        return "r9";
                    case AssemRegisters.R10:
                        return "r10";
                    case AssemRegisters.R11:
                        return "r11";
                    case AssemRegisters.R12:
                        return "r12";
                    case AssemRegisters.R13:
                        return "r13";
                    case AssemRegisters.R14:
                        return "r14";
                    case AssemRegisters.R15:
                        return "r15";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 4)
                switch (idx)
                {
                    case AssemRegisters.Rax:
                        return "eax";
                    case AssemRegisters.Rbx:
                        return "ebx";
                    case AssemRegisters.Rcx:
                        return "ecx";
                    case AssemRegisters.Rdx:
                        return "edx";
                    case AssemRegisters.Rbp:
                        return "ebp";
                    case AssemRegisters.Rsp:
                        return "esp";
                    case AssemRegisters.Rsi:
                        return "esi";
                    case AssemRegisters.Rdi:
                        return "edi";
                    case AssemRegisters.R8:
                        return "r8d";
                    case AssemRegisters.R9:
                        return "r9d";
                    case AssemRegisters.R10:
                        return "r10d";
                    case AssemRegisters.R11:
                        return "r11d";
                    case AssemRegisters.R12:
                        return "r12d";
                    case AssemRegisters.R13:
                        return "r13d";
                    case AssemRegisters.R14:
                        return "r14d";
                    case AssemRegisters.R15:
                        return "r15d";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 2)
                switch (idx)
                {
                    case AssemRegisters.Rax:
                        return "ax";
                    case AssemRegisters.Rbx:
                        return "bx";
                    case AssemRegisters.Rcx:
                        return "cx";
                    case AssemRegisters.Rdx:
                        return "dx";
                    case AssemRegisters.Rbp:
                        return "bp";
                    case AssemRegisters.Rsp:
                        return "sp";
                    case AssemRegisters.Rsi:
                        return "si";
                    case AssemRegisters.Rdi:
                        return "di";
                    case AssemRegisters.R8:
                        return "r8w";
                    case AssemRegisters.R9:
                        return "r9w";
                    case AssemRegisters.R10:
                        return "r10w";
                    case AssemRegisters.R11:
                        return "r11w";
                    case AssemRegisters.R12:
                        return "r12w";
                    case AssemRegisters.R13:
                        return "r13w";
                    case AssemRegisters.R14:
                        return "r14w";
                    case AssemRegisters.R15:
                        return "r15w";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else if (size == 1)
                switch (idx)
                {
                    case AssemRegisters.Rax:
                        return "al";
                    case AssemRegisters.Rbx:
                        return "bl";
                    case AssemRegisters.Rcx:
                        return "cl";
                    case AssemRegisters.Rdx:
                        return "dl";
                    case AssemRegisters.Rbp:
                        return "bpl";
                    case AssemRegisters.Rsp:
                        return "spl";
                    case AssemRegisters.Rsi:
                        return "sil";
                    case AssemRegisters.Rdi:
                        return "dil";
                    case AssemRegisters.R8:
                        return "r8b";
                    case AssemRegisters.R9:
                        return "r9b";
                    case AssemRegisters.R10:
                        return "r10b";
                    case AssemRegisters.R11:
                        return "r11b";
                    case AssemRegisters.R12:
                        return "r12b";
                    case AssemRegisters.R13:
                        return "r13b";
                    case AssemRegisters.R14:
                        return "r14b";
                    case AssemRegisters.R15:
                        return "r15b";
                    default:
                        throw new Exception("Unexpected register index");
                }
            else
                throw new Exception();
        }

        private static string RegisterName(AssemRegisters idx)
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
        
        #region Mov
        public void MovLabelAddressToRegister(AssemRegisters reg, string label)
        {
            LinesAdd($"mov {RegisterName(reg)}, [{label}]");
        }

        public void MovRegisterToRegisterRelativeAddress(AssemRegisters srcReg, AssemRegisters dstReg, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [{RegisterName(dstReg)} + {offset}], {RegisterName(srcReg)}");
            else
                LinesAdd($"mov [{RegisterName(dstReg)} - {-offset}], {RegisterName(srcReg)}");
        }

        public void MovRegisterToRegisterRelativeAddressMultSize(AssemRegisters srcReg, int src_sz, AssemRegisters dstReg, AssemRegisters dstMultReg, int multiplier, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [{RegisterName(dstReg)} + {RegisterName(dstMultReg)} * {multiplier} + {offset}], {RegisterName(srcReg, src_sz)}");
            else
                LinesAdd($"mov [{RegisterName(dstReg)} + {RegisterName(dstMultReg)} * {multiplier} - {-offset}], {RegisterName(srcReg, src_sz)}");
        }

        public void MovRegisterToLabelRelativeAddressSize(AssemRegisters srcReg, int src_sz, string lbl, int offset)
        {
            if (offset >= 0)
                LinesAdd($"mov [rel {lbl} + {offset}], {RegisterName(srcReg, src_sz)}");
            else
                LinesAdd($"mov [rel {lbl} - {-offset}], {RegisterName(srcReg, src_sz)}");
        }

        public void MovLabelRelativeAddressToRegisterSize(string lbl, int offset, AssemRegisters dstReg, int dstSz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dstSz)}, [rel {lbl} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dstSz)}, [rel {lbl} - {-offset}]");
        }

        public void MovRelativeAddressToRegister(AssemRegisters srcReg, int offset, AssemRegisters dstReg)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg)}, [{RegisterName(srcReg)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg)}, [{RegisterName(srcReg)} - {-offset}]");
        }

        public void MovRelativeAddressToRegisterSize(AssemRegisters srcReg, int offset, AssemRegisters dstReg, int dst_sz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} - {-offset}]");
        }

        public void MovRelativeAddressMultToRegisterSize(AssemRegisters srcReg, AssemRegisters srcRegMult, int multiplier, int offset, AssemRegisters dstReg, int dst_sz)
        {
            if (offset >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {multiplier} * {RegisterName(srcRegMult)} + {offset}]");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, [{RegisterName(srcReg)} + {multiplier} * {RegisterName(srcRegMult)} - {-offset}]");
        }

        public void MovConstantToRegister(ulong val, AssemRegisters dstReg)
        {
            if (val == 0)
                LinesAdd($"xor {RegisterName(dstReg, 4)}, {RegisterName(dstReg, 4)}");
            else
                LinesAdd($"mov {RegisterName(dstReg)}, 0x{val:X}");
        }

        public void MovConstantToRegisterSize(ulong val, AssemRegisters dstReg, int dst_sz)
        {
            if (val == 0)
                LinesAdd($"xor {RegisterName(dstReg, 4)}, {RegisterName(dstReg, 4)}");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, 0x{val:X}");
        }

        public void MovLabelRelativeConstantToRegisterSize(string lbl, int off, AssemRegisters dstReg, int dst_sz)
        {
            if (off >= 0)
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, {lbl} + {off}");
            else
                LinesAdd($"mov {RegisterName(dstReg, dst_sz)}, {lbl} - {-off}");
        }

        public void MovRegisterToRegister(AssemRegisters src, AssemRegisters dst)
        {
            if (src != dst)
                LinesAdd($"mov {RegisterName(dst)}, {RegisterName(src)}");
        }

        public void MovRegisterToRegisterSize(AssemRegisters src, int src_size, AssemRegisters dst, int dst_size)
        {
            if (src_size > dst_size)
                src_size = dst_size;

            if (src != dst | src_size != dst_size)
            {
                LinesAdd($"mov {RegisterName(dst, dst_size)}, {RegisterName(src, src_size)}");
            }
        }

        public void MovRegisterToRegisterSignSize(AssemRegisters src, int src_size, AssemRegisters dst, int dst_size, bool signed)
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

        public void MovRegisterToRegisterSigned(AssemRegisters src, int src_sz, AssemRegisters dst, int dst_sz)
        {
            if (src != dst)
                LinesAdd($"movsx {RegisterName(dst, dst_sz)}, {RegisterName(src, src_sz)}");
        }

        public void MovRegisterToRegisterUnsigned(AssemRegisters src, int src_sz, AssemRegisters dst, int dst_sz)
        {
            if (src != dst)
                LinesAdd($"movzx {RegisterName(dst, dst_sz)}, {RegisterName(src, src_sz)}");
        }

        public void MovRegisterToRegisterAddressSize(AssemRegisters srcReg, AssemRegisters dstReg, int size)
        {
            LinesAdd($"mov [{RegisterName(dstReg)}], {RegisterName(srcReg, size)}");
        }

        public void MovRegisterToAddressSize(AssemRegisters srcReg, ulong dstReg, int size)
        {
            LinesAdd($"mov [{dstReg:X}], {RegisterName(srcReg, size)}");
        }

        #endregion

        #region Labels
        public void MakeLineLabel(int line)
        {
            Lines.Add($"\t.addr_{line:X}:");
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


        public void AddRegConst(AssemRegisters reg, int constV)
        {
            LinesAdd($"add {RegisterName(reg)}, {constV.ToString()}");
        }

        public void AddConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            if (src == 1)
                LinesAdd($"inc {RegisterName(dst)}");
            else
                LinesAdd($"add {RegisterName(dst)}, {src:X}");

            MovRegisterToRegister(dst, dst0);
        }

        public void Add(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"add {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void SubConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"sub {RegisterName(dst)}, {src}");
            MovRegisterToRegister(dst, dst0);
        }

        public void SubRegConst(AssemRegisters reg, int constV)
        {
            LinesAdd($"sub {RegisterName(reg)}, {constV.ToString()}");
        }

        public void Sub(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"sub {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void MultiplyConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"imul {RegisterName(dst)}, {src}");
            MovRegisterToRegister(dst, dst0);
        }

        public void Multiply(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"imul {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void AndConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"and {RegisterName(dst)}, {src}");
            MovRegisterToRegister(dst, dst0);
        }

        public void And(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"and {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void OrConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"or {RegisterName(dst)}, {src}");
            MovRegisterToRegister(dst, dst0);
        }

        public void Or(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"or {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void XorConst(ulong src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"xor {RegisterName(dst)}, {src:X}");
            MovRegisterToRegister(dst, dst0);
        }

        public void Xor(AssemRegisters src, AssemRegisters dst, AssemRegisters dst0)
        {
            LinesAdd($"xor {RegisterName(dst)}, {RegisterName(src)}");
            MovRegisterToRegister(dst, dst0);
        }

        public void Neg(AssemRegisters src, AssemRegisters dst0)
        {
            LinesAdd($"neg {RegisterName(src)}");
            MovRegisterToRegister(src, dst0);
        }

        public void Not(AssemRegisters src, AssemRegisters dst0)
        {
            LinesAdd($"not {RegisterName(src)}");
            MovRegisterToRegister(src, dst0);
        }

        #region Shift
        public void ShiftLeft(AssemRegisters reg, AssemRegisters amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, false, true);
        }

        public void ShiftRight(AssemRegisters reg, AssemRegisters amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, true, true);
        }

        public void ShiftRightUn(AssemRegisters reg, AssemRegisters amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, true, false);
        }

        public void ShiftLeft(AssemRegisters reg, int amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, false, true);
        }

        public void ShiftRight(AssemRegisters reg, int amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, true, true);
        }

        public void ShiftRightUn(AssemRegisters reg, int amt_reg, AssemRegisters dst_reg)
        {
            ShiftGeneric(reg, amt_reg, dst_reg, true, false);
        }

        private void ShiftGeneric(AssemRegisters reg, AssemRegisters amt_reg, AssemRegisters dst_reg, bool right, bool signed)
        {
            if (amt_reg != AssemRegisters.Rcx)
            {
                Push(AssemRegisters.Rcx);
                MovRegisterToRegister(amt_reg, AssemRegisters.Rcx);
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

            if (amt_reg != AssemRegisters.Rcx)
            {
                Pop(AssemRegisters.Rcx);
            }
        }

        private void ShiftGeneric(AssemRegisters reg, int amt_reg, AssemRegisters dst_reg, bool right, bool signed)
        {
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

            LinesAdd($"{inst} {RegisterName(reg)}, {amt_reg}");
        }
        #endregion

        #region Division
        public void Divide(AssemRegisters src, AssemRegisters divisor, AssemRegisters dst)
        {
            if (src != AssemRegisters.Rax)
                throw new Exception("Rax expected.");

            MovConstantToRegister(0, AssemRegisters.Rdx);
            LinesAdd($"idiv {RegisterName(divisor)}");

            if (dst != AssemRegisters.Rax)
                throw new Exception("Rax expected.");
        }

        public void Remainder(AssemRegisters src, AssemRegisters divisor, AssemRegisters dst)
        {
            if (src != AssemRegisters.Rax)
                throw new Exception("Rax expected.");

            MovConstantToRegister(0, AssemRegisters.Rdx);
            LinesAdd($"idiv {RegisterName(divisor)}");

            if (dst != AssemRegisters.Rdx)
                throw new Exception("Rdx expected.");
        }

        public void UDivide(AssemRegisters src, AssemRegisters divisor, AssemRegisters dst)
        {
            if (src != AssemRegisters.Rax)
                throw new Exception("Rax expected.");

            MovConstantToRegister(0, AssemRegisters.Rdx);
            LinesAdd($"div {RegisterName(divisor)}");

            if (dst != AssemRegisters.Rax)
                throw new Exception("Rax expected.");
        }

        public void URemainder(AssemRegisters src, AssemRegisters divisor, AssemRegisters dst)
        {
            if (src != AssemRegisters.Rax)
                throw new Exception("Rax expected.");

            MovConstantToRegister(0, AssemRegisters.Rdx);
            LinesAdd($"div {RegisterName(divisor)}");

            if (dst != AssemRegisters.Rdx)
                throw new Exception("Rdx expected.");
        }
        #endregion
        #endregion

        public void LoadEffectiveAddress(AssemRegisters src_reg, int offset, AssemRegisters dst_reg)
        {
            if (offset >= 0)
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {offset} ");
            else
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} - {-offset} ");
        }

        public void LoadEffectiveMultAddress(AssemRegisters src_reg, AssemRegisters srcRegMult, int multiplier, int offset, AssemRegisters dst_reg)
        {
            if (offset >= 0)
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {RegisterName(srcRegMult)} * {multiplier} + {offset} ");
            else
                LinesAdd($"lea {RegisterName(dst_reg)}, {RegisterName(src_reg)} + {RegisterName(srcRegMult)} * {multiplier}- {-offset} ");
        }

        public void JmpRelativeLabel(int line)
        {
            LinesAdd($"jmp .addr_{line:X}");
        }

        public void JmpRelativeLocalLabel(int idx)
        {
            LinesAdd($"jmp .addr_{CurLine:X}_{idx}");
        }

        public void CallLabel(string label)
        {
            LinesAdd($"call {label}");
        }

        public void Ret()
        {
            LinesAdd("ret");
        }

        public void Push(AssemRegisters reg)
        {
            LinesAdd($"push {RegisterName(reg)}");
        }

        public void Pop(AssemRegisters reg)
        {
            LinesAdd($"pop {RegisterName(reg)}");
        }

        public void Compare(AssemRegisters v1, AssemRegisters v2)
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
        public void Out(AssemRegisters srcReg, AssemRegisters src2Reg, int size)
        {
            if (srcReg == AssemRegisters.Rax && src2Reg == AssemRegisters.Rdx)
            {
                LinesAdd($"xchg rax, rdx");
            }
            else
            {
                if (srcReg == AssemRegisters.Rax)
                {
                    Push(AssemRegisters.Rdx);
                    MovRegisterToRegisterSize(srcReg, 2, AssemRegisters.Rdx, 2);
                }

                if (src2Reg != AssemRegisters.Rax)
                {
                    Push(AssemRegisters.Rax);
                    MovRegisterToRegisterSize(src2Reg, size, AssemRegisters.Rax, size);
                }

                if (srcReg != AssemRegisters.Rdx && srcReg != AssemRegisters.Rax)
                {
                    Push(AssemRegisters.Rdx);
                    MovRegisterToRegisterSize(srcReg, 2, AssemRegisters.Rdx, 2);
                }
            }

            LinesAdd($"out {RegisterName(AssemRegisters.Rdx, 2)}, {RegisterName(AssemRegisters.Rax, size)}");


            if (srcReg == AssemRegisters.Rax && src2Reg == AssemRegisters.Rdx)
            {
                LinesAdd($"xchg rax, rdx");
            }
            else
            {
                if (srcReg != AssemRegisters.Rdx && srcReg == AssemRegisters.Rax)
                {
                    Pop(AssemRegisters.Rdx);
                }

                if (src2Reg != AssemRegisters.Rax)
                {
                    Pop(AssemRegisters.Rax);
                }

                if (srcReg != AssemRegisters.Rdx && srcReg != AssemRegisters.Rax)
                {
                    Pop(AssemRegisters.Rdx);
                }
            }
        }

        public void OutConst(AssemRegisters src_addr, AssemRegisters srcReg, int size)
        {
            if (srcReg != AssemRegisters.Rax)
            {
                Push(AssemRegisters.Rax);
                MovRegisterToRegisterSize(srcReg, size, AssemRegisters.Rax, size);
            }

            LinesAdd($"out {src_addr}, {RegisterName(AssemRegisters.Rax, size)}");

            if (srcReg != AssemRegisters.Rax)
            {
                Pop(AssemRegisters.Rax);
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
            LinesAdd($"je .addr_{line:X}");
        }

        public void JmpNeRelativeLabel(int line)
        {
            LinesAdd($"jne .addr_{line:X}");
        }

        public void JmpLtRelativeLabel(int line)
        {
            LinesAdd($"jl .addr_{line:X}");
        }

        public void JmpGtRelativeLabel(int line)
        {
            LinesAdd($"jg .addr_{line:X}");
        }

        public void JmpLeRelativeLabel(int line)
        {
            LinesAdd($"jle .addr_{line:X}");
        }

        public void JmpGeRelativeLabel(int line)
        {
            LinesAdd($"jge .addr_{line:X}");
        }

        public void JmpLtUnRelativeLabel(int line)
        {
            LinesAdd($"jb .addr_{line:X}");
        }

        public void JmpGtUnRelativeLabel(int line)
        {
            LinesAdd($"ja .addr_{line:X}");
        }

        public void JmpLeUnRelativeLabel(int line)
        {
            LinesAdd($"jbe .addr_{line:X}");
        }

        public void JmpZeroRelativeLabel(int line)
        {
            LinesAdd($"jz .addr_{line:X}");
        }

        public void JmpNZeroRelativeLabel(int line)
        {
            LinesAdd($"jnz .addr_{line:X}");
        }

        public void JmpGeUnRelativeLabel(int line)
        {
            LinesAdd($"jae .addr_{line:X}");
        }

        public void JmpEqRelativeLocalLabel(int idx)
        {
            LinesAdd($"je .addr_{CurLine:X}_{idx}");
        }

        public void JmpLtRelativeLocalLabel(int idx)
        {
            LinesAdd($"jl .addr_{CurLine:X}_{idx}");
        }

        public void JmpGtRelativeLocalLabel(int idx)
        {
            LinesAdd($"jg .addr_{CurLine:X}_{idx}");
        }

        public void JmpLtUnRelativeLocalLabel(int idx)
        {
            LinesAdd($"jb .addr_{CurLine:X}_{idx}");
        }

        public void JmpGtUnRelativeLocalLabel(int idx)
        {
            LinesAdd($"ja .addr_{CurLine:X}_{idx}");
        }
        #endregion

        #region Conditional Cmp
        public void CmpEqRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"sete {RegisterName(line, sz)}");
        }

        public void CmpNeRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setne {RegisterName(line, sz)}");
        }

        public void CmpLtRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setl {RegisterName(line, sz)}");
        }

        public void CmpGtRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setg {RegisterName(line, sz)}");
        }

        public void CmpLeRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setle {RegisterName(line, sz)}");
        }

        public void CmpGeRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setge {RegisterName(line, sz)}");
        }

        public void CmpLtUnRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setb {RegisterName(line, sz)}");
        }

        public void CmpGtUnRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"seta {RegisterName(line, sz)}");
        }

        public void CmpLeUnRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setbe {RegisterName(line, sz)}");
        }

        public void CmpGeUnRelativeLabel(AssemRegisters line, int sz)
        {
            LinesAdd($"setae {RegisterName(line, sz)}");
        }
        #endregion

        public void TestBool(AssemRegisters reg)
        {
            Test(reg, reg);
        }
        public void Test(AssemRegisters reg, AssemRegisters reg2)
        {
            LinesAdd($"test {RegisterName(reg)}, {RegisterName(reg2)}");
        }

        public string[] GetLines()
        {
            return Lines.ToArray();
        }

    }
}
