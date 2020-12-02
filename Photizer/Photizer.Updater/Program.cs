using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Photizer.Updater
{
    internal class Program
    {
        private static string _zipPath = Directory.GetCurrentDirectory() + @"\Photizer.zip";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("############################");
            Console.WriteLine("Photizer Updater");
            Console.WriteLine("############################\n");

            Console.WriteLine("Going to download the latest release from Github...");
            bool wasDownloaded = await DownloadLatestRelease();

            if (wasDownloaded)
            {
                Console.WriteLine("Downloaded the latest release. Now extracting...");
                bool wasExtracted = ExtractZipFolder();
                if (wasExtracted)
                {
                    Console.WriteLine("Extraction finished. Photizer is now updated.");
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory());
                    var photizer = files.Where(f => f.Contains("Photizer.exe")).FirstOrDefault();
                    _ = Task.Run(() => Process.Start(photizer)).ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine("Something went wrong extracting the zip file. Please go to your Photizer App directory to extract the latest release manually");
                }
            }
            else
            {
                Console.WriteLine("Something went wrong with the Download. Please go to https://github.com/DotNetMax/Photizer to download the latest release manually");
            }

            Console.Write("Press any key to close...");
            Console.ReadKey();
        }

        public static bool ExtractZipFolder()
        {
            string extractDestination = Directory.GetCurrentDirectory();

            try
            {
                ZipFile.ExtractToDirectory(_zipPath, extractDestination, true);

                File.Delete(_zipPath);

                return true;
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("Photizer.Updater.exe"))
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
                return false;
            }
        }

        public static async Task<bool> DownloadLatestRelease()
        {
            var client = new GitHubClient(new ProductHeaderValue("PhotizerUpdater"));
            var latestRelease = await client.Repository.Release.GetLatest("DotNetMax", "Photizer");

            var downloadUrl = latestRelease.Assets.First().BrowserDownloadUrl;

            try
            {
                if (File.Exists(_zipPath))
                {
                    File.Delete(_zipPath);
                }

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    await wc.DownloadFileTaskAsync(downloadUrl, _zipPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine($"Download Progress at: {e.ProgressPercentage}%");
        }
    }
}