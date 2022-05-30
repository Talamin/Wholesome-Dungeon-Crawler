using robotManager.Helpful;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Data;
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
            cbConditionType.ItemsSource = Enum.GetValues(typeof(CompleteConditionType));

        }

        private void AddVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Conditions.InGameAndConnected)
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
                ((InteractWithModel)this.SelectedItem).ExpectedPosition = nearestGO.Position;
                txtInteractPos.Text = $"{nearestGO.Position.X},{nearestGO.Position.Y},{nearestGO.Position.Z}";
            }
        }

        private void btnGotoGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                ((GoToModel)this.SelectedItem).TargetPosition = currentPos;
                txtGoToTargetPos.Text = $"{currentPos.X},{currentPos.Y},{currentPos.Z}";
            }
        }

        private void btnGetNearestGOFlags_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                nudInitialFlags.Value = nearestGO.FlagsInt;
            }
        }

        private void btnGetTargetMobPosEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                nudMobPosId.Value = target.Entry;
            }
        }

        private void btnGetTargetMobPosVector_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                this.SelectedItem.CompleteCondition.MobPositionVector = target.Position;
                nudMobPosVector.Text = $"{target.Position.X},{target.Position.Y},{target.Position.Z}";
            }
        }

        private void btnGetTargetMobDeadEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                nudMobDeadId.Value = target.Entry;
            }
        }

        private void btnGetNearestGOId_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                nudGameObject.Value = nearestGO.Entry;
            }
        }

        private void btnMoveToUnitGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var targetPos = ObjectManager.Target.Position;
            if (targetPos != null)
            {
                ((MoveToUnitModel)this.SelectedItem).ExpectedPosition = targetPos;
                txtMoveToUnitTargetPos.Text = $"{targetPos.X},{targetPos.Y},{targetPos.Z}";
            }
        }

        private void btnGetTargetEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                txtMoveToUnitId.Value = target.Entry;
            }
        }

        private void btnDefendSpotGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                ((DefendSpotModel)this.SelectedItem).DefendPosition = currentPos;
                txtDefendSpotTargetPos.Text = $"{currentPos.X},{currentPos.Y},{currentPos.Z}";
            }
        }

        private void btnPickupObjectGetNearestGO_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                txtPickupObjectObjectId.Value = nearestGO.Entry;
            }
        }

        private void btnPickupObjectGetNearestGOPos_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                ((PickupObjectModel)this.SelectedItem).ExpectedPosition = nearestGO.Position;
                txtPickupObjectPos.Text = $"{nearestGO.Position.X},{nearestGO.Position.Y},{nearestGO.Position.Z}";
            }
        }

        private void btnGetFollowUnitId_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                txtFollowUnitId.Value = target.Entry;
            }
        }

        private void btnFollowUnitGetCurrentPosStart_Click(object sender, RoutedEventArgs e)
        {
            var targetPos = ObjectManager.Target.Position;
            if (targetPos != null)
            {
                ((FollowUnitModel)this.SelectedItem).ExpectedStartPosition = targetPos;
                txtFollowUnitStartPos.Text = $"{targetPos.X},{targetPos.Y},{targetPos.Z}";
            }
        }

        private void btnFollowUnitGetCurrentPosEnd_Click(object sender, RoutedEventArgs e)
        {
            var targetPos = ObjectManager.Target.Position;
            if (targetPos != null)
            {
                ((FollowUnitModel)this.SelectedItem).ExpectedEndPosition = targetPos;
                txtFollowUnitEndPos.Text = $"{targetPos.X},{targetPos.Y},{targetPos.Z}";
            }
        }

        private void btnGetLOSCheckPosVector_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                this.SelectedItem.CompleteCondition.LOSPositionVector = currentPos;
                txtLOSCheckPos.Text = $"{currentPos.X},{currentPos.Y},{currentPos.Z}";
            }
        }

        private void btnGetCanGossipEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                nudCanGossipId.Value = target.Entry;
            }
        }
    }
}
