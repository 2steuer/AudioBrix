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

        public Gain(double samplerate, int channels, float gain, IFrameSource src)
        {
            _src = src;
            Format = new AudioFormat()
            {
                SampleRate = samplerate,
                Channels = channels
            };

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
