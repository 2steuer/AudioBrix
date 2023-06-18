// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography.X509Certificates;
using AudioBrix.Bricks.Active;
using AudioBrix.Bricks.Basic;
using AudioBrix.Bricks.Basic.Mixer;
using AudioBrix.Bricks.Buffer;
using AudioBrix.Bricks.Generators;
using AudioBrix.Material;
using AudioBrix.PortAudio.Helper;
using AudioBrix.PortAudio.Streams;
using PortAudio.Net;

Console.WriteLine("Hello, World!");

Console.WriteLine(PaLibrary.VersionInfo.versionText);

int sampleRate = 24000;
int channels = 1;

var format = new AudioFormat(sampleRate, channels);

var buf = new BufferBrick(format, 96000);
buf.FillWithZero = false;
buf.WaitOnEmpty = true;

var buf2 = new BufferBrick(format, 96000);
buf2.FillWithZero = true;
buf2.WaitOnEmpty = false;

var motor = new MotorBrick(buf, buf2, (int)(format.SampleRate * 0.025));
motor.OnFinished += (sender, eventArgs) => Console.WriteLine("Motor finished!");
motor.Start();


var audioSignal = new Concatenate(format,
    new Length(format, TimeSpan.FromSeconds(2), new Silence(format)),
    new Length(format, TimeSpan.FromSeconds(2.8), new SineWave(format, 220)),
    new Length(format, TimeSpan.FromSeconds(0.8), new SineWave(format, 440)),
    new Length(format, TimeSpan.FromSeconds(0.8), new SineWave(format, 880)),
    new Length(format, TimeSpan.FromSeconds(0.8), new SineWave(format, 1760))
);

var ha = PortAudioHelper.GetDefaultHostApi();
var id = PortAudioHelper.GetDefaultInputDevice(ha);
var od = PortAudioHelper.GetDefaultOutputDevice(ha);

var input = new PortAudioInput(ha, id.index, sampleRate, channels, 0.025);
input.Sink = buf;

var output = new PortAudioOutput(ha, od.index, sampleRate, channels, 0.025);

var mix = new Mixer(format, buf2, audioSignal);

output.Source = mix;

input.Start();
output.Start();

Console.ReadLine();

input.Stop();
output.Stop();

await Task.Delay(100);
input.Dispose();
output.Dispose();

PortAudioHelper.Instance.Dispose();
