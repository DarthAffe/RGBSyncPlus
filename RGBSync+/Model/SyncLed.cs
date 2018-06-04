using Newtonsoft.Json;
using RGB.NET.Core;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus.Model
{
    public class SyncLed : AbstractBindable
    {
        #region Properties & Fields

        private string _device;
        public string Device
        {
            get => _device;
            set => SetProperty(ref _device, value);
        }

        private LedId _ledId;
        public LedId LedId
        {
            get => _ledId;
            set => SetProperty(ref _ledId, value);
        }

        private Led _led;
        [JsonIgnore]
        public Led Led
        {
            get => _led;
            set => SetProperty(ref _led, value);
        }

        #endregion

        #region Constructors

        public SyncLed()
        { }

        public SyncLed(string device, LedId ledId)
        {
            this.Device = device;
            this.LedId = ledId;
        }

        public SyncLed(Led led)
        {
            this.Device = led.Device.GetDeviceName();
            this.LedId = led.Id;
            this.Led = led;
        }

        #endregion

        #region Methods

        protected bool Equals(SyncLed other) => string.Equals(_device, other._device) && (_ledId == other._ledId);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SyncLed)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_device != null ? _device.GetHashCode() : 0) * 397) ^ (int)_ledId;
            }
        }

        public static bool operator ==(SyncLed left, SyncLed right) => Equals(left, right);
        public static bool operator !=(SyncLed left, SyncLed right) => !Equals(left, right);

        #endregion
    }
}
