using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Tsanie.Network.Models;
using Tsanie.Utils;
using System.Text.RegularExpressions;
using System.IO;

namespace Tsanie.Network.Danmaku {

    /// <summary>
    /// BiLiBiLi 登录检查工具类
    /// </summary>
    public class LoginChecker {

        /// <summary>
        /// 根据传入的Cookie获取其登录信息
        /// </summary>
        /// <param name="host">BiLiBiLi 主站地址</param>
        /// <param name="cookie">Cookie 信息</param>
        /// <param name="callback">用户模型回调, UserModel</param>
        /// <param name="exCallback">异常回调</param>
        /// <returns>该动作的请求状态实例</returns>
        public static RequestState CheckLogin(
            string host,
            string cookie,
            Action<UserModel> callback,
            Action<Exception> exCallback
        ) {
            return HttpHelper.BeginConnect(host.Default(Config.DEFAULT_HOST) + "/dad.php?r=" + Utility.Rnd.NextDouble(),
                (request) => {
                    request.Headers["Cookie"] = cookie;
                }, (state) => {
                    if (state.Response.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new WebException("CheckLogin.StatusNotOK. " +
                            state.Response.StatusCode + ": " +
                            state.Response.StatusDescription, WebExceptionStatus.UnknownError);
                    StringBuilder result = new StringBuilder(0x40);
                    using (StreamReader reader = new StreamReader(state.StreamResponse)) {
                        if (state.IsCancelled())
                            throw new CancelledException(state, "CheckLogin.Interrupt");
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            result.Append(line);
                        }
                        reader.Dispose();
                    }
                    UserModel user = new UserModel() { Login = false };
                    Regex reg = new Regex("<([a-zA-Z^>]+)>([^<]+)</([a-zA-Z^>]+)>", RegexOptions.Singleline);
                    foreach (Match match in reg.Matches(result.ToString())) {
                        string key = match.Groups[1].Value;
                        string value = match.Groups[2].Value;
                        #region - 填充 key/value -
                        switch (key) {
                            case "login":
                                user.Login = bool.Parse(value);
                                break;
                            case "name":
                                user.Name = value;
                                break;
                            case "user":
                                user.User = int.Parse(value);
                                break;
                            case "scores":
                                user.Scores = int.Parse(value);
                                break;
                            case "money":
                                user.Money = int.Parse(value);
                                break;
                            case "pwd":
                                user.Pwd = value;
                                break;
                            case "isadmin":
                                user.IsAdmin = bool.Parse(value);
                                break;
                            case "permission":
                                string[] ps = value.Split(',');
                                Level[] levels = new Level[ps.Length];
                                for (int i = 0; i < ps.Length; i++)
                                    levels[i] = (Level)int.Parse(ps[i]);
                                user.Permission = levels;
                                break;
                            case "level":
                                user.Level = value;
                                break;
                            case "shot":
                                user.Shot = bool.Parse(value);
                                break;
                            case "chatid":
                                user.VideoDad.ChatID = int.Parse(value);
                                break;
                            case "aid":
                                user.VideoDad.Aid = int.Parse(value);
                                break;
                            case "pid":
                                user.VideoDad.Pid = int.Parse(value);
                                break;
                            case "acceptguest":
                                user.VideoDad.AcceptGuest = bool.Parse(value);
                                break;
                            case "duration":
                                user.VideoDad.Duration = value;
                                break;
                            case "acceptaccel":
                                user.AcceptAccel = bool.Parse(value);
                                break;
                            case "cache":
                                user.VideoDad.Cache = bool.Parse(value);
                                break;
                            case "server":
                                user.Server = value;
                                break;
                        }
                        #endregion
                    }
                    callback.SafeInvoke(user);
                }, exCallback);
        }
    }
}
