using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.DmPoster.Danmaku {
    public class DanmakuBase {
        private static int s_currentid = 0;

        #region - 私有字段 -

        internal int _dmid;
        internal string _usid;
        private int _mode = 1;
        private float _playTime = 0.0f;
        private int _fontsize = 25;
        private int _color = 0xffffff;
        private string _text = null;

        #endregion

        #region - 共有属性 -

        public int DmID { get { return _dmid; } }
        public string UsID { get { return _usid; } }
        public int Mode {
            get { return _mode; }
        }

        #endregion

    }
}
