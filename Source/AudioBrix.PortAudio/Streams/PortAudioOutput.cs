using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using PortAudio.Net;

namespace AudioBrix.PortAudio.Streams
{
    public class PortAudioOutput : PortAudioStreamBase
    {
        private IFrameSource? _source;

        public IFrameSource? Source
        {
            get
            {
                lock (this)
                {
                    return _source;
                }
            }
            set
            {
                if (value != null && !value.Format.Equals(Format))
                {
                    throw new InvalidOperationException(
                        $"Tried to add a source with the wrong format. Have {Format}, got {value.Format}");
                }

                lock (this)
                {
                    _source = value;
                }
            }
        }

        public PortAudioOutput(PaHostApiTypeId hostApi, int hostApiDeviceIndex, double sampleRate, int channelCount, double suggestedLatency)
            : base(true, hostApi, hostApiDeviceIndex, PaSampleFormat.paFloat32, sampleRate, channelCount, suggestedLatency)
        {
        }

        protected override PaStreamCallbackResult StreamCallback(PaBuffer input, PaBuffer output, int framecount,
            PaStreamCallbackTimeInfo timeinfo, PaStreamCallbackFlags statusflags, object userdata)
        {
            try
            {
                IFrameSource? source = null;

                lock (this)
                {
                    source = Source;
                }

                if (source == null)
                {
                    return PaStreamCallbackResult.paContinue; // output silence
                }

                var obuf = (PaBuffer<float>)output;

                var data = source.GetFrames(output.Frames);

                if (data.Length > 0)
                {
                    data.CopyTo(obuf.Span);
                    return IsRunning ? PaStreamCallbackResult.paContinue : PaStreamCallbackResult.paAbort;
                }
                else
                {
                    return PaStreamCallbackResult.paComplete;
                }


            }
            catch (Exception e)
            {
                // TODO: Better handling
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
