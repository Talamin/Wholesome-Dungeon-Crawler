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
        public ObservableCollection<Vector3> DeathRunCollection { get; set; }
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
            InitializeComponent();
            Setup();

        }

        private void Setup()
        {
            //Debugger.Launch();

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
            StepCollection = new ObservableCollection<StepModel>(currentProfile.StepModels);
            dgProfileSteps.ItemsSource = StepCollection;


            Lists.AllDungeons.Sort((a, b) => a.Name.CompareTo(b.Name));
            cbDungeon.ItemsSource = Lists.AllDungeons;
            cbDungeon.SelectedValuePath = "MapId";
            cbDungeon.DisplayMemberPath = "Name";

            openFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select a profile",
                Filter = "JSON files (*.json)|*.json",
                Title = "Open profile",
                InitialDirectory = Others.GetCurrentDirectory + @"/Profiles/WholesomeDungeonCrawler"
            };
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
                Debugger.Break();
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
                    //if (((StepModel)dgProfileSteps.SelectedItem).Condition == null)
                    //    ((StepModel)dgProfileSteps.SelectedItem).Condition = new DungeonStepCondition();
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

                    

                    //var deadcolour = System.Drawing.Color.Red;
                    //var deadpreviousVector = new Vector3();
                    //foreach (var vec in currentProfile.)
                    //{
                    //    if (deadpreviousVector == new Vector3())
                    //    {
                    //        deadpreviousVector = vec;
                    //    }
                    //    Radar3D.DrawCircle(vec, 1f, deadcolour, true, 200);
                    //    Radar3D.DrawLine(vec, deadpreviousVector, deadcolour, 200);
                    //    deadpreviousVector = vec;
                    //}


                    //foreach (var offmesh in currentProfile.offMeshConnections)
                    //{
                    //    var offmeshcolour = System.Drawing.Color.Green;
                    //    var offmeshcpreviousVector = new Vector3();
                    //    foreach (var vec in offmesh.Path)
                    //    {
                    //        if (offmeshcpreviousVector == new Vector3())
                    //        {
                    //            offmeshcpreviousVector = vec;
                    //        }
                    //        Radar3D.DrawCircle(vec, 1f, offmeshcolour, true, 200);
                    //        Radar3D.DrawLine(vec, offmeshcpreviousVector, offmeshcolour, 200);
                    //        offmeshcpreviousVector = vec;
                    //    }
                    //}
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
    #endregion
}
