// See https://aka.ms/new-console-template for more information

using AudioBrix.NAudio;
using AudioBrix.PortAudio.Helper;
using AudioBrix.PortAudio.Streams;
using NAudio.Wave;
using System.Threading.Channels;
using NAudio.Wave.SampleProviders;

Console.WriteLine("Hello, World!");

using var wf = new WaveFileReader(@"K:\tmp\test.wav");
var resampling = new WdlResamplingSampleProvider(wf.ToSampleProvider(), 8000);
var fs = resampling.ToFrameSource();

var ha = PortAudioHelper.GetDefaultHostApi();
var od = PortAudioHelper.GetDefaultOutputDevice(ha);


using var output = new PortAudioOutput(ha, od.index, fs.Format.SampleRate, fs.Format.Channels, 0.1);
output.Source = fs;

output.Start();

Console.ReadLine();
Console.WriteLine("Stopping");

output.Stop();
output.Dispose();
Console.WriteLine("Stopped");

PortAudioHelper.Instance.Dispose();