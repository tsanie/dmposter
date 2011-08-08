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

        public static Color ToColor(this string str) {
            if (str == null || str.Length == 0)
                throw new NullReferenceException("ColorString");
            if (str[0] != '#') {
                return Color.FromArgb(int.Parse(str.Substring(1), System.Globalization.NumberStyles.HexNumber) | -16777216);
            }
            return Color.FromArgb(int.Parse(str) | -16777216);
        }
    }
}
