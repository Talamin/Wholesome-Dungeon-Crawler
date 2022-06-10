using robotManager.Helpful;
using robotManager.Products;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using WholesomeDungeonCrawler.CrawlerSettings;

namespace WholesomeDungeonCrawler.Helpers
{
    public static class AutoUpdater
    {
        private static string _currentVersion = null;
        private static string _onlineVersion = null;
        private static readonly string _dllFileName = "WholesomeDungeonCrawler";
        private static readonly string _zipFileName = "default_wdc_profiles";
        private static readonly string _profilesFolder = Others.GetCurrentDirectory + $@"Profiles";
        private static readonly string _currentDll = Others.GetCurrentDirectory + $@"\Products\{_dllFileName}.dll";
        private static readonly string _oldDll = Others.GetCurrentDirectory + $@"\Products\{_dllFileName} dmp";

        public static bool CheckUpdate(string mainVersion)
        {
            _currentVersion = mainVersion;
            DateTime dateBegin = new DateTime(2020, 1, 1);
            DateTime currentDate = DateTime.Now;

            long elapsedTicks = currentDate.Ticks - dateBegin.Ticks;
            elapsedTicks /= 10000000;

            double timeSinceLastUpdate = elapsedTicks - WholesomeDungeonCrawlerSettings.CurrentSetting.LastUpdateDate;

            // Delete the old version
            if (File.Exists(_oldDll))
            {
                try
                {
                    var fs = new FileStream(_oldDll, FileMode.Open);
                    if (fs.CanWrite)
                    {
                        Logger.Log("Deleting dump file");
                        fs.Close();
                        File.Delete(_oldDll);
                    }
                    fs.Close();
                }
                catch
                {
                    ShowReloadMessage();
                    return true;
                }
            }

            // If last update try was < 30 seconds ago, we exit to avoid looping
            if (timeSinceLastUpdate < 30)
            {
                Logger.Log($"Last update attempt was {timeSinceLastUpdate} seconds ago. Exiting updater.");
                return false;
            }

            try
            {
                WholesomeDungeonCrawlerSettings.CurrentSetting.LastUpdateDate = elapsedTicks;
                WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
                Logger.Log("Starting updater");

                bool updatedProfiles = !Directory.Exists(@$"{_profilesFolder}\WholesomeDungeonCrawler") && UpdateProfiles();

                string onlineDll = "https://github.com/Talamin/Wholesome-Dungeon-Crawler/raw/master/Compiled/WholesomeDungeonCrawler.dll";
                string onlineVersion = "https://raw.githubusercontent.com/Talamin/Wholesome-Dungeon-Crawler/master/Compiled/Version.txt";

                _onlineVersion = new System.Net.WebClient { Encoding = Encoding.UTF8 }.DownloadString(onlineVersion);

                Logger.Log($"Online Version : {_onlineVersion}");
                if (_onlineVersion == null || _onlineVersion.Length > 10 || _onlineVersion == _currentVersion)
                {
                    Logger.Log($"Your version is up to date ({_currentVersion})");
                    return false;
                }

                byte[] onlineDllContent = new System.Net.WebClient { Encoding = Encoding.UTF8 }.DownloadData(onlineDll);
                
                // dll
                if (onlineDllContent != null && onlineDllContent.Length > 0)
                {
                    Logger.Log($"Your version : {_currentVersion}");
                    Logger.Log("Trying to update");

                    File.Move(_currentDll, _oldDll);

                    Logger.Log("Writing file");
                    File.WriteAllBytes(_currentDll, onlineDllContent); // replace user file by online file

                    Thread.Sleep(1000);

                    if (!updatedProfiles)
                    {
                        UpdateProfiles();
                        Thread.Sleep(1000);
                    }

                    ShowReloadMessage();
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Auto update: " + e);
            }
            return false;
        }

        private static void ShowReloadMessage()
        {
            Logger.LogError($"A new version of the Wholesome Dungeon Crawler has been downloaded, please restart WRobot.".ToUpper() +
                $"\r{_currentVersion} => {_onlineVersion}".ToUpper());
            Products.DisposeProduct();
        }

        private static bool UpdateProfiles()
        {
            try
            {
                string onlineZip = "https://github.com/Talamin/Wholesome-Dungeon-Crawler/raw/master/Compiled/default_wdc_profiles.zip";
                string currentZip = $@"{_profilesFolder}\WholesomeDungeonCrawler\{_zipFileName}.zip";
                byte[] onlineZipContent = new System.Net.WebClient { Encoding = Encoding.UTF8 }.DownloadData(onlineZip);

                if (onlineZipContent != null && onlineZipContent.Length > 0)
                {
                    Logger.Log("Downloading default profiles");
                    if (!Directory.Exists(@$"{_profilesFolder}\WholesomeDungeonCrawler"))
                    {
                        Directory.CreateDirectory(@$"{_profilesFolder}\WholesomeDungeonCrawler");
                    }

                    File.WriteAllBytes(currentZip, onlineZipContent);

                    using (FileStream zipFile = File.Open(currentZip, FileMode.Open))
                    {
                        ZipArchive zip = new ZipArchive(zipFile);

                        foreach (ZipArchiveEntry file in zip.Entries)
                        {
                            string completeFileName = Path.GetFullPath(Path.Combine(@$"{_profilesFolder}", file.FullName));
                            string directory = Path.GetDirectoryName(completeFileName);
                            if (!Directory.Exists(directory))
                            {
                                Logger.Log($"Creating directory {directory}");
                                Directory.CreateDirectory(directory);
                            }
                            if (file.Name != "")
                            {
                                file.ExtractToFile(completeFileName, true);
                            }
                        }
                    }

                    Logger.Log(@$"Profiles extracted to {_profilesFolder}\WholesomeDungeonCrawler");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.LogError("Auto update profiles: " + e);
                return false;
            }
        }
    }
}
