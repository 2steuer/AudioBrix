using System;
using AudioBrix.Material;

namespace AudioBrix.Interfaces
{
    public interface IFrameSink
    {
        /// <summary>
        /// Audio format of the sink.
        /// </summary>
        AudioFormat Format { get; }

        /// <summary>
        /// Adds frames to the audio sink.
        /// </summary>
        /// <param name="frames">The frames that shall be added to the sink.</param>
        /// <returns>The number of frames added to the sink. Might be less than the number of frames passed.</returns>
        int AddFrames(Span<float> frames);
    }
}
