using System.Collections.Generic;
using System.Linq;

namespace Logic.Unit
{
    public class PriorityQueue<T>
    {
        private readonly SortedDictionary<float, Queue<T>> queues = new();

        public int Count { get; private set; }

        public void Enqueue(T item, float priority)
        {
            if (!queues.TryGetValue(priority, out var queue))
            {
                queue = new Queue<T>();
                queues.Add(priority, queue);
            }
            queue.Enqueue(item);
            Count++;
        }

        public T Dequeue()
        {
            var first = queues.First();
            var item = first.Value.Dequeue();
            if (first.Value.Count == 0)
                queues.Remove(first.Key);
            Count--;
            return item;
        }
    }
}