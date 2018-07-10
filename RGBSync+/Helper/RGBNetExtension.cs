using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;
using RGBSyncPlus.Model;

namespace RGBSyncPlus.Helper
{
    public static class RGBNetExtension
    {
        public static string GetDeviceName(this IRGBDevice device) => $"{device.DeviceInfo.DeviceName} ({device.DeviceInfo.DeviceType})";

        public static IEnumerable<Led> GetLeds(this IEnumerable<SyncLed> syncLeds)
            => syncLeds.Select(GetLed).Where(led => led != null);

        public static Led GetLed(this SyncLed syncLed)
        {
            if (syncLed == null) return null;
            return RGBSurface.Instance.Leds.FirstOrDefault(l => (l.Id == syncLed.LedId) && (l.Device.GetDeviceName() == syncLed.Device));
        }
    }
}
