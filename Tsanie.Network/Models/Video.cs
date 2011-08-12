using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network.Models {

    /// <summary>
    /// 视频权限信息
    /// </summary>
    public struct VideoDad {
        /// <summary>
        /// ChatID
        /// </summary>
        public int ChatID;
        /// <summary>
        /// 视频 aid
        /// </summary>
        public int Aid;
        /// <summary>
        /// PID
        /// </summary>
        public int Pid;
        /// <summary>
        /// 是否允许游客
        /// </summary>
        public bool AcceptGuest;
        /// <summary>
        /// 视频长度
        /// </summary>
        public string Duration;
        /// <summary>
        /// 已缓存
        /// </summary>
        public bool Cache;
    }
}
