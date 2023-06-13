using System;

namespace AudioBrix.Util
{
    public class RingBuffer<T>
    {
        private T[] _buffer;

        private int _head = 0;
        private int _tail = 0;

        private int _count = 0;

        public bool IsFull
        {
            get
            {
                lock (this)
                {
                    return _count == _buffer.Length;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (this)
                {
                    return _count == 0;
                }
            }
        }

        public int Used
        {
            get
            {
                lock (this)
                {
                    return _count;
                }
            }
        }

        public int Free => _buffer.Length - Used;

        public RingBuffer(int capacity)
        {
            _buffer = new T[capacity];
        }

        public Span<T> Get(int count)
        {
            int firstReadIndex;
            int firstReadCount;
            bool wrapAround = false;
            int secondReadCount = 0;

            lock (this)
            {
                if (count > _count)
                {
                    count = _count;
                }

                firstReadIndex = _tail;
                firstReadCount = Math.Min(_buffer.Length - _tail, count);
                wrapAround = false;

                if (firstReadCount != count)
                {
                    secondReadCount = count - firstReadCount;
                    wrapAround = true;
                }

                _tail = (_tail + count) % _buffer.Length;
                _count -= count;
            }

            if (!wrapAround)
            {
                return new Span<T>(_buffer, firstReadIndex, firstReadCount);
            }
            else
            {
                var nspan = new Span<T>(new T[count]);
                var bufSpan = new Span<T>(_buffer);

                bufSpan.Slice(firstReadIndex, firstReadCount).CopyTo(nspan);
                bufSpan.Slice(0, secondReadCount).CopyTo(nspan.Slice(firstReadCount));

                return nspan;
            }
        }

        public int Add(Span<T> data)
        {
            int firstCopyIndex;
            int firstCopyCount;
            bool wrapAround = false;
            int secondCopyCount = 0;

            lock (this)
            {
                int copyCount = data.Length;
                if (copyCount > _buffer.Length - _count)
                {
                    copyCount = _buffer.Length - _count;
                }

                firstCopyIndex = _head;
                firstCopyCount = Math.Min(_buffer.Length - firstCopyIndex, copyCount);

                if (firstCopyCount != copyCount)
                {
                    wrapAround = true;
                    secondCopyCount = copyCount - firstCopyCount;
                }

                _head = (_head + copyCount) % _buffer.Length;
                _count += copyCount;
            }

            var bufSpan = new Span<T>(_buffer);

            data.Slice(0, firstCopyCount).CopyTo(bufSpan.Slice(firstCopyIndex));

            if (wrapAround)
            {
                data.Slice(firstCopyCount, secondCopyCount).CopyTo(bufSpan);
            }

            return firstCopyCount + secondCopyCount;
        }

        public void Reset()
        {
            lock (this)
            {
                _count = 0;
                _head = 0;
                _tail = 0;
            }
        }
    }
}
