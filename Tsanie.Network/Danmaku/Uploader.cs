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
        /// <param name="danmaku">弹幕实例</param>
        /// <param name="callback"></param>
        /// <param name="exCallback"></param>
        /// <returns></returns>
        public static RequestState PostDanmaku(
            string host,
            string playerPath,
            string cookie,
            int vid,
            BiliDanmaku danmaku,
            Action<int> callback,
            Action<Exception> exCallback
        ) {
            if (danmaku == null) {
                exCallback.SafeInvoke(new NullReferenceException("danmaku"));
                return null;
            }
            return HttpHelper.BeginConnect(host.Default(Config.DEFAULT_HOST) + "/dmpost",
                (request) => {
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
                    string dataString = string.Format(
                        "mode={0}&rnd={1}&pool={2}&message={3}&date={4}&playTime={5}&fontsize={6}&vid={7}&color={8}",
                        (int)danmaku.Mode,
                        Utility.Rnd.Next(1000, 9999),
                        danmaku.Pool,
                        Utility.UrlEncode(danmaku.Text),
                        Utility.UrlEncode(danmaku.Date.ToString(Config.DEFAULT_DATEFORMAT)),
                        Utility.UrlEncode(danmaku.PlayTime.ToString()),
                        danmaku.Fontsize,
                        vid,
                        danmaku.Color.ToRgbIntString());
                    byte[] data = Encoding.ASCII.GetBytes(dataString);
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream()) {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
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
    }
}
