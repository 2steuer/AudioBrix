using System;
using System.Threading;
using AudioBrix.Interfaces;
using AudioBrix.Material;
using AudioBrix.Util;

namespace AudioBrix.Bricks.Buffer
{
    public class AudioBuffer : IFrameSource, IFrameSink
    {
        public AudioFormat Format { get; }

        private RingBuffer<float> _buffer;

        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        /// <summary>
        /// Capacity in frames
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// If true, the number of frames <see cref="GetFrames"/> returns will always
        /// match the number of samples requested, the rest is filled with zeroes.
        ///
        /// If false, the number of frames returned might be lower or even zero.
        /// </summary>
        public bool FillWithZero { get; set; } = false;

        /// <summary>
        /// If set to true, <see cref="GetFrames"/> will wait for data to be available if the buffer
        /// is empty. If false, it will just return zero frames.
        /// </summary>
        public bool WaitOnEmpty { get; set; } = true;

        /// <summary>
        /// Number of milliseconds until waiting for new samples times out.
        ///
        /// -1 for waiting without timeout.
        /// </summary>
        public int WaitTimeout { get; set; } = -1;

        public AudioBuffer(AudioFormat format, int frameCapacity)
        {
            Format = format;

            _buffer = new RingBuffer<float>(Format.Channels * frameCapacity);
            Capacity = frameCapacity;
        }

        public Span<float> GetFrames(int frameCount)
        {
            var samples = frameCount * Format.Channels;

            if (_buffer.IsEmpty && WaitOnEmpty)
            {
                if (WaitTimeout < 0)
                {
                    _resetEvent.WaitOne();
                }
                else
                {
                    _resetEvent.WaitOne(WaitTimeout);
                }
                _resetEvent.Reset();
            }

            var rsp = _buffer.Get(samples);

            if (rsp.Length < samples && FillWithZero)
            {
                var nBuf = new Span<float>(new float[samples]);
                rsp.CopyTo(nBuf);

                return nBuf;
            }

            return rsp;
        }

        public int AddFrames(Span<float> samples)
        {
            var addedFrames = _buffer.Add(samples);

            if (addedFrames < samples.Length)
            {
                // TODO: Warning about buffer overflow here...
            }
            _resetEvent.Set();

            return addedFrames;
        }
    }
}
