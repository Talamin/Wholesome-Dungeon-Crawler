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
        public OpenFileDialog openFileDialog1;
        private static ProfileModel _currentProfile;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<StepModel> StepCollection { get; set; }
        public ObservableCollection<Vector3> DeathrunCollection { get; set; }
        private static System.Timers.Timer addDeathrunVectorTimer;
        public ObservableCollection<PathFinder.OffMeshConnection> OffMeshCollection { get; set; }
        public ObservableCollection<Vector3> OffMeshPathCollection { get; set; }

        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        private MetroDialogSettings basicDialogSettings;
        private MetroDialogSettings addDialogSettings;

        public ProfileModel currentProfile
        {
            get { return _currentProfile; }
            set
            {
                _currentProfile = value;
                OnPropertyChanged();
            }
        }

        public ProfileEditor()
        {
            this.DataContext = this;
            currentProfile = new ProfileModel();
            currentProfile.StepModels = new List<StepModel>();
            currentProfile.DeathRunPath = new List<Vector3>();
            currentProfile.OffMeshConnections = new List<PathFinder.OffMeshConnection>();
            InitializeComponent();

            cbDungeon.ItemsSource = Lists.AllDungeons;
            cbDungeon.SelectedValuePath = "Name";
            cbDungeon.DisplayMemberPath = "Name";
            //cbDungeon.SelectedValue = Usefuls.ContinentId;

            Setup();
        }

        private void Setup()
        {
            //Debugger.Launch();
            #region DialogSetup
            basicDialogSettings = new MetroDialogSettings()
            {
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme
            };
            addDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Add",
                NegativeButtonText = "Cancel",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme
            };
            openFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select a profile",
                Filter = "JSON files (*.json)|*.json",
                Title = "Open profile",
                InitialDirectory = Others.GetCurrentDirectory + @$"/Profiles/{ProfileManager.ProfilesDirectoryName}"
            };
            #endregion

            StepCollection = new ObservableCollection<StepModel>(currentProfile.StepModels);
            dgProfileSteps.ItemsSource = StepCollection;

            DeathrunCollection = new ObservableCollection<Vector3>(currentProfile.DeathRunPath);
            dgDeathrun.ItemsSource = DeathrunCollection;
            addDeathrunVectorTimer = new System.Timers.Timer(200);
            addDeathrunVectorTimer.Elapsed += AddDeathrunVectorTimer_Elapsed;
            addDeathrunVectorTimer.AutoReset = true;
            addDeathrunVectorTimer.Enabled = true;

            OffMeshCollection = new ObservableCollection<PathFinder.OffMeshConnection>(currentProfile.OffMeshConnections);
            dgOffmeshList.ItemsSource = OffMeshCollection;
            cbOffMeshDirection.ItemsSource = Enum.GetValues(typeof(PathFinder.OffMeshConnectionType));

            cbFaction.ItemsSource = Enum.GetValues(typeof(Npc.FactionType));
            cbFaction.SelectedItem = currentProfile.Faction;

            cbDungeon.SelectedValue = currentProfile.DungeonName;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            currentProfile = new ProfileModel();
            currentProfile.StepModels = new List<StepModel>();
            Setup();
        }

        private async void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentProfile.DungeonName))
                {
                    await this.ShowMessageAsync("Save Failed.", "You need to select a dungeon in the list", MessageDialogStyle.Affirmative, basicDialogSettings);
                    return;
                }

                if (currentProfile.MapId <= 0)
                {
                    await this.ShowMessageAsync("Save Failed.", "Dungeon ID not found", MessageDialogStyle.Affirmative, basicDialogSettings);
                    return;
                }

                if (string.IsNullOrWhiteSpace(currentProfile.ProfileName))
                {
                    await this.ShowMessageAsync("Save Failed.", "You need to enter a profile name", MessageDialogStyle.Affirmative, basicDialogSettings);
                    return;
                }

                var dungeon = Lists.AllDungeons.FirstOrDefault(x => x.Name == currentProfile.DungeonName);

                if (dungeon == null)
                {
                    await this.ShowMessageAsync("Save Failed.", $"Dungeon {currentProfile.DungeonName} has not been found in the list", MessageDialogStyle.Affirmative, basicDialogSettings);
                }

                var rootpath = Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/{ProfileManager.ProfilesDirectoryName}/{dungeon.Name}");
                currentProfile.StepModels = currentProfile.StepModels.OrderBy(x => x.Order).ToList();

                var output = JsonConvert.SerializeObject(currentProfile, Formatting.Indented, jsonSettings);
                var path = $@"{rootpath.FullName}\{currentProfile.ProfileName.Replace(" ", "_")}_{currentProfile.Faction}.json";
                File.WriteAllText(path, output);
                Setup();

                await this.ShowMessageAsync("Profile Saved!", "Saved to " + path, MessageDialogStyle.Affirmative, basicDialogSettings);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Save Failed.", $"Error message: {ex.Message}\n\n" +
                $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void dgProfileSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgProfileSteps.SelectedItem != null)
                {
                    //psControl = new ProfileStep();
                    //Debugger.Launch();
                    if (((StepModel)dgProfileSteps.SelectedItem).CompleteCondition == null)
                        ((StepModel)dgProfileSteps.SelectedItem).CompleteCondition = new StepCompleteConditionModel();
                    psControl.SelectedItem = (StepModel)dgProfileSteps.SelectedItem;

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
                $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }

        }

        private MD5 md5;
        private bool closeMe;

        private void btnToggleOverlay_Click(object sender, RoutedEventArgs e)
        {

            if (!Radar3D.IsLaunched)
            {
                md5 = MD5.Create();
                Radar3D.Pulse();
                Radar3D.OnDrawEvent += new Radar3D.OnDrawHandler(Monitor);
                Radar3D.OnDrawEvent += new Radar3D.OnDrawHandler(psControl.Monitor);
            }
            else
            {
                Radar3D.OnDrawEvent -= new Radar3D.OnDrawHandler(Monitor);
                Radar3D.OnDrawEvent -= new Radar3D.OnDrawHandler(psControl.Monitor);
                Radar3D.Stop();
            }

        }

        public void Monitor()
        {
            try
            {
                if (Conditions.InGameAndConnected)
                {
                    foreach (StepModel step in currentProfile.StepModels)
                    {
                        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(step.Name));
                        Color randomColor = Color.FromArgb(hash[0], hash[1], hash[2]);

                        // Draw pull to safespot
                        if (step is PullToSafeSpotModel)
                        {
                            PullToSafeSpotModel model = (PullToSafeSpotModel)step;
                            if (model.SafeSpotPosition != null)
                            {
                                Radar3D.DrawCircle(model.SafeSpotPosition, model.SafeSpotRadius, Color.DarkBlue, true, 100);
                                Radar3D.DrawCircle(model.SafeSpotPosition, model.SafeSpotRadius + model.DEFAULT_MELEE_FIGHT_RANGE, Color.MediumBlue, false, 100);
                                Radar3D.DrawCircle(model.SafeSpotPosition, model.SafeSpotRadius + model.DEFAULT_RANGED_FIGHT_RANGE, Color.LightSteelBlue, false, 100);
                            }
                            if (model.ZoneToClearPosition != null)
                            {
                                Radar3D.DrawCircle(model.ZoneToClearPosition, 2f, Color.Red, true, 100);
                                Radar3D.DrawCircle(model.ZoneToClearPosition, model.ZoneToClearRadius, Color.Red, false, 100);
                            }
                            if (model.SafeSpotPosition != null
                                && model.ZoneToClearPosition != null)
                            {
                                Radar3D.DrawLine(model.SafeSpotPosition, model.ZoneToClearPosition, Color.MediumBlue, 100);
                            }
                            foreach (WoWUnit unit in ObjectManager.GetWoWUnitAttackables())
                            {
                                if (unit.Position.DistanceTo(model.ZoneToClearPosition) <= model.ZoneToClearRadius)
                                {
                                    Radar3D.DrawCircle(unit.Position, 1f, Color.Red, true, 100);
                                    Radar3D.DrawLine(model.ZoneToClearPosition, unit.Position, Color.Red, 100);
                                }
                            }
                        }

                        // Draw talk to
                        if (step is TalkToUnitModel)
                        {
                            TalkToUnitModel model = (TalkToUnitModel)step;
                            if (model.ExpectedPosition != null)
                            {
                                Radar3D.DrawCircle(model.ExpectedPosition, 3f, randomColor, false, 200);
                            }
                        }

                        // Draw interact
                        if (step is InteractWithModel)
                        {
                            InteractWithModel model = (InteractWithModel)step;
                            if (model.ExpectedPosition != null)
                            {
                                Radar3D.DrawCircle(model.ExpectedPosition, model.InteractDistance, randomColor, false, 200);
                            }
                        }

                        // Draw defend spot
                        if (step is DefendSpotModel)
                        {
                            DefendSpotModel model = (DefendSpotModel)step;
                            if (model.DefendPosition != null)
                            {
                                Radar3D.DrawCircle(model.DefendPosition, model.DefendSpotRadius, Color.MediumVioletRed, false, 200);
                            }
                        }

                        // Draw regroup spots
                        if (step is RegroupModel)
                        {
                            RegroupModel model = (RegroupModel)step;
                            if (model.RegroupSpot != null)
                            {
                                Radar3D.DrawCircle(model.RegroupSpot, 4f, Color.White, false, 200);
                            }
                        }

                        // Draw follow unit
                        if (step is FollowUnitModel)
                        {
                            FollowUnitModel model = (FollowUnitModel)step;
                            if (model.ExpectedStartPosition != null)
                            {
                                Radar3D.DrawCircle(model.ExpectedStartPosition, 2f, Color.LawnGreen, false, 200);
                            }
                            if (model.ExpectedEndPosition != null)
                            {
                                Radar3D.DrawCircle(model.ExpectedEndPosition, 2f, Color.LawnGreen, false, 200);
                            }
                            if (model.ExpectedStartPosition != null
                                && model.ExpectedEndPosition != null)
                            {
                                Radar3D.DrawLine(model.ExpectedStartPosition, model.ExpectedEndPosition, Color.LawnGreen, 50);
                            }
                            WoWUnit unitToEscort = ObjectManager.GetObjectWoWUnit().Find(unit => unit.Entry == model.UnitId);
                            if (unitToEscort != null)
                            {
                                Radar3D.DrawCircle(model.ExpectedEndPosition, 3f, Color.LawnGreen, true, 200);
                            }
                        }

                        // Draw moveAlong paths
                        if (step is MoveAlongPathModel)
                        {
                            var previousVector = new Vector3();
                            foreach (Vector3 node in ((MoveAlongPathModel)step).Path)
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
                        if (step.CompleteCondition.ConditionType == CompleteConditionType.LOSCheck)
                        {
                            if (step.CompleteCondition.LOSPositionVectorFrom != null)
                            {
                                Radar3D.DrawCircle(step.CompleteCondition.LOSPositionVectorFrom, 0.3f, Color.Magenta, true, 200);
                            }
                            if (step.CompleteCondition.LOSPositionVectorTo != null)
                            {
                                Radar3D.DrawCircle(step.CompleteCondition.LOSPositionVectorTo, 0.3f, Color.Magenta, true, 200);
                            }
                            if (step.CompleteCondition.LOSPositionVectorTo != null
                                && step.CompleteCondition.LOSPositionVectorFrom != null)
                            {
                                Radar3D.DrawLine(step.CompleteCondition.LOSPositionVectorFrom, step.CompleteCondition.LOSPositionVectorTo, Color.Magenta, 200);
                            }
                        }
                    }

                    // Draw death run paths
                    Color deadcolour = Color.Red;
                    Vector3 deadpreviousVector = new Vector3();
                    foreach (Vector3 node in currentProfile.DeathRunPath)
                    {
                        if (deadpreviousVector == new Vector3())
                        {
                            deadpreviousVector = node;
                        }
                        Radar3D.DrawCircle(node, 1f, deadcolour, true, 200);
                        Radar3D.DrawLine(node, deadpreviousVector, deadcolour, 200);
                        deadpreviousVector = node;
                    }

                    // Draw offmesh connections
                    foreach (var offmesh in currentProfile.OffMeshConnections)
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
            catch
            {
                //Main.logError("Failed to run the Monitor() function.");
            }
        }

        private void cbDungeon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DungeonModel)cbDungeon.SelectedItem) != null
                && Lists.AllDungeons.Exists(dungeon => dungeon.Name == ((DungeonModel)cbDungeon.SelectedItem).Name))
            {
                currentProfile.DungeonName = ((DungeonModel)cbDungeon.SelectedItem).Name;
                currentProfile.MapId = Lists.AllDungeons.Find(dungeon => dungeon.Name == currentProfile.DungeonName).MapId;
            }
        }

        private async void btnLoadProfile_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    //Debugger.Launch();
                    var filePath = openFileDialog1.FileName;
                    currentProfile = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(filePath), jsonSettings);
                    Setup();
                }
                catch (Exception ex)
                {
                    await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
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
                //Debugger.Launch();

                //foreach (var step in dgProfileSteps.SelectedItems)
                //{
                //    StepCollection.Remove((StepModel)step);
                //    currentProfile.StepModels = StepCollection.ToList();
                //}
                StepCollection.Remove((StepModel)dgProfileSteps.SelectedItem);
                RefreshStepOrder();
            }
        }

        private void RefreshStepOrder()
        {
            for (int i = 0; i < StepCollection.Count; i++)
            {
                StepCollection[i].Order = i;
            }
            currentProfile.StepModels = StepCollection.ToList();
            dgProfileSteps.ItemsSource = StepCollection;
        }

        private async void miMoveAlongPathStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    MoveAlongPathModel stepModel = new MoveAlongPathModel() { Name = x, Order = StepCollection.Count, Path = new List<Vector3>() };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miInteractWithStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    InteractWithModel stepModel = new InteractWithModel() { Name = x, Order = StepCollection.Count, InteractDistance = 3 };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miTalkToUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    TalkToUnitModel stepModel = new TalkToUnitModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miDefendSpotStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    DefendSpotModel stepModel = new DefendSpotModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miFollowUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    FollowUnitModel stepModel = new FollowUnitModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miPullToSafeSpotStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    PullToSafeSpotModel stepModel = new PullToSafeSpotModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miLeaveDungeonStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    LeaveDungeonModel stepModel = new LeaveDungeonModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void regroupStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    RegroupModel stepModel = new RegroupModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void jumpToStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    JumpToStepModel stepModel = new JumpToStepModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(stepModel);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        #endregion

        #region Add Deathrun
        private void btnAddDeathRunVector_Click(object sender, RoutedEventArgs e)
        {
            DeathrunCollection.Add(ObjectManager.Me.Position);
            currentProfile.DeathRunPath = DeathrunCollection.ToList();
        }

        private void btnDeleteDeathRunVector_Click(object sender, RoutedEventArgs e)
        {
            if (dgDeathrun.SelectedItem != null)
            {
                DeathrunCollection.Remove((Vector3)dgDeathrun.SelectedItem);
                currentProfile.DeathRunPath = DeathrunCollection.ToList();
            }
        }

        private async void AddDeathrunVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (gbDeathRun.IsVisible && (bool)chkRecordDeathRunPath.IsChecked && (DeathrunCollection.Count == 0 || DeathrunCollection.LastOrDefault().DistanceTo(ObjectManager.Me.Position) > 8))
                    {
                        DeathrunCollection.Add(ObjectManager.Me.Position);
                        currentProfile.DeathRunPath = DeathrunCollection.ToList();
                    }
                });
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }
        #endregion

        #region Add Offmesh
        private void btnOcAdd_Click(object sender, RoutedEventArgs e)
        {
            OffMeshCollection.Add(new PathFinder.OffMeshConnection() { Name = Usefuls.SubMapZoneName ?? Usefuls.MapZoneName, ContinentId = currentProfile.MapId, TryToUseEvenIfCanFindPathSuccess = true, Type = PathFinder.OffMeshConnectionType.Unidirectional });
            currentProfile.OffMeshConnections = OffMeshCollection.ToList();
        }

        private void btnOcDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOffmeshList.SelectedItem != null)
            {
                OffMeshCollection.Remove((PathFinder.OffMeshConnection)dgOffmeshList.SelectedItem);
                currentProfile.OffMeshConnections = OffMeshCollection.ToList();
            }
        }

        private void btnOCPAdd_Click(object sender, RoutedEventArgs e)
        {
            OffMeshPathCollection.Add(ObjectManager.Me.Position);
            currentProfile.OffMeshConnections.FirstOrDefault(x => x == dgOffmeshList.SelectedItem).Path = OffMeshPathCollection.ToList();
        }
        private void btnOCPDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOffmeshPath.SelectedItem != null)
            {
                OffMeshPathCollection.Remove((Vector3)dgOffmeshPath.SelectedItem);
                currentProfile.OffMeshConnections.FirstOrDefault(x => x == dgOffmeshList.SelectedItem).Path = OffMeshPathCollection.ToList();
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
                        $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }
        #endregion

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (e.Cancel) return;
            e.Cancel = !this.closeMe;
            if (this.closeMe) return;
            var result = await this.ShowMessageAsync("", "Are you sure you want to close?", MessageDialogStyle.AffirmativeAndNegative, basicDialogSettings);
            this.closeMe = result == MessageDialogResult.Affirmative;

            if (this.closeMe) this.Close();
        }

        private async void btnMoveStepUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProfileSteps.SelectedItem != null)
                {
                    RefreshStepOrder();
                    var currentOrder = ((StepModel)dgProfileSteps.SelectedItem).Order;
                    var closestStep = currentProfile.StepModels.OrderByDescending(x => x.Order).FirstOrDefault(y => y.Order < currentOrder);
                    if (closestStep != null)
                    {
                        var newOrder = closestStep.Order;
                        closestStep.Order = currentOrder;
                        ((StepModel)dgProfileSteps.SelectedItem).Order = newOrder;
                        StepCollection = new ObservableCollection<StepModel>(currentProfile.StepModels.OrderBy(x => x.Order));
                        dgProfileSteps.ItemsSource = StepCollection;
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void btnMoveStepDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProfileSteps.SelectedItem != null)
                {
                    RefreshStepOrder();
                    var currentOrder = ((StepModel)dgProfileSteps.SelectedItem).Order;
                    var closestStep = currentProfile.StepModels.OrderBy(x => x.Order).FirstOrDefault(y => y.Order > currentOrder);
                    if (closestStep != null)
                    {
                        var newOrder = closestStep.Order;
                        closestStep.Order = currentOrder;
                        ((StepModel)dgProfileSteps.SelectedItem).Order = newOrder;
                        StepCollection = new ObservableCollection<StepModel>(currentProfile.StepModels.OrderBy(x => x.Order));
                        dgProfileSteps.ItemsSource = StepCollection;
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
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
