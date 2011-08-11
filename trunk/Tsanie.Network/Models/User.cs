using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network.Models {
    public class UserModel {

        public bool Login = false;
        public string Name = null;
        public int User = 0;
        public int Scores = 0;
        public int Money = 0;
        public string Pwd = null;
        public bool IsAdmin = false;
        public Level[] Permission = new Level[0];
        public string Level = null;
        public bool Shot = false;
        public int ChatID = 0;
        public int Aid = 0;
        public int Pid = 0;
        public bool AcceptGuest = false;
        public string Duration = null;
        public bool AcceptAccel = false;
        public bool Cache = false;
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
