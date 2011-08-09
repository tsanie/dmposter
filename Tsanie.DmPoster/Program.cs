using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using Tsanie.UI;
using Tsanie.Utils;

namespace Tsanie.DmPoster {
    static class Program {
        public static readonly Version Version;
        public static readonly Font UIFont;

        static Program() {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
            UIFont = new Font(Language.FontName, Language.Fontsize, FontStyle.Regular, GraphicsUnit.Point, Language.GdiCharset);
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
