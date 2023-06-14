using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;

namespace AudioBrix.Bricks.Active
{
    public class MotorBrick : IStartStopBrick
    {
        public event EventHandler<EventArgs>? OnFinished;

        private IFrameSource? _source;
        private IFrameSink? _sink;

        public int QueryFrameCount { get; set; }

        private bool _running = false;

        public IFrameSource? Source
        {
            get => _source;
            set
            {
                if (Sink != null && value != null)
                {
                    Sink.Format.ThrowIfNotEqual(value.Format);
                }
                _source = value;
            }
        }

        public IFrameSink? Sink
        {
            get => _sink;
            set
            {
                if (Source != null && value != null)
                {
                    Source.Format.ThrowIfNotEqual(value.Format);
                }
                _sink = value;
            }
        }

        public bool Running
        {
            get
            {
                lock (this)
                {
                    return _running;
                }
            }
            private set
            {
                lock (this)
                {
                    _running = value;
                }
            }
        }

        private CancellationTokenSource? _cancelTokenSource = null;

        private Thread? _runnerThread = null;

        public MotorBrick()
        {
            QueryFrameCount = 20; // just any non-zero value, actually.
        }

        public MotorBrick(IFrameSource source, IFrameSink sink, int queryFrameCount)
        {
            _source = source;
            _sink = sink;
            QueryFrameCount = queryFrameCount;
        }

        private void Runner()
        {
            Running = true;

            // cancellation token source is set before starting the thread
            while (!_cancelTokenSource!.Token.IsCancellationRequested)
            {
                if (_source == null)
                {
                    goto End;
                }

                var input = _source.GetFrames(QueryFrameCount);

                if (input.Length == 0)
                {
                    goto End; // the source is done, e.g. file is over
                }

                _sink?.AddFrames(input);
            }

            End:
            _runnerThread = null;
            Running = false;
            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            if (Running)
            {
                return;
                // TODO ERROR?
            }

            _cancelTokenSource = new CancellationTokenSource();

            _runnerThread = new Thread(Runner);
            _runnerThread.IsBackground = true;
            _runnerThread.Start();
        }

        public void Stop()
        {
            if (!Running)
            {
                // TODO ERROR?
                return;
            }

            if (_cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
            {
                _cancelTokenSource.Cancel();
            }

            _cancelTokenSource = null;
        }

        public void Abort()
        {
            Stop();
        }
    }
}
