using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private MetroDialogSettings basicDialogSettings;
        private MetroDialogSettings addDialogSettings;

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
                ObservableCollection<string> partyMemberCollection = new ObservableCollection<string>(CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers);
                dgParty.ItemsSource = partyMemberCollection;
                txtErrorChooseRoleFirst.Visibility = System.Windows.Visibility.Collapsed;

                addDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Accented
                };

                basicDialogSettings = new MetroDialogSettings()
                {
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Accented
                };

                btnAddPartyMember.Click += async (sender, e) =>
                {
                    if (partyMemberCollection.Count >= 4)
                    {
                        await this.ShowMessageAsync("Warning", "Cannot add more than 4 players to invite.", MessageDialogStyle.Affirmative, basicDialogSettings);
                    }

                    var x = await this.ShowInputAsync("Add", "Party Member Name", addDialogSettings);
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
                    tbTankName.Visibility = System.Windows.Visibility.Collapsed;
                    spPartyGrid.Visibility = System.Windows.Visibility.Collapsed;
                    txtErrorChooseRoleFirst.Visibility = System.Windows.Visibility.Visible;
                }

                if (CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == LFGRoles.Tank)
                {
                    CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.TankName = ObjectManager.Me.Name;
                    tbTankName.Visibility = System.Windows.Visibility.Collapsed;
                    cbSelectDungeon.Visibility = System.Windows.Visibility.Visible;
                    spPartyGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tbTankName.Visibility = System.Windows.Visibility.Visible;
                    spPartyGrid.Visibility = System.Windows.Visibility.Collapsed;
                    cbSelectDungeon.Visibility = System.Windows.Visibility.Collapsed;
                }

                // dungeon selection
                List<DungeonModel> availableDungeons = Toolbox.GetListAvailableDungeons();
                cbSelectDungeon.Items.Clear();
                cbSelectDungeon.SelectedValuePath = "Key";
                //cbSelectDungeon.DisplayMemberPath = "Value";

                //cbSelectDungeon.Items.Add(new KeyValuePair<int, string>(-1, "Random Dungeon"));
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
    }
}
