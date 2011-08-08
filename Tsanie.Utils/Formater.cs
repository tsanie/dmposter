using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tsanie.Utils {
    public static class DanmakuFormater {
        public static string ToTimeString(this float playTime) {
            int minute = (int)Math.Floor(playTime) / 60;
            float seconds = playTime % 60.0f;
            return string.Format("{0:D2}:{1:00.#}", minute, seconds);
        }

        public static string ToColorString(this Color color) {
            return "#" + (color.ToArgb() & 0xffffff).ToString("X6");
        }
    }
}
