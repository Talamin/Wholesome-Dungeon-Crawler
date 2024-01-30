﻿using robotManager.Helpful;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WholesomeDungeonCrawler.Helpers;
using WholesomeDungeonCrawler.Models;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static wManager.Wow.Helpers.PathFinderClass.FindPathPacket;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Timers.Timer;
using UserControl = System.Windows.Controls.UserControl;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProfileStep.xaml
    /// </summary>
    public partial class ProfileStep : UserControl, INotifyPropertyChanged
    {
        private StepModel selectedItem;
        private static Timer addVectorTimer;
        private static Vector3 selectedMAPNode;

        public StepModel SelectedItem
        {
            get
            {
                return selectedItem;
            }
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
            chkRecordPath.IsChecked = false;
            cbStepFaction.ItemsSource = Enum.GetValues(typeof(Npc.FactionType));
            cbStepRole.ItemsSource = Enum.GetValues(typeof(LFGRoles));
        }

        private void AddVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Conditions.InGameAndConnected)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (IsVisible
                            && (bool)chkRecordPath.IsChecked
                            && (fpsCollection.Count == 0 || fpsCollection.LastOrDefault().DistanceTo(ObjectManager.Me.Position) > 8))
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
                if (dgFPS.SelectedItem == null || dgFPS.SelectedIndex == fpsCollection.Count)
                {
                    fpsCollection.Add(ObjectManager.Me.Position);
                    dgFPS.SelectedIndex = fpsCollection.Count - 1;
                }
                else
                {
                    int index = dgFPS.SelectedIndex;
                    fpsCollection.Insert(index + 1, ObjectManager.Me.Position);
                    dgFPS.SelectedIndex = index + 1;
                }
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
                this.SelectedItem.CompleteCondition.MobAtPositionVector = target.Position;
                nudMobPosVector.Text = TextBoxVectorConverter.GetStringFromVector3(target.Position);
            }
        }

        private void btnGetTargetMobAttackableEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                nudMobAttackable.Value = target.Entry;
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

        private void btnTalkToUnitGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var targetPos = ObjectManager.Target.Position;
            if (targetPos != null)
            {
                ((TalkToUnitModel)this.SelectedItem).ExpectedPosition = targetPos;
                txtTalkToUnitTargetPos.Text = TextBoxVectorConverter.GetStringFromVector3(targetPos);
            }
        }

        private void btnGetTalkToUnitTargetEntry_Click(object sender, RoutedEventArgs e)
        {
            var target = ObjectManager.Target;
            if (target != null)
            {
                txtTalkToUnitId.Value = target.Entry;
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
        private void btnDefendVioletHoldGetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            var currentPos = ObjectManager.Me.Position;
            if (currentPos != null)
            {
                ((DefendVioletHoldModel)this.SelectedItem).DefendPosition = currentPos;
                txtDefendSpotTargetPos.Text = TextBoxVectorConverter.GetStringFromVector3(currentPos);
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

        private void btnPullToSafeSpotGetCurrentPosSafeSpot_Click(object sender, RoutedEventArgs e)
        {
            Vector3 myPos = ObjectManager.Me.Position;
            if (myPos != null)
            {
                ((PullToSafeSpotModel)this.SelectedItem).SafeSpotPosition = myPos;
                txtPullToSafeSpotSafePosition.Text = TextBoxVectorConverter.GetStringFromVector3(myPos);
            }
        }

        private void btnPullToSafeSpotGetCurrentPosZoneToClear_Click(object sender, RoutedEventArgs e)
        {
            Vector3 myPos = ObjectManager.Me.Position;
            if (myPos != null)
            {
                ((PullToSafeSpotModel)this.SelectedItem).ZoneToClearPosition = myPos;
                txtPullToSafeSpotZoneToClear.Text = TextBoxVectorConverter.GetStringFromVector3(myPos);
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

        private void DgFPSSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            selectedMAPNode = (Vector3)dgFPS.SelectedItem;
        }

        public void Monitor()
        {
            // Draw selected Move Along Path node
            if (selectedMAPNode != null)
            {
                Radar3D.DrawCircle(selectedMAPNode, 2f, Color.White, false, 200);
            }
        }
    }
}
