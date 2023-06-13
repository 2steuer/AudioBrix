using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortAudio.Net;

namespace AudioBrix.PortAudio.Helper
{
    public class PortAudioHelper
    {
        public static IEnumerable<PaHostApiTypeId> GetHostApis()
        {
            using var pa = PaLibrary.Initialize();
            for (int i = 0; i < pa.HostApiCount; i++)
            {
                var info = pa.GetHostApiInfo(i);
                yield return info!.Value.type;
            }
        }

        public static PaHostApiTypeId GetDefaultHostApi()
        {
            using var pa = PaLibrary.Initialize();

            return pa.GetHostApiInfo(pa.GetDefaultHostApi)!.Value.type;
        }

        public static PaDeviceInfo GetDeviceInfo(PaHostApiTypeId hostApi, int hostApiIndex)
        {
            using var pa = PaLibrary.Initialize();
            return GetDeviceInfoInternal(pa, hostApi, hostApiIndex);
        }

        public static bool CheckFormat(PaHostApiTypeId hostApi, int hostApiDeviceIndex,
            double sampleRate, PaSampleFormat sampleFormat, int channelCount, bool output)
        {
            using var pa = PaLibrary.Initialize();

            return CheckFormatInternal(pa, hostApi, hostApiDeviceIndex, sampleRate, sampleFormat, channelCount, output);
        }

        internal static bool CheckFormatInternal(PaLibrary pa, PaHostApiTypeId hostApi, int hostApiDeviceIndex,
            double sampleRate, PaSampleFormat sampleFormat, int channelCount, bool output)
        {
            var ha = GetHostApiIndexAndInfo(pa, hostApi);
            var dev = GetDeviceInfoInternal(pa, hostApi, hostApiDeviceIndex);

            var parameters = new PaStreamParameters()
            {
                channelCount = channelCount,
                device = pa.HostApiDeviceIndexToDeviceIndex(ha.index, hostApiDeviceIndex),
                sampleFormat = sampleFormat,
                suggestedLatency = output ? dev.defaultLowOutputLatency : dev.defaultLowInputLatency
            };

            return pa.IsFormatSupported(output ? null : parameters, output ? parameters : null, sampleRate) ==
                   PaLibrary.paFormatIsSupported;
        }

        public static IEnumerable<double> FilterSampleRates(PaHostApiTypeId hostApi, int hostApiDeviceIndex,
            double[] sampleRates, PaSampleFormat sampleFormat, int channelCount, bool output)
        {
            using var pa = PaLibrary.Initialize();
            
            for (int i = 0; i < sampleRates.Length; i++)
            {
                if (CheckFormatInternal(pa, hostApi, hostApiDeviceIndex, sampleRates[i], sampleFormat, channelCount, output))
                {
                    yield return sampleRates[i];
                }
            }
        }

        internal static PaDeviceInfo GetDeviceInfoInternal(PaLibrary pa, PaHostApiTypeId hostApi, int hostApiIndex)
        {
            var ha = GetHostApiIndexAndInfo(pa, hostApi);

            var devId = pa.HostApiDeviceIndexToDeviceIndex(ha.index, hostApiIndex);

            var devInfo = pa.GetDeviceInfo(devId);

            if (devInfo == null)
            {
                throw new ArgumentException($"No device with Host-API Index {hostApiIndex} present!",
                    nameof(hostApiIndex));
            }

            return devInfo.Value;
        }

        public static IEnumerable<(int index, PaDeviceInfo info)> GetInputDevices(PaHostApiTypeId hostApi)
            => GetFilteredDevices(hostApi, info => info.maxInputChannels > 0);

        public static IEnumerable<(int index, PaDeviceInfo info)> GetOutputDevices(PaHostApiTypeId hostApi)
            => GetFilteredDevices(hostApi, info => info.maxOutputChannels > 0);

        public static (int index, PaDeviceInfo info) GetDefaultInputDevice(PaHostApiTypeId hostApi)
        {
            using var pa = PaLibrary.Initialize();

            var ha = GetHostApiIndexAndInfo(pa, hostApi);

            return (ha.info.defaultInputDevice, GetDeviceInfoInternal(pa, hostApi, ha.info.defaultInputDevice));
        }

        public static (int index, PaDeviceInfo info) GetDefaultOutputDevice(PaHostApiTypeId hostApi)
        {
            using var pa = PaLibrary.Initialize();

            var ha = GetHostApiIndexAndInfo(pa, hostApi);

            return (ha.info.defaultOutputDevice, GetDeviceInfoInternal(pa, hostApi, ha.info.defaultOutputDevice));
        }

        internal static IEnumerable<(int index, PaDeviceInfo info)> GetFilteredDevices(PaHostApiTypeId hostApi, Func<PaDeviceInfo, bool> filter)
        {
            using var pa = PaLibrary.Initialize();

            var ha = GetHostApiIndexAndInfo(pa, hostApi);

            for (int j = 0; j < ha.info.deviceCount; j++)
            {
                var devInfo = pa.GetDeviceInfo(pa.HostApiDeviceIndexToDeviceIndex(ha.index, j))!.Value;

                if (filter(devInfo))
                {
                    yield return (j, devInfo);
                }
            }
        }

        internal static (int index, PaHostApiInfo info) GetHostApiIndexAndInfo(PaLibrary pa, PaHostApiTypeId hostApi)
        {
            for (int i = 0; i < pa.HostApiCount; i++)
            {
                var haInfo = pa.GetHostApiInfo(i)!.Value;

                if (haInfo.type == hostApi)
                {
                    return (i, haInfo);
                }
            }

            throw new ArgumentException($"Host API {hostApi} is not supported on this system.", nameof(hostApi));
        }
    }
}
