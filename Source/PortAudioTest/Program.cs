// See https://aka.ms/new-console-template for more information

using AudioBrix.Bricks.Buffer;
using AudioBrix.PortAudio.Helper;
using AudioBrix.PortAudio.Streams;

Console.WriteLine("Hello, World!");

int sampleRate = 48000;
int channels = 1;

var buf = new BufferBrick(sampleRate, channels, 96000);
buf.FillWithZero = false;

var ha = PortAudioHelper.GetDefaultHostApi();
var id = PortAudioHelper.GetDefaultInputDevice(ha);
var od = PortAudioHelper.GetDefaultOutputDevice(ha);

var input = new PortAudioInput(ha, id.index, sampleRate, channels, 0.05);
input.Sink = buf;

var output = new PortAudioOutput(ha, od.index, sampleRate, channels, 0.05);
output.Source = buf;

input.Start();
output.Start();

Console.ReadLine();

output.Stop();
input.Stop();