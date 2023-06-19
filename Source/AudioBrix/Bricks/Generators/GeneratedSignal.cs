using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Bricks.Generators.Signals;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Generators
{
    public class GeneratedSignal<TGenerator> : IFrameSource
    where TGenerator : ISignalGenerator
    {
        public AudioFormat Format { get; }

        protected TGenerator Generator { get; }

        public GeneratedSignal(AudioFormat format, TGenerator generator)
        {
            Format = format;
            Generator = generator;

            Generator.Initialize(Format.SampleRate);

        }

        public Span<float> GetFrames(int frameCount)
        {
            float[] d = new float[frameCount * Format.Channels];

            int i = 0;

            foreach (var f in Generator.Get(frameCount))
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
