using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    /// <summary>
    /// From https://thewolfsound.com/sine-saw-square-triangle-pulse-basic-waveforms-in-synthesis/#triangle
    /// </summary>
    public class Triangle : IWaveFormGenerator
    {
        public double Frequency { get; set; }

        public double Amplitude { get; set; } = 1;

        private double _sampleRate;

        private int _sampleCounter;

        public Triangle(double frequency, double amplitude = 1)
        {
            Frequency = frequency;
            Amplitude = amplitude;
        }

        public Triangle()
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
                var ft = Frequency * (_sampleCounter / _sampleRate);

                var s = 4 * Math.Abs(ft - Math.Floor(ft + 0.5)) - 1;
                s *= Amplitude;

                yield return (float)s;

                _sampleCounter++;
            }
        }
    }
}
