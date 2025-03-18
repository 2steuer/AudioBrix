// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

List<(double I, double Q)> iqsamples = new List<(double I, double Q)>() {
    (1, 1),
    (-1, 1),
    (-1, -1),
    (1, -1)
};

List<double> samples = new();

int samplesPerBit = 100;

int sampleRate = 32000;

double frequency = 1000;

int sampleCounter = 0;

foreach(var iq in iqsamples)
{
    for (int i = 0; i < samplesPerBit; i++)
    {
        double t = (double)sampleCounter / sampleRate;

        samples.Add(iq.I * Math.Cos(2 * Math.PI * frequency * t) +
                            iq.Q * Math.Sin(2 * Math.PI * frequency * t));

        sampleCounter++;
    }
}

double[] I = new double[samples.Count];
double[] Q = new double[samples.Count];

sampleCounter = 0;

for (int i = 0; i < samples.Count; i++)
{
    double t =  (double) sampleCounter / sampleRate;

    I[i] = samples[i] * Math.Cos(2 * Math.PI * frequency * t);
    Q[i] = samples[i] * Math.Sin(2 * Math.PI * frequency * t);
    sampleCounter++;
}

Q = LowPassFilter(Q, 100);
I = LowPassFilter(I, 100);

for (int i = 0; i < Q.Length; i++)
{
    Console.WriteLine($"{I[i]:F2}, {Q[i]:F2}");
}

ScottPlot.Plot p = new();
p.Title("Signal in Time Domain");
p.Add.Scatter(I, Q);
p.SavePng("signal.png", 800, 600);

static double[] LowPassFilter(double[] signal, int windowSize)
{
    double[] filtered = new double[signal.Length];
    for (int i = windowSize; i < signal.Length; i++)
    {
        double sum = 0;
        for (int j = 0; j < windowSize; j++)
            sum += signal[i - j];
        filtered[i] = sum / windowSize;
    }
    return filtered;
}
