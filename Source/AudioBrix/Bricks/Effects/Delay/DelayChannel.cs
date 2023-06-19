using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Effects.Delay
{
    internal class DelayChannel
    {
        public TimeSpan DelayLength
        {
            get => TimeSpan.FromSeconds(_delaySamples / _sampleRate);
            set => _delaySamples = (int) (_sampleRate * value.TotalSeconds);
        }

        public double Feedback { get; set; }

        public double DryMix { get; set; }

        public double WetMix { get; set; }

        private float[] _buffer;
        private double _sampleRate;
        private int _delaySamples;

        private int _currentIndex = 0;

        public DelayChannel(double sampleRate, TimeSpan bufferLength, TimeSpan delayLength, double feedback,
            double dryMix, double wetMix)
        {
            _sampleRate = sampleRate;
            Feedback = feedback;
            DryMix = dryMix;
            WetMix = wetMix;

            _buffer = new float[(int)Math.Ceiling(_sampleRate * bufferLength.TotalSeconds)];
            _currentIndex = 0;

            DelayLength = delayLength;
        }

        public float ProcessSample(float sample)
        {
            float delayed = _buffer[_currentIndex];

            float rv = (float) (delayed * WetMix + sample * DryMix);

            _buffer[_currentIndex] = (float) (Feedback * (delayed + sample));

            _currentIndex = (_currentIndex + 1) % _delaySamples;

            return rv;
        }
    }
}
