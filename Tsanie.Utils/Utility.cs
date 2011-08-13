using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Tsanie.Utils {
    public class Utility {
        public static readonly Random Rnd;

        static Utility() {
            Rnd = new Random();
        }

        public static string UrlEncode(string url) {
            return UrlEncode(url, Encoding.UTF8, false);
        }
        public static string UrlEncode(string url, Encoding encoding) {
            return UrlEncode(url, encoding, false);
        }
        public static string UrlEncode(string url, bool notSafe) {
            return UrlEncode(url, Encoding.UTF8, notSafe);
        }
        public static string UrlEncode(string url, Encoding encoding, bool notSafe) {
            if (url == null)
                return null;
            byte[] bytes = encoding.GetBytes(url);
            return Encoding.ASCII.GetString(UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false, notSafe));
        }
        public static string UrlDecode(string url) {
            return UrlDecode(url, Encoding.UTF8);
        }
        public static string UrlDecode(string url, Encoding encoding) {
            if (url == null)
                return null;
            return UrlDecodeStringFromStringInternal(url, encoding);
        }

        #region - 内部 -
        internal static char IntToHex(int n) {
            if (n <= 9) {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x41); // 0x61 => 0x41，修改%e6等为%E6
        }

        internal static bool IsSafe(char ch, bool notSafe) {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9'))) {
                return true;
            }
            if (!notSafe)
                return false;
            switch (ch) {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        private static byte[] UrlEncodeBytesToBytesInternal(
            byte[] bytes,
            int offset,
            int count,
            bool alwaysCreateReturnValue,
            bool notSafe
        ) {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++) {
                char ch = (char)bytes[offset + i];
                if (notSafe && ch == ' ') {
                    num++;
                } else if (!IsSafe(ch, notSafe)) {
                    num2++;
                }
            }
            if (!alwaysCreateReturnValue &&
                (notSafe && (num == 0)) &&
                (num2 == 0)) {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++) {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsSafe(ch2, notSafe)) {
                    buffer[num4++] = num6;
                } else if (notSafe && ch2 == ' ') {
                    buffer[num4++] = 0x2b;
                } else {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        private class UrlDecoder {
            private int _bufferSize;
            private byte[] _byteBuffer;
            private char[] _charBuffer;
            private Encoding _encoding;
            private int _numBytes;
            private int _numChars;

            internal UrlDecoder(int bufferSize, Encoding encoding) {
                this._bufferSize = bufferSize;
                this._encoding = encoding;
                this._charBuffer = new char[bufferSize];
            }

            internal void AddByte(byte b) {
                if (this._byteBuffer == null) {
                    this._byteBuffer = new byte[this._bufferSize];
                }
                this._byteBuffer[this._numBytes++] = b;
            }

            internal void AddChar(char ch) {
                if (this._numBytes > 0) {
                    this.FlushBytes();
                }
                this._charBuffer[this._numChars++] = ch;
            }

            private void FlushBytes() {
                if (this._numBytes > 0) {
                    this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
                    this._numBytes = 0;
                }
            }

            internal string GetString() {
                if (this._numBytes > 0) {
                    this.FlushBytes();
                }
                if (this._numChars > 0) {
                    return new string(this._charBuffer, 0, this._numChars);
                }
                return string.Empty;
            }
        }

        private static int HexToInt(char h) {
            if ((h >= '0') && (h <= '9')) {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f')) {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F')) {
                return ((h - 'A') + 10);
            }
            return -1;
        }

        private static string UrlDecodeStringFromStringInternal(string s, Encoding e) {
            int length = s.Length;
            UrlDecoder decoder = new UrlDecoder(length, e);
            for (int i = 0; i < length; i++) {
                char ch = s[i];
                if (ch == '+') {
                    ch = ' ';
                } else if ((ch == '%') && (i < (length - 2))) {
                    if ((s[i + 1] == 'u') && (i < (length - 5))) {
                        int num3 = HexToInt(s[i + 2]);
                        int num4 = HexToInt(s[i + 3]);
                        int num5 = HexToInt(s[i + 4]);
                        int num6 = HexToInt(s[i + 5]);
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0))) {
                            goto label_next;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        decoder.AddChar(ch);
                        continue;
                    }
                    int num7 = HexToInt(s[i + 1]);
                    int num8 = HexToInt(s[i + 2]);
                    if ((num7 >= 0) && (num8 >= 0)) {
                        byte b = (byte)((num7 << 4) | num8);
                        i += 2;
                        decoder.AddByte(b);
                        continue;
                    }
                }
            label_next:
                if ((ch & 0xff80) == 0) {
                    decoder.AddByte((byte)ch);
                } else {
                    decoder.AddChar(ch);
                }
            }
            return decoder.GetString();
        }

        #endregion

        private static char[] _htmlEntityEndingChars = new char[] { ';', '&' };

        public static string HtmlDecode(string value) {
            if (string.IsNullOrEmpty(value))
                return value;
            if (value.IndexOf('&') < 0)
                return value;
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            HtmlDecode(value, output);
            return output.ToString();
        }
        public static void HtmlDecode(string value, TextWriter output) {
            if (value != null) {
                if (output == null) {
                    throw new ArgumentNullException("output");
                }
                if (value.IndexOf('&') < 0) {
                    output.Write(value);
                } else {
                    int length = value.Length;
                    for (int i = 0; i < length; i++) {
                        char ch = value[i];
                        if (ch == '&') {
                            int num3 = value.IndexOfAny(_htmlEntityEndingChars, i + 1);
                            if ((num3 > 0) && (value[num3] == ';')) {
                                string entity = value.Substring(i + 1, (num3 - i) - 1);
                                if ((entity.Length > 1) && (entity[0] == '#')) {
                                    ushort num4;
                                    if ((entity[1] == 'x') || (entity[1] == 'X')) {
                                        ushort.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out num4);
                                    } else {
                                        ushort.TryParse(entity.Substring(1), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out num4);
                                    }
                                    if (num4 != 0) {
                                        ch = (char)num4;
                                        i = num3;
                                    }
                                } else {
                                    i = num3;
                                    char ch2 = HtmlEntities.Lookup(entity);
                                    if (ch2 != '\0') {
                                        ch = ch2;
                                    } else {
                                        output.Write('&');
                                        output.Write(entity);
                                        output.Write(';');
                                        goto Label_0117;
                                    }
                                }
                            }
                        }
                        output.Write(ch);
                    Label_0117:
                        ;
                    }
                }
            }
        }
        public static string HtmlEncode(string value) {
            if (string.IsNullOrEmpty(value))
                return value;
            if (IndexOfHtmlEncodingChars(value, 0) == -1)
                return value;
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            HtmlEncode(value, output);
            return output.ToString();
        }
        public static unsafe void HtmlEncode(string value, TextWriter output) {
            if (value != null) {
                if (output == null) {
                    throw new ArgumentNullException("output");
                }
                int num = IndexOfHtmlEncodingChars(value, 0);
                if (num == -1) {
                    output.Write(value);
                } else {
                    int num2 = value.Length - num;
                    fixed (char* str = value) {
                        char* chPtr = str;
                        while (num-- > 0) {
                            output.Write(*chPtr++);
                        }
                        while (num2-- > 0) {
                            char ch = *chPtr++;
                            if (ch <= '>') {
                                switch (ch) {
                                    case '&': {
                                            output.Write("&amp;");
                                            continue;
                                        }
                                    /*
                                     * fix: B站的xml未转义单引号
                                    case '\'': {
                                            output.Write("&#39;");
                                            continue;
                                        }
                                     */
                                    case '"': {
                                            output.Write("&quot;");
                                            continue;
                                        }
                                    case '<': {
                                            output.Write("&lt;");
                                            continue;
                                        }
                                    case '>': {
                                            output.Write("&gt;");
                                            continue;
                                        }
                                }
                                output.Write(ch);
                                continue;
                            }
                            /*
                             * fix: 也不转义这些字符
                            if ((ch >= '\x00a0') && (ch < 'Ā')) {
                                output.Write("&#");
                                output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                                output.Write(';');
                            } else {
                                output.Write(ch);
                            }
                            */
                            output.Write(ch);
                        }
                    }
                }
            }
        }

        #region - 内部 -

        private static unsafe int IndexOfHtmlEncodingChars(string s, int startPos) {
            int num = s.Length - startPos;
            fixed (char* str = s) {
                char* chPtr = str + startPos;
                while (num > 0) {
                    char ch = *chPtr++;
                    if (ch <= '>') {
                        switch (ch) {
                            case '&':
                            case '\'':
                            case '"':
                            case '<':
                            case '>':
                                return (s.Length - num);
                        }
                    } else if ((ch >= '\x00a0') && (ch < 'Ā')) {
                        return (s.Length - num);
                    }
                    num--;
                }
            }
            return -1;
        }

        #endregion

        #region - HtmlEntities -

        private static class HtmlEntities {
            // Fields
            private static string[] _entitiesList = new string[] { 
                "\"-quot", "&-amp", "'-apos", "<-lt", ">-gt", "\x00a0-nbsp", "\x00a1-iexcl", "\x00a2-cent", "\x00a3-pound", "\x00a4-curren", "\x00a5-yen", "\x00a6-brvbar", "\x00a7-sect", "\x00a8-uml", "\x00a9-copy", "\x00aa-ordf", 
                "\x00ab-laquo", "\x00ac-not", "\x00ad-shy", "\x00ae-reg", "\x00af-macr", "\x00b0-deg", "\x00b1-plusmn", "\x00b2-sup2", "\x00b3-sup3", "\x00b4-acute", "\x00b5-micro", "\x00b6-para", "\x00b7-middot", "\x00b8-cedil", "\x00b9-sup1", "\x00ba-ordm", 
                "\x00bb-raquo", "\x00bc-frac14", "\x00bd-frac12", "\x00be-frac34", "\x00bf-iquest", "\x00c0-Agrave", "\x00c1-Aacute", "\x00c2-Acirc", "\x00c3-Atilde", "\x00c4-Auml", "\x00c5-Aring", "\x00c6-AElig", "\x00c7-Ccedil", "\x00c8-Egrave", "\x00c9-Eacute", "\x00ca-Ecirc", 
                "\x00cb-Euml", "\x00cc-Igrave", "\x00cd-Iacute", "\x00ce-Icirc", "\x00cf-Iuml", "\x00d0-ETH", "\x00d1-Ntilde", "\x00d2-Ograve", "\x00d3-Oacute", "\x00d4-Ocirc", "\x00d5-Otilde", "\x00d6-Ouml", "\x00d7-times", "\x00d8-Oslash", "\x00d9-Ugrave", "\x00da-Uacute", 
                "\x00db-Ucirc", "\x00dc-Uuml", "\x00dd-Yacute", "\x00de-THORN", "\x00df-szlig", "\x00e0-agrave", "\x00e1-aacute", "\x00e2-acirc", "\x00e3-atilde", "\x00e4-auml", "\x00e5-aring", "\x00e6-aelig", "\x00e7-ccedil", "\x00e8-egrave", "\x00e9-eacute", "\x00ea-ecirc", 
                "\x00eb-euml", "\x00ec-igrave", "\x00ed-iacute", "\x00ee-icirc", "\x00ef-iuml", "\x00f0-eth", "\x00f1-ntilde", "\x00f2-ograve", "\x00f3-oacute", "\x00f4-ocirc", "\x00f5-otilde", "\x00f6-ouml", "\x00f7-divide", "\x00f8-oslash", "\x00f9-ugrave", "\x00fa-uacute", 
                "\x00fb-ucirc", "\x00fc-uuml", "\x00fd-yacute", "\x00fe-thorn", "\x00ff-yuml", "Œ-OElig", "œ-oelig", "Š-Scaron", "š-scaron", "Ÿ-Yuml", "ƒ-fnof", "ˆ-circ", "˜-tilde", "Α-Alpha", "Β-Beta", "Γ-Gamma", 
                "Δ-Delta", "Ε-Epsilon", "Ζ-Zeta", "Η-Eta", "Θ-Theta", "Ι-Iota", "Κ-Kappa", "Λ-Lambda", "Μ-Mu", "Ν-Nu", "Ξ-Xi", "Ο-Omicron", "Π-Pi", "Ρ-Rho", "Σ-Sigma", "Τ-Tau", 
                "Υ-Upsilon", "Φ-Phi", "Χ-Chi", "Ψ-Psi", "Ω-Omega", "α-alpha", "β-beta", "γ-gamma", "δ-delta", "ε-epsilon", "ζ-zeta", "η-eta", "θ-theta", "ι-iota", "κ-kappa", "λ-lambda", 
                "μ-mu", "ν-nu", "ξ-xi", "ο-omicron", "π-pi", "ρ-rho", "ς-sigmaf", "σ-sigma", "τ-tau", "υ-upsilon", "φ-phi", "χ-chi", "ψ-psi", "ω-omega", "ϑ-thetasym", "ϒ-upsih", 
                "ϖ-piv", " -ensp", " -emsp", " -thinsp", "‌-zwnj", "‍-zwj", "‎-lrm", "‏-rlm", "–-ndash", "—-mdash", "‘-lsquo", "’-rsquo", "‚-sbquo", "“-ldquo", "”-rdquo", "„-bdquo", 
                "†-dagger", "‡-Dagger", "•-bull", "…-hellip", "‰-permil", "′-prime", "″-Prime", "‹-lsaquo", "›-rsaquo", "‾-oline", "⁄-frasl", "€-euro", "ℑ-image", "℘-weierp", "ℜ-real", "™-trade", 
                "ℵ-alefsym", "←-larr", "↑-uarr", "→-rarr", "↓-darr", "↔-harr", "↵-crarr", "⇐-lArr", "⇑-uArr", "⇒-rArr", "⇓-dArr", "⇔-hArr", "∀-forall", "∂-part", "∃-exist", "∅-empty", 
                "∇-nabla", "∈-isin", "∉-notin", "∋-ni", "∏-prod", "∑-sum", "−-minus", "∗-lowast", "√-radic", "∝-prop", "∞-infin", "∠-ang", "∧-and", "∨-or", "∩-cap", "∪-cup", 
                "∫-int", "∴-there4", "∼-sim", "≅-cong", "≈-asymp", "≠-ne", "≡-equiv", "≤-le", "≥-ge", "⊂-sub", "⊃-sup", "⊄-nsub", "⊆-sube", "⊇-supe", "⊕-oplus", "⊗-otimes", 
                "⊥-perp", "⋅-sdot", "⌈-lceil", "⌉-rceil", "⌊-lfloor", "⌋-rfloor", "〈-lang", "〉-rang", "◊-loz", "♠-spades", "♣-clubs", "♥-hearts", "♦-diams"
             };
            private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

            // Methods
            private static Dictionary<string, char> GenerateLookupTable() {
                Dictionary<string, char> dictionary = new Dictionary<string, char>(StringComparer.Ordinal);
                foreach (string str in _entitiesList) {
                    dictionary.Add(str.Substring(2), str[0]);
                }
                return dictionary;
            }

            public static char Lookup(string entity) {
                char ch;
                _lookupTable.TryGetValue(entity, out ch);
                return ch;
            }
        }

        #endregion
    }
}
