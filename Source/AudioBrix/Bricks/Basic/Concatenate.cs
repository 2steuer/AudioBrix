using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic
{
    public class Concatenate : IFrameSource
    {
        public AudioFormat Format { get; }

        private List<IFrameSource> _sources = new();

        private int _currentSource = 0;

        public Concatenate(AudioFormat format, params IFrameSource[] sources)
        {
            Format = format;

            foreach (var frameSource in sources)
            {
                AddSource(frameSource);
            }
        }

        public void AddSource(IFrameSource source)
        {
            Format.ThrowIfNotEqual(source.Format);

            lock (_sources)
            {
                _sources.Add(source);
            }
        }

        public Span<float> GetFrames(int frameCount)
        {
            IFrameSource currentSource;

            lock (_sources)
            {
                if (_currentSource >= _sources.Count)
                {
                    return Span<float>.Empty;
                }

                currentSource = _sources[_currentSource];
            }

            var rv1 = currentSource.GetFrames(frameCount);

            var rv1FrameCount = rv1.Length / Format.Channels;

            if (rv1FrameCount == frameCount)
            {
                return rv1;
            }
            else
            {
                _currentSource++;

                var rv2 = GetFrames(frameCount - rv1FrameCount);

                var combinedBuffer = new Span<float>(new float[rv1.Length + rv2.Length]);
                rv1.CopyTo(combinedBuffer);
                rv2.CopyTo(combinedBuffer.Slice(rv1.Length));

                return rv2;
            }

        }
    }
}
