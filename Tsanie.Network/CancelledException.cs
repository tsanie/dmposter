using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network {

    /// <summary>
    /// 取消动作异常类
    /// </summary>
    public class CancelledException : Exception {
        private string _command;

        /// <summary>
        /// 获取因何动作而取消
        /// </summary>
        public string Command {
            get { return _command; }
        }

        /// <summary>
        /// 构建取消动作异常
        /// </summary>
        /// <param name="state">当前的请求状态实例</param>
        /// <param name="command">命令字符串</param>
        public CancelledException(RequestState state, string command)
            : base((state == null ? null : state.Url) + " Interrupt.") {
            _command = command;
        }

        /// <summary>
        /// 获取该异常表达的字符串
        /// </summary>
        public override string Message {
            get {
                return base.Message + "\nCommand: " + _command;
            }
        }
    }
}
