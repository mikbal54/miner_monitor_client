using System.Collections.Generic;

namespace MinerMonitorClient
{
    public class LimitedQueue<T> : Queue<T>
    {
        public int maxNumber = 1000;

        public void Enqueue(T item)
        {
            if (Count == maxNumber)
                Dequeue();
            base.Enqueue(item);
        }

    }
}
