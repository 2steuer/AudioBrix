using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic
{
    public class Length : IFrameSource
    {
        private int _lengthSamples;

        private int _samplesRemaining = int.MaxValue;

        public AudioFormat Format { get; }

        public TimeSpan AudioLength
        {
            get => TimeSpan.FromSeconds(_lengthSamples / Format.SampleRate);
            set
            {
                _lengthSamples = (int)(value.TotalSeconds * Format.SampleRate);

                if (_samplesRemaining > _lengthSamples)
                {
                    _samplesRemaining = _lengthSamples;
                }
            }
        }

        public IFrameSource? Source { get; set; }

        public Length(AudioFormat format, TimeSpan audioLength, IFrameSource? source = null)
        {
            Format = format;
            AudioLength = audioLength;
            Source = source;
        }

        public Span<float> GetFrames(int frameCount)
        {
            if (Source == null || _samplesRemaining == 0)
            {
                return Span<float>.Empty;
            }

            int samplesHere = Math.Min(frameCount, _samplesRemaining);

            var ret = Source.GetFrames(samplesHere);

            _samplesRemaining -= (ret.Length / Format.Channels);

            return ret;
        }
    }
}
