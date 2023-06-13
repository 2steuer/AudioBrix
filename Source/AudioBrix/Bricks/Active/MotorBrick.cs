using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;

namespace AudioBrix.Bricks.Active
{
    public class MotorBrick
    {
        private IFrameSource _source;
        private IFrameSink _sink;

        private bool _running;

        public MotorBrick(IFrameSource source, IFrameSink sink)
        {
            _source = source;
            _sink = sink;
        }
    }
}
