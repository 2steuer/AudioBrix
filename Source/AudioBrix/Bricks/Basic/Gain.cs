using System;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic
{
    public class Gain : IFrameSource
    {
        public AudioFormat Format { get; }

        private IFrameSource _src;

        public float GainValue { get; set; }

        public Gain(AudioFormat format, float gain, IFrameSource src)
        {
            _src = src;
            Format = format;

            GainValue = gain;
        }

        public Span<float> GetFrames(int frameCount)
        {
            var src = _src.GetFrames(frameCount);

            for (int i = 0; i < src.Length; i++)
            {
                src[i] = src[i] * GainValue;
            }

            return src;
        }
    }
}
