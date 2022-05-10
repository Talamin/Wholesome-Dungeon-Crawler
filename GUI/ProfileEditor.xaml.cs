﻿using MahApps.Metro.Controls.Dialogs;
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
using WholesomeDungeonCrawler.Dungeonlogic;
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

        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as FrameworkElement;
            if (addButton != null)
            {
                addButton.ContextMenu.IsOpen = true;
            }
        }

        

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            currentProfile = new ProfileModel();
            currentProfile.StepModels = new List<StepModel>();
            Setup();
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debugger.Break();
                if (currentProfile.MapId > 0)
                {
                    //System.Windows.MessageBox.Show(currentProfile.DungeonModel.Name);
                    if (!string.IsNullOrWhiteSpace(currentProfile.Name))
                    {
                        var dungeon = Lists.AllDungeons.FirstOrDefault(x => x.MapId == currentProfile.MapId);
                        if (dungeon != null)
                        {
                            var rootpath = System.IO.Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/WholesomeDungeonCrawler/{dungeon.Name}");
                            currentProfile.StepModels = currentProfile.StepModels.OrderBy(x => x.Order).ToList();

                            var output = JsonConvert.SerializeObject(currentProfile, Formatting.Indented, jsonSettings);
                            var path = $@"{rootpath.FullName}/{currentProfile.Name}.json";
                            File.WriteAllText(path, output);
                            Setup();
                            System.Windows.MessageBox.Show("Saved to " + path);
                        }
                    }
                }
                else System.Windows.MessageBox.Show("Dungeon is null");

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error message: {ex.Message}\n\n" +
                $"Details:\n\n{ex.StackTrace}");
            }
        }

        private void dgProfileSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                    if (psControl.SelectedItem.StepType is MoveAlongPathModel)
                    {
                        psControl.fpsCollection = new ObservableCollection<Vector3>(((MoveAlongPathModel)psControl.SelectedItem.StepType).Path);
                        psControl.dgFPS.ItemsSource = psControl.fpsCollection;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private MD5 md5;
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
                    foreach (var step in currentProfile.StepModels.Where(x => x.StepType is MoveAlongPathModel))
                    {
                        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(step.Name));
                        var colour = System.Drawing.Color.FromArgb(hash[0], hash[1], hash[2]);
                        var previousVector = new Vector3();
                        foreach (var vec in ((MoveAlongPathModel)step.StepType).Path)
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

        private void btnLoadProfile_Click(object sender, RoutedEventArgs e)
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
                    System.Windows.MessageBox.Show($"Error message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        #region Add Steps
        private async void miMoveAlongPathStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                //System.Windows.MessageBox.Show(x);
                var pathStep = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new MoveAlongPathModel() { Path = new List<Vector3>() } };
                StepCollection.Add(pathStep);
                currentProfile.StepModels = StepCollection.ToList();
                //System.Windows.MessageBox.Show(currentProfile.Steps.Length.ToString());
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private async void miInteractWithStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                var Step = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new InteractWithModel() { ExpectedPosition=new Vector3() } };
                StepCollection.Add(Step);
                currentProfile.StepModels = StepCollection.ToList();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private async void miGoToStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                var Step = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new GoToModel() { TargetPosition = new Vector3()  } };
                StepCollection.Add(Step);
                currentProfile.StepModels = StepCollection.ToList();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private async void miExecuteStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                var Step = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new ExecuteModel() };
                StepCollection.Add(Step);
                currentProfile.StepModels = StepCollection.ToList();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private async void miMoveToUnitStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                var Step = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new MoveToUnitModel() { ExpectedPosition= new Vector3() } };
                StepCollection.Add(Step);
                currentProfile.StepModels = StepCollection.ToList();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private async void miPickupObjectStep_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var metroDialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Add",
                    NegativeButtonText = "Cancel",
                    AnimateHide = true,
                    AnimateShow = true,
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
                var Step = new StepModel() { Name = x, Order = StepCollection.Count, StepType = new PickupObjectModel() { ExpectedPosition = new Vector3()} };
                StepCollection.Add(Step);
                currentProfile.StepModels = StepCollection.ToList();
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }
        #endregion
    }
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
}
