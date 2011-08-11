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
    public class Downloader {
        public static RequestState GetVidOfAv(
            string host,
            string cookie,
            int aid,
            int pageno,
            Action<string> callback,
            Action<Exception> exCallback
        ) {
            string url = host + string.Format("/plus/view.php?aid={0}&pageno={1}", aid, pageno);
            return HttpHelper.BeginConnect(url,
                (request) => {
                    request.Referer = url;
                    request.Headers["Cookie"] = cookie;
                }, (state) => {
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (state.IsCancelled()) {
                                reader.Dispose();
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
                                        reader.Dispose();
                                        if (callback != null)
                                            callback(line);
                                        return;
                                    }
                                }
                            }
                        }
                        reader.Dispose();
                        throw new CancelledException(state, "GetVidOfAv.Failed");
                    }
                }, exCallback);
        }

        public static RequestState DownloadDanmaku(
            string host,
            string playerPath,
            string vid,
            Action readyCallback,
            Action<BiliDanmaku> callback,
            Action doneCallback,
            Action<Exception> exCallback
        ) {
            return HttpHelper.BeginConnect(host + "/dm," + vid,
                (request) => {
                    //request.Headers["Cookie"] = cookie;
                    request.Referer = playerPath;
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
