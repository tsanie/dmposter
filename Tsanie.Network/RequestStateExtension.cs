using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network {
    public static class RequestStateExtension {

        public static void Cancel(this RequestState state) {
            if (state == null)
                return;
            if (state.Response == null) {
                state.Request.Abort();
            }
            state.Cancelled = true;
        }

        public static bool IsCancelled(this RequestState state) {
            if (state == null || state.Cancelled)
                return true;
            return false;
        }

        /*
         * 为啥不能搞个这样的扩展属性。。。。。
        public static string GetUrl(this RequestState state) {
            get {
                return (state == null ? null : state.Url);
            }
            set {
                if (state != null)
                    state.Url = value;
            }
        }
        */

#if DEBUG
        private enum State {
            NotStarted,
            Request,
            Response
        }

        private static object _syncObject = new object();
        private static State _lastState = State.NotStarted;

        public static void WriteInfo(this RequestState state) {
            if (state != null) {
                State st;
                if (state.Request == null)
                    st = State.NotStarted;
                else {
                    if (state.Response == null)
                        st = State.Request;
                    else
                        st = State.Response;
                }
                if (_lastState == st)
                    return;
                Console.WriteLine("[{0:HH:mm:ss.fffffff}] {1}, {2}",
                    DateTime.Now,
                    state.Url,
                    st);
                lock (_syncObject) { _lastState = st; }
            }
        }
#endif

    }
}
