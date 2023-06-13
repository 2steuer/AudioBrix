using AudioBrix.Interfaces;
using AudioBrix.Material;
using NAudio.Wave;

namespace AudioBrix.NAudio
{
    internal class SampleProviderFrameSource : IFrameSource
    {
        private ISampleProvider _sampleProvider;

        public AudioFormat Format { get; }

        internal SampleProviderFrameSource(ISampleProvider src)
        {
            Format = new AudioFormat(src.WaveFormat.SampleRate, src.WaveFormat.Channels);
            _sampleProvider = src;
        }

        public Span<float> GetFrames(int frameCount)
        {
            float[] buf = new float[frameCount * Format.Channels];

            var read = _sampleProvider.Read(buf, 0, frameCount * Format.Channels);

            return new Span<float>(buf, 0, read);
        }
    }
}