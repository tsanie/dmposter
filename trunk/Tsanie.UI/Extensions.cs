using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tsanie.UI {
    public static class ControlsExtension {
        /// <summary>
        /// UI 线程安全执行
        /// </summary>
        /// <param name="control">在其上执行的控件</param>
        /// <param name="invoker">委托</param>
        public static void SafeRun(this Control control, MethodInvoker invoker) {
            if (control.InvokeRequired)
                control.Invoke(invoker);
            else
                invoker();
        }
    }
}
