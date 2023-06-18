using System;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Generators
{
    public class SineWave : IFrameSource
    {
        public AudioFormat Format { get; }

        private int _phase = 0;

        public double Frequency { get; set; }

        public SineWave(AudioFormat format, double frequency)
        {
            Format = format;

            Frequency = frequency;
        }

        public Span<float> GetFrames(int frameCount)
        {
            var d = new float[frameCount * Format.Channels];

            for (int i = 0; i < frameCount; i++)
            {
                var s = (float)Math.Sin(2 * Math.PI * Frequency * (_phase / Format.SampleRate));

                for (int ch = 0; ch < Format.Channels; ch++)
                {
                    d[i * Format.Channels + ch] = s;
                }

                _phase++;
            }

            return new Span<float>(d);
        }
    }
}
