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
using Tsanie.UI;
using System.Threading;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<DanmakuBase> _listDanmakus = new List<DanmakuBase>();
        private Dictionary<DanmakuBase, ListViewItem> _cacheItems = new Dictionary<DanmakuBase, ListViewItem>();

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
            this.Text = Language.Untitled + " - " + Config.Title;
            this.dataGridView1.DataSource = _listDanmakus;
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
            lvi.SubItems[1].BackColor = danmaku.Color;
            return lvi;
        }

        private ListViewItem GetCacheItem(DanmakuBase danmaku) {
            ListViewItem result;
            if (_cacheItems.TryGetValue(danmaku, out result))
                return result;
            result = CreateItemFromDanmaku(danmaku);
            return result;
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
            // 设置 explorer 样式
            VistaStuff.SetWindowTheme(this.listDanmakus.Handle, "explorer", null);
            VistaStuff.SetWindowTheme(this.dataGridView1.Handle, "explorer", null);

            dataGridView1.Rows.Add("abc", "cc");
            dataGridView1.Rows.Add("abcfcdsc", "cc"); dataGridView1.Rows.Add("abc", "csdfsdfc");
            return;
            listDanmakus.Enabled = false;
            System.Timers.Timer timer = new System.Timers.Timer(20);
            Random rand = new Random();
            timer.Elapsed += delegate {
                if (_listDanmakus.Count > 50) {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                    this.SafeRun(delegate { listDanmakus.Enabled = true; });
                    return;
                }
                _listDanmakus.Add(new BiliDanmaku() {
                    Color = Color.FromArgb(-16777216 | rand.Next(16777216)),
                    Fontsize = rand.Next(1, 127),
                    Mode = DanmakuMode.That_beam_of_light,
                    PlayTime = 85.2f,
                    Text = "test"
                });
                this.SafeRun(delegate { listDanmakus.VirtualListSize = _listDanmakus.Count; });
            };
            timer.Start();
        }

        private void listDanmakus_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = GetCacheItem(_listDanmakus[e.ItemIndex]);
        }
    }
}
