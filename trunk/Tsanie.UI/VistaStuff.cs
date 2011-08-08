using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tsanie.UI {
    /// <summary>
    /// Vista界面工具类
    /// </summary>
    public class VistaStuff {
        /// <value>
        /// Returns true on Windows Vista or newer operating systems; otherwise, false.
        /// </value>
        [Browsable(false)]
        public static bool IsVistaOrNot {
            get {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;
            }
        }

        /// <summary>
        /// Returns true on Windows 7 or newer operation systems; otherwise, false.
        /// </summary>
        [Browsable(false)]
        public static bool IsWin7 {
            get {
                return (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1)
                    || Environment.OSVersion.Version.Major > 6;
            }
        }

        /// <summary>
        /// 设置组件主题
        /// </summary>
        /// <param name="hWnd">句柄</param>
        /// <param name="pszSubAppName"></param>
        /// <param name="pszSubIdList"></param>
        /// <returns></returns>
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

    }
}
