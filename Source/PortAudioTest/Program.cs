// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography.X509Certificates;
using AudioBrix.Bricks.Basic;
using AudioBrix.Bricks.Buffer;
using AudioBrix.Bricks.Generators;
using AudioBrix.Material;
using AudioBrix.PortAudio.Helper;
using AudioBrix.PortAudio.Streams;
using PortAudio.Net;

Console.WriteLine("Hello, World!");

Console.WriteLine(PaLibrary.VersionInfo.versionText);

int sampleRate = 48000;
int channels = 1;

var format = new AudioFormat(sampleRate, channels);

var buf = new BufferBrick(format, 96000);
buf.FillWithZero = false;
buf.WaitOnEmpty = false;

var ha = PortAudioHelper.GetDefaultHostApi();
var id = PortAudioHelper.GetDefaultInputDevice(ha);
var od = PortAudioHelper.GetDefaultOutputDevice(ha);

var input = new PortAudioInput(ha, id.index, sampleRate, channels, 0.025);
input.Sink = buf;

var output = new PortAudioOutput(ha, od.index, sampleRate, channels, 0.025);

output.Source = buf;

input.Start();
output.Start();

Console.ReadLine();

input.Stop();
output.Stop();

PortAudioHelper.Instance.Dispose();
