using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public class Combined : ISignalGenerator
    {
        private ISignalGenerator _a;
        private ISignalGenerator _b;

        private readonly Func<float, float, float> _operator;

        public Combined(ISignalGenerator a, ISignalGenerator b, Func<float, float, float> @operator)
        {
            _a = a;
            _b = b;
            _operator = @operator;
        }

        public void Initialize(double sampleRate)
        {
            _a.Initialize(sampleRate);
            _b.Initialize(sampleRate);
        }

        public IEnumerable<float> Get(int count)
        {
            return _a.Get(count).Zip(_b.Get(count))
                .Select(d => _operator(d.First, d.Second));
        }
    }
}
