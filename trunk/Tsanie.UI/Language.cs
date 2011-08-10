using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Tsanie.UI {

    /// <summary>
    /// 字符集枚举
    /// </summary>
    public enum GdiCharSet : byte {
        ANSI_CHARSET = 0,
        DEFAULT_CHARSET = 1,
        SYMBOL_CHARSET = 2,
        SHIFTJIS_CHARSET = 128,
        HANGEUL_CHARSET = 129,
        HANGUL_CHARSET = 129,
        GB2312_CHARSET = 134,
        CHINESEBIG5_CHARSET = 136,
        OEM_CHARSET = 255,
        JOHAB_CHARSET = 130,
        HEBREW_CHARSET = 177,
        ARABIC_CHARSET = 178,
        GREEK_CHARSET = 161,
        TURKISH_CHARSET = 162,
        VIETNAMESE_CHARSET = 163,
        THAI_CHARSET = 222,
        EASTEUROPE_CHARSET = 238,
        RUSSIAN_CHARSET = 204
    }

    /// <summary>
    /// UI 语言类
    /// </summary>
    public class Language {
        private static readonly CultureInfo _defaultCulture = new System.Globalization.CultureInfo("zh-CN");

        private static Language _instance;
        static Language() {
            _instance = new Language();
        }

        public static Language Lang { get { return _instance; } }

        private ResourceManager _resource;
        private CultureInfo _culture;
        private Dictionary<string, string> _resourceCache;
        private object _syncObject = new object();

        public Language() {
            _resource = new ResourceManager("Tsanie.UI.Resource.Lang", Assembly.GetExecutingAssembly());
            _resourceCache = new Dictionary<string, string>();
            _culture = null;
        }

        internal void ClearCache() {
            _resourceCache.Clear();
        }

        public string this[string key] {
            get {
                string val;
                if (_resourceCache.TryGetValue(key, out val))
                    return val;
                StringBuilder builder = new StringBuilder(0x40);
                foreach (string k in key.Split('.')) {
                    val = _resource.GetString(k, _culture);
                    builder.Append((val == null || val.Length <= 0) ? k : val);
                }
                val = builder.ToString();
                lock (_syncObject) {
                    _resourceCache.Add(key, val);
                }
                return val;
            }
        }

        public CultureInfo CultureInfo {
            get { return _culture; }
            set { _culture = value; }
        }
    }
}
