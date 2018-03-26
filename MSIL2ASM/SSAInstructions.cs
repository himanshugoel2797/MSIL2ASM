using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public enum InstructionTypes
    {
        None,
        Add,
        AddCheckOverflow,
        UAddCheckOverflow,
        And,
        ArgList,    //TODO
        Beq,
        Bge,
        BgeUn,
        Bgt,
        BgtUn,
        Ble,
        BleUn,
        Blt,
        BltUn,
        BneUn,
        Br,         //Unconditional branch
        BrFalse,    //Branch if zero
        BrTrue,     //Branch if not zero
        Call,
        Ceq,
        Cgt,
        CgtUn,
        Clt,
        CltUn,
        Convert,    //Convert integer
        ConvertCheckOverflow,   //Convert integer, checking for overflow
        Divide,
        UDivide,
        Dup,
        EndFinally,
        LdArg,      //Load argument
        LdArga,     //Load argument address
        Ldc,        //Load constant
        Ldftn,
        LdInd,
        LdLoc,      //Load local
        LdLoca,
        LdNull,
        Leave,
        Localloc,   //TODO
        Multiply,
        MultiplyCheckOverflow,
        UMultiplyCheckOverflow,
        Neg,
        Nop,
        Not,
        Or,
        Pop,
        Rem,
        URem,
        Ret,        //Return from function
        Shl,
        Shr,
        ShrUn,
        StArg,
        Stind,
        StLoc,      //Store local
        Subtract,
        SubtractCheckOverflow,
        USubtractCheckOverflow,
        Switch,
        Xor,

        Box,        //TODO
        CallVirt,
        CallVirtConstrained,
        CastClass,  //TODO
        InitObj,    //TODO
        IsInst,     //TODO
        Ldelem,
        Ldelema,
        Ldfld,
        Ldflda,
        Ldlen,
        LdObj,      //TODO
        Ldsfld,
        Ldsflda,
        LdStr,
        Ldtoken,    //TODO
        LdVirtFtn,  //TODO
        MkRefAny,   //TODO
        Newarr,
        Newobj,
        RefAnyType, //TODO
        RefAnyVal,  //TODO
        Rethrow,    //TODO
        SizeOf,     //TODO
        Stelem,
        Stfld,
        StObj,      //TODO
        Stsfld,
        Throw,      //TODO
        Unbox,      //TODO
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
}
