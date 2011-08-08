using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsanie.DmPoster {
    /// <summary>
    /// 配置类
    /// </summary>
    class Config {

        /// <summary>
        /// 应用程序标题
        /// </summary>
        public static readonly string Title;

        static Config() {
            Config.Title = string.Format("DmPoster {0}.{1}.{2}", Program.Version.Major, Program.Version.Minor, Program.Version.Build);
        }

    }
}
