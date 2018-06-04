using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json;
using RGB.NET.Core;
using RGB.NET.Groups;

namespace RGBSyncPlus.Model
{
    public class SyncGroup : AbstractBindable
    {
        #region Properties & Fields

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(unnamed)" : Name;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(DisplayName));
            }
        }

        private SyncLed _syncLed;
        public SyncLed SyncLed
        {
            get => _syncLed;
            set => SetProperty(ref _syncLed, value);
        }

        private ObservableCollection<SyncLed> _leds = new ObservableCollection<SyncLed>();
        public ObservableCollection<SyncLed> Leds
        {
            get => _leds;
            set => SetProperty(ref _leds, value);
        }

        [JsonIgnore]
        public ListLedGroup LedGroup { get; set; }

        [JsonIgnore]
        public NotifyCollectionChangedEventHandler LedsChangedEventHandler { get; set; }

        #endregion
    }
}
