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

namespace Tsanie.DmPoster {
    public partial class LoginForm : Form {
        private string _session = null;
        private Thread _thread = null;

        public LoginForm() {
            InitializeComponent();
            this.Font = Program.UIFont;
            toolTip.SetToolTip(pictureValidCode, "点我重新获取");
        }

        private void GetValidCode() {
            buttonLogin.Enabled = false;
            HttpHelper.BeginConnect(Config.HttpHost + "/include/vdimgck.php?r=" + Utility.Rnd.NextDouble(),
                (request) => {
                    request.Accept = "image/png,image/*;q=0.8,*/*;q=0.5";
                    request.Referer = Config.HttpHost + "/member/";
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK) {
                        throw new Exception("获取验证码返回不成功！");
                    }
                    _session = state.Response.Headers["Set-Cookie"];
                    if (string.IsNullOrEmpty(_session)) {
                        throw new Exception("获取验证码 session 失败！");
                    }
                    pictureValidCode.Image = Image.FromStream(state.StreamResponse);
                    state.StreamResponse.Close();
                    state.StreamResponse.Dispose();
                    this.SafeRun(delegate { buttonLogin.Enabled = true; });
                }, (ex) => {
                    this.SafeRun(delegate {
                        MessageBox.Show(this, ex.Message + "\n\nStacktrace:\n" + ex.StackTrace, "验证码",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        buttonLogin.Enabled = true;
                    });
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
    }
}
