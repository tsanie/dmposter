using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network.Models {

    /// <summary>
    /// 登录用户模型
    /// </summary>
    public class UserModel {

        /// <summary>
        /// 是否登录
        /// </summary>
        public bool Login = false;
        /// <summary>
        /// 昵称
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Uid
        /// </summary>
        public int User = 0;
        /// <summary>
        /// 积分数
        /// </summary>
        public int Scores = 0;
        /// <summary>
        /// 硬币
        /// </summary>
        public int Money = 0;
        /// <summary>
        /// 用户识别码（播放器用）
        /// </summary>
        public string Pwd = null;
        /// <summary>
        /// 是否为管理员
        /// </summary>
        public bool IsAdmin = false;
        /// <summary>
        /// 拥有的权限
        /// </summary>
        public Level[] Permission = new Level[0];
        /// <summary>
        /// 权限字符串
        /// </summary>
        public string Level = null;
        /// <summary>
        /// Shot
        /// </summary>
        public bool Shot = false;
        /// <summary>
        /// 视频权限信息
        /// </summary>
        public VideoDad VideoDad = new VideoDad() {
            ChatID = 0,
            Aid = 0,
            Pid = 0,
            AcceptGuest = false,
            Duration = null,
            Cache = false
        };
        /// <summary>
        /// Accept Accel
        /// </summary>
        public bool AcceptAccel = false;
        /// <summary>
        /// 聊天服务器
        /// </summary>
        public string Server = null;
    }

    /// <summary>
    /// 级别枚举
    /// </summary>
    public enum Level {
        /// <summary>游客</summary>
        Guest = 0,
        /// <summary>未知</summary>
        Unknown = 1001,
        /// <summary>搬运工标识</summary>
        PorterLogo = 1002,
        /// <summary>哔哩哔哩一周年</summary>
        Anniversary = 1003,
        /// <summary>哔哩哔哩·2011</summary>
        BiliBili_2011 = 1004,
        /// <summary>哔哩哔哩·夏</summary>
        BiliBili_Summer = 1007,
        /// <summary>会员</summary>
        Member = 10000,
        /// <summary>搬运工</summary>
        Porter = 15000,
        /// <summary>字幕君</summary>
        Commenter = 20000,
        /// <summary>VIP</summary>
        Vip = 25000,
        /// <summary>真·职人</summary>
        Major = 30000,
        /// <summary>管理员</summary>
        Admin = 32000
    }
}
