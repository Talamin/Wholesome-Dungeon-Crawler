using MahApps.Metro.Controls.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace WholesomeDungeonCrawler.GUI
{
    public partial class AdvancedSettings
    {
        private MetroDialogSettings basicDialogSettings;
        private MetroDialogSettings addDialogSettings;
        public ObservableCollection<string> PartyMemberCollection { get; set; }

        public AdvancedSettings()
        {
            InitializeComponent();
            this.DataContext = CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting;
            Setup();
        }

        private void Setup()
        {
            PartyMemberCollection = new ObservableCollection<string>(CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers);
            dgParty.ItemsSource = PartyMemberCollection;

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
                if (PartyMemberCollection.Count >= 4)
                {
                    await this.ShowMessageAsync("Warning", "Cannot add more than 4 players to invite.", MessageDialogStyle.Affirmative, basicDialogSettings);
                }

                var x = await this.ShowInputAsync("Add", "Party Member Name", addDialogSettings);
                if (x != null)
                {
                    PartyMemberCollection.Add(x);
                    CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers = PartyMemberCollection.ToList();
                }

            };

            btnDeletePartyMember.Click += (sender, e) =>
            {
                if (dgParty.SelectedIndex >= 0)
                {
                    PartyMemberCollection.Remove(dgParty.SelectedValue.ToString());
                    CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.GroupMembers = PartyMemberCollection.ToList();
                }
            };
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            CrawlerSettings.WholesomeDungeonCrawlerSettings.CurrentSetting.Save();
        }
    }
}
