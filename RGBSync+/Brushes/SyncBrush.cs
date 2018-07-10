using System.ComponentModel;
using RGB.NET.Core;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;

namespace RGBSyncPlus.Brushes
{
    public class SyncBrush : AbstractBrush
    {
        #region Properties & Fields

        private readonly SyncGroup _syncGroup;

        private Led _syncLed;

        #endregion

        #region Constructors

        public SyncBrush(SyncGroup syncGroup)
        {
            this._syncGroup = syncGroup;

            syncGroup.PropertyChanged += SyncGroupOnPropertyChanged;
            _syncLed = syncGroup.SyncLed?.GetLed();
        }

        #endregion

        #region Methods

        private void SyncGroupOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SyncGroup.SyncLed))
                _syncLed = _syncGroup.SyncLed?.GetLed();
        }

        protected override Color GetColorAtPoint(Rectangle rectangle, BrushRenderTarget renderTarget)
        {
            if(_syncLed == null)
                _syncLed = _syncGroup.SyncLed?.GetLed();

            if (renderTarget.Led == _syncLed)
                return Color.Transparent;

            return _syncLed?.Color ?? Color.Transparent;
        }

        #endregion
    }
}
