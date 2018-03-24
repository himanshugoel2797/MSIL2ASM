using MSIL2ASM.x86_64.Nasm.Assembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    public enum OptimizationInstruction
    {
        Ld,
        St,
        Add,
        And,
        Div,
        Mul,
        Neg,
        Not,
        Or,
        Rem,
        Shl,
        Shr,
        Sub,
        Xor,
        Branch,
        Call,
        CallVirt,
        Compare,
        Convert,
        Dup,
        Pop,
        Ret,
        Switch,
    }

    [Flags]
    public enum OptimizationInstructionSubType
    {
        None = 0,
        Arg = 1,
        ArgAddress = 2,
        Constant = 4,
        Element = 8,
        ElementAddress = 16,
        Field = 32,
        FieldAddress = 64,
        Indirect = 128,
        Length = 256,
        Local = 512,
        LocalAddress = 1024,
        Null = 2048,
        StaticField = 4096,
        StaticFieldAddress = 8192,
        String = 16384,
        Token = 32768,
        Function = 65536,

        CheckOverflow = 131072,
        Unsigned = 262144,

        Equal = 524288,
        NotEqual = 1048576,
        Greater = 2097152,
        Less = 4194304,

        True = 8388608,
        False = 16777216,
    }

    public enum OptimizationParameterLocation
    {
        Index = 0,  //Only used for Parameters, input is the result of a previous instruction
        Const,  //Used for both, value is known and is constant
        Result, //Only used for Results, output is the result of the current instruction
    }

    [Flags]
    public enum OptimizationParameterType
    {
        Unknown = 0,
        Float = 1,
        Integer = 2,
        ManagedPointer = 4,
        UnmanagedPointer = 8
    }

    public struct OptimizationParameter
    {
        public OptimizationParameterLocation ParameterLocation { get; set; }
        public OptimizationParameterType ParameterType { get; set; }
        public ulong Value { get; set; }
        public int Size { get; set; }
    }

    public struct OptimizationToken
    {
        public int ID { get; set; }
        public int Offset { get; set; }

        public OptimizationInstruction Instruction { get; set; }
        public OptimizationInstructionSubType SubType { get; set; }

        public OptimizationParameter[] Parameters { get; set; }
        public OptimizationParameter[] Results { get; set; }

        public AssemRegisters[] ParameterRegisters { get; set; }
        public AssemRegisters[] ResultRegisters { get; set; }
        public AssemRegisters[] ThunkRegisters { get; set; }

        public bool[] SaveParameterRegister { get; set; }

        public ulong[] Constants { get; set; }
        public string[] Strings { get; set; }

        private int ResultIdx;

        public int GetResultIdx()
        {
            if (ResultIdx == 0)
                ResultIdx = Results.Length;

            if (Results.Length == 0)
                throw new Exception();

            return ResultIdx-- - 1;
        }

        public override string ToString()
        {
            var str = $"\t\t{Instruction}, {SubType}";

            for (int i = 0; i < ParameterRegisters.Length; i++)
            {
                str += $"\n\t\t\tParameter {i} : {ParameterRegisters[i]}, {Parameters[i].ParameterLocation}";
            }

            for (int i = 0; i < ResultRegisters.Length; i++)
            {
                str += $"\n\t\t\tResult {i} : {ResultRegisters[i]}, {Results[i].ParameterLocation}";
            }

            for (int i = 0; i < ThunkRegisters.Length; i++)
            {
                str += $"\n\t\t\tThunk {i} : {ThunkRegisters[i]}";
            }

            return str;
        }
    }
}
