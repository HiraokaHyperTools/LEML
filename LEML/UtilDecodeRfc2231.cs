using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace kenjiuno.LEML
{
    /// <summary>
    /// RFC 2231 extended value decoder
    /// </summary>
    public class UtilDecodeRfc2231
    {
        public static string Decode(string body, Encoding defaultEncoding)
        {
            var match = Regex.Match(body, "^(?<charset>[^']+)'(?<language>([a-zA-Z]{1,8})(-[a-zA-Z]{1,8})?)?'(?<value>.*)$");
            if (match.Success)
            {
                var charset = match.Groups["charset"].Value;
                var value = match.Groups["value"].Value;
                return Encoding.GetEncoding(charset).GetString(
                    ParseUrlEncoded(value)
                );
            }
            else
            {
                var decoded1 = UtilDecodeRfc2047.Decode(body);
                if (decoded1 == body)
                {
                    return defaultEncoding.GetString(ParseUrlEncoded(body));
                }
                else
                {
                    return decoded1;
                }
            }
        }

        private static byte[] ParseUrlEncoded(string body)
        {
            var bytes = new List<byte>(body.Length);
            for (var x = 0; x < body.Length;)
            {
                if (body[x] == '%')
                {
                    if (byte.TryParse(body.Substring(x + 1, 2), NumberStyles.HexNumber, null, out byte one))
                    {
                        bytes.Add(one);
                        x += 3;
                    }
                    else
                    {
                        bytes.Add((byte)body[x]);
                        x++;
                    }
                }
                else
                {
                    bytes.Add((byte)body[x]);
                    x++;
                }
            }
            return bytes.ToArray();
        }
    }
}
