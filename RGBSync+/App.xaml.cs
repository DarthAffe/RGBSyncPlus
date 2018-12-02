using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using RGBSyncPlus.Configuration;
using RGBSyncPlus.Configuration.Legacy;
using RGBSyncPlus.Helper;

namespace RGBSyncPlus
{
    public partial class App : Application
    {
        #region Constants

        private const string PATH_SETTINGS = "Settings.json";

        #endregion

        #region Properties & Fields

        private TaskbarIcon _taskbarIcon;

        #endregion

        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

                _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
                _taskbarIcon.DoubleClickCommand = ApplicationManager.Instance.OpenConfigurationCommand;

                Settings settings = null;
                try { settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PATH_SETTINGS), new ColorSerializer()); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    /* File doesn't exist or is corrupt - just create a new one. */
                }

                if (settings == null)
                {
                    settings = new Settings { Version = Settings.CURRENT_VERSION };
                    _taskbarIcon.ShowBalloonTip("RGBSync+ is starting in the tray!", "Click on the icon to open the configuration.", BalloonIcon.Info);
                }
                else if (settings.Version != Settings.CURRENT_VERSION)
                    ConfigurationUpdates.PerformOn(settings);

                ApplicationManager.Instance.Settings = settings;
                ApplicationManager.Instance.Initialize();

                if (!settings.MinimizeToTray) //HACK DarthAffe 02.12.2018: Workaround to create the window
                {
                    ApplicationManager.Instance.OpenConfigurationCommand.Execute(null);
                    ApplicationManager.Instance.HideConfigurationCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", $"[{DateTime.Now:G}] Exception!\r\n\r\nMessage:\r\n{ex.GetFullMessage()}\r\n\r\nStackTrace:\r\n{ex.StackTrace}\r\n\r\n");
                MessageBox.Show("An error occured while starting RGBSync+.\r\nMore information can be found in the error.log file in the application directory.", "Can't start RGBSync+.");

                try { ApplicationManager.Instance.ExitCommand.Execute(null); }
                catch { Environment.Exit(0); }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            File.WriteAllText(PATH_SETTINGS, JsonConvert.SerializeObject(ApplicationManager.Instance.Settings, new ColorSerializer()));
        }

        #endregion
    }
}
