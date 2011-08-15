using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using Tsanie.Utils;
using System.Threading;
using System.Globalization;

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

        /// <summary>
        /// Win32 窗口上显示异常信息对话框
        /// </summary>
        /// <param name="window">在其上显示对话框的窗口</param>
        /// <param name="ex">异常对象</param>
        /// <param name="title">标题</param>
        public static void ShowExceptionMessage(this Form window, Exception ex, string title) {
            ShowExceptionMessage(window, ex, title, true);
        }

        /// <summary>
        /// Win32 窗口上显示异常信息对话框，并指定是否显示堆栈调试
        /// </summary>
        /// <param name="window">在其上显示对话框的窗口</param>
        /// <param name="ex">异常对象</param>
        /// <param name="title">标题</param>
        /// <param name="stackTrace">是否显示堆栈调试</param>
        public static void ShowExceptionMessage(this Form window, Exception ex, string title, bool stackTrace) {
            window.SafeRun(delegate {
                MessageBox.Show(window, ex.Message + (!stackTrace ? "" :
                        "\n\nStackTrace:\n" + ex.StackTrace),
                    string.IsNullOrEmpty(title) ? ex.GetType().FullName :
                        (stackTrace ? title + " (" + ex.GetType().FullName + ")" : title),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        /// <summary>
        /// 超级任务栏改变进度条状态扩展
        /// </summary>
        /// <param name="taskbar">任务栏接口</param>
        /// <param name="window">句柄窗体</param>
        /// <param name="flag">样式</param>
        public static void SetProgressState(this ITaskbarList3 taskbar, IWin32Window window, TBPFLAG flag) {
            if (taskbar == null)
                return;
            taskbar.SetProgressState(window.Handle, flag);
        }

        public static void SetProgressValue(this ITaskbarList3 taskbar, IWin32Window window, int completed, int total) {
            if (taskbar == null)
                return;
            taskbar.SetProgressValue(window.Handle, (ulong)completed, (ulong)total);
        }

    }

    public static class ThreadExtension {
        public static System.Timers.Timer SetTimeout(double interval, Action<ElapsedEventArgs> action) {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += (sender, e) => {
                action.SafeInvoke(e);
                timer.Close();
            };
            timer.Start();
            return timer;
        }

        public static System.Timers.Timer CreateTimer(
            this System.Timers.Timer timer,
            double interval,
            Action<ElapsedEventArgs> action
        ) {
            timer.Cancel();
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += (sender, e) => {
                action.SafeInvoke(e);
            };
            return timer;
        }

        public static System.Timers.Timer CreateTimeout(
            this System.Timers.Timer timer,
            double interval,
            Action<ElapsedEventArgs> action
        ) {
            timer.Cancel();
            return SetTimeout(interval, action);
        }

        public static void Cancel(this System.Timers.Timer timer) {
            if (timer != null)
                timer.Close();
        }
    }
}
