using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public class Saw : IWaveFormGenerator
    {
        public double Frequency { get; set; }

        public double Amplitude { get; set; } = 1;


        /// <summary>
        /// Is true, the saw has a rising slope, if false,
        /// the saw has a falling slope.
        /// </summary>
        public bool Rising { get; set; }

        private double _sampleRate;

        private int _sampleCounter;

        public Saw()
        {

        }

        public Saw(double frequency, double amplitude = 1)
        {
            Frequency = frequency;
            Amplitude = amplitude;
        }

        public void Initialize(double sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public IEnumerable<float> Get(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var tf = (_sampleCounter / _sampleRate) * Frequency;

                var s = 2 * (tf - Math.Floor(0.5 + tf));
                s *= Amplitude;

                yield return (float)s;

                _sampleCounter++;
            }
        }
    }
}
