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
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Profiles.Base;
using robotManager.Helpful;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using wManager.Wow.Helpers;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : INotifyPropertyChanged
    {
        public OpenFileDialog openFileDialog1;
        private static Profile _currentProfile;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Step> sCollection { get; set; }
        public ObservableCollection<Vector3> drCollection { get; set; }
        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        public Profile currentProfile
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
            InitializeComponent();

            this.DataContext = this;
            currentProfile = new Profile(new LogicRunner());
            currentProfile.Steps = new Step[0];

            Setup();
        }

        private void Setup()
        {
            //Debugger.Launch();


            sCollection = new ObservableCollection<Step>(currentProfile.Steps);
            dgProfileSteps.ItemsSource = sCollection;


            Lists.AllDungeons.Sort((a, b) => a.Name.CompareTo(b.Name));
            cbDungeon.ItemsSource = Lists.AllDungeons;
            //cbDungeon.SelectedValuePath = "Name";
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
            //Debugger.Launch();
            //var metroDialogSettings = new MetroDialogSettings()
            //{
            //    AffirmativeButtonText = "Add",
            //    NegativeButtonText = "Cancel",
            //    AnimateHide = true,
            //    AnimateShow = true,
            //    ColorScheme = MetroDialogColorScheme.Theme
            //};
            //var x = await this.ShowInputAsync("Add", "Step", metroDialogSettings);
            //_currentProfile.Steps.Add(new MoveAlongPath(new List<Vector3>(),x));
        }

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
                sCollection.Add(new MoveAlongPath(new List<Vector3>(), x));
                currentProfile.Steps = sCollection.ToArray();
                //System.Windows.MessageBox.Show(currentProfile.Steps.Length.ToString());
            }
            catch (Exception Ex)
            {
                System.Windows.MessageBox.Show(Ex.Message);
            }
        }

        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            currentProfile = new Profile(new LogicRunner());
            currentProfile.Steps = new Step[0];
            Setup();
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debugger.Launch();
                if (currentProfile.Dungeon != null)
                {
                    System.Windows.MessageBox.Show(currentProfile.Dungeon.Name);
                    if (!string.IsNullOrWhiteSpace(currentProfile.Name))
                    {
                        var rootpath = System.IO.Directory.CreateDirectory($@"{Others.GetCurrentDirectory}/Profiles/WholesomeDungeonCrawler/{currentProfile.Dungeon.Name}");
                        currentProfile.Steps = currentProfile.Steps.OrderBy(x => x.Order).ToArray();

                        var output = JsonConvert.SerializeObject(currentProfile, Formatting.Indented, jsonSettings);
                        var path = $@"{rootpath.FullName}/{currentProfile.Name}.json";
                        File.WriteAllText(path, output);
                        Setup();
                        System.Windows.MessageBox.Show("Saved to " + path);
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
                    foreach (var step in currentProfile.Steps.Where(x => x.GetType() == typeof(MoveAlongPath)))
                    {
                        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(step.Name));
                        var colour = System.Drawing.Color.FromArgb(hash[0], hash[1], hash[2]);
                        var previousVector = new Vector3();
                        foreach (var vec in ((MoveAlongPath)step).Path)
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
