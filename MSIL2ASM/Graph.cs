using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public interface IGraphNode
    {
        int GetID();
    }

    public class Graph<T> where T : IGraphNode
    {
        public class GraphNode
        {
            public T Node { get; internal set; }
            public int Label { get; internal set; }
            public List<int> Outgoing { get; internal set; }
            public List<int> Incoming { get; internal set; }
        }

        public Dictionary<int, GraphNode> Nodes { get; private set; }

        public Graph()
        {
            Nodes = new Dictionary<int, GraphNode>();
        }

        public void AddNode(T node)
        {
            GraphNode graphNode = new GraphNode()
            {
                Label = 0,
                Node = node,
                Outgoing = new List<int>(),
                Incoming = new List<int>(),
            };

            Nodes[node.GetID()] = graphNode;
        }

        public void AddDirectionalEdge(int from, int to)
        {
            if (!Nodes.ContainsKey(from))
                throw new IndexOutOfRangeException();

            if (!Nodes.ContainsKey(to))
                throw new IndexOutOfRangeException();

            Nodes[from].Outgoing.Add(to);
            Nodes[to].Incoming.Add(from);
        }

        public void RemoveDirectionalEdge(int from, int to)
        {
            if (!Nodes.ContainsKey(from))
                throw new IndexOutOfRangeException();

            if (!Nodes.ContainsKey(to))
                throw new IndexOutOfRangeException();

            if (!Nodes[from].Outgoing.Contains(to) | !Nodes[to].Incoming.Contains(from))
                throw new IndexOutOfRangeException();


            Nodes[from].Outgoing.Remove(to);
            Nodes[to].Incoming.Remove(from);
        }

        private Dictionary<int, List<int>> LabelMappings;
        private void Label(int idx, int lbl)
        {
            if (Nodes[idx].Label != 0)
            {
                if (Nodes[idx].Label != lbl)
                {
                    LabelMappings[lbl].Add(Nodes[idx].Label);
                    LabelMappings[Nodes[idx].Label].Add(lbl);
                }

                return;
            }

            Nodes[idx].Label = lbl;
            for (int i = 0; i < Nodes[idx].Outgoing.Count; i++)
                Label(Nodes[idx].Outgoing[i], lbl);

            for (int i = 0; i < Nodes[idx].Incoming.Count; i++)
                Label(Nodes[idx].Incoming[i], lbl);
        }

        public Graph<T>[] FloodFill()
        {
            Dictionary<int, Graph<T>> graphs = new Dictionary<int, Graph<T>>();
            LabelMappings = new Dictionary<int, List<int>>();

            int lbl = 1;
            foreach (KeyValuePair<int, GraphNode> node_entry in Nodes)
            {
                var node = node_entry.Value;

                LabelMappings[lbl] = new List<int>();
                LabelMappings[lbl].Add(lbl);

                if (node.Label == 0)
                    Label(node_entry.Key, lbl++);
            }

            foreach (int i in LabelMappings.Keys)
                LabelMappings[i].Sort();

            foreach (int key in Nodes.Keys)
            {
                //Find the lowest mapped label
                int cur_lbl = LabelMappings[Nodes[key].Label][0];
                Nodes[key].Label = cur_lbl;

                if (!graphs.ContainsKey(cur_lbl))
                    graphs[cur_lbl] = new Graph<T>();

                graphs[cur_lbl].AddNode(Nodes[key].Node);
            }

            foreach (int key in Nodes.Keys)
            {
                for (int i = 0; i < Nodes[key].Incoming.Count; i++)
                {
                    graphs[Nodes[key].Label].AddDirectionalEdge(Nodes[key].Incoming[i], key);
                }
            }

            return graphs.Values.ToArray();
        }

        public int[] GetLeafNodes()
        {
            List<int> leaves = new List<int>();

            foreach (int key in Nodes.Keys)
            {
                if (Nodes[key].Outgoing.Count == 0)
                    leaves.Add(key);
            }

            return leaves.ToArray();
        }

        public int[] GetRootNodes()
        {
            List<int> roots = new List<int>();

            foreach (int key in Nodes.Keys)
            {
                if (Nodes[key].Incoming.Count == 0)
                    roots.Add(key);
            }

            return roots.ToArray();
        }

        public void RemoveNode(int id)
        {
            if (Nodes.ContainsKey(id))
                Nodes.Remove(id);
        }

        public void RemoveDisconnected()
        {
            //Remove all nodes that have no connections
            var keys = Nodes.Keys.ToArray();
            if (keys.Length != 1)
            {
                foreach (int key in keys)
                {
                    if (Nodes[key].Outgoing.Count == 0 && Nodes[key].Incoming.Count == 0)
                        Nodes.Remove(key);
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int j = 0; j < Nodes.Count; j++)
            {
                var str = $"\tEntry: {Nodes.ElementAt(j).Key} [";

                for (int k = 0; k < Nodes.ElementAt(j).Value.Incoming.Count; k++)
                {
                    str += Nodes.ElementAt(j).Value.Incoming[k];
                    if (k < Nodes.ElementAt(j).Value.Incoming.Count - 1)
                        str += ", ";
                }

                str += "]";

                s += str + $"\n{Nodes.ElementAt(j).Value.Node}\n";
            }

            return s;
        }
    }
}
