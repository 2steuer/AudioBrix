using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public class WhiteNoise : ISignalGenerator
    {
        public double Amplitude { get; set; } = 1;

        public WhiteNoise(double amplitude = 1)
        {
            Amplitude = amplitude;
        }

        public void Initialize(double sampleRate)
        {
            
        }

        public IEnumerable<float> Get(int count)
        {
            var r = new Random();

            for (int i = 0; i < count; i++)
            {
                yield return (float) ((r.NextSingle() - 0.5) * (Amplitude * 2));
            }
        }
    }
}
