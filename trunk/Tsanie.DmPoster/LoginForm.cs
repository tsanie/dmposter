using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tsanie.Network;
using Tsanie.Utils;
using Tsanie.UI;
using System.Threading;
using System.IO;

namespace Tsanie.DmPoster {
    public partial class LoginForm : Form {
        private string _session = null;
        private Thread _thread = null;

        public LoginForm() {
            InitializeComponent();
            LoadUIText();
        }

        private void LoadUIText() {
            this.Font = Config.Instance.UIFont;
            this.Text = Language.Lang["Login"];
            foreach (Control c in this.Controls) {
                if (c is Label || c is Button || c is CheckBox)
                    c.Text = Language.Lang[c.Name];
            }
            toolTip.SetToolTip(pictureValidCode, Language.Lang["pictureValidCode_ToolTipText"]);
        }

        private void Loading(bool enabled) {
            this.SafeRun(delegate {
                buttonLogin.Enabled = !enabled;
                pictureLoading.Visible = enabled;
            });
            if (this.Owner != null) {
                this.Owner.SafeRun(delegate {
                    Program.Taskbar.SetProgressState(this.Owner, enabled ? TBPFLAG.TBPF_INDETERMINATE : TBPFLAG.TBPF_NOPROGRESS);
                });
            }
        }

        private void GetValidCode() {
            Loading(true);
            _thread = HttpHelper.BeginConnect(Config.Instance.HttpHost + "/include/vdimgck.php?r=" + Utility.Rnd.NextDouble(),
                (request) => {
                    request.Accept = "image/png,image/*;q=0.8,*/*;q=0.5";
                    request.Referer = Config.Instance.HttpHost + "/member/";
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception(Language.Lang["GetValidCode.StatusNotOK"] +
                            state.Response.StatusCode + ": " + state.Response.StatusDescription);
                    }
                    _session = state.Response.Headers["Set-Cookie"];
                    if (string.IsNullOrEmpty(_session)) {
                        throw new Exception(Language.Lang["GetValidCode. session .Failed"]);
                    }
                    int index = _session.IndexOf(';');
                    if (index > 0)
                        _session = _session.Substring(0, index + 1);
                    pictureValidCode.Image = Image.FromStream(state.StreamResponse);
                    state.StreamResponse.Dispose();
                    Loading(false);
                }, (ex) => {
                    this.SafeRun(delegate { this.ShowExceptionMessage(ex, Language.Lang["GetValidCode"]); });
                    Loading(false);
                });
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void pictureValidCode_Click(object sender, EventArgs e) {
            GetValidCode();
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (_thread != null && _thread.ThreadState == ThreadState.Running) {
                _thread.Abort();
                _thread = null;
            }
        }

        private void textValidCode_Enter(object sender, EventArgs e) {
            if (_session == null) {
                GetValidCode();
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(textUser.Text)) {
                MessageBox.Show(this, Language.Lang["textUser.PropertyNull"], Language.Lang["Login"],
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textUser.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(textPassword.Text)) {
                MessageBox.Show(this, Language.Lang["textPassword.PropertyNull"], Language.Lang["Login"],
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textPassword.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(textValidCode.Text)) {
                MessageBox.Show(this, Language.Lang["textValidCode.PropertyNull"], Language.Lang["Login"],
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textValidCode.Focus();
                return;
            }
            string user = Utility.UrlEncode(textUser.Text);
            string password = Utility.UrlEncode(textPassword.Text);
            string validcode = Utility.UrlEncode(textValidCode.Text);
            string data = "fmdo=login&dopost=login&gourl=&userid=" + user +
                "&pwd=" + password + "&vdcode=" + validcode + "&keeptime=2592000";
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            Loading(true);
            _thread = HttpHelper.BeginConnect(Config.Instance.HttpHost + "/member/index_do.php",
                (request) => {
                    request.Method = "POST";
                    request.Referer = Config.Instance.HttpHost + "/member/";
                    request.Headers["Cache-Control"] = "max-age=0";
                    request.Headers["Origin"] = Config.Instance.HttpHost;
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    request.Headers["Cookie"] = _session;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = bytes.Length;
                    using (Stream stream = request.GetRequestStream()) {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Dispose();
                    }
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception(Language.Lang["Login.StatusNotOK"] +
                            state.Response.StatusCode + ": " + state.Response.StatusDescription);
                    }
                    string cookie = state.Response.Headers["Set-Cookie"];
                    if (!string.IsNullOrEmpty(cookie)) {
                        // 拆分，组合 Cookie
                        StringBuilder builder = new StringBuilder(0x40);
                        foreach (string pair in cookie.Split(new char[] { ',', ';' }, StringSplitOptions.None)) {
                            if (pair.StartsWith("Dede")) {
                                string[] kv = pair.Split('=');
                                if (kv.Length == 2 && kv[1] != "deleted") {
                                    // 找到一个键值
                                    builder.Append(pair + "; ");
                                }
                            }
                        }
                        if (builder.Length > 0) {
                            if (checkAutoLogin.Checked)
                                Config.Instance.SetValue("Cookies", builder);
                            else
                                Config.Instance.Cookies = builder.ToString();
                            Loading(false);
                            this.SafeRun(delegate {
                                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                                this.Close();
                            });
                            return;
                        }
                    }
                    // 登录不成功
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            line = line.TrimStart();
                            if (line.StartsWith("document.write(\"")) {
                                line = line.Substring(16);
                                line = line.Substring(0, line.LastIndexOf("\");"));
                                reader.Dispose();
                                throw new Exception(line);
                            }
                        }
                        reader.Dispose();
                    }
                    throw new Exception(Language.Lang["Login.Failed.Unknown"]);
                }, (ex) => {
                    this.SafeRun(delegate {
                        this.ShowExceptionMessage(ex, Language.Lang["Login"]);
                        GetValidCode();
                    });
                });
        }
    }
}
