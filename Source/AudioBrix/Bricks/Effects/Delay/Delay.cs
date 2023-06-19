using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.Material;
using Microsoft.Win32;

namespace AudioBrix.Bricks.Effects.Delay
{
    public class Delay : IFrameSource
    {
        public AudioFormat Format { get; }

        private DelayChannel[] _channels;

        private IFrameSource _source;

        public Delay(AudioFormat format, IFrameSource source, TimeSpan delayLength, double feedback, double dryMix, double wetMix)
        {
            Format = format;

            _channels = new DelayChannel[format.Channels];
            for (int i = 0; i < format.Channels; i++)
            {
                _channels[i] = new DelayChannel(format.SampleRate, delayLength * 2, delayLength, feedback, dryMix,
                    wetMix);
            }

            _source = source;
        }

        public Span<float> GetFrames(int frameCount)
        {
            if (_source == null)
            {
                return Span<float>.Empty;
            }

            var v = _source.GetFrames(frameCount);

            for (int frame = 0; frame < v.Length / Format.Channels; frame++)
            {
                for (int channel = 0; channel < Format.Channels; channel++)
                {
                    int idx = frame * Format.Channels + channel;

                    v[idx] = _channels[channel].ProcessSample(v[idx]);
                }
            }

            return v;
        }
    }
}
