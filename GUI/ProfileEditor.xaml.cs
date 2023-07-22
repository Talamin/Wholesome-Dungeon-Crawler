using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Managers;
using WholesomeDungeonCrawler.Models;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : INotifyPropertyChanged
    {
        public OpenFileDialog OpenFileDialog1;
        private static ProfileModel _currentProfile;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<StepModel> StepCollection { get; set; }
        public ObservableCollection<Vector3> DeathrunCollection { get; set; }
        private static System.Timers.Timer _addDeathrunVectorTimer;
        public ObservableCollection<PathFinder.OffMeshConnection> OffMeshCollection { get; set; }
        public ObservableCollection<Vector3> OffMeshPathCollection { get; set; }

        private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        private MetroDialogSettings _basicDialogSettings;
        private MetroDialogSettings _addDialogSettings;
        private bool _radarRunning = false;

        public ProfileModel CurrentProfile
        {
            get 
            { 
                return _currentProfile;
            }
            set
            {
                _currentProfile = value;
                OnPropertyChanged();
            }
        }

        public ProfileEditor()
        {
            this.DataContext = this;
            ReInitializeProfile();
            InitializeComponent();

            cbDungeon.ItemsSource = Lists.AllDungeons;
            cbDungeon.SelectedValuePath = "Name";
            cbDungeon.DisplayMemberPath = "Name";

            Setup();
        }

        private void ReInitializeProfile()
        {
            CurrentProfile = new ProfileModel();
            CurrentProfile.StepModels = new List<StepModel>();
            CurrentProfile.DeathRunPath = new List<Vector3>();
            CurrentProfile.OffMeshConnections = new List<PathFinder.OffMeshConnection>();
        }

        private void Setup()
        {
            //Debugger.Launch();
            #region DialogSetup
            _basicDialogSettings = new MetroDialogSettings()
            {
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme
            };
            _addDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Add",
                NegativeButtonText = "Cancel",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme
            };
            OpenFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select a profile",
                Filter = "JSON files (*.json)|*.json",
                Title = "Open profile",
                InitialDirectory = Others.GetCurrentDirectory + @$"/Profiles/{ProfileManager.ProfilesDirectoryName}"
            };
            #endregion

            StepCollection = new ObservableCollection<StepModel>(CurrentProfile.StepModels);
            dgProfileSteps.ItemsSource = StepCollection;

            DeathrunCollection = new ObservableCollection<Vector3>(CurrentProfile.DeathRunPath);
            dgDeathrun.ItemsSource = DeathrunCollection;
            _addDeathrunVectorTimer = new System.Timers.Timer(200);
            _addDeathrunVectorTimer.Elapsed += AddDeathrunVectorTimer_Elapsed;
            _addDeathrunVectorTimer.AutoReset = true;
            _addDeathrunVectorTimer.Enabled = true;

            OffMeshCollection = new ObservableCollection<PathFinder.OffMeshConnection>(CurrentProfile.OffMeshConnections);
            dgOffmeshList.ItemsSource = OffMeshCollection;
            cbOffMeshDirection.ItemsSource = Enum.GetValues(typeof(PathFinder.OffMeshConnectionType));

            cbFaction.ItemsSource = Enum.GetValues(typeof(Npc.FactionType));
            cbFaction.SelectedItem = CurrentProfile.Faction;

            cbDungeon.SelectedValue = CurrentProfile.DungeonName;

            md5 = MD5.Create();
            EnableRadar();
        }

        private void EnableRadar()
        {
            if (!_radarRunning)
            {
                Radar3D.Pulse();
                Radar3D.OnDrawEvent += new Radar3D.OnDrawHandler(Monitor);
                Radar3D.OnDrawEvent += new Radar3D.OnDrawHandler(psControl.Monitor);
                _radarRunning = true;
            }
        }

        private void DisableRadar()
        {
            Radar3D.OnDrawEvent -= new Radar3D.OnDrawHandler(Monitor);
            Radar3D.OnDrawEvent -= new Radar3D.OnDrawHandler(psControl.Monitor);
            Radar3D.Stop();
            _radarRunning = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            ReInitializeProfile();
            Setup();
        }

        private async void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentProfile.DungeonName))
                {
                    await this.ShowMessageAsync("Save Failed.", "You need to select a dungeon in the list", MessageDialogStyle.Affirmative, _basicDialogSettings);
                    return;
                }

                if (CurrentProfile.MapId <= 0)
                {
                    await this.ShowMessageAsync("Save Failed.", "Dungeon ID not found", MessageDialogStyle.Affirmative, _basicDialogSettings);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CurrentProfile.ProfileName))
                {
                    await this.ShowMessageAsync("Save Failed.", "You need to enter a profile name", MessageDialogStyle.Affirmative, _basicDialogSettings);
                    return;
                }

                var dungeon = Lists.AllDungeons.FirstOrDefault(x => x.Name == CurrentProfile.DungeonName);

                if (dungeon == null)
                {
                    await this.ShowMessageAsync("Save Failed.", $"Dungeon {CurrentProfile.DungeonName} has not been found in the list", MessageDialogStyle.Affirmative, _basicDialogSettings);
                }

                var rootpath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/{ProfileManager.ProfilesDirectoryName}/{dungeon.Name}");

                var output = JsonConvert.SerializeObject(CurrentProfile, Formatting.Indented, _jsonSettings);
                var path = $@"{rootpath.FullName}\{CurrentProfile.ProfileName.Replace(" ", "_")}_{CurrentProfile.Faction}.json";
                File.WriteAllText(path, output);
                Setup();

                await this.ShowMessageAsync("Profile Saved!", "Saved to " + path, MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Save Failed.", $"Error message: {ex.Message}\n\n" +
                $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void dgProfileSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgProfileSteps.SelectedItem != null)
                {
                    if (((StepModel)dgProfileSteps.SelectedItem).CompleteCondition == null)
                        ((StepModel)dgProfileSteps.SelectedItem).CompleteCondition = new StepCompleteConditionModel();
                    psControl.SelectedItem = (StepModel)dgProfileSteps.SelectedItem;
                    psControl.chkRecordPath.IsChecked = false;

                    if (psControl.SelectedItem is MoveAlongPathModel)
                    {
                        psControl.fpsCollection = new ObservableCollection<Vector3>(((MoveAlongPathModel)psControl.SelectedItem).Path);
                        psControl.dgFPS.ItemsSource = psControl.fpsCollection;
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }

        }

        private MD5 md5;
        private bool closeMe;

        private void btnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {

            if (!_radarRunning)
            {
                EnableRadar();
            }
            else
            {
                DisableRadar();
            }
        }

        public void Monitor()
        {
            try
            {
                if (Conditions.InGameAndConnected && CurrentProfile != null
                    && CurrentProfile.StepModels != null
                    && psControl.SelectedItem != null)
                {
                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(psControl.SelectedItem.Name));
                    Color randomColor = Color.FromArgb(hash[0], hash[1], hash[2]);

                    // Draw pull to safespot
                    if (psControl.SelectedItem is PullToSafeSpotModel ptssModel)
                    {
                        if (ptssModel.SafeSpotPosition != null)
                        {
                            Radar3D.DrawCircle(ptssModel.SafeSpotPosition, ptssModel.SafeSpotRadius, Color.DarkBlue, true, 100);
                        }
                        if (ptssModel.ZoneToClearPosition != null)
                        {
                            Radar3D.DrawCircle(ptssModel.ZoneToClearPosition, 2f, Color.Red, true, 100);
                            Radar3D.DrawCircle(ptssModel.ZoneToClearPosition, ptssModel.ZoneToClearRadius, Color.Red, false, 100);

                            foreach (WoWUnit unit in ObjectManager.GetWoWUnitAttackables())
                            {
                                if (unit != null
                                    && unit.Position.DistanceTo(ptssModel.ZoneToClearPosition) <= ptssModel.ZoneToClearRadius
                                    && unit.Position.Z <= ptssModel.ZoneToClearPosition.Z + ptssModel.ZoneToClearZLimit
                                    && unit.Position.Z >= ptssModel.ZoneToClearPosition.Z - ptssModel.ZoneToClearZLimit
                                    && unit.IsAttackable)
                                {
                                    Radar3D.DrawCircle(unit.Position, 1f, Color.Red, true, 100);
                                    Radar3D.DrawLine(ptssModel.ZoneToClearPosition, unit.Position, Color.Red, 100);
                                }
                            }
                        }
                        if (ptssModel.SafeSpotPosition != null
                            && ptssModel.ZoneToClearPosition != null)
                        {
                            Radar3D.DrawLine(ptssModel.SafeSpotPosition, ptssModel.ZoneToClearPosition, Color.MediumBlue, 100);
                        }
                    }

                    // Draw talk to
                    if (psControl.SelectedItem is TalkToUnitModel ttModel)
                    {
                        if (ttModel.ExpectedPosition != null)
                        {
                            Radar3D.DrawCircle(ttModel.ExpectedPosition, 3f, randomColor, false, 200);
                        }
                    }

                    // Draw interact
                    if (psControl.SelectedItem is InteractWithModel iwModel)
                    {
                        if (iwModel.ExpectedPosition != null)
                        {
                            Radar3D.DrawCircle(iwModel.ExpectedPosition, iwModel.InteractDistance, randomColor, false, 200);
                        }
                    }

                    // Draw defend spot
                    if (psControl.SelectedItem is DefendSpotModel dsModel)
                    {
                        if (dsModel.DefendPosition != null)
                        {
                            Radar3D.DrawCircle(dsModel.DefendPosition, dsModel.DefendSpotRadius, Color.MediumVioletRed, false, 200);
                        }
                    }

                    // Draw regroup spots
                    if (psControl.SelectedItem is RegroupModel rModel)
                    {
                        if (rModel.RegroupSpot != null)
                        {
                            Radar3D.DrawCircle(rModel.RegroupSpot, 4f, Color.White, false, 200);
                        }
                    }

                    // Draw follow unit
                    if (psControl.SelectedItem is FollowUnitModel fuModel)
                    {
                        if (fuModel.ExpectedStartPosition != null)
                        {
                            Radar3D.DrawCircle(fuModel.ExpectedStartPosition, 2f, Color.LawnGreen, false, 200);
                        }
                        if (fuModel.ExpectedEndPosition != null)
                        {
                            Radar3D.DrawCircle(fuModel.ExpectedEndPosition, 2f, Color.LawnGreen, false, 200);
                        }
                        if (fuModel.ExpectedStartPosition != null
                            && fuModel.ExpectedEndPosition != null)
                        {
                            Radar3D.DrawLine(fuModel.ExpectedStartPosition, fuModel.ExpectedEndPosition, Color.LawnGreen, 50);
                        }
                        WoWUnit unitToEscort = ObjectManager.GetObjectWoWUnit().Find(unit => unit.Entry == fuModel.UnitId);
                        if (unitToEscort != null)
                        {
                            Radar3D.DrawCircle(unitToEscort.Position, 3f, Color.LawnGreen, true, 200);
                        }
                    }

                    // Draw moveAlong paths
                    if (psControl.SelectedItem is MoveAlongPathModel mapModel
                        && mapModel.Path != null)
                    {
                        var previousVector = new Vector3();
                        foreach (Vector3 node in mapModel.Path)
                        {
                            if (previousVector == new Vector3())
                            {
                                previousVector = node;
                            }
                            Radar3D.DrawCircle(node, 1f, randomColor, true, 200);
                            Radar3D.DrawLine(node, previousVector, randomColor, 200);
                            previousVector = node;
                        }
                    }

                    // Draw LOS checks
                    if (psControl.SelectedItem.CompleteCondition != null
                        && psControl.SelectedItem.CompleteCondition.ConditionType == CompleteConditionType.LOSCheck)
                    {
                        if (psControl.SelectedItem.CompleteCondition.LOSPositionVectorFrom != null)
                        {
                            Radar3D.DrawCircle(psControl.SelectedItem.CompleteCondition.LOSPositionVectorFrom, 0.3f, Color.Magenta, true, 200);
                        }
                        if (psControl.SelectedItem.CompleteCondition.LOSPositionVectorTo != null)
                        {
                            Radar3D.DrawCircle(psControl.SelectedItem.CompleteCondition.LOSPositionVectorTo, 0.3f, Color.Magenta, true, 200);
                        }
                        if (psControl.SelectedItem.CompleteCondition.LOSPositionVectorTo != null
                            && psControl.SelectedItem.CompleteCondition.LOSPositionVectorFrom != null)
                        {
                            Radar3D.DrawLine(psControl.SelectedItem.CompleteCondition.LOSPositionVectorFrom, psControl.SelectedItem.CompleteCondition.LOSPositionVectorTo, Color.Magenta, 200);
                        }
                    }

                    // Draw death run paths
                    Color deadcolour = Color.Red;
                    Vector3 deadpreviousVector = new Vector3();
                    if (CurrentProfile.DeathRunPath != null)
                    {
                        foreach (Vector3 node in CurrentProfile.DeathRunPath)
                        {
                            if (deadpreviousVector == new Vector3())
                            {
                                deadpreviousVector = node;
                            }
                            Radar3D.DrawCircle(node, 1f, deadcolour, true, 200);
                            Radar3D.DrawLine(node, deadpreviousVector, deadcolour, 200);
                            deadpreviousVector = node;
                        }
                    }

                    // Draw offmesh connections
                    if (CurrentProfile.OffMeshConnections != null)
                    {
                        foreach (var offmesh in CurrentProfile.OffMeshConnections)
                        {
                            Color offmeshcolour = Color.Green;
                            Vector3 offmeshcpreviousVector = new Vector3();
                            foreach (var vec in offmesh.Path)
                            {
                                if (offmeshcpreviousVector == new Vector3())
                                {
                                    offmeshcpreviousVector = vec;
                                }
                                Radar3D.DrawCircle(vec, 1f, offmeshcolour, true, 200);
                                Radar3D.DrawLine(vec, offmeshcpreviousVector, offmeshcolour, 200);
                                offmeshcpreviousVector = vec;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        private void cbDungeon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DungeonModel)cbDungeon.SelectedItem) != null
                && Lists.AllDungeons.Exists(dungeon => dungeon.Name == ((DungeonModel)cbDungeon.SelectedItem).Name))
            {
                CurrentProfile.DungeonName = ((DungeonModel)cbDungeon.SelectedItem).Name;
                CurrentProfile.MapId = Lists.AllDungeons.Find(dungeon => dungeon.Name == CurrentProfile.DungeonName).MapId;
            }
        }

        private async void btnLoadProfile_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    //Debugger.Launch();
                    var filePath = OpenFileDialog1.FileName;
                    CurrentProfile = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(filePath), _jsonSettings);
                    Setup();
                }
                catch (Exception ex)
                {
                    await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
                }
            }
        }

        #region Add Steps

        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }
        private void btnDeleteStep_Click(object sender, RoutedEventArgs e)
        {
            if (dgProfileSteps.SelectedItem != null)
            {
                StepCollection.Remove((StepModel)dgProfileSteps.SelectedItem);
                CurrentProfile.StepModels = StepCollection.ToList();
            }
        }

        private async void miMoveAlongPathStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    MoveAlongPathModel stepModel = new MoveAlongPathModel() { Name = x, Path = new List<Vector3>() };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miInteractWithStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    InteractWithModel stepModel = new InteractWithModel() { Name = x, InteractDistance = 3 };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miTalkToUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    TalkToUnitModel stepModel = new TalkToUnitModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miDefendSpotStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    DefendSpotModel stepModel = new DefendSpotModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miFollowUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    FollowUnitModel stepModel = new FollowUnitModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miPullToSafeSpotStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    PullToSafeSpotModel stepModel = new PullToSafeSpotModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void miLeaveDungeonStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    LeaveDungeonModel stepModel = new LeaveDungeonModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void regroupStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    RegroupModel stepModel = new RegroupModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void jumpToStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", _addDialogSettings);
                if (x != null)
                {
                    JumpToStepModel stepModel = new JumpToStepModel() { Name = x };
                    StepCollection.Add(stepModel);
                    CurrentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        #endregion

        #region Add Deathrun
        private void btnAddDeathRunVector_Click(object sender, RoutedEventArgs e)
        {
            DeathrunCollection.Add(ObjectManager.Me.Position);
            CurrentProfile.DeathRunPath = DeathrunCollection.ToList();
        }

        private void btnDeleteDeathRunVector_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeathrun.SelectedItem != null)
            {
                DeathrunCollection.Remove((Vector3)dgDeathrun.SelectedItem);
                CurrentProfile.DeathRunPath = DeathrunCollection.ToList();
            }
        }

        private async void AddDeathrunVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (gbDeathRun.IsVisible 
                    && (bool)chkRecordDeathRunPath.IsChecked 
                    && (DeathrunCollection.Count == 0 || DeathrunCollection.LastOrDefault().DistanceTo(ObjectManager.Me.Position) > 8))
                    {
                        DeathrunCollection.Add(ObjectManager.Me.Position);
                        CurrentProfile.DeathRunPath = DeathrunCollection.ToList();
                    }
                });
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }
        #endregion

        #region Add Offmesh
        private void btnOcAdd_Click(object sender, RoutedEventArgs e)
        {
            OffMeshCollection.Add(new PathFinder.OffMeshConnection() { Name = Usefuls.SubMapZoneName ?? Usefuls.MapZoneName, ContinentId = CurrentProfile.MapId, TryToUseEvenIfCanFindPathSuccess = true, Type = PathFinder.OffMeshConnectionType.Unidirectional });
            CurrentProfile.OffMeshConnections = OffMeshCollection.ToList();
        }

        private void btnOcDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOffmeshList.SelectedItem != null)
            {
                OffMeshCollection.Remove((PathFinder.OffMeshConnection)dgOffmeshList.SelectedItem);
                CurrentProfile.OffMeshConnections = OffMeshCollection.ToList();
            }
        }

        private void btnOCPAdd_Click(object sender, RoutedEventArgs e)
        {
            OffMeshPathCollection.Add(ObjectManager.Me.Position);
            CurrentProfile.OffMeshConnections.FirstOrDefault(x => x == dgOffmeshList.SelectedItem).Path = OffMeshPathCollection.ToList();
        }
        private void btnOCPDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOffmeshPath.SelectedItem != null)
            {
                OffMeshPathCollection.Remove((Vector3)dgOffmeshPath.SelectedItem);
                CurrentProfile.OffMeshConnections.FirstOrDefault(x => x == dgOffmeshList.SelectedItem).Path = OffMeshPathCollection.ToList();
            }
        }

        private async void dgOffmeshList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgOffmeshList.SelectedIndex >= 0)
                {
                    OffMeshPathCollection = new ObservableCollection<Vector3>(((PathFinder.OffMeshConnection)dgOffmeshList.SelectedItem).Path);
                    dgOffmeshPath.ItemsSource = OffMeshPathCollection;
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }
        #endregion

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (e.Cancel) return;
            e.Cancel = !this.closeMe;
            if (this.closeMe) return;
            var result = await this.ShowMessageAsync("", "Are you sure you want to close?", MessageDialogStyle.AffirmativeAndNegative, _basicDialogSettings);
            this.closeMe = result == MessageDialogResult.Affirmative;

            DisableRadar();

            if (this.closeMe)
            {
                this.Close();
            }
        }

        private async void btnMoveStepUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StepModel currentStep = (StepModel)dgProfileSteps.SelectedItem;
                if (currentStep != null)
                {
                    ObservableCollection<StepModel> allSteps = (ObservableCollection<StepModel>)dgProfileSteps.ItemsSource;
                    int currentStepIndex = allSteps.IndexOf(currentStep);
                    if (currentStepIndex > 0)
                    {
                        allSteps.Move(currentStepIndex, currentStepIndex - 1);
                        CurrentProfile.StepModels = allSteps.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

        private async void btnMoveStepDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StepModel currentStep = (StepModel)dgProfileSteps.SelectedItem;
                if (currentStep != null)
                {
                    ObservableCollection<StepModel> allSteps = (ObservableCollection<StepModel>)dgProfileSteps.ItemsSource;
                    int currentStepIndex = allSteps.IndexOf(currentStep);
                    if (currentStepIndex < allSteps.Count - 1)
                    {
                        allSteps.Move(currentStepIndex, currentStepIndex + 1);
                        CurrentProfile.StepModels = allSteps.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, _basicDialogSettings);
            }
        }

    }

    #region ValueConverters
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return System.Windows.Visibility.Collapsed;

            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.GetType().Name;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    public class ComboboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value >= 0)
                return System.Windows.Visibility.Visible;
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TextBoxVectorConverter : IValueConverter
    {
        public static readonly char Vector3Separator = ';';
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetStringFromVector3((Vector3)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetVector3FromString(value.ToString());
        }

        public static Vector3 GetVector3FromString(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            string[] vectorValues = text.ToString().Split(Vector3Separator);
            if (vectorValues.Length != 3)
            {
                return null;
            }

            foreach (string s in vectorValues)
            {
                if (!float.TryParse(s, out _))
                {
                    return null;
                }
            }
            return new Vector3(float.Parse(vectorValues[0]), float.Parse(vectorValues[1]), float.Parse(vectorValues[2]));
        }

        public static string GetStringFromVector3(Vector3 vector3)
        {
            if (vector3 == null || !(vector3 is Vector3)) return "";
            return $"{vector3.X}{Vector3Separator}{vector3.Y}{Vector3Separator}{vector3.Z}";
        }
    }

    public class VectorValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (TextBoxVectorConverter.GetVector3FromString(value.ToString()) == null && !string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, $"The value must be 3 floating numbers separated by semi-colons");
            }
            return ValidationResult.ValidResult;
        }
    }

    public class MultipleEntriesValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return ValidationResult.ValidResult;
            }
            string[] entryValues = text.ToString().Split(';');
            foreach (string s in entryValues)
            {
                if (!int.TryParse(s, out _))
                {
                    return new ValidationResult(false, $"Must be one entry or multiple entries separated by semi-colons");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
    #endregion
}
