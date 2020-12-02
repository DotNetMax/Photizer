using Caliburn.Micro;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Photizer.ViewModels
{
    public class SplashScreenViewModel : Screen
    {
        public SplashScreenViewModel()
        {
            Init().ContinueWith(init =>
            {
                if (init.IsFaulted)
                {
                    Debug.WriteLine("Error Initializing");
                }
            });
        }

        private async Task Init()
        {
            bool hasUpdate = await CheckForUpdate();

            if (hasUpdate)
            {
                var askForUpdateResult = MessageBox.Show(Multilang.PhotizerUpdateNotification, "Photizer Update", MessageBoxButton.YesNo);
                if (askForUpdateResult == MessageBoxResult.Yes)
                {
                    await StartUpdater();
                }
                else
                {
                    await StartPhotizer();
                }
            }
            else
            {
                await StartPhotizer();
            }
        }

        private async Task StartPhotizer()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var windowManager = IoC.Get<IWindowManager>();

            await windowManager.ShowWindowAsync(IoC.Get<MainMenuViewModel>());
            await TryCloseAsync();
        }

        private async Task StartUpdater()
        {
            try
            {
                var files = Directory.GetFiles(Directory.GetCurrentDirectory());
                var updaterApp = files.Where(f => f.Contains("Photizer.Updater.exe")).FirstOrDefault();
                if (!string.IsNullOrEmpty(updaterApp))
                {
                    await Task.Run(() => Process.Start(updaterApp));
                    Application.Current.Shutdown();
                }
                else
                {
                    //skip
                    MessageBox.Show(Multilang.PhotizerUpdaterNotFound);
                    await StartPhotizer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Multilang.PhotizerUpdaterError + ex.Message);
                await StartPhotizer();
            }
        }

        private async Task<bool> CheckForUpdate()
        {
            string versionJsonUrl = "https://raw.githubusercontent.com/DotNetMax/Photizer/master/version.json";
            try
            {
                Version onlineVersion = new Version();
                using (var wc = new WebClient())
                {
                    string download = await wc.DownloadStringTaskAsync(versionJsonUrl);
                    string version = download.Split(',')[0].Split(':')[1].Replace("\"", "");
                    onlineVersion = new Version(version);
                }

                Version appVersion = Assembly.GetEntryAssembly().GetName().Version;

                int result = appVersion.CompareTo(onlineVersion);

                if (result < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //skip for now
                return false;
            }
        }
    }
}