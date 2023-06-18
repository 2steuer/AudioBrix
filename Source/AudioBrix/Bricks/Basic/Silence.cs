using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic
{
    public class Silence : IFrameSource
    {
        public AudioFormat Format { get; }

        public Silence(AudioFormat format)
        {
            Format = format;
        }

        public Span<float> GetFrames(int frameCount)
        {
            return new Span<float>(new float[frameCount * Format.Channels]);
        }
    }
}
