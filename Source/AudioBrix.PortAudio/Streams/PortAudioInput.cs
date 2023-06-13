using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using PortAudio.Net;

namespace AudioBrix.PortAudio.Streams
{
    public class PortAudioInput : PortAudioStreamBase
    {
        private IFrameSink? _sink;

        public IFrameSink? Sink
        {
            get
            {
                lock (this)
                {
                    return _sink;
                }
            }
            set
            {
                lock (this)
                {
                    if (value != null && !Format.Equals(value.Format))
                    {
                        throw new InvalidOperationException($"Trief to add a sink with the wrong audio format! Have: {Format}, got {value.Format}");
                    }

                    _sink = value;
                }
            }
        }

        public PortAudioInput(PaHostApiTypeId hostApi, int hostApiDeviceIndex, double sampleRate, int channelCount, double suggestedLatency)
            : base(false, hostApi, hostApiDeviceIndex, PaSampleFormat.paFloat32, sampleRate, channelCount, suggestedLatency)
        {
        }

        protected override PaStreamCallbackResult StreamCallback(PaBuffer input, PaBuffer output, int framecount,
            PaStreamCallbackTimeInfo timeinfo, PaStreamCallbackFlags statusflags, object userdata)
        {
            IFrameSink? sink;

            lock (this)
            {
                sink = Sink;
            }

            if (sink == null)
            {
                return PaStreamCallbackResult.paContinue;
            }

            var fbuf = (PaBuffer<float>)input;

            sink.AddFrames(fbuf.Span);

            return PaStreamCallbackResult.paContinue;
        }
    }
}
