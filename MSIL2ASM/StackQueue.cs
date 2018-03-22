using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public class StackQueue<T>
    {
        private LinkedList<T> linkedList = new LinkedList<T>();

        public void Push(T obj)
        {
            this.linkedList.AddFirst(obj);
        }

        public void Enqueue(T obj)
        {
            this.linkedList.AddLast(obj);
        }

        public T Pop()
        {
            var obj = this.linkedList.First.Value;
            this.linkedList.RemoveFirst();
            return obj;
        }

        public T Dequeue()
        {
            var obj = this.linkedList.Last.Value;
            this.linkedList.RemoveLast();
            return obj;
        }

        public T PeekStack()
        {
            return this.linkedList.First.Value;
        }

        public T PeekQueue()
        {
            return this.linkedList.Last.Value;
        }

        public T this[int idx]
        {
            get
            {
                return linkedList.ElementAt(idx);
            }
        }

        public int Count
        {
            get
            {
                return this.linkedList.Count;
            }
        }
    }
}
