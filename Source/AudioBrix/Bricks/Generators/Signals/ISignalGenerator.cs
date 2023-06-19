using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals
{
    public interface ISignalGenerator
    {
        void Initialize(double sampleRate);

        IEnumerable<float> Get(int count);
    }
}
