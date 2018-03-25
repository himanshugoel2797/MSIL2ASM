using MSIL2ASM.Builtins;
using MSIL2ASM.CoreLib;
using MSIL2ASM.x86_64.Nasm.Assembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.Nasm
{
    partial class NasmEmitter
    {
        class LocalAllocs
        {
            public int TypeSize;
        }

        private List<TypeDef> types;
        private List<string> Lines;
        private List<string> data;
        private List<string> bss;
        private List<string> externals;
        private List<string> stringTable;
        private List<string> static_ctors;

        private string prefix;
        private InstrEmitter Emitter;
        private int ArgumentTopOffset = 0;
        private int LocalTopOffset = 0;
        private List<LocalAllocs> Locals;

        public NasmEmitter(List<TypeDef> types)
        {
            Lines = new List<string>();
            data = new List<string>();
            bss = new List<string>();
            stringTable = new List<string>();
            externals = new List<string>();
            static_ctors = new List<string>();

            Emitter = new InstrEmitter();

            this.types = types;
        }

        private string GenerateVariableName(int id)
        {
            return ".var" + id.ToString() + "_";
        }

        private string GenerateParameterName(int id)
        {
            return "_par" + id.ToString() + "_";
        }

        public string GetFile()
        {
            string file = "[BITS 64]\n";

            file += "\n";
            file += "section .data\n";
            foreach (string s in data)
                file += s + "\n";

            file += "\n";
            file += "section .bss\n";
            foreach (string s in bss)
                file += s + "\n";

            file += "\n";
            file += "section .static_ctors\n";
            foreach (string s in static_ctors)
                file += "dq " + s + "\n";

            file += "\n";
            file += "section .text\n";
            foreach (string s in externals)
                file += "extern " + s + "\n";

            foreach (string s in Lines)
                file += s + "\n";

            return file;
        }

        public void Generate(TypeDef tDef)
        {
            EmitStaticStruct(MachineSpec.GetTypeName(tDef) + "_static", tDef.StaticSize);

            for (int i = 0; i < tDef.StaticMethods.Length; i++)
                GenerateMethod(tDef, tDef.StaticMethods[i]);

            for (int i = 0; i < tDef.InstanceMethods.Length; i++)
                GenerateMethod(tDef, tDef.InstanceMethods[i]);

            Lines.AddRange(Emitter.GetLines());
        }

        public class GraphNode<T> : IGraphNode
        {
            public T Token;

            private int ID;

            public GraphNode(int id, T tkn)
            {
                Token = tkn;
                ID = id;
            }

            public int GetID()
            {
                return ID;
            }

            public override string ToString()
            {
                return Token.ToString();
            }
        }

        public void GenerateMethod(TypeDef tDef, MethodDef method)
        {
            prefix = MachineSpec.GetTypeName(tDef);
            var mthdName = MachineSpec.GetMethodName(method);
            Emitter.MakeGlobalFunction(mthdName);

            //Setup ArgumentTopOffset
            ArgumentTopOffset = MachineSpec.PointerSize;

            //Setup LocalTopOffset
            ArgumentTopOffset += method.LocalsSize;
            LocalTopOffset = method.LocalsSize;
            Locals = new List<LocalAllocs>();

            Emitter.SubRegConst(AssemRegisters.Rsp, LocalTopOffset);

            //Initialize Locals table
            for (int i = 0; i < method.LocalsSize / MachineSpec.PointerSize; i++)
            {
                Locals.Add(new LocalAllocs());
            }

            //Setup StackSpillOffset
            if (method.StackSize > 15)
                throw new NotImplementedException();

            //Convert the tokens into a more flexible form that is suited to optimization
            if (method.ByteCode == null)
                return;

            var tkns = method.ByteCode.GetTokens();
            var mainGraph = new Graph<GraphNode<OptimizationToken>>();
            for (int i = 0; i < tkns.Length; i++)
            {
                if (tkns[i].Operation == InstructionTypes.Nop)
                    continue;

                mainGraph.AddNode(new GraphNode<OptimizationToken>(tkns[i].ID, OptimizationTokenParser.Parse(tkns[i])));

                if (tkns[i].Parameters != null)
                    for (int j = 0; j < tkns[i].Parameters.Length; j++)
                        mainGraph.AddDirectionalEdge(tkns[i].Parameters[j], tkns[i].ID);
            }

            var subGraphs = mainGraph.FloodFill();

            //Get root nodes to propogate constant evaluations
            for (int i = 0; i < subGraphs.Length; i++)
            {
                var leaves = subGraphs[i].GetLeafNodes();

                for (int j = 0; j < leaves.Length; j++)
                    SimplifyGraph(leaves[j], subGraphs[i]);

                subGraphs[i].RemoveDisconnected();
            }


            //Propogate thunk registers through the graph
            var thunkSets = new Dictionary<AssemRegisters, List<int>>[subGraphs.Length];
            for (int i = 0; i < subGraphs.Length; i++)
            {
                Thunks = new Dictionary<AssemRegisters, List<int>>();
                var leaves = subGraphs[i].GetLeafNodes();

                for (int j = 0; j < leaves.Length; j++)
                    ThunkRegisters(leaves[j], subGraphs[i]);

                thunkSets[i] = Thunks;
            }

            //Propogate register preferences through the graph
            var registerSets = new Dictionary<AssemRegisters, int>[subGraphs.Length];
            var registerInUseSets = new Dictionary<AssemRegisters, bool>[subGraphs.Length];
            for (int i = 0; i < subGraphs.Length; i++)
            {
                Thunks = thunkSets[i];

                RegisterAllocs = new Dictionary<AssemRegisters, int>();
                RegisterAllocs[AssemRegisters.Rax] = 0;
                RegisterAllocs[AssemRegisters.Rbx] = 0;
                RegisterAllocs[AssemRegisters.Rcx] = 0;
                RegisterAllocs[AssemRegisters.Rdx] = 0;
                RegisterAllocs[AssemRegisters.Rsi] = 0;
                RegisterAllocs[AssemRegisters.Rdi] = 0;
                RegisterAllocs[AssemRegisters.Rbp] = 0;
                RegisterAllocs[AssemRegisters.R8] = 0;
                RegisterAllocs[AssemRegisters.R9] = 0;
                RegisterAllocs[AssemRegisters.R10] = 0;
                RegisterAllocs[AssemRegisters.R11] = 0;
                RegisterAllocs[AssemRegisters.R12] = 0;
                RegisterAllocs[AssemRegisters.R13] = 0;
                RegisterAllocs[AssemRegisters.R14] = 0;
                RegisterAllocs[AssemRegisters.R15] = 0;

                RegisterInUse = new Dictionary<AssemRegisters, bool>();
                RegisterInUse[AssemRegisters.Rax] = false;
                RegisterInUse[AssemRegisters.Rbx] = false;
                RegisterInUse[AssemRegisters.Rcx] = false;
                RegisterInUse[AssemRegisters.Rdx] = false;
                RegisterInUse[AssemRegisters.Rsi] = false;
                RegisterInUse[AssemRegisters.Rdi] = false;
                RegisterInUse[AssemRegisters.Rbp] = false;
                RegisterInUse[AssemRegisters.R8] = false;
                RegisterInUse[AssemRegisters.R9] = false;
                RegisterInUse[AssemRegisters.R10] = false;
                RegisterInUse[AssemRegisters.R11] = false;
                RegisterInUse[AssemRegisters.R12] = false;
                RegisterInUse[AssemRegisters.R13] = false;
                RegisterInUse[AssemRegisters.R14] = false;
                RegisterInUse[AssemRegisters.R15] = false;

                var leaves = subGraphs[i].GetLeafNodes();

                for (int j = 0; j < leaves.Length; j++)
                    PropogateRegisters(leaves[j], subGraphs[i]);

                registerSets[i] = RegisterAllocs;
                registerInUseSets[i] = RegisterInUse;
            }

            //Allocate registers, each subgraph has its own allocation set
            for (int i = 0; i < subGraphs.Length; i++)
            {
                RegisterAllocs = registerSets[i];
                RegisterInUse = registerInUseSets[i];

                var nodes = subGraphs[i].Nodes.Values.ToArray();
                var node_tkns = nodes.Select(a => a.Node.Token).ToArray();

                for (int j = 0; j < node_tkns.Length; j++)
                {
                    AllocateRegisters(node_tkns);
                }

                registerInUseSets[i] = RegisterInUse;
                registerSets[i] = RegisterAllocs;
            }

#if DEBUG
            for (int i = 0; i < subGraphs.Length; i++)
            {
                Console.WriteLine($"Subgraph {i}");
                Console.WriteLine(subGraphs[i].ToString());
            }
#endif

            //Generate the assembly output
            for (int i = 0; i < subGraphs.Length; i++)
            {
                GenerateCode(subGraphs[i]);
            }
        }

        private Dictionary<AssemRegisters, List<int>> Thunks = new Dictionary<AssemRegisters, List<int>>();
        private Dictionary<AssemRegisters, int> RegisterAllocs = new Dictionary<AssemRegisters, int>();
        private Dictionary<AssemRegisters, bool> RegisterInUse = new Dictionary<AssemRegisters, bool>();
        public void ThunkRegisters(int root, Graph<GraphNode<OptimizationToken>> graph)
        {
            var node = graph.Nodes[root];
            for (int i = 0; i < node.Incoming.Count; i++)
            {
                for (int j = 0; j < node.Node.Token.ThunkRegisters.Length; j++)
                {
                    if (!Thunks.ContainsKey(node.Node.Token.ThunkRegisters[j]))
                        Thunks[node.Node.Token.ThunkRegisters[j]] = new List<int>();

                    if (!Thunks[node.Node.Token.ThunkRegisters[j]].Contains(node.Node.Token.Offset))
                        Thunks[node.Node.Token.ThunkRegisters[j]].Add(node.Node.Token.Offset);
                }

                ThunkRegisters(node.Incoming[i], graph);
            }
        }

        public void AllocateRegisters(OptimizationToken[] graph)
        {
            for (int i0 = 0; i0 < graph.Length; i0++)
            {
                var tkn = graph[i0];

                for (int i = 0; i < tkn.ResultRegisters.Length; i++)
                    if (tkn.ResultRegisters[i].HasFlag(AssemRegisters.Any))
                    {
                        //Ensure that the chosen register isn't used between the instructions
                        List<AssemRegisters> UsedRegisters = new List<AssemRegisters>();
                        for (int i1 = 0; i1 < graph.Length; i1++)
                        {
                            var tkn0 = graph[i1];

                            for (int i2 = 0; i2 < tkn0.ResultRegisters.Length; i2++)
                                if (!tkn0.ResultRegisters[i2].HasFlag(AssemRegisters.Any))
                                    UsedRegisters.Add(tkn0.ResultRegisters[i2]);

                            if (i1 < i0)
                                for (int i2 = 0; i2 < tkn0.ParameterRegisters.Length; i2++)
                                    if (!tkn0.ParameterRegisters[i2].HasFlag(AssemRegisters.Any))
                                        UsedRegisters.Remove(tkn0.ParameterRegisters[i2]);

                        }

                        //Allocate a register, ensuring that is doesn't cross a thunk boundary
                        var avl_regs = RegisterAllocs.Where(a => !UsedRegisters.Contains(a.Key)).Select(a => a.Key).ToArray();
                        AssemRegisters alloc_reg = AssemRegisters.None;

                        if (avl_regs.Length == 0)
                            throw new Exception("Not enough registers!");

                        for (int i1 = 0; i1 < avl_regs.Length; i1++)
                        {
                            if (Thunks.ContainsKey(avl_regs[i1]))
                            {
                                var thunk_inbetween = Thunks[avl_regs[i1]].Where(a => (a < i0));

                                if (thunk_inbetween.Count() == 0)
                                {
                                    //Allocate this register
                                    alloc_reg = avl_regs[i];
                                    break;
                                }
                            }
                            else
                            {
                                alloc_reg = avl_regs[i];
                                break;
                            }
                        }

                        //Allocate this register
                        graph[i0].ResultRegisters[i] = alloc_reg;

                        //Pass this allocation onto any parameters that refer to this result
                        for (int i1 = 0; i1 < graph.Length; i1++)
                        {
                            var tkn0 = graph[i1];
                            for (int i2 = 0; i2 < tkn0.ParameterRegisters.Length; i2++)
                            {
                                if (tkn0.Parameters[i2].ParameterLocation == OptimizationParameterLocation.Index && tkn0.Parameters[i2].Value == (ulong)tkn.ID)
                                    graph[i1].ParameterRegisters[i2] = alloc_reg;
                            }
                        }
                    }

                for (int i = 0; i < tkn.ParameterRegisters.Length; i++)
                {
                    if (tkn.Parameters[i].ParameterLocation == OptimizationParameterLocation.Const && (tkn.ParameterRegisters[i] & AssemRegisters.Const) == 0)
                    {
                        List<AssemRegisters> UsedRegisters = new List<AssemRegisters>();
                        for (int i1 = 0; i1 <= i0; i1++)
                        {
                            var tkn0 = graph[i1];

                            for (int i2 = 0; i2 < tkn0.ResultRegisters.Length; i2++)
                                if (!tkn0.ResultRegisters[i2].HasFlag(AssemRegisters.Any))
                                    UsedRegisters.Add(tkn0.ResultRegisters[i2]);

                            if (i1 < i0)
                                for (int i2 = 0; i2 < tkn0.ParameterRegisters.Length; i2++)
                                    if (!tkn0.ParameterRegisters[i2].HasFlag(AssemRegisters.Any))
                                        UsedRegisters.Remove(tkn0.ParameterRegisters[i2]);
                        }

                        var avl_regs = RegisterAllocs.Where(a => !UsedRegisters.Contains(a.Key)).Select(a => a.Key).ToArray();
                        if (avl_regs.Length == 0)
                            throw new Exception("Not enough registers!");

                        tkn.ParameterRegisters[i] = avl_regs[0];
                    }
                }
            }
        }

        public void PropogateRegisters(int root, Graph<GraphNode<OptimizationToken>> graph)
        {
            var node = graph.Nodes[root];
            var tkn = node.Node.Token;

            //If the ParameterRegisters is given, is not an active thunk register and the associated incoming connection is Any, update ResultRegisters
            //If the ParameterRegisters is given and is an active thunk register, mark the register as needing to be saved
            for (int j = 0; j < tkn.ParameterRegisters.Length; j++)
            {
                OptimizationToken res = null;
                int resultIdx = -1;

                if (tkn.Parameters[j].ParameterLocation == OptimizationParameterLocation.Index)
                {
                    res = graph.Nodes[(int)tkn.Parameters[j].Value].Node.Token;
                    resultIdx = res.GetResultIdx();
                }

                bool isRegBased = true;
                if (tkn.Parameters[j].ParameterLocation == OptimizationParameterLocation.Const && (tkn.ParameterRegisters[j] & AssemRegisters.Const) != 0)
                {
                    isRegBased = false;

                    //Update the entry to be using the constant form
                    if (tkn.Parameters[j].Value <= byte.MaxValue & tkn.ParameterRegisters[j].HasFlag(AssemRegisters.Const8))
                    {
                        tkn.ParameterRegisters[j] = AssemRegisters.Const8;
                        tkn.Parameters[j].Size = 1;
                    }
                    else if (tkn.Parameters[j].Value <= ushort.MaxValue & tkn.ParameterRegisters[j].HasFlag(AssemRegisters.Const16))
                    {
                        tkn.ParameterRegisters[j] = AssemRegisters.Const16;
                        tkn.Parameters[j].Size = 2;
                    }
                    else if (tkn.Parameters[j].Value <= uint.MaxValue & tkn.ParameterRegisters[j].HasFlag(AssemRegisters.Const32))
                    {
                        tkn.ParameterRegisters[j] = AssemRegisters.Const32;
                        tkn.Parameters[j].Size = 4;
                    }
                    else
                    {
                        tkn.ParameterRegisters[j] = AssemRegisters.Const64;
                        tkn.Parameters[j].Size = 8;
                    }
                }

                if (isRegBased && !tkn.ParameterRegisters[j].HasFlag(AssemRegisters.Any))
                {
                    var reg = tkn.ParameterRegisters[j] & ~AssemRegisters.Const;

                    //Propogate the result register
                    if (res.ResultRegisters.Length != 0)
                    {
                        if (res.ResultRegisters[resultIdx].HasFlag(AssemRegisters.Any))
                            res.ResultRegisters[resultIdx] = node.Node.Token.ParameterRegisters[j];
                        else if (res.ResultRegisters[resultIdx] != reg)
                        {
                            //We have a conflict of assignments, will need to insert a move during code generation
                        }
                    }
                }
                else if (isRegBased && tkn.ParameterRegisters[j].HasFlag(AssemRegisters.Any))
                {
                    if (tkn.Parameters[j].ParameterLocation != OptimizationParameterLocation.Const)
                    {
                        //Propogate the register from Result into the Parameter
                        if (res.ResultRegisters.Length == 0)
                            throw new Exception("Expected more than 0 results from previous token.");

                        tkn.ParameterRegisters[j] = res.ResultRegisters[resultIdx];
                        tkn.Parameters[j].Size = res.Results[resultIdx].Size;
                    }
                }

                if (tkn.Parameters[j].ParameterLocation == OptimizationParameterLocation.Index)
                    PropogateRegisters((int)tkn.Parameters[j].Value, graph);
            }
        }

        public void SimplifyGraph(int root, Graph<GraphNode<OptimizationToken>> graph)
        {
            var node = graph.Nodes[root];
            for (int i = 0; i < node.Incoming.Count; i++)
            {
                SimplifyGraph(node.Incoming[i], graph);
                var paramNode = graph.Nodes[node.Incoming[i]];

                var results = paramNode.Node.Token.Results;
                if (results.Length == 0) continue;

                int result_idx = paramNode.Node.Token.GetResultIdx();

                if (results != null && results[result_idx].ParameterLocation == OptimizationParameterLocation.Const && results[result_idx].ParameterType == OptimizationParameterType.Integer)
                {
                    int id = node.Incoming[i];
                    graph.RemoveDirectionalEdge(id, root);
                    graph.Nodes[root].Node.Token.Parameters[i].ParameterLocation = OptimizationParameterLocation.Const;
                    graph.Nodes[root].Node.Token.Parameters[i].ParameterType = results[result_idx].ParameterType;
                    graph.Nodes[root].Node.Token.Parameters[i].Size = results[result_idx].Size;
                    graph.Nodes[root].Node.Token.Parameters[i].Value = results[result_idx].Value;

                    if (paramNode.Incoming.Count == 0 && paramNode.Outgoing.Count == 0)
                        graph.RemoveNode(id);

                    i--;
                }
            }
        }
    }
}
