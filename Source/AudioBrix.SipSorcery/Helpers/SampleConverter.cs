using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.SipSorcery.Helpers
{
    internal class SampleConverter
    {
        public static float GetSample(short sample)
        {
            return (float)(sample / (float)short.MaxValue);
        }

        public static short GetPcm(float sample)
        {
            float clipped = Math.Max(-1f, Math.Min(sample, 1f));
            return (short)(short.MaxValue * clipped);
        }
    }
}
