﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Material;

namespace AudioBrix.Bricks.Generators.Signals
{
    public interface IWaveFormGenerator : ISignalGenerator
    {
        double Frequency { get; set; }

        double Amplitude { get; set; }
    }
}
