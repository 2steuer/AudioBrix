using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using NAudio.Wave;

namespace AudioBrix.NAudio
{
    public static class SampleProviderExtensions
    {
        public static IFrameSource ToFrameSource(this ISampleProvider sampleProvider)
        {
            return new SampleProviderFrameSource(sampleProvider);
        }
    }
}
