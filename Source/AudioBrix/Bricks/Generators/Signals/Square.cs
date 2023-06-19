using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public class Square : IWaveFormGenerator
    {
        private Sine _sine;

        public double Frequency
        {
            get => _sine.Frequency;
            set => _sine.Frequency = value;
        }

        public double Amplitude
        {
            get => _sine.Amplitude;
            set => _sine.Amplitude = value;
        }

        public Square(double frequency, double amplitude)
        {
            _sine = new Sine(frequency, amplitude);
        }

        public Square()
        {
            _sine = new Sine();
        }

        public void Initialize(double sampleRate)
        {
            _sine.Initialize(sampleRate);
        }

        public IEnumerable<float> Get(int count)
        {
            foreach (float f in _sine.Get(count))
            {
                yield return (float)(Math.Sign(f) * _sine.Amplitude);
            }
        }
    }
}
