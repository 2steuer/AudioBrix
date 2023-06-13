using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioBrix.Interfaces
{
    public interface IEncoder
    {
        int Encode(Span<float> frames, byte[] buffer, int offset, int maxLength);

        Span<byte> Encode(Span<float> frames);
    }
}
