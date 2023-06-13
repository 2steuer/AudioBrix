using System;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic
{
    public class Mixer : IFrameSource
    {
        public AudioFormat Format { get; }

        private IFrameSource[] _sources;

        public Mixer(double sampleRate, int channelCount, params IFrameSource[] sources)
        {
            Format = new AudioFormat()
            {
                SampleRate = sampleRate,
                Channels = channelCount
            };

            _sources = sources;
        }

        public Span<float> GetFrames(int frameCount)
        {
            var fa = new Span<float>(new float[frameCount * Format.Channels]);

            foreach (var source in _sources)
            {
                var sa = source.GetFrames(frameCount);

                for (int i = 0; i < sa.Length; i++)
                {
                    fa[i] += sa[i];
                }
            }

            return fa;
        }
    }
}
