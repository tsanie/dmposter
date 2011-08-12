using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tsanie.DmPoster.Danmaku;
using Tsanie.UI;
using Tsanie.Network;
using Tsanie.Network.Models;
using System.Net;
using System.Collections;

namespace Tsanie.DmPoster {
    public partial class MainForm : Form {

        #region - 私有字段 -

        private List<DanmakuBase> _listDanmakus;
        private Dictionary<DanmakuBase, DataGridViewRow> _cacheRows;
        private UserModel _user;
        private RequestState _state;
        private System.Timers.Timer _timer;
        private DataGridViewColumn _lastOrderColumn;
        private FileState _fileState;
        private string _fileName;

        #endregion

        #region - 构造 -

        public MainForm() {
            InitializeComponent();
            this.Icon = Tsanie.DmPoster.Properties.Resources.AppIcon;
            this.toolTextInterval.LostFocus += new EventHandler(toolTextInterval_LostFocus);
            this.toolComboPool.SelectedIndexChanged += new EventHandler(toolComboPool_SelectedIndexChanged);
            // 变量初始化
            _listDanmakus = new List<DanmakuBase>();
            _cacheRows = new Dictionary<DanmakuBase, DataGridViewRow>();
            _user = null;
            _state = null;
            _timer = null;
            _lastOrderColumn = null;
            _fileName = null;
            _fileState = FileState.Untitled;
            // DataGridView 列初始化
            #region - gridDanmakus -
            this.gridDanmakus.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn() {
                    Name = "datacolChange",
                    HeaderText = "",
                    Width = 2,
                    ReadOnly = true,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Frozen = true },
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
                new TsDataGridViewModeColumn(){
                    Name = "datacolMode",
                    Width = 72,
                    MinimumWidth = 40 },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolState",
                    ValueType = typeof(System.String),
                    HeaderText = "",
                    Width = 24,
                    MinimumWidth = 20,
                    ReadOnly = true,
                    Resizable = DataGridViewTriState.False,
                    DefaultCellStyle = new DataGridViewCellStyle() { Font = new Font("Webdings", 9f) } },
                new DataGridViewTextBoxColumn() {
                    Name = "datacolText",
                    ValueType = typeof(System.String),
                    Width = 1000,
                    MinimumWidth = 62}
            });
            #endregion
            // 文字
            LoadMainUIText();
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
                case "Open":
                    #region - 打开 -
                    LoadFile();
                    break;
                    #endregion
                case "Download":
                    #region - 下载 -
                    if (_fileState == FileState.Changed) {
                        // 询问保存
                        DialogResult dr = QuerySave();
                        if (dr == System.Windows.Forms.DialogResult.Cancel)
                            return;
                        else if (dr == System.Windows.Forms.DialogResult.Yes)
                            if (!SaveFile())
                                return;
                    }
                    // 开始下载
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
                            WebException webe = ex as WebException;
                            if (webe != null && webe.Status == WebExceptionStatus.UnknownError) {
                                ex = new Exception(Language.Lang[webe.Message]);
                            }
                            this.ShowExceptionMessage(ex, Language.Lang["DownloadDanmaku"]);
                            EnabledUI(true, null, Language.Lang["DownloadDanmaku.Interrupt"], null);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    });
                    break;
                    #endregion
                case "Save":
                    #region - 保存 -
                    SaveFile();
                    break;
                    #endregion
                case "SaveAs":
                    #region - 另存为 -
                    SaveFileAs();
                    break;
                    #endregion
                case "Post":
                    #region - 发送 -
                    IEnumerable danmakuRows;
                    DataGridViewSelectedRowCollection rowCollection = gridDanmakus.SelectedRows;
                    int total = rowCollection.Count;
                    if (total > 0) {
                        // 已选择
                        DialogResult dr = MessageBox.Show(
                            this,
                            Language.Lang["PostOnlySelected"],
                            Language.Lang["PostDanmaku"],
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);
                        if (dr == System.Windows.Forms.DialogResult.Cancel)
                            return;
                        if (dr == System.Windows.Forms.DialogResult.Yes) {
                            danmakuRows = new DataGridViewRow[total];
                            rowCollection.CopyTo((DataGridViewRow[])danmakuRows, 0);
                        } else {
                            danmakuRows = gridDanmakus.Rows;
                        }
                    } else {
                        // 发送所有
                        DialogResult dr = MessageBox.Show(
                            this,
                            Language.Lang["PostDanmaku.Confirm"],
                            Language.Lang["PostDanmaku"],
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question);
                        if (dr == System.Windows.Forms.DialogResult.Cancel)
                            return;
                        danmakuRows = gridDanmakus.Rows;
                        total = gridDanmakus.RowCount;
                    }
                    PostDanmakus(toolTextVid.Text, danmakuRows, total,
                        (count) => {
                            // 总发送个数
                            ShowMessage(Language.Lang["PostDanmaku.Succeed"], Language.Lang["PostDanmaku"],
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            EnabledUI(true, null, null, null);
                            SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                        }, (ex) => {
                            if (ex is CancelledException) {
                                EnabledUI(true, null, Language.Lang["PostDanmaku.Interrupt"], null);
                            } else {
                                WebException webe = ex as WebException;
                                if (webe != null && webe.Status == WebExceptionStatus.UnknownError) {
                                    ex = new Exception(Language.Lang[webe.Message]);
                                }
                                this.ShowExceptionMessage(ex, Language.Lang["PostDanmaku"]);
                                EnabledUI(true, null, Language.Lang["PostDanmaku.Interrupt"], null);
                            }
                            SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                        });
                    break;
                    #endregion
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
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (_fileState == FileState.Changed) {
                // 询问是否保存
                DialogResult dr = QuerySave();
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                    e.Cancel = true;
                else if (dr == System.Windows.Forms.DialogResult.Yes)
                    if (!SaveFile())  // 如果保存不成功
                        e.Cancel = true;
            }
        }

        private void gridDanmakus_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.ColumnIndex == 0) // 改动列不排序
                return;
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
            // 排序开始
            // TODO:
        }
        private void gridDanmakus_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e) {
            DataGridViewRow row = GetCacheRow(_listDanmakus[e.RowIndex]);
            e.Value = row.Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 2) {
                // Color
                gridDanmakus[2, e.RowIndex].Style.BackColor = (Color)e.Value;
            }
        }
        private void gridDanmakus_CellValuePushed(object sender, DataGridViewCellValueEventArgs e) {
            DanmakuBase danmaku = _listDanmakus[e.RowIndex];
            DataGridViewRow row = GetCacheRow(danmaku);
            switch (e.ColumnIndex) {
                case 1: // 时间
                    float playTime = (float)(e.Value);
                    if (danmaku.PlayTime == playTime)
                        return;
                    danmaku.PlayTime = playTime;
                    break;
                case 2: // 颜色
                    Color color = (Color)(e.Value);
                    if (danmaku.Color == color)
                        return;
                    danmaku.Color = color;
                    break;
                case 3: // 字号
                    int fontsize = (int)(e.Value);
                    if (danmaku.Fontsize == fontsize)
                        return;
                    danmaku.Fontsize = fontsize;
                    break;
                case 4: // 模式
                    DanmakuMode mode = (DanmakuMode)((int)(e.Value));
                    if (danmaku.Mode == mode)
                        return;
                    danmaku.Mode = mode;
                    break;
                case 6: // 文本
                    string text = (string)(e.Value);
                    if (danmaku.Text == text)
                        return;
                    danmaku.Text = text;
                    break;
                default:
                    return;
            }
            gridDanmakus[0, e.RowIndex].Style.BackColor = Config.ColorChanged;
            row.Cells[e.ColumnIndex].Value = e.Value;
            ChangeFileState(FileState.Changed);
        }
        private void gridDanmakus_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && files[0].ToLower().EndsWith(".xml"))
                    e.Effect = DragDropEffects.Link;
            }
        }
        private void gridDanmakus_DragDrop(object sender, DragEventArgs e) {
            string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            LoadFile(file);
        }

        private void toolComboPool_SelectedIndexChanged(object sender, EventArgs e) {
            Config.Instance.SetValue("Pool", toolComboPool.SelectedIndex);
        }

        private void toolTextInterval_LostFocus(object sender, EventArgs e) {
            float f;
            if (!float.TryParse(toolTextInterval.Text, out f)) {
                ShowMessage(Language.Lang["NumberFormatInvalid"], Language.Lang["PostInterval"],
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                toolTextInterval.Text = (Config.Instance.PostInterval / 1000.0f).ToString("0.0");
                toolTextInterval.Focus();
                return;
            }
            int interval = (int)(f * 1000);
            Config.Instance.PostInterval = interval;
            if (!ApplyPermission(true)) {
                ShowMessage(Language.Lang["PostInterval.Invalid"], Language.Lang["PostInterval"],
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                toolTextInterval.Focus();
            }
            Config.Instance.SetValue("PostInterval", interval);
        }

        #endregion

    }
}
