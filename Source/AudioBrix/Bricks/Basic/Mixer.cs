using System;
using System.ComponentModel;
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
            Format.ThrowIfNotEqual(source.Format);

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
            List<IFrameSource> sourcesCopy;

            lock (this)
            {
                sourcesCopy = _sources.ToList();
            }

            if (sourcesCopy.Count == 0)
            {
                return Span<float>.Empty; // for performance reasong, better than 
                                            // allocating the whole array below and
                                            // not iterating anything in the foreach loop.
            }
            else if (sourcesCopy.Count == 1)
            {
                // if there is only one source, we can easily pass the result on...
                var frms = sourcesCopy[0].GetFrames(frameCount);


                if (frms.Length == 0)
                {
                    RemoveSource(sourcesCopy[0]);
                }

                return frms;
            }

            var fa = new Span<float>(new float[frameCount * Format.Channels]);

            int maxLength = 0;

            foreach (var source in sourcesCopy)
            {
                var sa = source.GetFrames(frameCount);

                for (int i = 0; i < sa.Length; i++)
                {
                    fa[i] += sa[i];
                }

                maxLength = Math.Max(maxLength, sa.Length);

                if (sa.Length == 0)
                {
                    // source ended, remove it now
                    // so it will not be queried on the next run anymore
                    RemoveSource(source);
                }
            }

            return fa.Slice(0, maxLength);
        }
    }
}
