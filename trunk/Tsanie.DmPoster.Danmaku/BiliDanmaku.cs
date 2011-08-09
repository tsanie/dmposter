using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tsanie.UI;

namespace Tsanie.DmPoster.Danmaku {
    public class BiliDanmaku : DanmakuBase {
        private int _pool = 0;

        public int Pool {
            get { return _pool; }
            set {
                if (value < 0 || value > 2)
                    throw new DanmakuPropertyException(Language.PropertyInvalidPool, "Pool");
                _pool = value;
            }
        }
    }
}
