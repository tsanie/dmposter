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
using System.Xml;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<DanmakuBase> _listDanmakus = new List<DanmakuBase>();
        private Dictionary<DanmakuBase, DataGridViewRow> _cacheRows = new Dictionary<DanmakuBase, DataGridViewRow>();
        private UserModel _user = null;
        private RequestState _state = null;
        private DataGridViewColumn _lastOrderColumn = null;
        private FileState _fileState = FileState.Untitled;
        private string _fileName = null;

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
            // DataGridView 列初始化
            #region - gridDanmakus -
            this.gridDanmakus.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewNumericUpDownColumn() {
                    Name = "datacolPlayTime",
                    ValueType = typeof(System.Single),
                    DecimalPlaces = 1,
                    Increment = 0.1M,
                    Maximum = 2147483647,
                    Width = 50,
                    MinimumWidth = 44,
                    Frozen = true },
                new TsDataGridViewColorColumn() {
                    Name = "datacolColor",
                    Width = 74,
                    MinimumWidth = 64},
                new DataGridViewNumericUpDownColumn() {
                    Name = "datacolFontsize",
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
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle() { Font = new Font("Webdings", 9f) } },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolText",
                    Width = 1000,
                    MinimumWidth = 62},
                new DataGridViewTextBoxColumn() {
                    Name = "datacolMode",
                    Width = 120,
                    MinimumWidth = 120,
                    ReadOnly = true}
            });
            #endregion
            // 文字
            LoadMainUIText();
        }

        #endregion

        #region - 私有方法 -

        private void ChangeFileState(FileState fileState) {
            _fileState = fileState;
            if (_fileName == null)
                this.Text = Language.Lang["Untitled"];
            else
                this.Text = _fileName.Substring(_fileName.LastIndexOf('\\') + 1);
            if (fileState == FileState.Changed)
                this.Text += "*";
            this.Text += " - " + Config.Title;
        }
        private void EnumMenuItem(ToolStripMenuItem item) {
            foreach (ToolStripItem it in item.DropDownItems) {
                if (it is ToolStripMenuItem) {
                    this.SafeRun(delegate { it.Text = Language.Lang[it.Name]; });
                    EnumMenuItem((ToolStripMenuItem)it);
                }
            }
        }
        private void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
            this.SafeRun(delegate { MessageBox.Show(this, message, title, buttons, icon); });
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
                //this.statusStrip.Enabled = enabled;
                if (userMessage != null)
                    statusAccount.Text = userMessage;
                if (message != null)
                    statusMessage.Text = message;
            });
        }
        private void SetProgressState(TBPFLAG flag) {
            this.SafeRun(delegate {
                Program.Taskbar.SetProgressState(this, flag);
                // 修正位置
                statusMessage.Spring = false;
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
                statusMessage.Width = 1;
                statusMessage.Spring = true;
            });
        }
        private void SetProgressValue(int completed, int total) {
            if (completed < 0)
                completed = 0;
            if (completed > total)
                completed = total;
            this.SafeRun(delegate {
                Program.Taskbar.SetProgressValue(this, completed, total);
                if (statusProgressBar.Maximum != total)
                    statusProgressBar.Maximum = total;
                statusProgressBar.Value = completed;
            });
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
            if (!string.IsNullOrEmpty(Config.Instance.Cookies)) {
                EnabledUI(false, Language.Lang["CheckLogin"], Language.Lang["Communicating"], delegate {
                    _state.Cancel();
                    _state = null;
                });
                SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
                // 登录 dad.php 检查权限
                _state = HttpHelper.BeginConnect(Config.Instance.HttpHost + "/dad.php?r=" + Utility.Rnd.NextDouble(),
                    (request) => {
                        request.Headers["Cookie"] = Config.Instance.Cookies;
                    }, (state) => {
                        if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                            throw new Exception(Language.Lang["CheckLogin.StatusNotOK"] +
                                state.Response.StatusCode + ": " + state.Response.StatusDescription);
                        StringBuilder result = new StringBuilder(0x40);
                        using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                            if (state.IsCancelled())
                                throw new CancelledException(state, "CheckLogin.Interrupt");
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
                            #region - 填充 key/value -
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
                                case "chatid":
                                    user.ChatID = int.Parse(value);
                                    break;
                                case "aid":
                                    user.Aid = int.Parse(value);
                                    break;
                                case "pid":
                                    user.Pid = int.Parse(value);
                                    break;
                                case "acceptguest":
                                    user.AcceptGuest = bool.Parse(value);
                                    break;
                                case "duration":
                                    user.Duration = value;
                                    break;
                                case "acceptaccel":
                                    user.AcceptAccel = bool.Parse(value);
                                    break;
                                case "cache":
                                    user.Cache = bool.Parse(value);
                                    break;
                                case "server":
                                    user.Server = value;
                                    break;
                            }
                            #endregion
                        }
                        if (user.Login) {
                            _user = user;
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.logined; });
                            EnabledUI(true, _user.Name + " (" + _user.Level + ")", Language.Lang["Done"], null);
                        } else {
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.guest; });
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang["Done"], null);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    }, (ex) => {
                        if (ex is CancelledException) {
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang[((CancelledException)ex).Command], null);
                        } else {
                            this.ShowExceptionMessage(ex, Language.Lang["CheckLogin"]);
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang["CheckLogin.Failed"], null);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    });
            } else {
                EnabledUI(true, Language.Lang["Guest"], Language.Lang["Done"], null);
            }
        }

        private void DownloadDanmaku(string avOrVid, Action<int, int> callback, Action<Exception> exCallback) {
            Action refresher = delegate {
                this.SafeRun(delegate { gridDanmakus.RowCount = _listDanmakus.Count; });
            };
            // timer
            System.Timers.Timer timer = new System.Timers.Timer(Config.Interval);
            timer.Elapsed += (sender, e) => { refresher(); };
            try {
                if (string.IsNullOrWhiteSpace(avOrVid))
                    throw new Exception(Language.Lang["AvVidEmpty"]);

                #region - state callback -
                Action<RequestState> stateCallback = (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception(Language.Lang["DownloadDanmaku.StatusNotOK"] +
                            state.Response.StatusCode + ": " + state.Response.StatusDescription);
                    this.SafeRun(delegate {
                        gridDanmakus.RowCount = 0;
                        gridDanmakus.Enabled = true;
                        ChangeFileState(FileState.Changed);
                    });
                    _listDanmakus.Clear();
                    timer.Start();
                    // 读取压缩流
                    using (StreamReader reader = new StreamReader(new DeflateStream(state.StreamResponse, CompressionMode.Decompress))) {
                        StringBuilder builder = new StringBuilder(0x40);
                        Regex regex = new Regex("<d p=\"([^\"]+?)\">([^<]+?)</d>");
                        string line;
                        int count = 0;
                        int failed = 0;  // 失败的弹幕数
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Dispose();
                                timer.Close();
                                refresher();
                                throw new CancelledException(state, "DownloadDanmaku.Interrupt");
                            }
                            builder.AppendLine(line);
                            if (line.EndsWith("</d>")) {
                                foreach (Match match in regex.Matches(builder.ToString())) {
                                    string property = match.Groups[1].Value;
                                    string text = match.Groups[2].Value;
                                    // 读取属性
                                    string[] vals = property.Split(',');
                                    try {
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
                                    } catch (Exception e) {
                                        LogUtil.Error(new DanmakuException(e.Message + "\n" + builder.ToString(), e), null);
                                        failed++;
                                    }
                                }
                                builder.Clear();
                            }
                        }
                        reader.Dispose();
                        timer.Close();
                        refresher();
                        if (callback != null)
                            callback(count, failed);
                    }
                };
                #endregion
                if (avOrVid.StartsWith("av")) {
                    if (avOrVid.Length == 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    avOrVid = avOrVid.Substring(2);
                    string[] pars = avOrVid.Split(',');
                    if (pars.Length > 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    int aid = int.Parse(pars[0]);
                    int pageno = (pars.Length > 1 ? int.Parse(pars[1]) : 1);
                    // 输入的是Av号
                    GetVidFromAv(aid, pageno, (vid) => DownloadDanmakuFromVid(vid, stateCallback, exCallback), exCallback);
                } else {
                    int.Parse(avOrVid);
                    // Vid
                    DownloadDanmakuFromVid(avOrVid, stateCallback, exCallback);
                }
            } catch (Exception e) {
                timer.Close();
                refresher();
                exCallback.SafeInvoke(e);
            }
        }
        private void GetVidFromAv(int aid, int pageno, Action<string> callback, Action<Exception> exCallback) {
            EnabledUI(false, null, string.Format(Language.Lang["GetVidOfAv"], aid + "," + pageno), delegate {
                _state.Cancel();
                _state = null;
            });
            string url = Config.Instance.HttpHost + string.Format("/plus/view.php?aid={0}&pageno={1}", aid, pageno);
            _state = HttpHelper.BeginConnect(url,
                (request) => {
                    request.Referer = url;
                    request.Headers["Cookie"] = Config.Instance.Cookies;
                }, (state) => {
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Dispose();
                                throw new CancelledException(state, "GetVidOfAv.Interrupt");
                            }
                            int index = line.IndexOf("flashvars=\"");
                            if (index > 0) {
                                line = line.Substring(index + 11);
                                line = line.Substring(0, line.IndexOf('\"'));
                                line = Utility.UrlDecode(line);
                                foreach (string pair in line.Split('&')) {
                                    index = pair.IndexOf("id=");
                                    if (index >= 0) {
                                        line = line.Substring(index + 3);
                                        reader.Dispose();
                                        if (callback != null)
                                            callback(line);
                                        return;
                                    }
                                }
                            }
                        }
                        reader.Dispose();
                        throw new CancelledException(state, "GetVidOfAv.Failed");
                    }
                }, exCallback);
        }
        private void DownloadDanmakuFromVid(string vid, Action<RequestState> stateCallback, Action<Exception> exCallback) {
            EnabledUI(false, null, string.Format(Language.Lang["DownloadDanmakuStatus"], vid), delegate {
                _state.Cancel();
                _state = null;
            });
            _state = HttpHelper.BeginConnect(Config.Instance.HttpHost + "/dm," + vid,
                (request) => {
                    request.Referer = Config.PlayerPath;
                }, stateCallback, exCallback);
        }

        private bool SaveFile() {
            if (_fileName == null)
                return SaveFileAs();
            if (_fileState != FileState.Changed)
                return false;
            return SaveFilename(_fileName);
        }
        private bool SaveFileAs() {
            if (_fileState != FileState.Changed)
                return false;
            // 新文件
            SaveFileDialog dialog = new SaveFileDialog() {
                DefaultExt = ".xml",
                Filter = "弹幕文件 (*.xml)|*.xml|所有文件|*.*",
                Title = "保存弹幕文件"
            };
            if (dialog.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) {
                dialog.Dispose();
                return false;
            }
            _fileName = dialog.FileName;
            dialog.Dispose();
            return SaveFilename(_fileName);
        }
        private bool SaveFilename(string filename) {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(filename, settings);
            writer.WriteStartElement("information");
            foreach (DanmakuBase danmaku in _listDanmakus) {
                writer.WriteStartElement("data");
                // playTime
                writer.WriteStartElement("playTime");
                writer.WriteString(danmaku.PlayTime.ToString());
                writer.WriteEndElement();
                // message
                writer.WriteStartElement("message");
                writer.WriteAttributeString("fontsize", danmaku.Fontsize.ToString());
                writer.WriteAttributeString("color", danmaku.Color.ToRgbIntString());
                writer.WriteAttributeString("mode", ((int)danmaku.Mode).ToString());
                writer.WriteString(Utility.UrlEncode(danmaku.Text));
                writer.WriteEndElement();
                // times
                writer.WriteStartElement("times");
                writer.WriteString(danmaku.Date.ToString(Config.DateFormat));
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            writer = null;
            ChangeFileState(FileState.Saved);
            return true;
        }

        #endregion

        #region - 公共方法 -

        /// <summary>
        /// 装载主体 UI 文字
        /// </summary>
        public void LoadMainUIText() {
            // 主要界面文字
            this.Font = Config.Instance.UIFont;
            this.menuStrip.Font = Config.Instance.UIFont;
            this.toolStrip.Font = Config.Instance.UIFont;
            this.toolTextVid.Font = Config.Instance.UIFont;
            this.statusStrip.Font = Config.Instance.UIFont;
            this.gridDanmakus.DefaultCellStyle.Font = Config.Instance.WidthFont;

            this.Text = Language.Lang["Untitled"] + " - " + Config.Title;
            foreach (ToolStripMenuItem item in menuStrip.Items) {
                item.Text = Language.Lang[item.Name];
            }
            toolButtonPost.Text = Language.Lang["toolButtonPost"];
            foreach (DataGridViewColumn column in gridDanmakus.Columns) {
                if (column.Name != "datacolState")
                    column.HeaderText = Language.Lang[column.Name];
            }
        }

        /// <summary>
        /// 装载深层 UI 文字
        /// </summary>
        public void LoadUIText() {
            new Thread(delegate() {
                foreach (ToolStripMenuItem item in menuStrip.Items) {
                    EnumMenuItem(item);
                }
                foreach (ToolStripItem item in toolStrip.Items) {
                    if (item is ToolStripButton) {
                        this.SafeRun(delegate { item.Text = Language.Lang[item.Name]; });
                    } else if (item is ToolStripSplitButton) {
                        this.SafeRun(delegate {
                            item.ToolTipText = Language.Lang[item.Name + "_ToolTipText"];
                        });
                    }
                }
            }) { Name = "threadLoadUIText" }.Start();
        }

        #endregion

        #region - 事件 -

        private void Command_OnAction(object sender, EventArgs e) {
            string command = (sender as ToolStripItem).Tag as string;
            switch (command) {
                case "Login":
                    #region - 登录 -
                    LoginForm login = new LoginForm();
                    DialogResult result = login.ShowDialog(this);
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        CheckLogin();
                    }
                    break;
                    #endregion
                case "Download":
                    #region - 下载 -
                    SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
                    DownloadDanmaku(toolTextVid.Text.ToLower(), (count, failed) => {
                        if (failed > 0) {
                            ShowMessage(string.Format(Language.Lang["FailedCreateDanmakus"], failed), Language.Lang["DownloadDanmaku"],
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                        EnabledUI(true, null, string.Format(Language.Lang["DownloadDanmakuSucceed"], count), null);
                    }, (ex) => {
                        if (ex is CancelledException) {
                            EnabledUI(true, null, Language.Lang[((CancelledException)ex).Command], null);
                        } else {
                            this.ShowExceptionMessage(ex, Language.Lang["DownloadDanmaku"]);
                            EnabledUI(true, null, Language.Lang["DownloadDanmaku.Interrupt"], null);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    });
                    break;
                    #endregion
                case "Save":
                    SaveFile();
                    break;
                case "SaveAs":
                    SaveFileAs();
                    break;
                case "Exit":
                    #region - 退出 -
                    this.Close();
                    Application.Exit();
                    break;
                    #endregion
                default:
                    this.ShowMessage(Language.Lang["NotImplemented"] + ": " + command, Language.Lang["Event"],
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e) {
#if DEBUG
            System.Timers.Timer timer = new System.Timers.Timer(1);
            timer.Elapsed += (sen, ev) => { _state.WriteInfo(); };
            timer.Start();
            statusAccount.Click += (send, ev) => {
                CheckLogin();
            };
#endif
            // 界面文字
            LoadUIText();
            // 登录
            CheckLogin();
        }

        private void gridDanmakus_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            DataGridViewColumn column = gridDanmakus.Columns[e.ColumnIndex];
            if (_lastOrderColumn != column) {
                if (_lastOrderColumn != null)
                    _lastOrderColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                column.HeaderCell.SortGlyphDirection = SortOrder.Ascending;

                _lastOrderColumn = column;
            } else {
                // Asc/Desc更换
                column.HeaderCell.SortGlyphDirection = (
                    column.HeaderCell.SortGlyphDirection == SortOrder.Ascending ?
                    SortOrder.Descending : SortOrder.Ascending
                    );
            }
        }
        private void gridDanmakus_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e) {
            DataGridViewRow row = GetCacheRow(_listDanmakus[e.RowIndex]);
            if (e.ColumnIndex == 5) {
                // Mode
                e.Value = Language.Lang["DanmakuMode_" + row.Cells[5].Value];
            } else {
                e.Value = row.Cells[e.ColumnIndex].Value;
                if (e.ColumnIndex == 1) {
                    // Color
                    gridDanmakus[1, e.RowIndex].Style.BackColor = (Color)e.Value;

                }
            }
        }
        private void gridDanmakus_CellValuePushed(object sender, DataGridViewCellValueEventArgs e) {
            DanmakuBase danmaku = _listDanmakus[e.RowIndex];
            DataGridViewRow row = GetCacheRow(danmaku);
            switch (e.ColumnIndex) {
                case 0: // 时间
                    danmaku.PlayTime = (float)(e.Value);
                    break;
                case 1: // 颜色
                    danmaku.Color = (Color)(e.Value);
                    break;
                case 2: // 字号
                    danmaku.Fontsize = (int)(e.Value);
                    break;
                case 4: // 文本
                    danmaku.Text = (string)(e.Value);
                    break;
            }
            row.Cells[e.ColumnIndex].Value = e.Value;
            ChangeFileState(FileState.Changed);
        }

        #endregion

    }
}
