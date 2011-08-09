using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tsanie.UI {
    public class TsToolStripButton : ToolStripButton {
        public Action ClickHandler { get; set; }

        protected override void OnClick(EventArgs e) {
            //base.OnClick(e);
            if (ClickHandler != null) {
                ClickHandler();
            }
        }
    }
}
