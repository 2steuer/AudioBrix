using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Material;
using AudioBrix.PortAudio.Helper;
using PortAudio.Net;

namespace AudioBrix.PortAudio.Streams
{
    public class PortAudioStreamBase : IDisposable
    {
        private readonly PaLibrary _pa;
        private PaStream _stream = null;

        private readonly bool _output;
        private readonly PaStreamParameters _params;
        private readonly double _sampleRate;

        public bool IsRunning { get; private set; } = false;

        public AudioFormat Format { get; }

        public PortAudioStreamBase(bool output, PaHostApiTypeId hostApi, int hostApiDeviceIndex, PaSampleFormat sampleFormat, double sampleRate, int channelCount, double suggestedLatency)
        {
            _pa = PaLibrary.Initialize();

            var ha = PortAudioHelper.GetHostApiIndexAndInfo(_pa, hostApi);
            var devInfo = PortAudioHelper.GetDeviceInfoInternal(_pa, hostApi, hostApiDeviceIndex);

            if (output && devInfo.maxOutputChannels == 0)
            {
                throw new ArgumentException(
                    $"Tried to create an output stream, but the given device does not have output channels.",
                    nameof(output));
            }
            else if (!output && devInfo.maxInputChannels == 0)
            {
                throw new ArgumentException(
                    $"Tried to create an input stream, but the given device does not have input channels.",
                    nameof(output));
            }

            if (!PortAudioHelper.CheckFormatInternal(_pa, hostApi, hostApiDeviceIndex, sampleRate, sampleFormat,
                    channelCount, output))
            {
                throw new InvalidOperationException($"The given stream parameters are invalid!");
            }

            _sampleRate = sampleRate;

            _params = new PaStreamParameters()
            {
                channelCount = channelCount,
                device = _pa.HostApiDeviceIndexToDeviceIndex(ha.index, hostApiDeviceIndex),
                sampleFormat = sampleFormat,
                suggestedLatency = suggestedLatency
            };
            _output = output;

            Format = new AudioFormat(_sampleRate, _params.channelCount);
        }

        public void Start()
        {
            // Minimum of 20ms per buffer
            var framesPerBuffer = (int) (_sampleRate * Math.Max(_params.suggestedLatency, 0.02));

            _stream = _pa.OpenStream(_output ? null : _params, _output ? _params : null, _sampleRate, framesPerBuffer,
                PaStreamFlags.paNoFlag, StreamCallback, null);

            _stream.SetStreamFinishedCallback(StreamFinishedCallback, null);
            
            _stream.StartStream();

            lock (this)
            {
                IsRunning = true;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (!IsRunning)
                {
                    return;
                }
            }

            _stream.StopStream();
        }

        private void StreamFinishedCallback(object userdata)
        {
            lock (this)
            {
                IsRunning = false;
            }
        }

        protected virtual PaStreamCallbackResult StreamCallback(PaBuffer input, PaBuffer output, int framecount, PaStreamCallbackTimeInfo timeinfo, PaStreamCallbackFlags statusflags, object userdata)
        {
            return PaStreamCallbackResult.paContinue;
        }

        public void Dispose()
        {
            _pa.Dispose();
        }
    }
}
