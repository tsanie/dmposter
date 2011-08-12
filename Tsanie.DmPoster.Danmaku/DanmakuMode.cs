using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.DmPoster.Danmaku {
    /// <summary>
    /// 弹幕模式枚举
    /// </summary>
    public enum DanmakuMode : int {
        /// <summary>
        /// 滚动弹幕 - 1
        /// </summary>
        Scroll = 1,
        /// <summary>
        /// 底端固定弹幕 - 4
        /// </summary>
        BottomFixed = 4,
        /// <summary>
        /// 顶端固定弹幕 - 5
        /// </summary>
        TopFixed = 5,
        /// <summary>
        /// 逆向滚动弹幕 - 6
        /// </summary>
        ReverseScroll = 6,
        /// <summary>
        /// 模式7定位弹幕 - 7
        /// </summary>
        Mode7 = 7,
        /// <summary>
        /// 那道光 - 8
        /// </summary>
        That_beam_of_light = 8
    }
}
