using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public TimeSpan QueryInterval { get; set; } = TimeSpan.FromSeconds(0);

        private bool _running = false;

        private AutoResetEvent _sourceSetEvent = new AutoResetEvent(false);

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
                if (Sink != null && value != null)
                {
                    Sink.Format.ThrowIfNotEqual(value.Format);
                }

                lock (this)
                {
                    var sourceBefore = _source;
                    _source = value;

                    if (sourceBefore == null && _source != null)
                    {
                        _sourceSetEvent.Set();
                    }
                    else if (sourceBefore != null && _source == null)

                        _sourceSetEvent.Reset();
                }
            }
        }

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
                if (Source != null && value != null)
                {
                    Source.Format.ThrowIfNotEqual(value.Format);
                }

                lock (this)
                {
                    _sink = value;
                }
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
            StartWhile:
            while (!_cancelTokenSource!.Token.IsCancellationRequested)
            {
                Stopwatch sw = Stopwatch.StartNew();

                var mySource = Source; // Thread-Safe copy!

                if (mySource == null)
                {
                    _sourceSetEvent.WaitOne(TimeSpan.FromMilliseconds(16));
                    // this waits for at most 16ms and then goes to the top of the while loop
                    // 1. if the source is then set, hooray, we skip this here and read samples
                    // 2. if the source is still not set, we get here again
                    // 3. if the motor is cancelled, we fall through the while condition and end the motor
                    goto StartWhile;
                }

                var input = mySource.GetFrames(QueryFrameCount);

                if (input.Length == 0)
                {
                    goto End; // the source is done, e.g. file is over
                }

                Sink?.AddFrames(input);

                while (sw.Elapsed < QueryInterval && !_cancelTokenSource!.Token.IsCancellationRequested)
                {
                    Thread.Yield(); // do nothing until the query interval is elapsed
                }
            }

            End:
            _runnerThread = null;
            _cancelTokenSource = null;
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
        }

        public void Abort()
        {
            Stop();
        }
    }
}
