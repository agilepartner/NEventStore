using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NEventStore.Logging;

namespace NEventStore.Example.CF
{
    public partial class MainView : Form
    {

        public MainView()
        {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            Program.Store = Program.WireupEventStore();
        }

        private void MainView_Closing(object sender, CancelEventArgs e)
        {
            Program.Store.Dispose();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            Program.OpenOrCreateStream();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            Program.AppendToStream();
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            Program.TakeSnapshot();
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            Program.LoadFromSnapshotForwardAndAppend();
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            Program.CopyEventStoreToStorageCard();
        }


    }
}