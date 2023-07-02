// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using SIPSorcery.SIP.App;
using SIPSorcery.SIP;
using System.Net.Sockets;
using AudioBrix.Bricks.Basic;
using AudioBrix.Bricks.Basic.Mixer;
using AudioBrix.Bricks.Buffer;
using AudioBrix.Bricks.Generators;
using AudioBrix.Bricks.Generators.Signals;
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

var rua = new SIPRegistrationUserAgent(transport, cfg.GetValue<string>("User"), cfg.GetValue<string>("Password"),
    cfg.GetValue<string>("Server"), 1, sendUsernameInContactHeader:true);

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

var ep = SIPEndPoint.ParseSIPEndPoint(cfg.GetValue<string>("Server"));

var uac2 = new SIPUserAgent(transport, null, false);

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
AudioBuffer? buf = null;

var endpoint = new AudioBrixEndpoint(new AudioEncoder());

endpoint.SetSourceLatency(TimeSpan.FromMilliseconds(25));

endpoint.OnSourceFormatChanged += (sender, eventArgs) =>
{
    var af = eventArgs.NewFormat;
    Console.WriteLine($"SOURCE Format has changed: {af}");

    endpoint.Source = new Gain(af, 0.1f, new WaveForm<Sine>(af, 500));

};

endpoint.OnSinkFormatChanged += (sender, eventArgs) =>
{
    var af = eventArgs.NewFormat;
    Console.WriteLine($"SINK Format has changed: {af}");

    paStream = new PortAudioOutput(ha, od.index, af.SampleRate, af.Channels, 0.05);
    buf = new AudioBuffer(af, (int)af.SampleRate * 2);
    paStream.Source = buf;
    endpoint.Sink = buf;

};

endpoint.OnStart += (sender, eventArgs) =>
{
    Console.WriteLine("Audio started");
    paStream!.Start();
};

endpoint.OnStop += (sender, eventArgs) =>
{
    Console.WriteLine("Audio stopped");
    paStream!.Stop();
};


var session = new VoIPMediaSession(endpoint.ToMediaEndpoints());

var success2 = await uac2.Answer(callServer, session);

if (!success2)
{
    Console.WriteLine("Answering the call failed. :-(");
}

await Task.Delay(5000);

uac2.Hangup();

Stop:
Console.WriteLine("Shutting down");
rua.Stop();

await Task.Delay(1000);