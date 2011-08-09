using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Tsanie.Utils;
using Tsanie.Network;

namespace Tsanie.DmPoster {

    /// <summary>
    /// 配置类
    /// </summary>
    class Config {

        public static readonly string Title;
        public static readonly string AppPath;
        private const string ConfigFile = @"\Tsanie.DmPoster.xml";
        private const string LogFile = @"\Tsanie.DmPoster.log";

        private static readonly XmlDocument _config = null;

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
            Config.PlayerPath = "http://static.loli.my/play.swf";
            Config.Interval = 100;

            if (!File.Exists(Config.AppPath + Config.ConfigFile)) {
                CreateConfig();
            }
            _config = new XmlDocument();
            _config.Load(Config.AppPath + Config.ConfigFile);
            LoadConfig();
        }


        public static void SetValue(string key, object value) {
            FieldInfo fi = typeof(Config).GetField("_" + key, BindingFlags.Static | BindingFlags.NonPublic);
            if (fi == null)
                return;
            string val = (value == null ? null : value.ToString());
            fi.SetValue(null, val);
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
        public static string GetValue(string key) {
            XmlNode node = _config.SelectSingleNode("/configuration/" + key);
            if (node == null)
                return null;
            string val = (string.IsNullOrEmpty(node.InnerText) ? null : node.InnerText);
            return val;
        }
        static void Save() {
            _config.Save(Config.AppPath + Config.ConfigFile);
        }
        public static void LoadConfig() {
            Type stringType = typeof(System.String);
            foreach (FieldInfo fi in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.NonPublic)) {
                if (fi.FieldType == stringType && fi.Name[0] == '_') {
                    fi.SetValue(null, GetValue(fi.Name.Substring(1)));
                }
            }
            Config.LogError = Config.LogError;
            HttpHelper.Timeout = Config.Timeout;
            HttpHelper.UserAgent = Config.UserAgent;
        }
        private static void CreateConfig() {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Config.AppPath + Config.ConfigFile, settings);
            writer.WriteStartElement("configuration");
            Type stringType = typeof(System.String);
            foreach (FieldInfo fi in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.NonPublic)) {
                if (fi.FieldType != stringType || fi.Name[0] != '_')
                    continue;
                writer.WriteStartElement(fi.Name.Substring(1));
                object obj = fi.GetValue(null);
                writer.WriteCData(obj == null ? null : obj.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            writer = null;
        }


        public static string UserAgent { get; set; }
        public static string PlayerPath { get; set; }
        public static double Interval { get; set; }

        private static string _HttpHost = "http://www.bilibili.tv";
        public static string HttpHost {
            get { return _HttpHost; }
            set { _HttpHost = value; }
        }

        private static string _Cookies = null;
        public static string Cookies {
            get { return _Cookies; }
            set { _Cookies = value; }
        }

        private static string _Timeout = "10000";
        public static int Timeout {
            get {
                int i;
                if (int.TryParse(_Timeout, out i))
                    return i;
                _Timeout = "10000";
                return 10000;
            }
            set { _Timeout = value.ToString(); }
        }

        private static string _LogError = "False";
        public static bool LogError {
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
                    LogUtil.LogFile = Config.AppPath + Config.ConfigFile;
                } else {
                    LogUtil.LogFile = null;
                }
            }
        }
    }
}
