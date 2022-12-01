using robotManager.Helpful;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
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
                if (string.IsNullOrEmpty(txtInteractO.Text))
                {
                    txtInteractO.Text = nearestGO.Entry.ToString();
                }
                else
                {
                    txtInteractO.Text = txtInteractO.Text + ";" + nearestGO.Entry.ToString();
                }
            }
        }

        private void btnGetNearestGOPos_Click(object sender, RoutedEventArgs e)
        {
            var nearestGO = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetObjectWoWGameObject());
            if (nearestGO != null)
            {
                ((InteractWithModel)this.SelectedItem).ExpectedPosition = nearestGO.Position;
                txtInteractPos.Text = TextBoxVectorConverter.GetStringFromVector3(nearestGO.Position);
            }
        }

        private void btnGotoGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                ((GoToModel)this.SelectedItem).TargetPosition = currentPos;
                txtGoToTargetPos.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
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
                nudMobPosVector.Text = TextBoxVectorConverter.GetStringFromVector3(target.Position);
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
                txtMoveToUnitTargetPos.Text = TextBoxVectorConverter.GetStringFromVector3(targetPos);
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
                txtDefendSpotTargetPos.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
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
                txtPickupObjectPos.Text = TextBoxVectorConverter.GetStringFromVector3(nearestGO.Position);
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
                txtFollowUnitStartPos.Text = TextBoxVectorConverter.GetStringFromVector3(targetPos);
            }
        }

        private void btnFollowUnitGetCurrentPosEnd_Click(object sender, RoutedEventArgs e)
        {
            var targetPos = ObjectManager.Target.Position;
            if (targetPos != null)
            {
                ((FollowUnitModel)this.SelectedItem).ExpectedEndPosition = targetPos;
                txtFollowUnitEndPos.Text = TextBoxVectorConverter.GetStringFromVector3(targetPos);
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

        private void btnRegroupSpotMyPosition_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                ((RegroupModel)this.SelectedItem).RegroupSpot = currentPos;
                txtRegroupSpot.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
            }
        }

        private void btnGetLOSCheckPosVectorFrom_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                this.SelectedItem.CompleteCondition.LOSPositionVectorFrom = currentPos;
                txtLOSCheckPosFrom.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
            }
        }

        private void btnGetLOSCheckPosVectorTo_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                this.SelectedItem.CompleteCondition.LOSPositionVectorTo = currentPos;
                txtLOSCheckPosTo.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
            }
        }
    }
}
