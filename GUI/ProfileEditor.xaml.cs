using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WholesomeDungeonCrawler.Helpers;
using robotManager.Helpful;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using wManager.Wow.Helpers;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using WholesomeDungeonCrawler.Data.Model;
using wManager.Wow.ObjectManager;
using wManager.Wow.Class;

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
            cbDungeon.SelectedValuePath = "MapId";
            cbDungeon.DisplayMemberPath = "Name";
            cbDungeon.SelectedValue = Usefuls.ContinentId;

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
                InitialDirectory = Others.GetCurrentDirectory + @"/Profiles/WholesomeDungeonCrawler"
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
                //Debugger.Break();
                if (currentProfile.MapId > 0)
                {
                    if (!string.IsNullOrWhiteSpace(currentProfile.Name))
                    {
                        var dungeon = Lists.AllDungeons.FirstOrDefault(x => x.MapId == currentProfile.MapId);
                        if (dungeon != null)
                        {
                            var rootpath = System.IO.Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/WholesomeDungeonCrawler/{dungeon.Name}");
                            currentProfile.StepModels = currentProfile.StepModels.OrderBy(x => x.Order).ToList();

                            var output = JsonConvert.SerializeObject(currentProfile, Formatting.Indented, jsonSettings);
                            var path = $@"{rootpath.FullName}\{currentProfile.Name}.json";
                            File.WriteAllText(path, output);
                            Setup();

                            
                            await this.ShowMessageAsync("Profile Saved!", "Saved to " + path, MessageDialogStyle.Affirmative, basicDialogSettings);
                        }
                    }
                    else 
                    {
                        await this.ShowMessageAsync("Save Failed.", "Profile Name has not been set.", MessageDialogStyle.Affirmative, basicDialogSettings);
                    }
                }
                else
                {
                    await this.ShowMessageAsync("Save Failed.", "Dungeon has not been set.", MessageDialogStyle.Affirmative, basicDialogSettings);
                }

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
            }
            else
            {
                Radar3D.OnDrawEvent -= new Radar3D.OnDrawHandler(Monitor);
                Radar3D.Stop();
            }

        }

        public void Monitor()
        {
            try
            {
                if (Conditions.InGameAndConnected)
                {
                    foreach (var step in currentProfile.StepModels.Where(x => x is MoveAlongPathModel))
                    {
                        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(step.Name));
                        var colour = System.Drawing.Color.FromArgb(hash[0], hash[1], hash[2]);
                        var previousVector = new Vector3();
                        foreach (var vec in ((MoveAlongPathModel)step).Path)
                        {
                            if (previousVector == new Vector3())
                            {
                                previousVector = vec;
                            }
                            Radar3D.DrawCircle(vec, 1f, colour, true, 200);
                            Radar3D.DrawLine(vec, previousVector, colour, 200);
                            previousVector = vec;
                        }
                    }



                    var deadcolour = System.Drawing.Color.Red;
                    var deadpreviousVector = new Vector3();
                    foreach (var vec in currentProfile.DeathRunPath)
                    {
                        if (deadpreviousVector == new Vector3())
                        {
                            deadpreviousVector = vec;
                        }
                        Radar3D.DrawCircle(vec, 1f, deadcolour, true, 200);
                        Radar3D.DrawLine(vec, deadpreviousVector, deadcolour, 200);
                        deadpreviousVector = vec;
                    }


                    foreach (var offmesh in currentProfile.OffMeshConnections)
                    {
                        var offmeshcolour = System.Drawing.Color.Green;
                        var offmeshcpreviousVector = new Vector3();
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
            //System.Windows.MessageBox.Show($"{((Dungeon)cbDungeon.SelectedItem).DungeonId} {((Dungeon)cbDungeon.SelectedItem).Name}");

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
                currentProfile.StepModels = StepCollection.ToList();
            }
        }

        private async void miMoveAlongPathStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if(x != null)
                {
                    var pathStep = new MoveAlongPathModel() { Name = x, Order = StepCollection.Count, Path = new List<Vector3>() };
                    StepCollection.Add(pathStep);
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
                    var Step = new InteractWithModel() { Name = x, Order = StepCollection.Count, ExpectedPosition = new Vector3() };
                    StepCollection.Add(Step);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miGoToStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    var Step = new GoToModel() { Name = x, Order = StepCollection.Count, TargetPosition = new Vector3() };
                    StepCollection.Add(Step);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miExecuteStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    var Step = new ExecuteModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(Step);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miMoveToUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    var Step = new MoveToUnitModel() { Name = x, Order = StepCollection.Count, ExpectedPosition = new Vector3() };
                    StepCollection.Add(Step);
                    currentProfile.StepModels = StepCollection.ToList();
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error.", $"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}", MessageDialogStyle.Affirmative, basicDialogSettings);
            }
        }

        private async void miPickupObjectStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = await this.ShowInputAsync("Add", "Step", addDialogSettings);
                if (x != null)
                {
                    var Step = new PickupObjectModel() { Name = x, Order = StepCollection.Count, ExpectedPosition = new Vector3() };
                    StepCollection.Add(Step);
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
                    var Step = new DefendSpotModel() { Name = x, Order = StepCollection.Count };
                    StepCollection.Add(Step);
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
                    var Step = new FollowUnitModel() { Name = x, Order = StepCollection.Count, ExpectedStartPosition = new Vector3(), ExpectedEndPosition = new Vector3() };
                    StepCollection.Add(Step);
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
            if((int)value >= 0)
                return System.Windows.Visibility.Visible;
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
    #endregion
}
