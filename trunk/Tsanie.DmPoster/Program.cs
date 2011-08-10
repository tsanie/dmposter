using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using Tsanie.UI;
using Tsanie.Utils;
using System.Globalization;
using System.Diagnostics;

namespace Tsanie.DmPoster {
    static class Program {
        public static readonly Version Version;
        public static readonly ITaskbarList3 Taskbar;

        static Program() {
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            // Win7 超级任务栏
            if (Win7Stuff.IsWin7)
                Taskbar = (ITaskbarList3)new ProgressTaskbar();

            Config.GetInstance();
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
