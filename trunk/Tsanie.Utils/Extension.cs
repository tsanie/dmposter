using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace Tsanie.Utils {

    public static class ObjectExtension {

        /// <summary>
        /// 回调安全执行
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="t">实例</param>
        public static void SafeInvoke<T>(this Action<T> callback, T t) {
            if (callback != null)
                callback(t);
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> callback, T1 t1, T2 t2) {
            if (callback != null)
                callback(t1, t2);
        }

        public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> callback, T1 t1, T2 t2, T3 t3) {
            if (callback != null)
                callback(t1, t2, t3);
        }

        public static void SafeInvoke(this Action callback) {
            if (callback != null)
                callback();
        }
    }

    public static class StringExtension {

        /// <summary>
        /// 取值或默认值
        /// </summary>
        /// <param name="value">返回值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>返回值为空则返回默认值</returns>
        public static string Default(this string value, string defaultValue) {
            return (value == null ? defaultValue : value);
        }
    }

    public static class ThreadExt {
        public static Thread Create(CultureInfo cultureInfo, ThreadStart start) {
            return ThreadExt.Create(null, cultureInfo, start);
        }
        public static Thread Create(string name, CultureInfo cultureInfo, ThreadStart start) {
            if (cultureInfo == null)
                return new Thread(start) { Name = name };
            return new Thread(start) {
                Name = name,
                CurrentUICulture = cultureInfo
            };
        }
        public static void WriteCulture(this Thread thread) {
            Console.WriteLine("[{0:HH:mm:ss.fffffff}] {1}: {2} - {3}",
                DateTime.Now,
                thread.ManagedThreadId,
                thread.Name ?? "Untitled",
                thread.CurrentUICulture);
        }
    }
}
