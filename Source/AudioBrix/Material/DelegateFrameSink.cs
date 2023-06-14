using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;

namespace AudioBrix.Material
{
    public delegate int FrameHandler(Span<float> frames);

    public class DelegateFrameSink : IFrameSink
    {
        public AudioFormat Format { get; }

        private FrameHandler? _frameHandler;

        public DelegateFrameSink(AudioFormat format, FrameHandler? frameHandler)
        {
            Format = format;
        }

        public int AddFrames(Span<float> frames)
        {
            return _frameHandler?.Invoke(frames) ?? 0;
        }
    }
}
