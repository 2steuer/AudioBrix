using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Bricks.Active;
using AudioBrix.Interfaces;
using AudioBrix.Material;
using AudioBrix.SipSorcery.Helpers;
using Microsoft.Extensions.Logging;
using SIPSorceryMedia.Abstractions;
using AudioFormat = SIPSorceryMedia.Abstractions.AudioFormat;

namespace AudioBrix.SipSorcery
{
    public class AudioBrixSource : IAudioSource
    {
        public event EventHandler<EventArgs>? OnStart;

        public event EventHandler<EventArgs>? OnResume;

        public event EventHandler<EventArgs>? OnPause;

        public event EventHandler<EventArgs>? OnStop;

        public event EventHandler<FormatChangedEventArgs>? OnFormatChanged;

        public event EncodedSampleDelegate? OnAudioSourceEncodedSample;

        public event RawAudioSampleDelegate? OnAudioSourceRawSample;
        public event SourceErrorDelegate? OnAudioSourceError;

        private IAudioEncoder _encoder;

        private MediaFormatManager<AudioFormat> _formatManager;

        public bool IsActive { get; private set; } = false;

        public bool IsPaused { get; private set; } = false;

        public IFrameSource? Source
        {
            get => _motor.Source;
            set => _motor.Source = value;
        }

        public TimeSpan SourceQueryInterval
        {
            get => _motor.QueryInterval;
            set => _motor.QueryInterval = value;
        }

        public double PackageSizeSeconds { get; set; } = 0.02;

        private MotorBrick _motor = new MotorBrick();
        
        public AudioBrixSource(IAudioEncoder encoder)
        {
            _encoder = encoder;
            _formatManager = new MediaFormatManager<AudioFormat>(_encoder.SupportedFormats);
        }

        public Task PauseAudio()
        {
            OnPause?.Invoke(this, EventArgs.Empty);
            _motor.Stop();
            IsPaused = true;
            return Task.CompletedTask;
        }

        public Task ResumeAudio()
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            _motor.Start();
            IsPaused = false;
            return Task.CompletedTask;
        }

        public Task StartAudio()
        {
            var fmt = new Material.AudioFormat(_formatManager.SelectedFormat.RtpClockRate,
                _formatManager.SelectedFormat.ChannelCount);

            _motor.Sink = new DelegateFrameSink(fmt, FrameHandler);

            _motor.QueryFrameCount = (int)(fmt.SampleRate * PackageSizeSeconds);

            _motor.Start();

            OnStart?.Invoke(this, EventArgs.Empty);

            IsActive = true;
            IsPaused = false;
            return Task.CompletedTask;
        }

        private int FrameHandler(Span<float> frames)
        {
            uint frameCount = (uint)(frames.Length / _formatManager.SelectedFormat.ChannelCount);

            short[] pcm = new short[frames.Length];
            for (int i = 0; i < pcm.Length; i++)
            {
                pcm[i] = SampleConverter.GetPcm(frames[i]);
            }

            var encoded = _encoder.EncodeAudio(pcm, _formatManager.SelectedFormat);

            OnAudioSourceEncodedSample?.Invoke(frameCount, encoded);

            return frames.Length;
        }

        public Task CloseAudio()
        {
            _motor.Stop();
            OnStop?.Invoke(this, EventArgs.Empty);

            IsActive = false;
            IsPaused = false;
            return Task.CompletedTask;
        }

        public List<AudioFormat> GetAudioSourceFormats() => _formatManager.GetSourceFormats();
        public void SetAudioSourceFormat(AudioFormat audioFormat)
        {
            _formatManager.SetSelectedFormat(audioFormat);
            _motor.Sink = null;
            OnFormatChanged?.Invoke(this, new FormatChangedEventArgs(audioFormat));
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter) => _formatManager.RestrictFormats(filter);

        public void ExternalAudioSourceRawSample(AudioSamplingRatesEnum samplingRate, uint durationMilliseconds, short[] sample)
        {
            throw new NotImplementedException();
        }

        public bool HasEncodedAudioSubscribers() => OnAudioSourceEncodedSample != null;

        public bool IsAudioSourcePaused() => IsPaused;

    }
}
