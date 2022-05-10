using robotManager.Helpful;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WholesomeDungeonCrawler.Data.Model;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = System.Timers.Timer;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProfileStep.xaml
    /// </summary>
    public partial class ProfileStep : UserControl, INotifyPropertyChanged
    {
        private StepModel selectedItem;
        private static Timer addVectorTimer;
        public StepModel SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ObservableCollection<Vector3> fpsCollection { get; set; }
        public ProfileStep()
        {
            InitializeComponent();
            this.DataContext = this;
            addVectorTimer = new Timer(200);
            addVectorTimer.Elapsed += AddVectorTimer_Elapsed;
            addVectorTimer.AutoReset = true;
            addVectorTimer.Enabled = true;

        }

        private void AddVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(Conditions.InGameAndConnected)
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.IsVisible && (bool)chkRecordPath.IsChecked && (fpsCollection.Count == 0 || fpsCollection.LastOrDefault().DistanceTo(ObjectManager.Me.Position) > 8))
                        {
                            fpsCollection.Add(ObjectManager.Me.Position);
                            ((MoveAlongPathModel)SelectedItem).Path = fpsCollection.ToList();
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnAddVector_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                fpsCollection.Add(ObjectManager.Me.Position);
                ((MoveAlongPathModel)SelectedItem).Path = fpsCollection.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error message: {ex.Message}\n\n" +
                 $"Details:\n\n{ex.StackTrace}");
            }
        }

        private void btnDeleteVector_Click(object sender, RoutedEventArgs e)
        {
            if (dgFPS.SelectedItem != null)
            {
                fpsCollection.Remove((Vector3)dgFPS.SelectedItem);
                ((MoveAlongPathModel)SelectedItem).Path = fpsCollection.ToList();
            }
        }

        private void btnGetNearestGO_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                txtInteractO.Value = nearestGO.Entry;
            }
        }

        private void btnGetNearestGOPos_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                txtInteractPos.Text = $"{nearestGO.Position.X},{nearestGO.Position.Y},{nearestGO.Position.Z}";
            }
        }

        private void btnGotoGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var currentsPos = ObjectManager.Me;
            if (currentsPos != null)
            {
                txtGoToTargetPos.Text = $"{currentsPos.Position.X},{currentsPos.Position.Y},{currentsPos.Position.Z}";
            }
        }
        
    }    
}
