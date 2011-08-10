using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tsanie.UI;

namespace Tsanie.DmPoster.Danmaku {
    /// <summary>
    /// 弹幕异常类，继承于 Exception
    /// </summary>
    public class DanmakuException : Exception {
        private Exception _innerException;

        #region - 构造 -

        public DanmakuException(string message)
            : base(message) { }
        public DanmakuException(string message, Exception innerException)
            : base(message, innerException) {
            this._innerException = innerException;
        }

        #endregion

        #region - 覆盖 -

        public override string Message {
            get {
                return (string.IsNullOrEmpty(base.Message) && (_innerException != null) ? _innerException.Message : base.Message);
            }
        }

        public override string Source {
            get {
                return (_innerException == null ? base.Source : _innerException.Source);
            }
            set {
                if (_innerException == null)
                    base.Source = value;
                else
                    _innerException.Source = value;
            }
        }

        public override string StackTrace {
            get {
                return (_innerException == null ? base.StackTrace : _innerException.StackTrace);
            }
        }

        public override System.Collections.IDictionary Data {
            get {
                return (_innerException == null ? base.Data : _innerException.Data);
            }
        }

        public override string HelpLink {
            get {
                return (_innerException == null ? base.HelpLink : _innerException.HelpLink);
            }
            set {
                if (_innerException == null)
                    base.HelpLink = value;
                else
                    _innerException.HelpLink = value;
            }
        }

        #endregion

    }

    /// <summary>
    /// 弹幕属性值异常类，继承于 DanmakuException
    /// </summary>
    public class DanmakuPropertyException : DanmakuException {
        private string _propertyName;

        public DanmakuPropertyException(string message, string propertyName)
            : base(message) {
            this._propertyName = propertyName;
        }

        public override string Message {
            get {
                return base.Message + Language.Lang["Property"] + ": " + this._propertyName;
            }
        }
    }
}
