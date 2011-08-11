using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static void SafeInvoke(this Action callback) {
            if (callback != null)
                callback();
        }
    }
}
