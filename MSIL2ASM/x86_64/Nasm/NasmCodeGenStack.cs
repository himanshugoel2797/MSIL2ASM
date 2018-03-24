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
        public void GenerateCode(Graph<GraphNode<OptimizationToken>> graph)
        {
            var tkns = graph.Nodes.Values.ToArray();

            for (int i = 0; i < tkns.Length; i++)
            {
                var tkn = tkns[i].Node.Token;
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

                }
            }
        }
    }
}
