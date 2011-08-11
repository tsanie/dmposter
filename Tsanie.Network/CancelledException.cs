using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.Network {
    public class CancelledException : Exception {
        private string _command;

        /// <summary>
        /// 获取因何动作而取消
        /// </summary>
        public string Command {
            get { return _command; }
        }

        public CancelledException(RequestState state, string command)
            : base(state.Url + " Interrupt.") {
            _command = command;
        }

        public override string Message {
            get {
                return base.Message + "\nCommand: " + _command;
            }
        }
    }
}
