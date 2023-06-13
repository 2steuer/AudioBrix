using System;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Basic.Mixer
{
    public class Mixer : IFrameSource
    {
        public AudioFormat Format { get; }

        private List<IFrameSource> _sources = new();

        public Mixer(AudioFormat format, params IFrameSource[] sources)
        {
            Format = format;

            foreach (var frameSource in sources)
            {
                AddSource(frameSource);
            }
        }

        public void AddSource(IFrameSource source)
        {
            lock (this)
            {
                _sources.Add(source);
            }
        }

        public void RemoveSource(IFrameSource source)
        {
            lock (this)
            {
                _sources.RemoveAll(frameSource => frameSource == source);
            }
        }

        public void ClearSources()
        {
            lock (this)
            {
                _sources.Clear();
            }
        }

        public Span<float> GetFrames(int frameCount)
        {
            var fa = new Span<float>(new float[frameCount * Format.Channels]);

            List<IFrameSource> sourcesCopy;

            lock (this)
            {
                sourcesCopy = _sources.ToList();
            }

            foreach (var source in sourcesCopy)
            {
                var sa = source.GetFrames(frameCount);

                for (int i = 0; i < sa.Length; i++)
                {
                    fa[i] += sa[i];
                }

                if (sa.Length == 0)
                {
                    // source ended, remove it now
                    // so it will not be queried on the next run anymore
                    RemoveSource(source);
                }
            }

            return fa;
        }
    }
}
