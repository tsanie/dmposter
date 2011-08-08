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
        private Dictionary<DanmakuBase, DataGridViewRow> _cacheRows = new Dictionary<DanmakuBase, DataGridViewRow>();

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
            this.Text = Language.Untitled + " - " + Config.Title;
            // DataGridView 列初始化
            this.gridDanmakus.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewNumericUpDownColumn() {
                    Name = "datacolPlayTime",
                    HeaderText = Language.ColumnPlayTime,
                    ValueType = typeof(System.Single),
                    DecimalPlaces = 1,
                    Increment = 0.1M,
                    Maximum = 2147483647,
                    Width = 50,
                    Frozen = true },
                new TsDataGridViewColorColumn() {
                    Name = "datacolColor",
                    HeaderText = Language.ColumnColor,
                    Width = 80 },
                new DataGridViewNumericUpDownColumn() {
                    Name = "datacolFontsize",
                    HeaderText = Language.ColumnFontsize,
                    ValueType = typeof(System.Int32),
                    Minimum = 1,
                    Maximum = 127,
                    Width = 50,
                    DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolState",
                    HeaderText = "",
                    Width = 24, ReadOnly = true },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolText",
                    HeaderText = Language.ColumnText,
                    Width = 320 },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolMode",
                    HeaderText = Language.ColumnMode,
                    Width = 70 }
            });
        }

        #endregion

        #region - 私有方法 -

        private void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
            MessageBox.Show(this, message, title, buttons, icon);
        }

        private DataGridViewRow CreateRowFromDanmaku(DanmakuBase danmaku) {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.AddRange(new DataGridViewCell[] {
                new DataGridViewTextBoxCell() { Value = danmaku.PlayTime },
                new DataGridViewTextBoxCell() { Value = danmaku.Color },
                new DataGridViewTextBoxCell() { Value = danmaku.Fontsize },
                new DataGridViewTextBoxCell() { Value = "" },
                new DataGridViewTextBoxCell() { Value = danmaku.Text },
                new DataGridViewTextBoxCell() { Value = danmaku.Mode }
            });
            return row;
        }

        private DataGridViewRow GetCacheRow(DanmakuBase danmaku) {
            DataGridViewRow row;
            if (_cacheRows.TryGetValue(danmaku, out row))
                return row;
            row = CreateRowFromDanmaku(danmaku);
            return row;
        }

        #endregion

        private void Command_OnAction(object sender, EventArgs e) {
            string command = (sender as ToolStripItem).Tag as string;
            switch (command) {
                case "Add":
                    _listDanmakus.RemoveAt(1);
                    gridDanmakus.RowCount = _listDanmakus.Count;
                    gridDanmakus.Invalidate();
                    break;
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
            Win7Stuff.SetWindowTheme(this.gridDanmakus.Handle, "explorer", null);

            System.Timers.Timer timer = new System.Timers.Timer(20);
            Random rand = new Random();
            timer.Elapsed += delegate {
                if (_listDanmakus.Count > 50) {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                    return;
                }
                _listDanmakus.Add(new BiliDanmaku() {
                    Color = Color.FromArgb(-16777216 | rand.Next(16777216)),
                    Fontsize = rand.Next(1, 127),
                    Mode = DanmakuMode.That_beam_of_light,
                    PlayTime = 85.2f,
                    Text = "test"
                });
                this.SafeRun(delegate { gridDanmakus.RowCount = _listDanmakus.Count; });
            };
            timer.Start();
        }

        private void gridDanmakus_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e) {
            DataGridViewRow row = GetCacheRow(_listDanmakus[e.RowIndex]);
            e.Value = row.Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 1) {
                // Color
                gridDanmakus[1, e.RowIndex].Style.BackColor = (Color)e.Value;
            }
        }

        private void gridDanmakus_CellValuePushed(object sender, DataGridViewCellValueEventArgs e) {

        }
    }
}
