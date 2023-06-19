using System;
using AudioBrix.Bricks.Generators.Signals;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Generators
{
    public class WaveForm<TSignal> : IFrameSource
        where TSignal: IWaveFormGenerator, new()
    {
        public AudioFormat Format { get; }

        private TSignal _gen;

        public TSignal Generator => _gen;

        public WaveForm(AudioFormat format, double frequency, double amplitude = 1)
        {
            Format = format;
            _gen = new TSignal();
            _gen.Frequency = frequency;
            _gen.Amplitude = amplitude;
            _gen.Initialize(Format.SampleRate);
        }

        public WaveForm(AudioFormat format, TSignal generator)
        {
            Format = format;
            _gen = generator;
            _gen.Initialize(format.SampleRate);
        }

        public Span<float> GetFrames(int frameCount)
        {
            float[] d = new float[frameCount * Format.Channels];

            int i = 0;

            foreach (var f in _gen.Get(frameCount))
            {
                for (int ch = 0; ch < Format.Channels; ch++)
                {
                    d[i * Format.Channels + ch] = f;
                }

                i++;
            }

            return new Span<float>(d);
        }
    }
}
