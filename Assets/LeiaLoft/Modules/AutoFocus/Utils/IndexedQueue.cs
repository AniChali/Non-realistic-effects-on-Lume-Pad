
using UnityEngine;

namespace LeiaLoft
{
    public class IndexedQueue<T>
    {
        private int startPos;
        private int count;
        readonly private T[] values;

        public int Count 
        { 
            get 
            { 
                return count; 
            } 
        }

        public IndexedQueue(int startCount)
        {
            values = new T[startCount];
        }
        
        public void Enqueue(T value)
        {
            int insertPosition = (startPos + count) % values.Length;
            values[insertPosition] = value;
            startPos = (startPos+1) % values.Length;
            count = Mathf.Min(values.Length, count+1); 
        }

        public T Dequeue()
        {
            T dequeuedValue = values[startPos];
            startPos = (startPos + 1) % values.Length;
            count--;
            return dequeuedValue;
        }

        public T this[int position]
        {
            get
            {
                return values[(startPos + position) % values.Length];
            }
            set
            {
                values[(startPos + position) % values.Length] = value;
            }
        }
    }
}