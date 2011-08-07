using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
        }

        private void Command_OnAction(object sender, EventArgs e) {
            string command = (sender as ToolStripItem).Tag as string;
            switch (command) {
                case "New":
                    break;
            }
        }
    }
}
