using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using SIPSorceryMedia.Abstractions;

namespace AudioBrix.SipSorcery
{
    public class AudioBrixAudio
    {
        private AudioBrixSink _sink;
        private AudioBrixSource _source;

        public event EventHandler<FormatChangedEventArgs>? OnSourceFormatChanged;
        public event EventHandler<FormatChangedEventArgs>? OnSinkFormatChanged;

        public event EventHandler<EventArgs>? OnStart;
        public event EventHandler<EventArgs>? OnStop;
        public event EventHandler<EventArgs>? OnPause;
        public event EventHandler<EventArgs>? OnResume;

        public IFrameSource? Source
        {
            get => _source.Source;
            set => _source.Source = value;
        }

        public IFrameSink? Sink
        {
            get => _sink.Sink;
            set => _sink.Sink = value;
        }

        public AudioBrixAudio(IAudioEncoder encoder)
        {
            _sink = new AudioBrixSink(encoder);
            _sink.OnFormatChanged += _sink_OnFormatChanged;

            _source = new AudioBrixSource(encoder);

            _source.OnStart += _source_OnStart;
            _source.OnStop += _source_OnStop;
            _source.OnPause += _source_OnPause;
            _source.OnResume += _source_OnResume;

            _source.OnFormatChanged += _source_OnFormatChanged;
        }

        private void _source_OnFormatChanged(object? sender, FormatChangedEventArgs e)
        {
            OnSourceFormatChanged?.Invoke(this, e);
        }

        private void _sink_OnFormatChanged(object? sender, FormatChangedEventArgs e)
        {
            OnSinkFormatChanged?.Invoke(this, e);
        }

        private void _source_OnResume(object? sender, EventArgs e)
        {
            _sink.ResumeAudioSink();
            OnResume?.Invoke(this, EventArgs.Empty);
        }

        private void _source_OnPause(object? sender, EventArgs e)
        {
            _sink.PauseAudioSink();
            OnPause?.Invoke(this, EventArgs.Empty);
        }

        private void _source_OnStop(object? sender, EventArgs e)
        {
            _sink.CloseAudioSink();
            OnStop?.Invoke(this, EventArgs.Empty);
        }

        private void _source_OnStart(object? sender, EventArgs e)
        {
            _sink.StartAudioSink();
            OnStart?.Invoke(this, EventArgs.Empty);
        }

        public void SetSourceLatency(TimeSpan latency)
        {
            _source.PackageSize = latency;
            _source.SourceQueryInterval = latency;
        }

        public MediaEndPoints ToMediaEndpoints()
        {
            return new MediaEndPoints()
            {
                AudioSink = _sink,
                AudioSource = _source
            };
        }
    }
}
