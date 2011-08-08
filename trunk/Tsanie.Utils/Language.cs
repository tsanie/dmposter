using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Utils {
    /// <summary>
    /// UI 语言类
    /// </summary>
    public class Language {
        public static string Untitled { get { return "未命名"; } }
        public static string Property { get { return "属性"; } }
        public static string PropertyNull { get { return "属性值不可为 null。"; } }
        public static string PropertyInvalidPlayTime { get { return "播放时间属性值无效。"; } }
        public static string PropertyInvalidFontsize { get { return "弹幕字号属性值无效。"; } }
    }
}
