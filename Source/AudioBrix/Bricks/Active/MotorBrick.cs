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

        private IFrameSource _source;
        private IFrameSink _sink;

        public int QueryFrameCount { get; set; }

        private bool _running = false;

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
                var input = _source.GetFrames(QueryFrameCount);

                if (input.Length == 0)
                {
                    goto End; // the source is done, e.g. file is over
                }

                _sink.AddFrames(input);
            }

            End:
            OnFinished?.Invoke(this, EventArgs.Empty);
            _runnerThread = null;
            Running = false;
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
