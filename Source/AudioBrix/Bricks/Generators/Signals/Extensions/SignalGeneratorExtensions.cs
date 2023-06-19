using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Bricks.Generators.Signals.Extensions
{
    public static class SignalGeneratorExtensions
    {
        public static Combined Add(this ISignalGenerator a, ISignalGenerator b)
        {
            return new Combined(a, b, (f, f1) => f + f1);
        }

        public static Combined Substract(this ISignalGenerator a, ISignalGenerator b)
        {
            return new Combined(a, b, (f, f1) => f - f1);
        }

        public static Combined Multiply(this ISignalGenerator a, ISignalGenerator b)
        {
            return new Combined(a, b, (f, f1) => f * f1);
        }
    }
}
