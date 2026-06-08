using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace kenjiuno.LEML
{
    /// <summary>
    /// RFC 2047 encoded-word decoder
    /// </summary>
    /// <example>
    /// - `abc` → `abc`
    /// - `=?iso-8859-1?q?this=20is=20some=20text?=` → `this is some text`
    /// - `=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?=
    ///   =?ISO-8859-2?B?dSB1bmRlcnN0YW5kIHRoZSBleGFtcGxlLg==?=` 
    ///   → `If you can read this you understand the example.`
    /// </example>
    public static class UtilDecodeRfc2047
    {
        public static string Decode(String text)
        {
            var result = "";

            var lastIndex = 0;

            var hadEncodedWord = false;

            var heading = true;

            text = text
                .Replace("\r\n", "")
                .Replace("\n", "");

            foreach (Match match in Regex.Matches(text, "=\\?(?<c>[^\\?]+)\\?(?<e>[BbQq])\\?(?<b>[^\\?]+)\\?="))
            {
                var leading = text.Substring(lastIndex, match.Index - lastIndex);

                if ((hadEncodedWord || heading) && string.IsNullOrWhiteSpace(leading))
                {
                    // skip

                    /*
                     * https://www.ietf.org/rfc/rfc2047.txt
                     * 
                     * from 6.2
                     * 
                     * When displaying a particular header field that contains multiple
                     * 'encoded-word's, any 'linear-white-space' that separates a pair of
                     * adjacent 'encoded-word's is ignored.
                     * 
                     */

                    heading = false;
                }
                else
                {
                    result += leading;
                }

                result += DecodeEncodedWord(match);

                lastIndex = match.Index + match.Length;

                hadEncodedWord = true;
            }

            var trailing = text.Substring(lastIndex);

            if (!string.IsNullOrWhiteSpace(trailing))
            {
                result += trailing;
            }

            return result;
        }

        static string DecodeEncodedWord(Match M)
        {
            var c = M.Groups["c"].Value;
            var e = M.Groups["e"].Value;
            var b = M.Groups["b"].Value;
            byte[] bin = null;
            if (e == "B" || e == "b")
            {
                bin = Convert.FromBase64String(b);
            }
            else
            {
                bin = FromQuotedPrintable(b);
            }
            Encoding enc = Encoding.GetEncoding(c) ?? Encoding.GetEncoding("latin1");
            return enc.GetString(bin);
        }

        static byte[] FromQuotedPrintable(String s)
        {
            MemoryStream os = new MemoryStream();
            String hex = "0123456789ABCDEF";
            for (int x = 0, cx = s.Length; x < cx;)
            {
                if (s[x] == ' ' || s[x] == '_')
                {
                    os.WriteByte((byte)' ');
                    x++;
                    continue;
                }
                if (s[x] == '=')
                {
                    if (x + 2 < cx)
                    {
                        int a = hex.IndexOf(char.ToUpperInvariant(s[x + 1]));
                        int b = hex.IndexOf(char.ToUpperInvariant(s[x + 2]));
                        if (a >= 0 && b >= 0)
                        {
                            os.WriteByte((byte)((a << 4) | b));
                            x += 3;
                            continue;
                        }
                    }
                }
                os.WriteByte((byte)s[x]);
                x++;
            }
            return os.ToArray();
        }
    }
}
