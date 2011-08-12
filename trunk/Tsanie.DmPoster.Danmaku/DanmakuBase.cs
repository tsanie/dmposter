using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tsanie.DmPoster.Danmaku {

    /// <summary>
    /// 弹幕基类，该类无法被实例化
    /// </summary>
    public abstract class DanmakuBase {
        private static int s_currentid = 0;

        #region - 私有字段 -

        private int _dmid;
        private string _usid;
        private DanmakuMode _mode = DanmakuMode.Scroll;
        private DateTime _date = DateTime.Now;
        private float _playTime = 0.0f;
        private int _fontsize = 25;
        private Color _color = Color.White;
        private string _text = null;

        #endregion

        #region - 共有属性 -

        /// <summary>
        /// 获取弹幕ID
        /// </summary>
        public virtual int DmID {
            get { return _dmid; }
            set { _dmid = value; }
        }

        /// <summary>
        /// 获取弹幕发送人ID
        /// </summary>
        public virtual string UsID {
            get { return _usid; }
            set { _usid = value; }
        }

        /// <summary>
        /// 获取或设置弹幕模式
        /// </summary>
        public virtual DanmakuMode Mode {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// 获取弹幕创建日期
        /// </summary>
        public virtual DateTime Date {
            get { return _date; }
            set { _date = value; }
        }

        /// <summary>
        /// 获取或设置弹幕播放时间
        /// </summary>
        public virtual float PlayTime {
            get { return _playTime; }
            set {
#if !TRACE
                if (value < 0)
                    throw new DanmakuPropertyException("PropertyInvalid, " + value, "PlayTime");
#endif
                _playTime = value;
            }
        }

        /// <summary>
        /// 获取或设置弹幕字号
        /// </summary>
        public virtual int Fontsize {
            get { return _fontsize; }
            set {
#if !TRACE
                if (value < 1 || value > 127)
                    throw new DanmakuPropertyException("PropertyInvalid, " + value, "Fontsize");
#endif
                _fontsize = value;
            }
        }

        /// <summary>
        /// 获取或设置弹幕字体颜色
        /// </summary>
        public virtual Color Color {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// 获取或设置弹幕文本
        /// </summary>
        public virtual string Text {
            get { return _text; }
            set {
                if (value == null)
                    throw new DanmakuException("PropertyNull");
                _text = value;
            }
        }

        #endregion

        #region - 构造 -

        /// <summary>
        /// 构造弹幕实例
        /// </summary>
        public DanmakuBase() {
            this._dmid = s_currentid++;
        }

        #endregion

        #region - 公共方法 -

        /// <summary>
        /// 设置弹幕ID
        /// </summary>
        /// <param name="dmid">要设置的弹幕ID</param>
        public void SetDmID(int dmid) {
            this._dmid = dmid;
        }
        /// <summary>
        /// 设置弹幕发送人ID
        /// </summary>
        /// <param name="usid">要设置的发送人ID</param>
        public void SetUsID(string usid) {
            this._usid = usid;
        }
        /// <summary>
        /// 设置弹幕创建日期
        /// </summary>
        /// <param name="date">要设置的日期</param>
        public void SetDate(DateTime date) {
            this._date = date;
        }

        #endregion

    }
}
