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
#if TRACE1
            Tsanie.Network.Danmaku.Uploader.PostDanmaku(null, null, null, 45821333,
                new Danmaku.BiliDanmaku() {
                    Mode = Danmaku.DanmakuMode.That_beam_of_light,
                    Pool = 2,
                    Text = "$.createComment(\"测试.test\", {x:20,y:100,lifeTime:2});",
                    PlayTime = -0.2f,
                    Fontsize = 25,
                    Color = System.Drawing.Color.FromArgb(-1)
                },
                null,
                (ex) => {
                    new System.Threading.Thread(delegate() {
                        Console.WriteLine("[{0}] - {1}\nStackTrace:\n{2}",
                            ex.GetType().FullName,
                            ex.Message,
                            ex.StackTrace);
                        Clipboard.SetText(ex.Message);
                        Console.ReadLine();
                    }) {
                        Name = "threadTraceOutput",
                        ApartmentState = System.Threading.ApartmentState.STA,
                        CurrentUICulture = new CultureInfo("zh-CN")
                    }.Start();
                });
            return;
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }
}
