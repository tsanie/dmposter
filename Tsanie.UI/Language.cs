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

        private static ResourceManager _resource;
        private static CultureInfo _culture = _defaultCulture;

        static Language() {
            _resource = new ResourceManager("Tsanie.UI.Resource.Lang", Assembly.GetExecutingAssembly());
        }

        public static string FontName { get { return _resource.GetString("FontName", _culture); } }
        public static string WidthFontName { get { return _resource.GetString("WidthFontName", _culture); } }
        public static byte GdiCharset { get { return byte.Parse(_resource.GetString("GdiCharSet", _culture)); } }

        public static CultureInfo CultureInfo {
            get { return _culture; }
            set { _culture = value; }
        }

        public static string Untitled { get { return "未命名"; } }
        public static string Property { get { return "属性"; } }
        public static string PropertyNull { get { return "属性值不可为 null。"; } }
        public static string PropertyInvalidPlayTime { get { return "播放时间属性值无效。"; } }
        public static string PropertyInvalidFontsize { get { return "弹幕字号属性值无效。"; } }
        public static string PropertyInvalidPool { get { return "弹幕池属性值无效。"; } }

        public static string ColumnPlayTime { get { return "时间"; } }
        public static string ColumnColor { get { return "颜色"; } }
        public static string ColumnFontsize { get { return "字号"; } }
        public static string ColumnText { get { return "弹幕内容"; } }
        public static string ColumnMode { get { return "模式"; } }

        public static string GetMenuText(string name) {
            return _resource.GetString(name, _culture);
        }
    }
}
