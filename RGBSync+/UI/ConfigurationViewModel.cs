using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using GongSolutions.Wpf.DragDrop;
using RGB.NET.Core;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;

namespace RGBSyncPlus.UI
{
    public sealed class ConfigurationViewModel : AbstractBindable, IDropTarget
    {
        #region Properties & Fields

        public Version Version => Assembly.GetEntryAssembly().GetName().Version;

        public double UpdateRate
        {
            get => 1.0 / ApplicationManager.Instance.UpdateTrigger.UpdateFrequency;
            set
            {
                double val = MathHelper.Clamp(value, 1, 100);
                ApplicationManager.Instance.Settings.UpdateRate = val;
                ApplicationManager.Instance.UpdateTrigger.UpdateFrequency = 1.0 / val;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SyncGroup> _syncGroups;
        public ObservableCollection<SyncGroup> SyncGroups
        {
            get => _syncGroups;
            set => SetProperty(ref _syncGroups, value);
        }

        private SyncGroup _selectedSyncGroup;
        public SyncGroup SelectedSyncGroup
        {
            get => _selectedSyncGroup;
            set
            {
                if (SetProperty(ref _selectedSyncGroup, value))
                    UpdateLedLists();
            }
        }

        private ListCollectionView _availableSyncLeds;
        public ListCollectionView AvailableSyncLeds
        {
            get => _availableSyncLeds;
            set => SetProperty(ref _availableSyncLeds, value);
        }

        private ListCollectionView _availableLeds;
        public ListCollectionView AvailableLeds
        {
            get => _availableLeds;
            set => SetProperty(ref _availableLeds, value);
        }

        private ListCollectionView _synchronizedLeds;
        public ListCollectionView SynchronizedLeds
        {
            get => _synchronizedLeds;
            set => SetProperty(ref _synchronizedLeds, value);
        }

        #endregion

        #region Commands

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        private ActionCommand _addSyncGroupCommand;
        public ActionCommand AddSyncGroupCommand => _addSyncGroupCommand ?? (_addSyncGroupCommand = new ActionCommand(AddSyncGroup));

        private ActionCommand<SyncGroup> _removeSyncGroupCommand;
        public ActionCommand<SyncGroup> RemoveSyncGroupCommand => _removeSyncGroupCommand ?? (_removeSyncGroupCommand = new ActionCommand<SyncGroup>(RemoveSyncGroup));

        #endregion

        #region Constructors

        public ConfigurationViewModel()
        {
            SyncGroups = new ObservableCollection<SyncGroup>(ApplicationManager.Instance.Settings.SyncGroups);

            AvailableSyncLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(x => x.Device.DeviceInfo.SupportsSyncBack));
            OnPropertyChanged(nameof(AvailableSyncLeds));
        }

        #endregion

        #region Methods

        private ListCollectionView GetGroupedLedList(IEnumerable<Led> leds) => GetGroupedLedList(leds.Select(led => new SyncLed(led)).ToList());

        private ListCollectionView GetGroupedLedList(IList syncLeds)
        {
            ListCollectionView collectionView = new ListCollectionView(syncLeds);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SyncLed.Device)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.Device), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(SyncLed.LedId), ListSortDirection.Ascending));
            collectionView.Refresh();
            return collectionView;
        }

        private void UpdateLedLists()
        {
            SynchronizedLeds = GetGroupedLedList(SelectedSyncGroup.Leds);
            OnPropertyChanged(nameof(SynchronizedLeds));

            AvailableLeds = GetGroupedLedList(RGBSurface.Instance.Leds.Where(led => !SelectedSyncGroup.Leds.Any(sc => (sc.LedId == led.Id) && (sc.Device == led.Device.GetDeviceName()))));
            OnPropertyChanged(nameof(AvailableLeds));
        }

        private void OpenHomepage() => Process.Start("https://github.com/DarthAffe/RGBSyncPlus");

        private void AddSyncGroup()
        {
            SyncGroup syncGroup = new SyncGroup();
            SyncGroups.Add(syncGroup);
            ApplicationManager.Instance.AddSyncGroup(syncGroup);
        }

        private void RemoveSyncGroup(SyncGroup syncGroup)
        {
            if (syncGroup == null) return;

            if (MessageBox.Show($"Are you sure that you want to delete the group '{syncGroup.DisplayName}'", "Remove Sync-Group", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            SyncGroups.Remove(syncGroup);
            ApplicationManager.Instance.RemoveSyncGroup(syncGroup);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is SyncLed || dropInfo.Data is IEnumerable<SyncLed>) && (dropInfo.TargetCollection is ListCollectionView))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetCollection is ListCollectionView targetList)) return;

            //HACK DarthAffe 04.06.2018: Super ugly hack - I've no idea how to do this correctly ...
            ListCollectionView sourceList = targetList == AvailableLeds ? SynchronizedLeds : AvailableLeds;

            if (dropInfo.Data is SyncLed syncLed)
            {
                targetList.AddNewItem(syncLed);
                sourceList.Remove(syncLed);

                targetList.CommitNew();
                sourceList.CommitEdit();
            }
            else if (dropInfo.Data is IEnumerable<SyncLed> syncLeds)
            {
                foreach (SyncLed led in syncLeds)
                {
                    targetList.AddNewItem(led);
                    sourceList.Remove(led);
                }
                targetList.CommitNew();
                sourceList.CommitEdit();
            }
        }

        #endregion
    }
}
