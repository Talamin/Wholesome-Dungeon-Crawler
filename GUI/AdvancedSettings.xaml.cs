using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.GUI
{
    public partial class AdvancedSettings
    {
        private MetroDialogSettings _basicDialogSettings;
        private MetroDialogSettings _addDialogSettings;
        private MetroDialogSettings _yesNoAlwaysDialogSettings = new MetroDialogSettings();

        public AdvancedSettings()
        {
            InitializeComponent();
            this.DataContext = CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting;
            Setup();
        }

        private void Setup()
        {
            try
            {
                _yesNoAlwaysDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No",
                    FirstAuxiliaryButtonText = "Always",
                    ColorScheme = MetroDialogColorScheme.Accented,
                    AnimateHide = false,
                    AnimateShow = false,
                    DialogMessageFontSize = 13,
                    DialogTitleFontSize = 18,
                };

                ObservableCollection<string> partyMemberCollection = new ObservableCollection<string>(CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers);
                dgParty.ItemsSource = partyMemberCollection;
                txtErrorChooseRoleFirst.Visibility = System.Windows.Visibility.Collapsed;

                _addDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Accented
                };

                _basicDialogSettings = new MetroDialogSettings()
                {
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Accented
                };

                btnAddPartyMember.Click += async (sender, e) =>
                {
                    if (partyMemberCollection.Count >= 4)
                    {
                        await this.ShowMessageAsync("Warning", "Cannot add more than 4 players to invite.", MessageDialogStyle.Affirmative, _basicDialogSettings);
                    }

                    var x = await this.ShowInputAsync("Add", "Party Member Name", _addDialogSettings);
                    if (x != null)
                    {
                        partyMemberCollection.Add(x);
                        CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers = partyMemberCollection.ToList();
                    }

                };

                btnDeletePartyMember.Click += (sender, e) =>
                {
                    if (dgParty.SelectedIndex >= 0)
                    {
                        partyMemberCollection.Remove(dgParty.SelectedValue.ToString());
                        CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers = partyMemberCollection.ToList();
                    }
                };

                if (CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Unspecified)
                {
                    tbTankNameStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    cbSelectDungeonStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    spPartyGrid.Visibility = System.Windows.Visibility.Collapsed;

                    txtErrorChooseRoleFirst.Visibility = System.Windows.Visibility.Visible;
                }
                else if (CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Tank)
                {
                    tbTankNameStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    cbSelectDungeonStackPanel.Visibility = System.Windows.Visibility.Visible;
                    spPartyGrid.Visibility = System.Windows.Visibility.Visible;

                    CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.TankName = ObjectManager.Me.Name;
                }
                else
                {
                    tbTankNameStackPanel.Visibility = System.Windows.Visibility.Visible;
                    cbSelectDungeonStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    spPartyGrid.Visibility = System.Windows.Visibility.Collapsed;
                }

                // dungeon selection
                List<DungeonModel> availableDungeons = Toolbox.GetListAvailableDungeons();
                cbSelectDungeon.Items.Clear();
                cbSelectDungeon.SelectedValuePath = "Key";

                cbSelectDungeon.Items.Add(new WDCComboboxItem()
                {
                    Key = -1,
                    Content = $"Random Dungeon",
                    Foreground = Brushes.Wheat
                });
                foreach (DungeonModel dungeonModel in availableDungeons.OrderBy(d => d.IsHeroic).OrderBy(d => d.IsRaid))
                {
                    DirectoryInfo profilePath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/{ProfileManager.ProfilesDirectoryName}/{dungeonModel.Name}");
                    int profilecount = profilePath.GetFiles().Count();
                    string suffix = "";
                    int nbProfilesFound = 0;
                    SolidColorBrush textColor = Brushes.White;
                    if (profilecount <= 0)
                    {
                        suffix = " NO PROFILE";
                        textColor = Brushes.Gray;
                    }
                    else
                    {
                        nbProfilesFound++;
                        List<FileInfo> files = profilePath.GetFiles().ToList();
                        files.RemoveAll(file => !file.Name.EndsWith(".json"));
                        foreach (FileInfo file in files)
                        {
                            ProfileModel deserializedProfile = null;
                            try
                            {
                                deserializedProfile = JsonConvert.DeserializeObject<ProfileModel>(
                                    File.ReadAllText(file.FullName),
                                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                            }
                            catch (JsonSerializationException ex)
                            {
                                Logger.LogError($"There was an error when trying to deserialize the profile {file.FullName}.");
                                Logger.LogError($"{ex}");
                                return;
                            }
                            suffix = $"{nbProfilesFound} PROFILES";
                        }
                    }
                    string prefix = dungeonModel.IsHeroic ? "[Heroic] " : "";
                    prefix = dungeonModel.IsRaid ? "[Raid] " : "";

                    WDCComboboxItem item = new WDCComboboxItem()
                    {
                        Key = dungeonModel.DungeonId,
                        Content = $"{prefix}{dungeonModel.Name} ({dungeonModel.DungeonId}) - {suffix}",
                        Foreground = textColor
                    };
                    cbSelectDungeon.Items.Add(item);
                }
            }
            catch (Exception e)
            { 
                Logger.LogError(e.ToString());
            }
        }

        public class WDCComboboxItem : ComboBoxItem
        {
            public int Key { get; set; }
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }

        private void cbSelectDungeon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }

        private async void btnDownloadProfiles_Click(object sender, EventArgs e)
        {
            string folderName = Others.GetCurrentDirectory + @"\Profiles\Wholesome-Dungeon-Crawler-Profiles";
            string dataZipFile = Others.GetCurrentDirectory + @"\Data\WDC-Profiles.zip";
            Directory.CreateDirectory(folderName);
            string onlineZipLink = "https://github.com/Talamin/Wholesome-Dungeon-Crawler-Profiles/archive/refs/heads/main.zip";
            byte[] onlineZipContent = new System.Net.WebClient { Encoding = Encoding.UTF8 }.DownloadData(onlineZipLink);
            File.WriteAllBytes(dataZipFile, onlineZipContent);
            int nbDownloaded = 0;
            int nbNew = 0;
            int nbOverwritten = 0;
            bool alwaysOverwrite = false;
            using (ZipArchive zip = ZipFile.OpenRead(dataZipFile))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (!entry.FullName.EndsWith(".json")) continue;
                    nbDownloaded++;
                    string path = entry.FullName.Substring(entry.FullName.IndexOf('/') + 1); // Ignore top folder
                    string directory = path.Substring(0, path.LastIndexOf("/"));
                    Directory.CreateDirectory($"{folderName}/{directory}");
                    string finalFilePath = $"{folderName}/{path}";
                    if (!File.Exists(finalFilePath))
                    {
                        entry.ExtractToFile(finalFilePath);
                        nbNew++;
                        Logger.Log($"Downloaded new profile {entry.Name}");
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(finalFilePath);
                        if (fi.Length != entry.Length)
                        {
                            bool overWriteOnce = false;
                            if (!alwaysOverwrite)
                            {
                                MessageDialogResult overWriteDialog = await this.ShowMessageAsync("Profiles download", $"{entry.Name} was modified.\rOverwrite?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, _yesNoAlwaysDialogSettings);
                                overWriteOnce = overWriteDialog == MessageDialogResult.Affirmative;
                                if (overWriteDialog == MessageDialogResult.FirstAuxiliary)
                                    alwaysOverwrite = true;
                            }

                            if (overWriteOnce || alwaysOverwrite)
                            {
                                entry.ExtractToFile(finalFilePath, true);
                                nbOverwritten++;
                                Logger.Log($"Downloaded profile {entry.Name} [Overwritten]");
                            }
                        }
                    }
                }
            }
            File.Delete(dataZipFile);
            await this.ShowMessageAsync("Profiles download", $"{nbDownloaded} profiles online\r{nbNew} were added\r{nbOverwritten} were overwritten", MessageDialogStyle.Affirmative, _basicDialogSettings);
        }
    }
}
