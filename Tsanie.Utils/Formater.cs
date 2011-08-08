using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tsanie.Utils {
    public static class DanmakuFormater {
        public static string ToTimeString(this float playTime) {
            int secondsInt = (int)Math.Floor(playTime);
            int hour = secondsInt / 3600;
            int minute = (secondsInt % 3600) / 60;
            float seconds = playTime % 60.0f;
            return string.Format("{0}:{1:D2}:{2:00.#}", hour, minute, seconds);
        }

        public static string ToColorString(this Color color) {
            return "#" + (color.ToArgb() & 0xffffff).ToString("X6");
        }
    }
}
