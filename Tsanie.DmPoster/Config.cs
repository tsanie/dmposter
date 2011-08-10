using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Globalization;
using Tsanie.Utils;
using Tsanie.Network;
using Tsanie.UI;

namespace Tsanie.DmPoster {

    enum FileState {
        Untitled,
        Opened,
        Changed,
        Saved
    }

    /// <summary>
    /// 配置类
    /// </summary>
    class Config {
        #region - 静态 -
        internal static readonly Type Type = typeof(Config);
        private static Config _instance = null;

        private const string ConfigFile = @"\Tsanie.DmPoster.xml";
        private const string LogFile = @"\Tsanie.DmPoster.log";

        public static readonly string Title;
        public static readonly string AppPath;
        public static readonly string UserAgent;
        public static readonly string PlayerPath;
        public static readonly double Interval;

        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        public static Config Instance {
            get {
                if (_instance == null)
                    _instance = new Config();
                return _instance;
            }
        }
        public static Config GetInstance() {
            return Config.Instance;
        }
        static Config() {
            Config.Title = string.Format("DmPoster {0}.{1}.{2}.{3}",
                Program.Version.Major,
                Program.Version.Minor,
                Program.Version.Build,
                Program.Version.Revision);
            Config.AppPath = Application.ExecutablePath.Substring(0,
                Application.ExecutablePath.LastIndexOf('\\'));
            Config.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ) AppleWebKit/534.12 (KHTML, like Gecko) Safari/534.12 DmPoster/"
                + Program.Version.Major + "." + Program.Version.Minor;
            HttpHelper.UserAgent = Config.UserAgent;
            Config.PlayerPath = "http://static.loli.my/play.swf";
            Config.Interval = 100;
        }
        private static void CreateConfig(Config config) {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Config.AppPath + Config.ConfigFile, settings);
            writer.WriteStartElement("configuration");
            Type stringType = typeof(System.String);
            foreach (FieldInfo fi in Config.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
                if (fi.FieldType != stringType || fi.Name[0] != '_')
                    continue;
                writer.WriteStartElement(fi.Name.Substring(1));
                object obj = fi.GetValue(config);
                writer.WriteCData(obj == null ? null : obj.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            writer = null;
        }
        #endregion

        public Font UIFont;
        public Font WidthFont;

        private readonly XmlDocument _config = null;

        public Config() {
            if (!File.Exists(Config.AppPath + Config.ConfigFile)) {
                Config.CreateConfig(this);
            }

            this._config = new XmlDocument();
            this._config.Load(Config.AppPath + Config.ConfigFile);
            LoadConfig();

            this.UIFont = new Font(
                Tsanie.UI.Language.Lang["FontName"],
                float.Parse(Tsanie.UI.Language.Lang["FontSize"]),
                FontStyle.Regular,
                GraphicsUnit.Point);
            this.WidthFont = new Font(
                Tsanie.UI.Language.Lang["WidthFontName"],
                float.Parse(Tsanie.UI.Language.Lang["WidthFontSize"]),
                FontStyle.Regular,
                GraphicsUnit.Point);
        }

        public void SetValue(string key, object value) {
            FieldInfo fi = Config.Type.GetField("_" + key, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                return;
            string val = (value == null ? null : value.ToString());
            fi.SetValue(this, val);
            XmlNode node = _config.SelectSingleNode("/configuration/" + key);
            if (node == null) {
                node = _config.CreateElement(key);
                node.AppendChild(_config.CreateCDataSection(val));
                _config.SelectSingleNode("/configuration").AppendChild(node);
            } else {
                node.RemoveAll();
                node.AppendChild(_config.CreateCDataSection(val));
            }
            Save();
        }
        public string GetValue(string key) {
            XmlNode node = _config.SelectSingleNode("/configuration/" + key);
            if (node == null)
                return null;
            string val = (string.IsNullOrEmpty(node.InnerText) ? null : node.InnerText);
            return val;
        }

        public void Save() {
            _config.Save(Config.AppPath + Config.ConfigFile);
        }

        public void LoadConfig() {
            Type stringType = typeof(System.String);
            foreach (FieldInfo fi in Config.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
                if (fi.FieldType == stringType && fi.Name[0] == '_') {
                    string val = GetValue(fi.Name.Substring(1));
                    if (val != null)
                        fi.SetValue(this, val);
                }
            }
            this.LogError = this.LogError;
            this.Timeout = this.Timeout;
            this.Language = this.Language;
        }

        private string _HttpHost = "http://www.bilibili.tv";
        public string HttpHost {
            get { return _HttpHost; }
            set { _HttpHost = value; }
        }

        private string _Language = null;
        public string Language {
            get {
                return _Language;
            }
            set {
                _Language = value;
                if (value == null || value.Length <= 0)
                    return;
                Tsanie.UI.Language.Lang.CultureInfo = new CultureInfo(value);
            }
        }

        private string _Cookies = null;
        public string Cookies {
            get { return _Cookies; }
            set { _Cookies = value; }
        }

        private string _Timeout = "10000";
        public int Timeout {
            get {
                int i;
                if (int.TryParse(_Timeout, out i))
                    return i;
                _Timeout = "10000";
                return 10000;
            }
            set {
                _Timeout = value.ToString();
                HttpHelper.Timeout = value;
            }
        }

        private string _LogError = "False";
        public bool LogError {
            get {
                bool b;
                if (bool.TryParse(_LogError, out b))
                    return b;
                _LogError = "False";
                return false;
            }
            set {
                _LogError = value.ToString();
                if (value) {
                    LogUtil.LogFile = Config.AppPath + Config.LogFile;
                } else {
                    LogUtil.LogFile = null;
                }
            }
        }
    }
}
