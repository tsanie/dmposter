using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tsanie.DmPoster.Danmaku;
using Tsanie.Utils;
using System.IO;
using System.Net;

namespace Tsanie.Network.Danmaku {

    /// <summary>
    /// BiLiBiLi 弹幕上载工具类
    /// </summary>
    public class Uploader {

        /// <summary>
        /// 发送一条弹幕
        /// </summary>
        /// <param name="host">BiLiBiLi 主站地址</param>
        /// <param name="cookie">Cookie 信息</param>
        /// <param name="playerPath">播放器地址</param>
        /// <param name="vid">弹幕池 Vid</param>
        /// <param name="pool">将要发送到的弹幕池</param>
        /// <param name="danmaku">弹幕实例</param>
        /// <param name="callback">成功回调，int</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的状态实例</returns>
        public static RequestState PostDanmaku(
            string host,
            string cookie,
            string playerPath,
            string vid,
            int pool,
            BiliDanmaku danmaku,
            Action<int> callback,
            Action<Exception> exCallback
        ) {
            return HttpHelper.BeginConnect(host.Default(Config.DEFAULT_HOST) + "/dmpost",
                (request) => {
                    #region - Request -
                    request.Method = "POST";
                    playerPath = playerPath.Default(Config.DEFAULT_PLAYERPATH);
                    request.Referer = playerPath;
                    int index = playerPath.IndexOf("://");
                    if (index > 0) {
                        index = playerPath.IndexOf('/', index + 3);
                        if (index > 0) {
                            request.Headers["Origin"] = playerPath.Substring(0, index);
                        }
                    }
                    request.Headers["Cookie"] = cookie;
                    string dataString = string.Format(
                        "mode={0}&rnd={1}&pool={2}&message={3}&date={4}&playTime={5}&fontsize={6}&vid={7}&color={8}",
                        (int)danmaku.Mode,
                        Utility.Rnd.Next(1000, 9999),
                        pool,
                        Utility.UrlEncode(danmaku.Text),
                        Utility.UrlEncode(DateTime.Now.ToString(Config.DEFAULT_DATEFORMAT)),
                        Utility.UrlEncode(danmaku.PlayTime.ToString()),
                        danmaku.Fontsize,
                        vid,
                        danmaku.Color.ToRgbIntString());
                    byte[] data = Encoding.ASCII.GetBytes(dataString);
#if DEBUG
                    foreach (string key in request.Headers.Keys) {
                        Console.WriteLine("[" + key + "]\t" + " - " + request.Headers[key]);
                    }
                    Console.WriteLine(new string('-', 20));
                    Console.WriteLine(dataString);
                    Console.WriteLine(new string('-', 20));
                    for (int i = 0; i < data.Length; i++) {
                        Console.Write(data[i].ToString("X2") + " ");
                        if (i % 21 == 20)
                            Console.WriteLine();
                    }
                    Console.WriteLine();
#endif
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream()) {
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();
                    }
                    #endregion
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("PostDanmaku.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    int count = state.StreamResponse.Read(state.Buffer, 0, RequestState.BUFFER_SIZE);
                    string total = Encoding.ASCII.GetString(state.Buffer, 0, count);
                    callback.SafeInvoke(int.Parse(total));
                }, exCallback);
        }

        /// <summary>
        /// 上传xml式弹幕
        /// </summary>
        /// <param name="host">BiLiBiLi 主站地址</param>
        /// <param name="cookie">Cookie 信息</param>
        /// <param name="dmid">弹幕池 ID</param>
        /// <param name="danmakus">要上传的弹幕数组</param>
        /// <param name="callback">成功回调，string</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的状态实例</returns>
        public static RequestState UploadDanmakus(
            string host,
            string cookie,
            string dmid,
            DanmakuBase[] danmakus,
            Action<string> callback,
            Action<Exception> exCallback
        ) {
            string url = host.Default(Config.DEFAULT_HOST) + "/member/dmm.php";
            return HttpHelper.BeginConnect(url,
                (request) => {
                    request.Method = "POST";
                    request.Referer = url + "?mode=ass&dm_inid=" + dmid;
                    request.Headers["Origin"] = host.Default(Config.DEFAULT_HOST);
                    request.Headers["Cookie"] = cookie;
                    request.ContentType = "application/x-www-form-urlencoded";
                    string dataString = string.Format("mode=asspost&dm_inid={0}{1}",
                        dmid, GetEncodedString(danmakus));
                    byte[] data = Encoding.ASCII.GetBytes(dataString);
#if DEBUG
                    foreach (string key in request.Headers.Keys) {
                        Console.WriteLine("[" + key + "]\t" + " - " + request.Headers[key]);
                    }
                    Console.WriteLine(new string('-', 20));
                    Console.WriteLine(dataString);
#endif
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream()) {
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();
                    }
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("UploadDanmakus.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            line = line.TrimStart();
                            if (line.StartsWith("document.write(\"")){
                                line = line.Substring(16);
                                int index = line.IndexOf('\"');
                                if (index > 0) {
                                    callback.SafeInvoke(line.Substring(0, index));
                                    break;
                                } else {
                                    throw new WebException("Unknown." + line);
                                }
                            }
                        }
                        reader.Close();
                    }
                }, exCallback);
        }

        private static string GetEncodedString(DanmakuBase[] danmakus) {
            if (danmakus == null)
                return null;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < danmakus.Length; i++) {
                builder.Append("&" + GetEncodedString(danmakus[i], i));
            }
            return builder.ToString();
        }
        private static string GetEncodedString(DanmakuBase danmaku, int index) {
            if (danmaku == null)
                return null;
            return string.Format("time{0}={1}&msg{0}={2}&fs{0}={3}&cl{0}={4}&mo{0}={5}",
                new object[] {
                    "%5B" + index + "%5D",
                    danmaku.PlayTime,
                    Utility.UrlEncode(danmaku.Text, true),
                    danmaku.Fontsize,
                    danmaku.Color.ToColorString(false),
                    (int)danmaku.Mode
                });
        }
    }
}
