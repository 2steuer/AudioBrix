using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public class Sine : IWaveFormGenerator
    {
        public double Frequency { get; set; }

        public double Amplitude { get; set; } = 1;

        private double _sampleRate;

        private int _sampleCounter;

        public Sine(double frequency, double amplitude = 1)
        {
            Frequency = frequency;
            Amplitude = amplitude;
        }

        public Sine()
        {

        }

        public void Initialize(double sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public IEnumerable<float> Get(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var s = (float)(Amplitude * Math.Sin(2 * Math.PI * Frequency * (_sampleCounter / _sampleRate)));

                yield return s;

                _sampleCounter++;
            }
        }
    }
}
