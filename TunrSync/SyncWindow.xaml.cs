﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TunrSync.Data;

namespace TunrSync
{
    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        public SyncWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Change sync directory button event
        /// </summary>
        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.SelectedPath;
                    (DataContext as DataModel).Configuration.SyncPath = file;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }
        }

        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as DataModel).SyncAgent.Sync(); // Do it!
        }
    }
}
