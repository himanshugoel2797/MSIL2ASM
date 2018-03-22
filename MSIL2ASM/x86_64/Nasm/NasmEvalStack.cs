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
        Stack<int> Registers;
        StackQueue<StackAllocs> EvalStack;

        //Allocate registers in a stack manner
        //When out of registers, make the bottom most item on the stack spill and use the register as the top
        enum AllocationType
        {
            Register,
            SpillStack
        }

        class StackAllocs
        {
            public AllocationType AllocationType;
            public int Position;
            public int TypeSize;
            public bool ValueKnown;
            public ulong Value;
        }

        public void InitRegisters()
        {
            //TODO look into having a mode of optimizing the code via shuffling register allocatin orders
            for (int i = 15; i >= 0; i--)
            {
                if (i == 5) continue;
                Registers.Push(i);
            }
        }

        public int EvalStackSize()
        {
            return EvalStack.Count;
        }

        public int AllocEvalStack(int tSize)
        {
            return AllocEvalStack(tSize, false, 0);
        }

        public int AllocEvalStack(int tSize, bool knowVal, ulong val)
        {
            if (Registers.Count > 0)
            {
                //Just use a register
                var reg = Registers.Pop();
                var evalEnt = new StackAllocs()
                {
                    AllocationType = AllocationType.Register,
                    Position = reg,
                    TypeSize = tSize,
                    ValueKnown = knowVal,
                    Value = val,
                };
                EvalStack.Push(evalEnt);
                return reg;
            }
            else
            {
                //Make last non-register entry be on the stack
                StackAllocs lastEnt;
                Stack<StackAllocs> history = new Stack<StackAllocs>();
                do
                {
                    lastEnt = EvalStack.Dequeue();
                    if (lastEnt.AllocationType != AllocationType.Register)
                        history.Push(lastEnt);
                } while (lastEnt.AllocationType != AllocationType.Register);

                var evalEnt = new StackAllocs()
                {
                    AllocationType = AllocationType.SpillStack,
                    Position = SpillCurOffset,
                    TypeSize = lastEnt.TypeSize,
                    ValueKnown = knowVal,
                    Value = val
                };
                EvalStack.Enqueue(evalEnt);
                for (int i = 0; i < history.Count; i++)
                    EvalStack.Enqueue(history.Pop());

                Emitter.MovRegisterToRegisterRelativeAddress(lastEnt.Position, (int)AssemRegisters.Rsp, (SpillTopOffset + SpillCurOffset));
                SpillCurOffset += AMD64Backend.PointerSize;

                lastEnt.TypeSize = tSize;
                EvalStack.Push(lastEnt);
                return lastEnt.Position;
            }
        }

        private int StackSize
        {
            get
            {
                return EvalStack.Count;
            }
        }

        private int PeekEvalStack(out int tSize)
        {
            int reg = PopEvalStack(out tSize);
            if (AllocEvalStack(tSize) != reg)
                throw new Exception("Unexpected behavior, same register should be reallocated!");
            return reg;
        }

        private StackAllocs PeekEvalStackFull()
        {
            var reg = PopEvalStackFull();
            if (AllocEvalStack(reg.TypeSize, reg.ValueKnown, reg.Value) != reg.Position)
                throw new Exception("Unexpected behavior, same register should be reallocated!");
            return reg;
        }

        private StackAllocs PopEvalStackFull()
        {
            var evalRes = EvalStack.Pop();
            if (evalRes.AllocationType == AllocationType.Register)
            {
                Registers.Push(evalRes.Position);
            }
            else
            {
                //Move this into a register and then return it
                var reg = Registers.Pop();
                Emitter.MovRelativeAddressToRegisterSize((int)AssemRegisters.Rsp, (SpillTopOffset + evalRes.Position), reg, evalRes.TypeSize);
                //TODO: free the spill space slot
                evalRes.AllocationType = AllocationType.Register;
                evalRes.Position = reg;
            }
            return evalRes;
        }

        private int PopEvalStack(out int tSize)
        {
            var r = PopEvalStackFull();
            tSize = r.TypeSize;
            return r.Position;
        }
    }
}
