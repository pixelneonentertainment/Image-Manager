using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GorselYoneticisi
{
    public partial class DiskSelectionWindow : Window
    {
        public DiskSelectionWindow()
        {
            InitializeComponent();
            LoadDrives();
            chkTumunuSec.Checked += (s, e) => SetAllChecks(true);
            chkTumunuSec.Unchecked += (s, e) => SetAllChecks(false);
        }

        private void LoadDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)
                    .ToArray();

                foreach (var d in drives)
                {
                    var cb = new CheckBox
                    {
                        Content = $"{d.Name} ({d.DriveType})",
                        Tag = d.Name,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(4)
                    };
                    drivesPanel.Children.Add(cb);
                }
            }
            catch { }
        }

        private void SetAllChecks(bool val)
        {
            foreach (var child in drivesPanel.Children)
            {
                if (child is CheckBox cb) cb.IsChecked = val;
            }
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnTamam_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public string[] GetSelectedDrives()
        {
            try
            {
                return drivesPanel.Children
                    .OfType<CheckBox>()
                    .Where(cb => cb.IsChecked == true)
                    .Select(cb => cb.Tag as string)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();
            }
            catch { return new string[0]; }
        }
    }
}
