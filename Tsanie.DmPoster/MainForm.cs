using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tsanie.DmPoster.Danmaku;
using Tsanie.Utils;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<ListViewItem> _listDanmakus = new List<ListViewItem>();

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
            this.Text = Language.Untitled + " - " + Config.Title;
        }

        #endregion

        #region - 私有方法 -

        private void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
            MessageBox.Show(this, message, title, buttons, icon);
        }

        private ListViewItem CreateItemFromDanmaku(DanmakuBase danmaku) {
            ListViewItem lvi = new ListViewItem(new string[] {
                danmaku.PlayTime.ToTimeString(),
                danmaku.Color.ToColorString(),
                danmaku.Fontsize.ToString(),
                "",
                danmaku.Text,
                danmaku.Mode.ToString()
            });
            return lvi;
        }

        #endregion

        private void Command_OnAction(object sender, EventArgs e) {
            string command = (sender as ToolStripItem).Tag as string;
            switch (command) {
                case "Exit":
                    this.Close();
                    Application.Exit();
                    break;
                default:
                    this.ShowMessage("未实现: " + command, "事件触发", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e) {
            _listDanmakus.Add(new ListViewItem(new string[]{

            }));
        }

        private void listDanmakus_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _listDanmakus[e.ItemIndex];
        }
    }
}
