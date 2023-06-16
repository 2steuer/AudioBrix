// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using SIPSorcery.SIP.App;
using SIPSorcery.SIP;
using System.Net.Sockets;
using AudioBrix.Bricks.Basic;
using AudioBrix.Bricks.Basic.Mixer;
using AudioBrix.Bricks.Buffer;
using AudioBrix.Bricks.Generators;
using AudioBrix.Material;
using AudioBrix.PortAudio.Helper;
using AudioBrix.PortAudio.Streams;
using AudioBrix.SipSorcery;
using Microsoft.Extensions.Logging;
using SIPSorcery.Media;
using SIPSorceryMedia.Abstractions;
using AudioFormat = AudioBrix.Material.AudioFormat;

Console.WriteLine("Hello, World!");

var f = LoggerFactory.Create(b =>
{
    b.SetMinimumLevel(LogLevel.Trace);
    b.AddSimpleConsole(c =>
    {
        c.TimestampFormat = "hh:mm:ss.fff ";
    });
});

SIPSorcery.LogFactory.Set(f);


var cfg = new ConfigurationBuilder()
    .AddJsonFile("config.json")
    .Build();

var transport = new SIPTransport();
transport.AddSIPChannel(transport.CreateChannel(SIPProtocolsEnum.udp, AddressFamily.InterNetwork));

ManualResetEvent regRe = new ManualResetEvent(false);
bool success = false;

var rua = new SIPRegistrationUserAgent(transport, cfg.GetValue<string>("User"), cfg.GetValue<string>("Password"), cfg.GetValue<string>("Server"), 120);

rua.RegistrationFailed += (sipuri, response, arg3) =>
{
    Console.WriteLine($"Reg. failed! {sipuri}, {response}, {arg3}");
    regRe.Set();
};
rua.RegistrationTemporaryFailure += (sipuri, response, arg3) =>
{
    Console.WriteLine($"Reg. failed temporarily! {sipuri}, {response}, {arg3}");
    regRe.Set();
};
rua.RegistrationSuccessful += (sipuri, response) =>
{
    Console.WriteLine($"Registration Success! {sipuri}, {response}");
    success = true;
    regRe.Set();
};
rua.RegistrationRemoved += (sipuri, response) =>
{
    Console.WriteLine($"Registration removed! {sipuri}, {response}");
    regRe.Set();
};

rua.Start();

Console.WriteLine("Waiting for Registration...");

regRe.WaitOne(TimeSpan.FromSeconds(2));

if (!success)
{
    Console.WriteLine("Not successful, stopping");
    goto Stop;
}

ManualResetEvent callEvent = new ManualResetEvent(false);

var uac2 = new SIPUserAgent(transport, SIPEndPoint.ParseSIPEndPoint(cfg.GetValue<string>("Server")));
SIPServerUserAgent callServer = null;

uac2.OnIncomingCall += (agent, request) =>
{
    Console.WriteLine($"Incoming call! {request.Header.From.FromURI}");
    callServer = agent.AcceptCall(request);
    callEvent.Set();
};
uac2.OnCallHungup += dialogue => Console.WriteLine($"Call hung up.");
uac2.ServerCallCancelled += uas => Console.WriteLine("Server call cancelled");

callEvent.WaitOne();

Console.WriteLine("Received call, setting up audio...");

var ha = PortAudioHelper.GetDefaultHostApi();
var id = PortAudioHelper.GetDefaultInputDevice(ha);
var od = PortAudioHelper.GetDefaultOutputDevice(ha);

PortAudioOutput? paStream = null;
BufferBrick? buf = null;

var sink = new AudioBrixSink(new AudioEncoder());

sink.OnFormatChanged += (sender, eventArgs) =>
{
    var af = new AudioFormat(eventArgs.NewFormat.RtpClockRate, eventArgs.NewFormat.ChannelCount);
    Console.WriteLine($"Format has changed: {af}");

    paStream = new PortAudioOutput(ha, od.index, af.SampleRate, af.Channels, 0.05);
    buf = new BufferBrick(af, (int)af.SampleRate * 2);
    paStream.Source = buf;
    sink.Sink = buf;
};

sink.OnStart += (sender, eventArgs) =>
{
    Console.WriteLine("Audio started");
    paStream!.Start();
};

sink.OnStop += (sender, eventArgs) =>
{
    Console.WriteLine("Audio stopped");
    paStream!.Stop();
};

var source = new AudioBrixSource(new AudioEncoder());
source.SourceQueryInterval = TimeSpan.FromSeconds(0.05);
source.PackageSizeSeconds = 0.05;

source.OnFormatChanged += (sender, eventArgs) =>
{
    var af = new AudioFormat(eventArgs.NewFormat.RtpClockRate, eventArgs.NewFormat.ChannelCount);
    source.Source = new Gain(af, 0.1f, new SineGenerator(af, 500));
};

source.OnStart += (sender, eventArgs) => sink.StartAudioSink();
source.OnStop += (sender, eventArgs) => sink.CloseAudioSink();

var session = new VoIPMediaSession(new MediaEndPoints() { AudioSink = sink, AudioSource = source});

var success2 = await uac2.Answer(callServer, session);

if (!success2)
{
    Console.WriteLine("Answering the call failed. :-(");
}

await Task.Delay(120000);

uac2.Hangup();

Stop:
Console.WriteLine("Shutting down");
rua.Stop();