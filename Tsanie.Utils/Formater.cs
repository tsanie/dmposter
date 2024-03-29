﻿using System;
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
            return color.ToColorString(true);
        }

        public static string ToColorString(this Color color, bool mark) {
            return (mark ? "#" : null) + (color.ToArgb() & 0xffffff).ToString("X6");
        }

        public static string ToRgbIntString(this Color color) {
            return (color.ToArgb() & 0xffffff).ToString();
        }

        public static Color ToColor(this string str) {
            if (str == null || str.Length == 0)
                throw new NullReferenceException("ColorString");
            if (str[0] == '#') {
                return Color.FromArgb(int.Parse(str.Substring(1), System.Globalization.NumberStyles.HexNumber) | -16777216);
            }
            return Color.FromArgb(int.Parse(str) | -16777216);
        }
    }

    public static class DateTimeFormater {
        public static readonly long Ticks_1970_1_1 = 621355968000000000L;
        public static long ToMilliseconds(this DateTime dt) {
            return (dt.ToUniversalTime().Ticks - Ticks_1970_1_1) / 10000000;
        }
        public static DateTime ParseDateTime(this long milliseconds) {
            long ticks = milliseconds * 10000000 + Ticks_1970_1_1;
            return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
        }
    }

    public static class StringFormater {
        public static string GetFilename(this string str) {
            return (str == null ? null : str.Substring(str.LastIndexOf('\\') + 1));
        }
    }
}
