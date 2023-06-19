using System;
using AudioBrix.Bricks.Generators.Signals;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Generators
{
    public class WaveForm<TSignal> : GeneratedSignal<TSignal>
        where TSignal: class, IWaveFormGenerator, new()
    {
        public WaveForm(AudioFormat format, double frequency, double amplitude = 1)
            :base(format, new TSignal())
        {
            Generator.Frequency = frequency;
            Generator.Amplitude = amplitude;
        }

        public WaveForm(AudioFormat format, TSignal generator)
        : base(format, generator)
        {

        }
    }
}
