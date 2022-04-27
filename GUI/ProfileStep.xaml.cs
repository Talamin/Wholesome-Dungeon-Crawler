using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using WholesomeDungeonCrawler.Dungeonlogic;
using WholesomeDungeonCrawler.Profiles.Base;
using wManager.Wow.ObjectManager;
using Timer = System.Timers.Timer;

namespace WholesomeDungeonCrawler.GUI
{
    /// <summary>
    /// Interaction logic for ProfileStep.xaml
    /// </summary>
    public partial class ProfileStep : UserControl, INotifyPropertyChanged
    {
        private Step selectedItem;
        private static Timer addVectorTimer;
        public Step SelectedItem
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
            //addVectorTimer.Elapsed += AddVectorTimer_Elapsed;
            addVectorTimer.AutoReset = true;
            addVectorTimer.Enabled = true;

        }

        //private void AddVectorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            if (this.IsVisible && (bool)chkRecordPath.IsChecked && (fpsCollection.Count == 0 || fpsCollection.LastOrDefault().DistanceTo(ObjectManager.Me.Position) > 8))
        //            {
        //                fpsCollection.Add(ObjectManager.Me.Position);
        //                ((FollowPathStep)SelectedItem.StepType).Path = fpsCollection.ToList();
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error message: {ex.Message}\n\n" +
        //            $"Details:\n\n{ex.StackTrace}");
        //    }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnAddVector_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                fpsCollection.Add(ObjectManager.Me.Position);
                ((MoveAlongPath)SelectedItem).Path = fpsCollection.ToList();
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
                ((MoveAlongPath)SelectedItem).Path = fpsCollection.ToList();
            }
        }

    }
}
