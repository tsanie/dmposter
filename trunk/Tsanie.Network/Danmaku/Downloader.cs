using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Tsanie.Utils;
using Tsanie.DmPoster.Danmaku;
using System.Net;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Tsanie.Network.Danmaku {

    /// <summary>
    /// BiLiBiLi 弹幕下载工具类
    /// </summary>
    public class Downloader {

        /// <summary>
        /// 根据传入的视频Av号获取其弹幕池Vid号
        /// </summary>
        /// <param name="host">BiLiBiLi 主站地址</param>
        /// <param name="cookie">Cookie 信息</param>
        /// <param name="aid">视频 aid</param>
        /// <param name="pageno">分页号</param>
        /// <param name="callback">成功回调，string</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的请求状态实例</returns>
        public static RequestState GetVidOfAv(
            string host,
            string cookie,
            int aid,
            int pageno,
            Action<string> callback,
            Action<Exception> exCallback
        ) {
            string url = host.Default(Config.DEFAULT_HOST) + string.Format("/plus/view.php?aid={0}&pageno={1}", aid, pageno);
            return HttpHelper.BeginConnect(url,
                (request) => {
                    request.Referer = url;
                    request.Headers["Cookie"] = cookie;
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("GetVidOfAv.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Close();
                                throw new CancelledException(state, "GetVidOfAv.Interrupt");
                            }
                            int index = line.IndexOf("flashvars=\"");
                            if (index > 0) {
                                line = line.Substring(index + 11);
                                line = line.Substring(0, line.IndexOf('\"'));
                                line = Utility.UrlDecode(line);
                                foreach (string pair in line.Split('&')) {
                                    index = pair.IndexOf("id=");
                                    if (index >= 0) {
                                        line = line.Substring(index + 3);
                                        reader.Close();
                                        callback.SafeInvoke(line);
                                        return;
                                    }
                                }
                            }
                        }
                        reader.Close();
                        throw new CancelledException(state, "GetVidOfAv.Failed");
                    }
                }, exCallback);
        }

        /// <summary>
        /// 根据传入的弹幕池Vid号获取其弹幕DmID号
        /// </summary>
        /// <param name="host">BiLiBiLi 主站地址</param>
        /// <param name="playerPath">播放器路径</param>
        /// <param name="vid">弹幕池 Vid</param>
        /// <param name="callback">成功回调，string</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的请求状态实例</returns>
        public static RequestState GetDmIDOfVid(
            string host,
            string playerPath,
            string vid,
            Action<string> callback,
            Action<Exception> exCallback
        ) {
            string url = host.Default(Config.DEFAULT_HOST) + "/dm," + vid;
            return HttpHelper.BeginConnect(url,
                (request) => {
                    request.Referer = playerPath.Default(Config.DEFAULT_PLAYERPATH);
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("GetDmIDOfVid.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    // 读取压缩流
                    using (StreamReader reader = new StreamReader(new DeflateStream(state.StreamResponse, CompressionMode.Decompress))) {
                        Regex regex = new Regex("<chatid>([0-9]+?)</chatid>");
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Close();
                                throw new CancelledException(state, "GetDmIDOfVid.Interrupt");
                            }
                            Match match = regex.Match(line);
                            if (match != null) {
                                // 匹配
                                reader.Close();
                                callback.SafeInvoke(match.Groups[1].Value);
                                return;
                            }
                        }
                        reader.Close();
                        throw new CancelledException(state, "GetDmIDOfVid.Failed");
                    }
                }, exCallback);
        }

        /// <summary>
        /// 根据弹幕池Vid号下载其弹幕
        /// </summary>
        /// <param name="host">BiliBili 主站地址</param>
        /// <param name="playerPath">播放器路径</param>
        /// <param name="vid">弹幕池 Vid</param>
        /// <param name="readyCallback">准备下载回调</param>
        /// <param name="callback">弹幕回调, BiliDanmaku</param>
        /// <param name="doneCallback">完成回调</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的请求状态实例</returns>
        public static RequestState DownloadDanmaku(
            string host,
            string playerPath,
            string vid,
            Action readyCallback,
            Action<BiliDanmaku> callback,
            Action doneCallback,
            Action<Exception> exCallback
        ) {
            return HttpHelper.BeginConnect(host.Default(Config.DEFAULT_HOST) + "/dm," + vid,
                (request) => {
                    //request.Headers["Cookie"] = cookie;
                    request.Referer = playerPath.Default(Config.DEFAULT_PLAYERPATH);
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("DownloadDanmaku.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    if (readyCallback != null)
                        readyCallback();
                    // 读取压缩流
                    using (StreamReader reader = new StreamReader(new DeflateStream(state.StreamResponse, CompressionMode.Decompress))) {
                        StringBuilder builder = new StringBuilder(0x40);
                        Regex regex = new Regex("<d p=\"([^\"]+?)\">([^<]+?)</d>");
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Dispose();
                                throw new CancelledException(state, "DownloadDanmaku.Interrupt");
                            }
                            builder.AppendLine(line);
                            if (line.EndsWith("</d>")) {
                                foreach (Match match in regex.Matches(builder.ToString())) {
                                    string property = match.Groups[1].Value;
                                    string text = match.Groups[2].Value;
                                    // 读取属性
                                    BiliDanmaku danmaku = BiliDanmaku.CreateFromProperties(property, builder.ToString());
                                    if (danmaku == null) {
                                        // 失败
                                        callback.SafeInvoke(null);
                                    } else {
                                        danmaku.Text = Utility.HtmlDecode(text);
                                        callback.SafeInvoke(danmaku);
                                    }
                                }
                                builder.Clear();
                            }
                        }
                        reader.Dispose();
                        doneCallback.SafeInvoke();
                    }
                }, exCallback);
        }
    }
}
