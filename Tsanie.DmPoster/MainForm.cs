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
using System.IO.Compression;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<DanmakuBase> _listDanmakus = new List<DanmakuBase>();
        private Dictionary<DanmakuBase, DataGridViewRow> _cacheRows = new Dictionary<DanmakuBase, DataGridViewRow>();
        private UserModel _user = UserModel.Guest;
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
        }

        #endregion

        #region - 私有方法 -

        private void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
            MessageBox.Show(this, message, title, buttons, icon);
        }
        private void EnabledUI(bool enabled, string userMessage, string message, Action action) {
            this.SafeRun(delegate {
                this.menuStrip.Enabled = enabled;
                foreach (ToolStripItem item in this.toolStrip.Items) {
                    if (item == toolButtonStop) {
                        if (action != null) {
                            toolButtonStop.ClickHandler = action;
                        }
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
        private void SetProgressState(TBPFLAG flag) {
            Program.Taskbar.SetProgressState(this, flag);
            switch (flag) {
                case TBPFLAG.TBPF_INDETERMINATE:
                    statusProgressBar.Visible = true;
                    statusProgressBar.Style = ProgressBarStyle.Marquee;
                    break;
                case TBPFLAG.TBPF_NOPROGRESS:
                    statusProgressBar.Visible = false;
                    break;
                case TBPFLAG.TBPF_ERROR:
                case TBPFLAG.TBPF_NORMAL:
                case TBPFLAG.TBPF_PAUSED:
                    statusProgressBar.Visible = true;
                    statusProgressBar.Style = ProgressBarStyle.Blocks;
                    break;
            }
            // 修正位置
            statusMessage.Spring = false;
            statusMessage.Spring = true;
        }
        private void SetProgressValue(int completed, int total) {
            if (completed < 0)
                completed = 0;
            if (completed > total)
                completed = total;
            Program.Taskbar.SetProgressValue(this, completed, total);
            if (statusProgressBar.Maximum != total)
                statusProgressBar.Maximum = total;
            statusProgressBar.Value = completed;
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
                            throw new Exception("检查身份返回不成功！" +
                                state.Response.StatusCode + ": " + state.Response.StatusDescription);
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
                                    user.Login = bool.Parse(value);
                                    break;
                                case "name":
                                    user.Name = value;
                                    break;
                                case "user":
                                    user.User = int.Parse(value);
                                    break;
                                case "scores":
                                    user.Scores = int.Parse(value);
                                    break;
                                case "money":
                                    user.Money = int.Parse(value);
                                    break;
                                case "pwd":
                                    user.Pwd = value;
                                    break;
                                case "isadmin":
                                    user.IsAdmin = bool.Parse(value);
                                    break;
                                case "permission":
                                    string[] ps = value.Split(',');
                                    Level[] levels = new Level[ps.Length];
                                    for (int i = 0; i < ps.Length; i++)
                                        levels[i] = (Level)int.Parse(ps[i]);
                                    user.Permission = levels;
                                    break;
                                case "level":
                                    user.Level = value;
                                    break;
                                case "shot":
                                    user.Shot = bool.Parse(value);
                                    break;
                                case "acceptaccel":
                                    user.AcceptAccel = bool.Parse(value);
                                    break;
                                case "server":
                                    user.Server = value;
                                    break;
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

        private void DownloadDanmaku(string avOrVid, Action<int> callback, Action<Exception> exCallback) {
            try {
                if (string.IsNullOrWhiteSpace(avOrVid))
                    throw new Exception("未输入Av或者Vid号！");

                int count = 0;
                Action<RequestState> stateCallback = (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception("下载弹幕返回不成功！" +
                            state.Response.StatusCode + ": " + state.Response.StatusDescription);

                    // timer
                    System.Timers.Timer timer = new System.Timers.Timer(Config.Interval);
                    timer.Elapsed += (sender, e) => {

                    };
                    // 读取压缩流
                    using (StreamReader reader = new StreamReader(new DeflateStream(state.StreamResponse, CompressionMode.Decompress))) {
                        StringBuilder builder = new StringBuilder(0x40);
                        Regex regex = new Regex("<d p=\"([^\"]+?)\">([^<]+?)</d>");
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            builder.AppendLine(line);
                            if (line.EndsWith("</d>")) {
                                foreach (Match match in regex.Matches(builder.ToString())) {
                                    string property = match.Groups[1].Value;
                                    string text = match.Groups[2].Value;
                                    // 读取属性
                                    string[] vals = property.Split(',');
                                    this.SafeRun(delegate {
                                        _listDanmakus.Add(new BiliDanmaku() {
                                            PlayTime = float.Parse(vals[0]),
                                            Mode = (DanmakuMode)int.Parse(vals[1]),
                                            Fontsize = int.Parse(vals[2]),
                                            Color = Color.FromArgb(int.Parse(vals[3]) | -16777216),
                                            Date = long.Parse(vals[4]).ParseDateTime(),
                                            Pool = int.Parse(vals[5]),
                                            UsID = vals[6],
                                            DmID = int.Parse(vals[7]),
                                            Text = text
                                        });
                                        count++;
                                    });
                                }
                                builder.Clear();
                            }
                        }
                        reader.Dispose();
                        if (callback != null)
                            callback(count);
                    }
                };
                if (avOrVid.StartsWith("av")) {
                    // 输入的是Av号
                    GetVidFromAv(avOrVid, (vid) => DownloadDanmakuFromVid(vid, stateCallback, exCallback), exCallback);
                } else {
                    // Vid
                    DownloadDanmakuFromVid(avOrVid, stateCallback, exCallback);
                }
            } catch (Exception e) {
                exCallback.SafeInvoke(e);
            }
        }
        private void GetVidFromAv(string av, Action<string> callback, Action<Exception> exCallback) {
            throw new Exception("还木有实现此功能！");
        }
        private void DownloadDanmakuFromVid(string vid, Action<RequestState> stateCallback, Action<Exception> exCallback) {
            _thread = HttpHelper.BeginConnect(Config.HttpHost + "/dm," + vid,
                (request) => {
                    request.Referer = Config.PlayerPath;
                }, stateCallback, exCallback);
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
                case "Download":
                    EnabledUI(false, null, null, delegate {
                        if (_thread != null && _thread.ThreadState == ThreadState.Running)
                            _thread.Abort();
                        EnabledUI(true, null, "中断下载弹幕...", null);
                    });
                    gridDanmakus.Enabled = true;
                    DownloadDanmaku(toolTextVid.Text, (count) => {
                        EnabledUI(true, null, string.Format("下载成功！一共 {0} 条弹幕。", count), null);
                    }, (ex) => {
                        this.ShowExceptionMessage(ex, "下载弹幕");
                        EnabledUI(true, null, "下载中断...", null);
                    });
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
