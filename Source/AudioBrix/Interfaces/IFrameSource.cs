using System;
using AudioBrix.Material;

namespace AudioBrix.Interfaces
{
    public interface IFrameSource
    {
        /// <summary>
        /// The audio format of the source.
        /// </summary>
        AudioFormat Format { get; }

        /// <summary>
        /// Reads at most the given number of frames from the audio source.
        ///
        /// Behaviour in case of less frames available in the source than requested is not specified.
        /// Sources may either
        ///   - Fill the buffer with zeroes (silence) for the remaining duration
        ///   - Block until the given number of frames is available
        ///   - Just return the available number of frames (default behaviour)
        ///
        /// Returning an empty span means that the source has reached it's end (i.e. stopped streams or
        /// audio files that were played to their end). Meaning that if no samples are available at all,
        /// but the source is not finished (i.e. audio input stream), the source should block until at least one
        /// frame is available, if the source is not configured to fill remaining samples with zeroes.
        /// </summary>
        /// <param name="frameCount">Number of frames that are requested by the caller</param>
        /// <returns>Span containing at most the given number of frames</returns>
        Span<float> GetFrames(int frameCount);
    }
}
