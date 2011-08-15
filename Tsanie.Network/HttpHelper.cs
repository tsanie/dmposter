using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using Tsanie.Utils;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace Tsanie.Network {

    /// <summary>
    /// Http 辅助类
    /// </summary>
    public class HttpHelper {
        private static int _timeout = 10000;  // default to 10 秒
        private static string _userAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ) AppleWebKit/534.12 (KHTML, like Gecko) Safari/534.12 Tsanie/" +
            Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
            Assembly.GetExecutingAssembly().GetName().Version.Minor;
        private static CultureInfo _culture = null;

        /// <summary>
        /// 获取或设置超时时间（毫秒），默认10秒
        /// </summary>
        public static int Timeout {
            get { return _timeout; }
            set {
                if (value > 0)
                    _timeout = value;
            }
        }
        /// <summary>
        /// 获取或设置用户代理字符串
        /// </summary>
        public static string UserAgent {
            get { return _userAgent; }
            set { _userAgent = value; }
        }
        /// <summary>
        /// 获取或设置当前UI区域信息
        /// </summary>
        public static CultureInfo CurrentUICulture {
            get { return _culture; }
            set { _culture = value; }
        }

        /// <summary>
        /// 开始连接请求
        /// </summary>
        /// <param name="url">url地址</param>
        /// <param name="requestBefore">请求发出前回调, HttpWebRequest</param>
        /// <param name="asyncCallback">回应回调，RequestState</param>
        /// <param name="errCallback">异常回调</param>
        /// <returns>该动作的请求状态实例</returns>
        public static RequestState BeginConnect(
            string url,
            Action<HttpWebRequest> requestBefore,
            Action<RequestState> asyncCallback,
            Action<Exception> errCallback
        ) {
            // 请求状态对象
            RequestState state = new RequestState() { Url = url };
            Thread result = ThreadExt.Create(_culture, delegate() {
                try {
#if DEBUG
                    Thread.CurrentThread.WriteCulture();
#endif
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.AllowAutoRedirect = false;
                    request.Method = "GET";
                    request.KeepAlive = true;
                    request.Accept = "*/*";
                    request.UserAgent = _userAgent;
                    if (requestBefore != null) {
                        requestBefore(request);
                    }
                    state.Request = request;
                    // 开始异步请求
                    IAsyncResult ar = request.BeginGetResponse((iar) => {
                        if (_culture != null)
                            Thread.CurrentThread.CurrentUICulture = _culture;
#if DEBUG
                        Thread.CurrentThread.WriteCulture();
#endif
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
                            if (ex is WebException) {
                                if (((WebException)ex).Status == WebExceptionStatus.RequestCanceled) {
                                    LogUtil.Info(ex.Message);
                                    errCallback.SafeInvoke(new CancelledException(state, ex.Message));
                                    return;
                                }
                            }
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
            });
            result.Name = "httpThread_" + url;
            result.Start();
            return state;
        }

        private static void TimeoutCallback(object state, bool timeOuted) {
            if (timeOuted) {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                    request.Abort();
            }
        }
    }

    /// <summary>
    /// 请求状态类
    /// </summary>
    public class RequestState {
        /// <summary>
        /// 默认的缓存块大小（1024）
        /// </summary>
        public static readonly int BUFFER_SIZE = 1024;

        /// <summary>
        /// 获取缓存块
        /// </summary>
        public byte[] Buffer { get; private set; }
        /// <summary>
        /// 获取Url
        /// </summary>
        public string Url { get; internal set; }
        /// <summary>
        /// 获取连接请求
        /// </summary>
        public HttpWebRequest Request { get; internal set; }
        /// <summary>
        /// 获取连接回应
        /// </summary>
        public HttpWebResponse Response { get; internal set; }
        /// <summary>
        /// 获取连接回应数据流
        /// </summary>
        public Stream StreamResponse { get; internal set; }
        /// <summary>
        /// 获取连接是否已取消
        /// </summary>
        public bool Cancelled { get; internal set; }

        /// <summary>
        /// 构造请求状态实例
        /// </summary>
        public RequestState() {
            Buffer = new byte[RequestState.BUFFER_SIZE];
            Request = null;
            Response = null;
            StreamResponse = null;
            Cancelled = false;
        }
    }
}
