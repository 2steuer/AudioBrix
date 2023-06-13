using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Interfaces
{
    internal interface IDecoder
    {
        Span<float> Decode(byte[] buffer, int offset, int length);

        Span<float> Decode(Span<byte> data);
    }
}
