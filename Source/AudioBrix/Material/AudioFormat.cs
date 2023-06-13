namespace AudioBrix.Material
{
    public struct AudioFormat
    {
        public double SampleRate { get; set; }

        public int Channels { get; set; }

        public AudioFormat()
        {

        }

        public AudioFormat(double sampleRate, int channelCount)
        {
            SampleRate = sampleRate;
            Channels = channelCount;
        }

        public override int GetHashCode()
        {
            return (int)(Channels * SampleRate);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not AudioFormat other)
            {
                return false;
            }

            return other.Channels == Channels && other.SampleRate == SampleRate;
        }

        public override string ToString()
        {
            return $"{Channels} ch @ {SampleRate} Hz";
        }
    }
}
