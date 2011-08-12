using System;
using System.Linq;
using System.Windows.Forms;
using Tsanie.DmPoster.Danmaku;
using System.Threading;
using System.IO;
using System.Xml;
using System.Net;
using Tsanie.Utils;
using Tsanie.UI;
using Tsanie.Network;
using Tsanie.Network.Models;
using Tsanie.Network.Danmaku;
using System.Collections;

namespace Tsanie.DmPoster {
    partial class MainForm {

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
                if (column.Name != "datacolState" &&
                    column.Name != "datacolChange")
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
                this.SafeRun(delegate {
                    toolLabelInterval.Text = Language.Lang["toolLabelInterval"];
                    toolTextInterval.Font = Config.Instance.UIFont;
                    toolLabelPool.Text = Language.Lang["toolLabelPool"];
                    toolComboPool.Items.AddRange(new string[] {
                        Language.Lang["Pool_Normal"],
                        Language.Lang["Pool_Comment"],
                        Language.Lang["Pool_Special"]
                    });
                    toolComboPool.SelectedIndex = 0;
                });
            }) { Name = "threadLoadUIText" }.Start();
        }

        #endregion

        #region - 私有方法 -

        private void ChangeFileState(FileState fileState) {
            _fileState = fileState;
            string title = "";
            if (_fileName == null)
                title = Language.Lang["Untitled"];
            else
                title = _fileName.GetFilename();
            if (fileState == FileState.Changed)
                title += "*";
            this.SafeRun(delegate { this.Text = title + " - " + Config.Title; });
        }

        #region - UI相关 -
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
        public void SetStatusMessage(string message) {
            this.SafeRun(delegate { statusMessage.Text = message; });
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
        #endregion

        private DataGridViewRow CreateRowFromDanmaku(DanmakuBase danmaku) {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.AddRange(new DataGridViewCell[] {
                new DataGridViewTextBoxCell() { Value = null },
                new DataGridViewTextBoxCell() {
                    Value = danmaku.PlayTime,
                    ValueType = typeof(System.Single)
                },
                new DataGridViewTextBoxCell() {
                    Value = danmaku.Color,
                    ValueType = typeof(System.Drawing.Color)
                },
                new DataGridViewTextBoxCell() {
                    Value = danmaku.Fontsize,
                    ValueType = typeof(System.Int32)
                },
                new DataGridViewTextBoxCell() {
                    Value = danmaku.Mode,
                    ValueType = typeof(Tsanie.DmPoster.Danmaku.DanmakuMode)
                },
                new DataGridViewTextBoxCell() {
                    Value = "",
                    ValueType = typeof(System.String)
                },
                new DataGridViewTextBoxCell() {
                    Value = danmaku.Text,
                    ValueType = typeof(System.String)
                }
            });
            return row;
        }
        private DataGridViewRow GetCacheRow(DanmakuBase danmaku) {
            DataGridViewRow row;
            if (_cacheRows.TryGetValue(danmaku, out row))
                return row;
            row = CreateRowFromDanmaku(danmaku);
            _cacheRows.Add(danmaku, row);
            return row;
        }

        private bool ApplyPermission(bool logined) {
            bool flag = true;
            bool isPermission = false;
            if (!logined || _user == null) {
                if (Config.Instance.PostInterval < 10000) {
                    Config.Instance.PostInterval = 10000;
                    flag = false;
                }
            } else {
                if (_user.Permission.Contains(Level.Commenter) ||
                    _user.Permission.Contains(Level.Vip) ||
                    _user.Permission.Contains(Level.Major)) {
                    isPermission = true;
                    // 0.5秒
                    if (Config.Instance.PostInterval < 500) {
                        Config.Instance.PostInterval = 500;
                        flag = false;
                    }
                } else {
                    if (Config.Instance.PostInterval < 5000) {
                        Config.Instance.PostInterval = 5000;
                        flag = false;
                    }
                }
            }
            toolTextInterval.Text = (Config.Instance.PostInterval / 1000.0f).ToString("0.0");
            if (!isPermission) {
                toolComboPool.SelectedIndex = 0;
                toolComboPool.Enabled = false;
            } else {
                toolComboPool.Enabled = true;
            }
            return flag;
        }
        private void CheckLogin() {
            if (!string.IsNullOrEmpty(Config.Instance.Cookies)) {
                EnabledUI(false, Language.Lang["CheckLogin"], Language.Lang["Communicating"], delegate {
                    _state.Cancel();
                    _state = null;
                });
                SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
                // 登录 dad.php 检查权限
                _state = LoginChecker.CheckLogin(Config.Instance.HttpHost, Config.Instance.Cookies,
                    (user) => {
                        if (user.Login) {
                            _user = user;
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.logined; });
                            EnabledUI(true, _user.Name + " (" + _user.Level + ")", Language.Lang["Done"], null);
                            ApplyPermission(true);
                        } else {
                            this.SafeRun(delegate { statusAccountIcon.Image = Tsanie.DmPoster.Properties.Resources.guest; });
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang["Done"], null);
                            ApplyPermission(false);
                        }
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    }, (ex) => {
                        if (ex is CancelledException) {
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang[((CancelledException)ex).Command], null);
                        } else {
                            WebException webe = ex as WebException;
                            if (webe != null && webe.Status == WebExceptionStatus.UnknownError) {
                                ex = new Exception(Language.Lang[webe.Message]);
                            }
                            this.ShowExceptionMessage(ex, Language.Lang["CheckLogin"]);
                            EnabledUI(true, Language.Lang["Guest"], Language.Lang["CheckLogin.Failed"], null);
                        }
                        ApplyPermission(false);
                        SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                    });
            } else {
                EnabledUI(true, Language.Lang["Guest"], Language.Lang["Done"], null);
                ApplyPermission(false);
            }
        }

        private void DownloadDanmaku(string avOrVid, Action<int, int> callback, Action<Exception> exCallback) {
            Action refresher = delegate {
                this.SafeRun(delegate { gridDanmakus.RowCount = _listDanmakus.Count; });
            };
            // timer
            _timer = _timer.CreateTimer(Config.Interval, (e) => refresher());
            Action<Exception> exCall = (ex) => {
                _timer.Cancel();
                refresher();
                exCallback.SafeInvoke(ex);
            };
            try {
                if (string.IsNullOrWhiteSpace(avOrVid))
                    throw new Exception(Language.Lang["AvVidEmpty"]);
                #region - callbacks -
                int count = 0;
                int failed = 0;  // 失败的弹幕数
                Action readyCallback = delegate {
                    this.SafeRun(delegate {
                        gridDanmakus.RowCount = 0;
                        gridDanmakus.Enabled = true;
                        ChangeFileState(FileState.Changed);
                    });
                    _listDanmakus.Clear();
                    _timer.Start();
                };
                Action<BiliDanmaku> danmakuCallback = (danmaku) => {
                    if (danmaku == null) {
                        // 失败
                        failed++;
                    } else {
                        _listDanmakus.Add(danmaku);
                        count++;
                    }
                };
                Action doneCallback = delegate {
                    _timer.Cancel();
                    refresher();
                    if (callback != null)
                        callback(count, failed);
                };
                #endregion
                if (avOrVid.StartsWith("av")) {
                    if (avOrVid.Length == 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    string[] pars = avOrVid.Substring(2).Split(',');
                    if (pars.Length > 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    int aid = int.Parse(pars[0]);
                    int pageno = (pars.Length > 1 ? int.Parse(pars[1]) : 1);
                    // 输入的是Av号
                    GetVidFromAv(aid, pageno,
                        (vid) => DownloadDanmakuFromVid(vid, readyCallback, danmakuCallback, doneCallback, exCall),
                        exCallback);
                } else {
                    int.Parse(avOrVid);
                    // Vid
                    DownloadDanmakuFromVid(avOrVid, readyCallback, danmakuCallback, doneCallback, exCall);
                }
            } catch (Exception e) {
                exCall.SafeInvoke(e);
            }
        }
        private void GetVidFromAv(int aid, int pageno, Action<string> callback, Action<Exception> exCallback) {
            EnabledUI(false, null, string.Format(Language.Lang["GetVidOfAv"], aid + "," + pageno), delegate {
                _state.Cancel();
                _state = null;
            });
            _state = Downloader.GetVidOfAv(
                Config.Instance.HttpHost,
                Config.Instance.Cookies,
                aid,
                pageno,
                callback,
                exCallback
            );
        }
        private void DownloadDanmakuFromVid(string vid,
            Action readyCallback,
            Action<BiliDanmaku> danmakuCallback,
            Action doneCallback,
            Action<Exception> exCallback
        ) {
            EnabledUI(false, null, string.Format(Language.Lang["DownloadDanmakuStatus"], vid), delegate {
                _state.Cancel();
                _state = null;
            });
            _state = Downloader.DownloadDanmaku(
                Config.Instance.HttpHost,
                Config.PlayerPath,
                vid,
                readyCallback,
                danmakuCallback,
                doneCallback,
                exCallback
            );
        }

        private void PostDanmakus(
            string avOrVid,
            IEnumerable danmakus,
            int total,
            Action<int> callback,
            Action<Exception> exCallback
        ) {
            Action<Exception> exCall = (ex) => {
                _timer.Cancel();
                exCallback.SafeInvoke(ex);
            };
            try {
                IEnumerator enumerator = danmakus.GetEnumerator();
                if (!enumerator.MoveNext())
                    throw new Exception(Language.Lang["PostEmpty"]);
                Action<int, int, BiliDanmaku> rtnCallback = (rtn, count, danmaku) => {
                    SetProgressValue(count, total);
                    GetCacheRow(danmaku).Cells[5].Value = "a";
                    gridDanmakus.Invalidate();
                    SetStatusMessage(string.Format(Language.Lang["Posting"], count, rtn));
                };
                Action<string> ready = (vid) => {
                    EnabledUI(false, null, Language.Lang["PostDanmaku"], delegate {
                        _timer.Cancel();
                        _state.Cancel();
                        _state = null;
                        exCall.SafeInvoke(new CancelledException(null, "PostDanmaku.Interrupt"));
                    });
                    SetProgressState(TBPFLAG.TBPF_NORMAL);
                    SetProgressValue(0, total);
                    PostDanmaku(vid, 0, enumerator, rtnCallback, callback, exCall);
                };
                if (string.IsNullOrWhiteSpace(avOrVid))
                    throw new Exception(Language.Lang["AvVidEmpty"]);
                if (avOrVid.StartsWith("av")) {
                    if (avOrVid.Length == 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    string[] pars = avOrVid.Substring(2).Split(',');
                    if (pars.Length > 2)
                        throw new Exception(Language.Lang["PropertyInvalidAvOrVid"]);
                    int aid = int.Parse(pars[0]);
                    int pageno = (pars.Length > 1 ? int.Parse(pars[1]) : 1);
                    // 输入的是Av号
                    GetVidFromAv(aid, pageno, ready, exCall);
                } else {
                    int.Parse(avOrVid);
                    // Vid
                    ready(avOrVid);
                }
            } catch (Exception e) {
                exCall.SafeInvoke(e);
            }
        }
        private void PostDanmaku(
            string vid,
            int count,
            IEnumerator enumerator,
            Action<int, int, BiliDanmaku> responseCallback,
            Action<int> totalCallback,
            Action<Exception> exCallback
        ) {
            DataGridViewRow row = (DataGridViewRow)enumerator.Current;
            BiliDanmaku danmaku = (BiliDanmaku)_listDanmakus[row.Index];
            _state = Uploader.PostDanmaku(
                Config.Instance.HttpHost,
                Config.PlayerPath,
                Config.Instance.Cookies,
                vid,
                Config.Instance.Pool,
                danmaku,
                (total) => {
                    // 返回
                    count++;
                    responseCallback.SafeInvoke(total, count, danmaku);
                    if (!enumerator.MoveNext()) {
                        // 完毕
                        _timer.Cancel();
                        totalCallback.SafeInvoke(count);
                    } else {
                        _timer = _timer.CreateTimeout(Config.Instance.PostInterval, delegate {
                            PostDanmaku(vid, count, enumerator, responseCallback, totalCallback, exCallback);
                        });
                    }
                },
                exCallback);
        }

        private bool SaveFile() {
            if (_fileName == null) {
                return SaveFileAs();
            }
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
            int total = _listDanmakus.Count;
            bool cancelled = false;
            Action<string> done = msg => {
                EnabledUI(true, null, msg, null);
                SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
            };
            EnabledUI(false, null, Language.Lang["SaveFile"], delegate { cancelled = true; });
            SetProgressState(TBPFLAG.TBPF_NORMAL);
            SetProgressValue(0, total);
            int n = 0;

#if CUSTOM
            StreamWriter writer = new StreamWriter(filename, false, System.Text.Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.Write("<information>");
            //foreach (DanmakuBase danmaku in _listDanmakus) {
            for (int i = 0; i < total; i++) {
                DanmakuBase danmaku = _listDanmakus[i];
                Application.DoEvents();
                if (cancelled)
                    break;
                writer.WriteLine("<data>");
                writer.WriteLine("\t<playTime>{0}</playTime>", danmaku.PlayTime);
                writer.WriteLine("\t<message fontsize=\"{0}\" color=\"{1}\" mode=\"{2}\">{3}</message>",
                    danmaku.Fontsize,
                    danmaku.Color.ToRgbIntString(),
                    (int)danmaku.Mode,
                    Utility.HtmlEncode(danmaku.Text)
                        .Replace(Environment.NewLine, "\n")
                        .Replace("/n", "\n"));
                writer.WriteLine("\t<times>{0}</times>", danmaku.Date.ToString(Config.DateFormat));
                writer.WriteLine("</data>");
                // 进度条，修改框变绿
                SetProgressValue(n++, total);
                DataGridViewCellStyle style = gridDanmakus[0, i].Style;
                if (style.BackColor == Config.ColorChanged)
                    style.BackColor = Config.ColorSaved;
            }
            writer.Write("</information>");
            writer.Flush();
            writer.Dispose();
            writer = null;
#else
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            //settings.CheckCharacters = false;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(filename, settings);
            writer.WriteStartElement("information");
            //foreach (DanmakuBase danmaku in _listDanmakus) {
            for (int i = total - 1; i >= 0; i--) {
                DanmakuBase danmaku = _listDanmakus[i];
                Application.DoEvents();
                if (cancelled)
                    break;
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
                writer.WriteString(HtmlUtility.HtmlEncode(danmaku.Text)
                    .Replace(Environment.NewLine, "\n")
                    .Replace("/n", "\n"));
                writer.WriteEndElement();
                // times
                writer.WriteStartElement("times");
                writer.WriteString(danmaku.Date.ToString(Config.DateFormat));
                writer.WriteEndElement();
                writer.WriteEndElement();
                // 进度条
                SetProgressValue(n++, total);
            }
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            writer = null;
#endif

            if (cancelled) {
                done(Language.Lang["SaveFile.Interrupt"]);
                return false;
            }
            ChangeFileState(FileState.Saved);
            done(Language.Lang["SaveFile.Succeed"]);
            return true;
        }

        private DialogResult QuerySave() {
            return MessageBox.Show(
                    this,
                    string.Format(Language.Lang["QuerySaveFile"], (_fileName == null ? Language.Lang["Untitled"] : _fileName.GetFilename())),
                    Language.Lang["SaveFile"],
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
        }

        private bool LoadFile() {
            if (_fileState == FileState.Changed) {
                // 询问保存
                DialogResult dr = QuerySave();
                if (dr == System.Windows.Forms.DialogResult.Cancel)
                    return false;
                else if (dr == System.Windows.Forms.DialogResult.Yes)
                    if (!SaveFile())
                        return false;
            }
            OpenFileDialog dialog = new OpenFileDialog() {
                Filter = "弹幕文件 (*.xml)|*.xml|所有文件|*.*",
                Title = "打开弹幕文件"
            };
            if (dialog.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) {
                dialog.Dispose();
                return false;
            }
            _fileName = dialog.FileName;
            dialog.Dispose();
            return LoadFile(_fileName);
        }
        private bool LoadFile(string fileName) {
            Thread thread = new Thread(delegate() {
                Action refresher = delegate {
                    this.SafeRun(delegate { gridDanmakus.RowCount = _listDanmakus.Count; });
                };
                // timer
                _timer = _timer.CreateTimer(Config.Interval, (e) => refresher());
                // count
                int count = 0;
                int failed = 0;
                Action<string> done = msg => {
                    EnabledUI(true, null, msg, null);
                    SetProgressState(TBPFLAG.TBPF_NOPROGRESS);
                };
                XmlReader reader = XmlReader.Create(
                    new StreamReader(fileName, System.Text.Encoding.UTF8),
                    new XmlReaderSettings() {
                        IgnoreComments = true,
                        IgnoreWhitespace = true
                    });
                try {
                    _timer.Start();
                    while (reader.Read()) {
                        if (reader.NodeType == XmlNodeType.Element) {
                            if (reader.LocalName == "data") {
                                BiliDanmaku danmaku = new BiliDanmaku();
                                try {
                                    while (!(reader.Name == "data" && reader.NodeType == XmlNodeType.EndElement)) {
                                        if (reader.LocalName == "playTime") {
                                            danmaku.PlayTime = reader.ReadElementContentAsFloat();
                                        } else if (reader.LocalName == "times") {
                                            danmaku.SetDate(DateTime.Parse(reader.ReadElementContentAsString()));
                                        } else if (reader.LocalName == "message") {
                                            if (reader.MoveToAttribute("fontsize"))
                                                danmaku.Fontsize = int.Parse(reader.Value);
                                            if (reader.MoveToAttribute("color"))
                                                danmaku.Color = reader.Value.ToColor();
                                            if (reader.MoveToAttribute("mode"))
                                                danmaku.Mode = (DanmakuMode)(int.Parse(reader.Value));
                                            reader.MoveToContent();
                                            danmaku.Text = Utility.HtmlDecode(reader.ReadElementContentAsString());
                                        } else {
                                            if (!reader.Read())
                                                break;
                                        }
                                    }
                                    _listDanmakus.Add(danmaku);
                                    count++;
                                } catch (Exception e) {
                                    LogUtil.Error(new DanmakuException(e.Message, e), null);
                                    failed++;
                                }
                            } else if (reader.LocalName == "d") {
                                if (reader.MoveToAttribute("p")) {
                                    string properties = reader.Value;
                                    BiliDanmaku danmaku = BiliDanmaku.CreateFromProperties(properties, properties);
                                    if (danmaku == null)
                                        failed++;
                                    else {
                                        reader.MoveToContent();
                                        danmaku.Text = Utility.HtmlDecode(reader.ReadElementContentAsString());
                                        _listDanmakus.Add(danmaku);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();
                    reader = null;
                    _timer.Cancel();
                    refresher();
                    _fileName = fileName;
                    ChangeFileState(FileState.Opened);
                    if (failed > 0) {
                        ShowMessage(string.Format(Language.Lang["FailedCreateDanmakus"], failed), Language.Lang["LoadFile"],
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    done(string.Format(Language.Lang["LoadFileDone"], count));
                } catch (ThreadAbortException) {
                    _timer.Cancel();
                    refresher();
                    done(Language.Lang["LoadFile.Interrupt"]);
                }
            }) { Name = "threadLoadFile_" + fileName };
            EnabledUI(false, null, Language.Lang["LoadFile"], delegate {
                if (thread.ThreadState != ThreadState.Stopped)
                    thread.Abort();
            });
            SetProgressState(TBPFLAG.TBPF_INDETERMINATE);
            gridDanmakus.RowCount = 0;
            _listDanmakus.Clear();
            thread.Start();
            return true;
        }

        #endregion
    }
}
