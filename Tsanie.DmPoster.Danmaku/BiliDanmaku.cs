using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tsanie.UI;
using Tsanie.Utils;

namespace Tsanie.DmPoster.Danmaku {
    public class BiliDanmaku : DanmakuBase {
        private int _pool = 0;

        public int Pool {
            get { return _pool; }
            set {
                if (value < 0 || value > 2)
                    throw new DanmakuPropertyException(Language.Lang["PropertyInvalidPool"] + value, "Pool");
                _pool = value;
            }
        }

        public static BiliDanmaku CreateFromProperties(string properties, string other) {
            // 读取属性
            string[] vals = properties.Split(',');
            BiliDanmaku danmaku = null;
            try {
                danmaku = new BiliDanmaku() {
                    PlayTime = float.Parse(vals[0]),
                    Mode = (DanmakuMode)int.Parse(vals[1]),
                    Fontsize = int.Parse(vals[2]),
                    Color = vals[3].ToColor(),
                    Date = long.Parse(vals[4]).ParseDateTime(),
                    Pool = int.Parse(vals[5]),
                    UsID = vals[6],
                    DmID = int.Parse(vals[7])
                };
            } catch (Exception e) {
                LogUtil.Error(new DanmakuException(e.Message + "\n" + other, e), null);
            }
            return danmaku;
        }
    }
}
