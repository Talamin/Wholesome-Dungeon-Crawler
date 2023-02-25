using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.GUI
{
    public partial class AdvancedSettings
    {
        private MetroDialogSettings basicDialogSettings;
        private MetroDialogSettings addDialogSettings;
        //public ObservableCollection<string> PartyMemberCollection { get; set; }

        public AdvancedSettings()
        {
            InitializeComponent();
            this.DataContext = CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting;
            Setup();
        }

        private void Setup()
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

            if (CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == Helpers.LFGRoles.Unknown)
            {
                tbTankName.Visibility = System.Windows.Visibility.Collapsed;
                spPartyGrid.Visibility= System.Windows.Visibility.Collapsed;
                txtErrorChooseRoleFirst.Visibility = System.Windows.Visibility.Visible;
            }

            if (CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.LFGRole == Helpers.LFGRoles.Tank)
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
            cbSelectDungeon.DisplayMemberPath = "Value";
            cbSelectDungeon.Items.Add(new KeyValuePair<int, string>(-1, "Random Dungeon"));
            foreach (DungeonModel dungeon in availableDungeons)
            {
                cbSelectDungeon.Items.Add(new KeyValuePair<int, string>(dungeon.DungeonId, dungeon.Name));
            }
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }

        private void cbSelectDungeon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.Log(cbSelectDungeon.SelectedItem.ToString());
            CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }
    }
}
