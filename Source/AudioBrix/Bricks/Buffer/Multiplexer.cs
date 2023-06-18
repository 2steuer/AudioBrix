using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Buffer
{
    /// <summary>
    /// Multiplexes frames passed to <see cref="AddFrames"/> to
    /// a number of zero or more endpoints, which are <see cref="IFrameSink"/> by itself.
    ///
    /// Use <see cref="CreateEndPoint"/> to create a <see cref="Buffer"/> backed end point or
    /// use <see cref="AddEndpoint"/> to add an endpoint of the <see cref="IFrameSink"/> implementation
    /// of your choice.
    ///
    /// This class is useful when you want to use with a signal on multiple branches of the
    /// processing pipeline. E.g. one branch for analyzing, visualizing or recording the audio, one branch
    /// to process it for output.
    /// </summary>
    internal class Multiplexer : IFrameSink
    {
        public AudioFormat Format { get; }

        private List<IFrameSink> _endPoints = new();

        public Multiplexer(AudioFormat format)
        {
            Format = format;
        }

        /// <summary>
        /// Creates a new endpoint from which the frames can be queried.
        ///
        /// Every endpoint is a <see cref="Buffer"/>.
        /// </summary>
        /// <returns>The newly created endpoint.</returns>
        public IFrameSource CreateEndPoint(TimeSpan capacity)
        {
            var newEp = new Buffer(Format, (int) (Format.SampleRate * capacity.TotalSeconds));

            AddEndpoint(newEp);

            return newEp;
        }

        /// <summary>
        /// Creates a new endpoint to the multiplexer with the default buffer capacity.
        /// See <see cref="DefaultCapacity"/>.
        /// </summary>
        public IFrameSource CreateEndpoint() => CreateEndPoint(DefaultCapacity);

        /// <summary>
        /// Adds the given <see cref="IFrameSink"/> as an endpoint of the multiplexer.
        /// </summary>
        public void AddEndpoint(IFrameSink endpoint)
        {
            Format.ThrowIfNotEqual(endpoint.Format);

            lock (_endPoints)
            {
                _endPoints.Add(endpoint);
            }
        }

        public void RemoveEndpoint(IFrameSink endpoint)
        {
            lock (_endPoints)
            {
                _endPoints.RemoveAll(e => e == endpoint);
            }
        }

        /// <summary>
        /// Adds the given frames to all endpoints.
        /// </summary>
        public int AddFrames(Span<float> frames)
        {
            List<IFrameSink> sinks;

            lock (_endPoints)
            {
                sinks = _endPoints.ToList();
            }

            foreach (var frameSink in sinks)
            {
                frameSink.AddFrames(frames);
            }

            return frames.Length;
        }

        public static readonly TimeSpan DefaultCapacity = TimeSpan.FromSeconds(2);
    }
}
