﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;
using AudioBrix.SipSorcery.Helpers;
using SIPSorceryMedia.Abstractions;

namespace AudioBrix.SipSorcery
{
    public class FormatChangedEventArgs : EventArgs
    {
        public AudioBrix.Material.AudioFormat NewFormat { get; }

        public FormatChangedEventArgs(AudioBrix.Material.AudioFormat newFormat)
        {
            NewFormat = newFormat;
        }
    }

    public class AudioBrixSink : IAudioSink
    {
        public event EventHandler<EventArgs>? OnStart;

        public event EventHandler<EventArgs>? OnResume;

        public event EventHandler<EventArgs>? OnPause;

        public event EventHandler<EventArgs>? OnStop;

        public event EventHandler<FormatChangedEventArgs>? OnFormatChanged; 

        public event SourceErrorDelegate? OnAudioSinkError;

        private IAudioEncoder _encoder;

        private MediaFormatManager<AudioFormat> _formatManager;

        private IFrameSink? _sink = null;

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
                    _sink = value;
                }
            }
        }

        public AudioBrixSink(IAudioEncoder encoder)
        {
            _encoder = encoder;
            _formatManager = new MediaFormatManager<AudioFormat>(_encoder.SupportedFormats);
        }

        public List<AudioFormat> GetAudioSinkFormats() => _formatManager.GetSourceFormats();
        public void SetAudioSinkFormat(AudioFormat audioFormat)
        {
            _formatManager.SetSelectedFormat(audioFormat);
            OnFormatChanged?.Invoke(this, new FormatChangedEventArgs(new Material.AudioFormat(audioFormat.ClockRate, audioFormat.ChannelCount)));
        }

        public void RestrictFormats(Func<AudioFormat, bool> filter) => _formatManager.RestrictFormats(filter);


        public void GotAudioRtp(IPEndPoint remoteEndPoint, uint ssrc, uint seqnum, uint timestamp, int payloadID, bool marker,
            byte[] payload)
        {
            var pcm = _encoder.DecodeAudio(payload, _formatManager.SelectedFormat);

            if (pcm == null)
            {
                return;
            }

            IFrameSink? mysink = Sink; // Thread-Safe copy!

            if (mysink == null)
            {
                return;
            }

            float[] samples = new float[pcm.Length];

            for (int i = 0; i < pcm.Length; i++)
            {
                samples[i] = SampleConverter.GetSample(pcm[i]);
            }

            mysink.AddFrames(samples);
        }


        public Task PauseAudioSink()
        {
            OnPause?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task ResumeAudioSink()
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task StartAudioSink()
        {
            OnStart?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task CloseAudioSink()
        {
            OnStop?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

    }
}
