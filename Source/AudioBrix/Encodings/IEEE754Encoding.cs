using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AudioBrix.Interfaces;

namespace AudioBrix.Encodings
{
    public class Ieee754Encoding : IEncoder, IDecoder
    {
        private static Ieee754Encoding _instance = new Ieee754Encoding();
        public static Ieee754Encoding Instance => _instance;

        private Ieee754Encoding()
        {

        }

        public int Encode(Span<float> frames, byte[] buffer, int offset, int maxLength)
        {
            var sp = MemoryMarshal.Cast<float, byte>(frames);
            var bsp = new Span<byte>(buffer, offset, maxLength);

            sp.Slice(0, bsp.Length).CopyTo(sp);

            return bsp.Length;
        }

        public Span<byte> Encode(Span<float> frames)
        {
            return MemoryMarshal.Cast<float, byte>(frames);
        }

        public Span<float> Decode(byte[] buffer, int offset, int length)
        {
            var inSpan = new Span<byte>(buffer, offset, length);
            return Decode(inSpan);
        }

        public Span<float> Decode(Span<byte> data)
        {
            return MemoryMarshal.Cast<byte, float>(data);
        }
    }
}
