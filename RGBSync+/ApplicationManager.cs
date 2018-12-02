using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using RGB.NET.Core;
using RGB.NET.Groups;
using RGBSyncPlus.Brushes;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Helper;
using RGBSyncPlus.Model;
using RGBSyncPlus.UI;

namespace RGBSyncPlus
{
    public class ApplicationManager
    {
        #region Constants

        private const string DEVICEPROVIDER_DIRECTORY = "DeviceProvider";

        #endregion

        #region Properties & Fields

        public static ApplicationManager Instance { get; } = new ApplicationManager();

        private ConfigurationWindow _configurationWindow;

        public Settings Settings { get; set; }
        public TimerUpdateTrigger UpdateTrigger { get; private set; }

        #endregion

        #region Commands

        private ActionCommand _openConfiguration;
        public ActionCommand OpenConfigurationCommand => _openConfiguration ?? (_openConfiguration = new ActionCommand(OpenConfiguration));

        private ActionCommand _hideConfiguration;
        public ActionCommand HideConfigurationCommand => _hideConfiguration ?? (_hideConfiguration = new ActionCommand(HideConfiguration));

        private ActionCommand _exitCommand;
        public ActionCommand ExitCommand => _exitCommand ?? (_exitCommand = new ActionCommand(Exit));

        #endregion

        #region Constructors

        private ApplicationManager() { }

        #endregion

        #region Methods

        public void Initialize()
        {
            RGBSurface surface = RGBSurface.Instance;
            LoadDeviceProviders();
            surface.AlignDevices();

            foreach (IRGBDevice device in surface.Devices)
                device.UpdateMode = DeviceUpdateMode.Sync | DeviceUpdateMode.SyncBack;

            UpdateTrigger = new TimerUpdateTrigger { UpdateFrequency = 1.0 / MathHelper.Clamp(Settings.UpdateRate, 1, 100) };
            surface.RegisterUpdateTrigger(UpdateTrigger);
            UpdateTrigger.Start();

            foreach (SyncGroup syncGroup in Settings.SyncGroups)
                RegisterSyncGroup(syncGroup);
        }

        private void LoadDeviceProviders()
        {
            string deviceProvierDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, DEVICEPROVIDER_DIRECTORY);
            if (!Directory.Exists(deviceProvierDir)) return;

            foreach (string file in Directory.GetFiles(deviceProvierDir, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    foreach (Type loaderType in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass
                                                                               && typeof(IRGBDeviceProviderLoader).IsAssignableFrom(t)))
                    {
                        if (Activator.CreateInstance(loaderType) is IRGBDeviceProviderLoader deviceProviderLoader)
                        {
                            //TODO DarthAffe 03.06.2018: Support Initialization
                            if (deviceProviderLoader.RequiresInitialization) continue;

                            RGBSurface.Instance.LoadDevices(deviceProviderLoader);
                        }
                    }
                }
                catch { /* #sadprogrammer */ }
            }
        }

        public void AddSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Add(syncGroup);
            RegisterSyncGroup(syncGroup);
        }

        private void RegisterSyncGroup(SyncGroup syncGroup)
        {
            syncGroup.LedGroup = new ListLedGroup(syncGroup.Leds.GetLeds()) { Brush = new SyncBrush(syncGroup) };
            syncGroup.LedsChangedEventHandler = (sender, args) => UpdateLedGroup(syncGroup.LedGroup, args);
            syncGroup.Leds.CollectionChanged += syncGroup.LedsChangedEventHandler;
        }

        public void RemoveSyncGroup(SyncGroup syncGroup)
        {
            Settings.SyncGroups.Remove(syncGroup);
            syncGroup.Leds.CollectionChanged -= syncGroup.LedsChangedEventHandler;
            syncGroup.LedGroup.Detach();
            syncGroup.LedGroup = null;
        }

        private void UpdateLedGroup(ListLedGroup group, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                List<Led> leds = group.GetLeds().ToList();
                group.RemoveLeds(leds);
            }
            else
            {
                if (args.NewItems != null)
                    group.AddLeds(args.NewItems.Cast<SyncLed>().GetLeds());

                if (args.OldItems != null)
                    group.RemoveLeds(args.OldItems.Cast<SyncLed>().GetLeds());
            }
        }

        private void HideConfiguration()
        {
            if (Settings.MinimizeToTray)
            {
                if (_configurationWindow.IsVisible)
                    _configurationWindow.Hide();
            }
            else
                _configurationWindow.WindowState = WindowState.Minimized;
        }

        private void OpenConfiguration()
        {
            if (_configurationWindow == null) _configurationWindow = new ConfigurationWindow();

            if (!_configurationWindow.IsVisible)
                _configurationWindow.Show();

            if (_configurationWindow.WindowState == WindowState.Minimized)
                _configurationWindow.WindowState = WindowState.Normal;
        }

        private void Exit()
        {
            try { RGBSurface.Instance?.Dispose(); } catch { /* well, we're shuting down anyway ... */ }
            Application.Current.Shutdown();
        }

        #endregion
    }
}
