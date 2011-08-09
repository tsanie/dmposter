using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using Tsanie.Utils;
using System.IO;
using System.Reflection;
using Tsanie.UI;

namespace Tsanie.Network {
    public class HttpHelper {
        private static int _timeout = 10000;  // default to 10 秒
        private static string _userAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ) AppleWebKit/534.12 (KHTML, like Gecko) Safari/534.12 Tsanie/" +
            Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
            Assembly.GetExecutingAssembly().GetName().Version.Minor;

        public static int Timeout {
            get { return _timeout; }
            set {
                if (value > 0)
                    _timeout = value;
            }
        }
        public static string UserAgent {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        public static Thread BeginConnect(string url,
            Action<HttpWebRequest> requestBefore,
            Action<RequestState> asyncCallback,
            Action<Exception> errCallback) {
            Thread result = new Thread(() => {
                try {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.Method = "GET";
                    request.KeepAlive = true;
                    request.Accept = "*/*";
                    request.UserAgent = _userAgent;
                    if (requestBefore != null) {
                        requestBefore(request);
                    }
                    RequestState state = new RequestState();
                    state.Request = request;
                    // 开始异步请求
                    IAsyncResult ar = request.BeginGetResponse((iar) => {
                        RequestState requestState = (RequestState)iar.AsyncState;
                        HttpWebRequest webRequest = requestState.Request;
                        try {
                            requestState.Response = (HttpWebResponse)webRequest.EndGetResponse(iar);
                            requestState.StreamResponse = requestState.Response.GetResponseStream();
                            requestState.StreamResponse.ReadTimeout = _timeout;
                            if (asyncCallback != null) {
                                asyncCallback(requestState);
                            }
                            requestState.Response.Close();
                        } catch (Exception ex) {
                            LogUtil.Error(ex, errCallback);
                        }
                    }, state);
                    // 超时控制
                    ThreadPool.RegisterWaitForSingleObject(
                        ar.AsyncWaitHandle,
                        new WaitOrTimerCallback(TimeoutCallback),
                        request,
                        _timeout,
                        true);
                } catch (Exception e) {
                    LogUtil.Error(e, errCallback);
                }
            }) { Name = "httpThread_" + url };
            result.Start();
            return result;
        }

        private static void TimeoutCallback(object state, bool timeOuted) {
            if (timeOuted) {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                    request.Abort();
            }
        }
    }

    public class RequestState {
        const int BUFFER_SIZE = 1024;
        public byte[] Buffer { get; private set; }
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public Stream StreamResponse { get; set; }
        public RequestState() {
            Buffer = new byte[BUFFER_SIZE];
            Request = null;
            Response = null;
            StreamResponse = null;
        }
    }
}
