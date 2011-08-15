using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tsanie.DmPoster {
    public partial class PlayerForm : Form {
        public PlayerForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.playerIcon;
            this.ClientSize = new Size(542, 384);
        }
    }
}
