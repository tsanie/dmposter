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
using Tsanie.DmPoster.Models;
using Tsanie.Network;
using System.IO;
using System.Text.RegularExpressions;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<DanmakuBase> _listDanmakus = new List<DanmakuBase>();
        private Dictionary<DanmakuBase, DataGridViewRow> _cacheRows = new Dictionary<DanmakuBase, DataGridViewRow>();
        private UserModel _user = UserModel.Guest;
        private ITaskbarList3 _taskbar = null;
        private Thread _thread = null;

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Font = Program.UIFont;
            this.menuStrip.Font = Program.UIFont;
            this.toolStrip.Font = Program.UIFont;
            this.statusStrip.Font = Program.UIFont;
            this.gridDanmakus.DefaultCellStyle.Font = Program.UIFont;
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
                    MinimumWidth = 44,
                    Frozen = true },
                new TsDataGridViewColorColumn() {
                    Name = "datacolColor",
                    HeaderText = Language.ColumnColor,
                    Width = 74,
                    MinimumWidth = 64},
                new DataGridViewNumericUpDownColumn() {
                    Name = "datacolFontsize",
                    HeaderText = Language.ColumnFontsize,
                    ValueType = typeof(System.Int32),
                    Minimum = 1,
                    Maximum = 127,
                    Width = 44,
                    MinimumWidth = 36,
                    DefaultCellStyle = new DataGridViewCellStyle() { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolState",
                    HeaderText = "",
                    Width = 24,
                    MinimumWidth = 20,
                    ReadOnly = true },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolText",
                    HeaderText = Language.ColumnText,
                    Width = 320,
                    MinimumWidth = 62},
                new DataGridViewTextBoxColumn() {
                    Name = "datacolMode",
                    HeaderText = Language.ColumnMode,
                    Width = 120,
                    MinimumWidth = 120,
                    ReadOnly = true}
            });
            // Win7 超级任务栏
            if (Win7Stuff.IsWin7)
                _taskbar = (ITaskbarList3)new ProgressTaskbar();
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

        private void CheckLogin() {
            if (!string.IsNullOrEmpty(Config.Cookies)) {
                EnabledUI(false, "检查..", "正在访问", delegate {
                    if (_thread != null && _thread.ThreadState == ThreadState.Running)
                        _thread.Abort();
                    EnabledUI(true, "游客", "中断检查...", null);
                });
                // 登录 dad.php 检查权限
                _thread = HttpHelper.BeginConnect(Config.HttpHost + "/dad.php?r=" + Utility.Rnd.NextDouble(),
                    (request) => {
                        request.Headers["Cookie"] = Config.Cookies;
                    }, (state) => {
                        if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                            throw new Exception("检查身份返回不成功！");
                        StringBuilder result = new StringBuilder(0x40);
                        using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                result.Append(line);
                            }
                            reader.Dispose();
                        }
                        UserModel user = new UserModel() { Login = false };
                        Regex reg = new Regex("<([a-zA-Z^>]+)>([^<]+)</([a-zA-Z^>]+)>", RegexOptions.Singleline);
                        foreach (Match match in reg.Matches(result.ToString())) {
                            string key = match.Groups[1].Value;
                            string value = match.Groups[2].Value;
                            switch (key) {
                                case "login":
                                    user.Login = bool.Parse(value); break;
                                case "name":
                                    user.Name = value; break;
                                case "user":
                                    user.User = int.Parse(value); break;
                                case "scores":
                                    user.Scores = int.Parse(value); break;
                                case "money":
                                    user.Money = int.Parse(value); break;
                                case "pwd":
                                    user.Pwd = value; break;
                                case "isadmin":
                                    user.IsAdmin = bool.Parse(value); break;
                                case "permission":
                                    string[] ps = value.Split(',');
                                    Level[] levels = new Level[ps.Length];
                                    for (int i = 0; i < ps.Length; i++)
                                        levels[i] = (Level)int.Parse(ps[i]);
                                    user.Permission = levels;
                                    break;
                                case "level":
                                    user.Level = value; break;
                                case "shot":
                                    user.Shot = bool.Parse(value); break;
                                case "acceptaccel":
                                    user.AcceptAccel = bool.Parse(value); break;
                                case "server":
                                    user.Server = value; break;
                            }
                        }
                        if (user.Login) {
                            _user = user;
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.logined; });
                            EnabledUI(true, _user.Name + " (" + _user.Level + ")", "就绪", null);
                        } else {
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.guest; });
                            EnabledUI(true, _user.Name, "就绪", null);
                        }
                    }, (ex) => {
                        this.ShowExceptionMessage(ex, "检查身份");
                        EnabledUI(true, _user.Name, "检查身份失败。", null);
                    });
            } else {
                EnabledUI(true, _user.Name, "就绪", null);
            }
        }

        private void EnabledUI(bool enabled, string userMessage, string message, Action action) {
            this.SafeRun(delegate {
                this.menuStrip.Enabled = enabled;
                foreach (ToolStripItem item in this.toolStrip.Items) {
                    if (item == toolButtonStop) {
                        toolButtonStop.ClickHandler = action;
                        toolButtonStop.Enabled = !enabled;
                    } else {
                        item.Enabled = enabled;
                    }
                }
                this.gridDanmakus.Enabled = enabled;
                this.statusStrip.Enabled = enabled;
                if (userMessage != null)
                    statusAccount.Text = userMessage;
                if (message != null)
                    statusMessage.Text = message;
            });
        }

        #endregion

        private void Command_OnAction(object sender, EventArgs e) {
            string command = (sender as ToolStripItem).Tag as string;
            switch (command) {
                case "Login":
                    LoginForm login = new LoginForm();
                    DialogResult result = login.ShowDialog(this);
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        CheckLogin();
                    }
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
            CheckLogin();
            return;

            // 设置 explorer 样式
            Win7Stuff.SetWindowTheme(this.menuStrip.Handle, "explorer", null);

            System.Timers.Timer timer = new System.Timers.Timer(20);
            Random rand = new Random();
            timer.Elapsed += delegate {
                if (_listDanmakus.Count > 20) {
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
