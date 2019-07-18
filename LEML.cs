using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace kenjiuno.LEML
{
    /// <summary>
    /// RFC 822 EML decoder
    /// </summary>
    public class EML
    {
        /// <summary>
        /// The parsed `Mail` entity
        /// </summary>
        public Mail entity { get; internal set; }
        /// <summary>
        /// All headers that allow same key names
        /// </summary>
        public List<KeyValuePair<string, string>> allHeaders { get; } = new List<KeyValuePair<string, string>>();
        /// <summary>
        /// All headers of last occurence associated with its header field name
        /// </summary>
        public SortedDictionary<string, string> dictHeaders { get; } = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Search last header field value by name
        /// </summary>
        /// <param name="key"></param>
        /// <returns>text if found, otherwise null</returns>
        public string GetValue(string key)
        {
            string text;
            if (!dictHeaders.TryGetValue(key, out text))
            {
                text = null;
            }
            return text;
        }

        /// <summary>
        /// MIME type
        /// </summary>
        /// <returns>Content type</returns>
        public override string ToString()
        {
            return ContentType;
        }

        /// <summary>
        /// body includes multipart entities (latin1, LF)
        /// </summary>
        public String fullBody { get; set; }

        /// <summary>
        /// Output (Main contents, latin1, LF)
        /// </summary>
        public String contents { get; set; }

        /// <summary>
        /// The decoded multipart entities
        /// </summary>
        public List<EML> multiparts = new List<EML>();

        /// <summary>
        /// Consturct from mail entity
        /// </summary>
        /// <param name="entity">mail entity</param>
        public EML(Mail entity)
        {
            this.entity = entity;
            var rows = entity.rawBody.Replace("\r\n", "\n").Split('\n');
            String key = null, value = null;
            int y = 0, cy = rows.Length;
            for (; y < cy; y++)
            {
                var row = rows[y];
                if (row.Length == 0)
                {
                    y++;
                    break;
                }
                if (!char.IsWhiteSpace(row[0]))
                {
                    if (key != null)
                    {
                        allHeaders.Add(new KeyValuePair<string, string>(key, value));
                        key = null; value = null;
                    }
                    String[] cols = row.Split(new char[] { ':' }, 2);
                    if (cols.Length == 2)
                    {
                        key = cols[0].TrimEnd();
                        value = cols[1].TrimStart();
                    }
                }
                else
                {
                    if (key != null)
                    {
                        value += "\n";
                        value += row;
                    }
                }
            }
            if (key != null)
            {
                allHeaders.Add(new KeyValuePair<string, string>(key, value));
            }
            foreach (var pair in allHeaders)
            {
                dictHeaders[pair.Key] = pair.Value;
            }
            fullBody = String.Join("\n", rows, y, cy - y);

            if ("multipart".Equals(PerceivedType))
            {
                String boundary = Boundary;
                String p1 = "--" + boundary;
                String p2 = "--" + boundary + "--";

                StringBuilder b = new StringBuilder();
                int z = 0;
                foreach (String row in fullBody.Split('\n'))
                {
                    if (p2.Equals(row)) break;
                    if (p1.Equals(row))
                    {
                        if (z == 0)
                        {
                            contents = b.ToString();
                        }
                        else
                        {
                            multiparts.Add(new EML(entity.CreateChild(b.ToString())));
                        }
                        b.Length = 0;
                        z++;
                    }
                    else
                    {
                        b.Append(row + "\n");
                    }
                }
                {
                    {
                        if (z == 0)
                        {
                            contents = b.ToString();
                        }
                        else
                        {
                            multiparts.Add(new EML(entity.CreateChild(b.ToString())));
                        }
                    }
                }
            }
            else
            {
                contents = fullBody;
            }
        }

        /// <summary>
        /// `Content-Type` media type, otherwise empty
        /// </summary>
        public String ContentType
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Type") ?? ""))
                {
                    if (pair.Key.Length == 0)
                    {
                        return pair.Value;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// `Content-Disposition` type, otherwise empty
        /// </summary>
        public String ContentDisposition
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Disposition") ?? ""))
                {
                    if (pair.Key.Length == 0)
                    {
                        return pair.Value;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// `Content-Disposition`.`FileName`, or `Content-Type`.`Name`, otherwise empty
        /// </summary>
        public String FileName
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Disposition") ?? ""))
                {
                    if (pair.Key.Equals("FileName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return pair.Value;
                    }
                }
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Type") ?? ""))
                {
                    if (pair.Key.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return pair.Value;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// `From` header field body, otherwise empty.
        /// </summary>
        public String From
        {
            get
            {
                return GetValue("From") ?? "";
            }
        }

        /// <summary>
        /// `To` header field body, otherwise empty.
        /// </summary>
        public String To
        {
            get
            {
                return GetValue("To") ?? "";
            }
        }

        /// <summary>
        /// `Subject` header field body, otherwise empty.
        /// </summary>
        public String Subject
        {
            get
            {
                return GetValue("Subject") ?? "";
            }
        }

        /// <summary>
        /// The `type` part of `ContentType`, otherwise empty.
        /// </summary>
        public String PerceivedType
        {
            get
            {
                String[] cols = ContentType.Split('/');
                return cols[0];
            }
        }

        /// <summary>
        /// boundary in `Content-Type`, otherwise empty.
        /// </summary>
        public String Boundary
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Type") ?? ""))
                {
                    if (StringComparer.InvariantCultureIgnoreCase.Compare("boundary", pair.Key) == 0)
                    {
                        return pair.Value;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// charset parameter value in `Content-Type`, otherwise empty.
        /// </summary>
        public String CharacterSet
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Type") ?? ""))
                {
                    if (StringComparer.InvariantCultureIgnoreCase.Compare("charset", pair.Key) == 0)
                    {
                        return pair.Value;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// `Content-Transfer-Encoding` header field body, otherwise empty
        /// 
        /// - empty
        /// - `base64`
        /// </summary>
        public String ContentTransferEncoding
        {
            get
            {
                foreach (var pair in FieldBodyParser.Parse(GetValue("Content-Transfer-Encoding") ?? ""))
                {
                    if (pair.Key.Length == 0)
                    {
                        return pair.Value;
                    }
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Main contents in byte array
        /// </summary>
        public byte[] RawContents
        {
            get
            {
                return UtilMimeBody.GetBodyBytes(this);
            }
        }

        /// <summary>
        /// Main contents text body in readable Unicode
        /// </summary>
        public String MessageBody
        {
            get
            {
                String charSet = CharacterSet;
                if (charSet.Length == 0)
                {
                    charSet = "ascii";
                }
                else if ("cp932".Equals(charSet))
                {
                    charSet = "Shift_JIS";
                }
                return Encoding.GetEncoding(charSet).GetString(UtilMimeBody.GetBodyBytes(this));
            }
        }

        /// <summary>
        /// Parsed `Date` header field body, otherwise null.
        /// </summary>
        public DateTime? Date
        {
            get
            {
                try
                {
                    return DateTime.Parse(GetValue("Date"));
                }
                catch (FormatException)
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Raw data composer for EML
    /// </summary>
    public static class UtilMimeBody
    {
        /// <summary>
        /// Encode `EML.contents` to message body bytes according to `EML.ContentTransferEncoding`
        /// </summary>
        /// <param name="eml"></param>
        /// <returns>byte array</returns>
        public static byte[] GetBodyBytes(EML eml)
        {
            var encoding = eml.ContentTransferEncoding;
            if (encoding == "base64")
            {
                return Convert.FromBase64String(eml.contents);
            }
            return Encoding.GetEncoding("latin1").GetBytes(eml.contents);
        }
    }

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

            foreach (Match match in Regex.Matches(text, "=\\?(?<c>[^\\?]+)\\?(?<e>[BbQq])\\?(?<b>[^\\?]+)\\?="))
            {
                var leading = text.Substring(lastIndex, match.Index - lastIndex);

                if (hadEncodedWord && string.IsNullOrWhiteSpace(leading))
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
                }
                else
                {
                    result += leading;
                }

                result += DecodeEncodedWord(match);

                lastIndex = match.Index + match.Length;

                hadEncodedWord = true;
            }

            result += text.Substring(lastIndex);

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
                if (s[x] == ' ')
                {
                    os.WriteByte((byte)' ');
                    x++;
                    continue;
                }
                if (s[x] == '=')
                {
                    if (x + 2 < cx)
                    {
                        int a = hex.IndexOf(s[x + 1]);
                        int b = hex.IndexOf(s[x + 2]);
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

    /// <summary>
    /// Field body key-value pair values parser
    /// </summary>
    /// <remarks>
    /// For example:
    /// 
    /// - `text/plain` → `{"": "text/plain"}`
    /// - `text/plain; charset=utf-8` → `{"": "text/plain", "charset": "utf-8"}`
    /// </remarks>
    public static class FieldBodyParser
    {
        /// <summary>
        /// Parse field body
        /// </summary>
        /// <param name="body"></param>
        /// <returns>pairs</returns>
        public static List<KeyValuePair<string, string>> Parse(String body)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            for (int x = 0, cx = body.Length; x < cx;)
            {
                while (x < cx && char.IsWhiteSpace(body[x]))
                {
                    x++;
                }
                String key = "";
                bool isPair = false;
                while (x < cx)
                {
                    if (body[x] == ';')
                    {
                        x++;
                        break;
                    }
                    else if (body[x] == '=')
                    {
                        x++;
                        isPair = true;
                        break;
                    }
                    else
                    {
                        key += body[x];
                        x++;
                    }
                }
                if (!isPair)
                {
                    pairs.Add(new KeyValuePair<string, string>("", key));
                    continue;
                }
                while (x < cx && char.IsWhiteSpace(body[x]))
                {
                    x++;
                }
                if (x < cx)
                {
                    String value = "";
                    if (body[x] == '"')
                    {
                        x++;
                        while (x < cx)
                        {
                            if (body[x] == '"')
                            {
                                x++;
                                break;
                            }
                            else
                            {
                                value += body[x];
                                x++;
                            }
                        }
                    }
                    else
                    {
                        while (x < cx)
                        {
                            if (body[x] == ';')
                            {
                                x++;
                                break;
                            }
                            else
                            {
                                value += body[x];
                                x++;
                            }
                        }
                    }
                    pairs.Add(new KeyValuePair<string, string>(key, value));
                }
            }
            return pairs;
        }
    }

    /// <summary>
    /// Mail or multipart entity
    /// </summary>
    public class Mail
    {
        /// <summary>
        /// file path
        /// </summary>
        public String filePath { get; set; }
        /// <summary>
        /// unique id
        /// </summary>
        public String uuid { get; set; }
        /// <summary>
        /// body in latin1
        /// </summary>
        public String rawBody { get; set; }

        /// <summary>
        /// parent entity for multipart/*
        /// </summary>
        public Mail parentEntity { get; set; }

        internal Mail CreateChild(String childRawBody)
        {
            return new Mail { filePath = filePath, uuid = uuid, rawBody = childRawBody, parentEntity = this };
        }

        /// <summary>
        /// Load from file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static Mail FromFile(string filePath)
        {
            return new Mail { filePath = filePath, uuid = filePath, rawBody = File.ReadAllText(filePath, Encoding.GetEncoding("latin1")) };
        }

    }

    /// <summary>
    /// EdMax `(Enter).(Enter)` exported mails format reader.
    /// </summary>
    public class EdMaxEnterDotEnterFormatReader
    {
        static readonly Encoding raw = Encoding.GetEncoding("latin1");
        static readonly Encoding jis = Encoding.GetEncoding("iso-2022-jp");
        static readonly Encoding sjis = Encoding.GetEncoding(932);
        static readonly Encoding eucjp = Encoding.GetEncoding("euc-jp");

        /// <summary>
        /// Load from file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>mails</returns>
        public IEnumerable<Mail> LoadFrom(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, raw))
            {
                return LoadFrom(reader, filePath);
            }
        }

        /// <summary>
        /// Load from string reader
        /// </summary>
        /// <param name="reader">reader</param>
        /// <param name="referenceFilePath">set this to filePath for future reference</param>
        /// <returns>mails</returns>
        public IEnumerable<Mail> LoadFrom(TextReader reader, string referenceFilePath)
        {
            String row;
            StringBuilder b = new StringBuilder();
            int z = 0;
            while (null != (row = reader.ReadLine()))
            {
                if (row == ".")
                {
                    yield return Convert(new Mail { filePath = referenceFilePath, uuid = referenceFilePath + "/" + z, rawBody = b.ToString() });
                    z++;
                    b.Length = 0;
                }
                else
                {
                    b.Append(row + "\n");
                }
            }
            yield return Convert(new Mail { filePath = referenceFilePath, uuid = referenceFilePath + "/" + z, rawBody = b.ToString() });
        }

        private Mail Convert(Mail entity)
        {
            EML eml = new EML(entity);
            var cs = eml.CharacterSet;
            // box は sjis で入っている。
            if ("|utf-8|utf_8|utf8|".IndexOf("|" + cs + "|", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                String s = raw.GetString(Encoding.UTF8.GetBytes(sjis.GetString(raw.GetBytes(entity.rawBody))));
                entity.rawBody = s;
            }
            else if ("|euc_jp|eucjp|euc-jp|".IndexOf("|" + cs + "|", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                String s = raw.GetString(eucjp.GetBytes(sjis.GetString(raw.GetBytes(entity.rawBody))));
                entity.rawBody = s;
            }
            else if ("|iso-2022-jp|".IndexOf("|" + cs + "|", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                String s = raw.GetString(jis.GetBytes(sjis.GetString(raw.GetBytes(entity.rawBody))));
                entity.rawBody = s;
            }
            else
            {
                String s = raw.GetString(sjis.GetBytes(sjis.GetString(raw.GetBytes(entity.rawBody))));
                entity.rawBody = s;
            }
            return entity;
        }
    }

    /// <summary>
    /// UNIX MBOX file decoder.
    /// Splitting mails by `"From "` line.
    /// </summary>
    public static class UnixMboxReader
    {
        /// <summary>
        /// Load mails from UNIX MBOX file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>mails</returns>
        public static IEnumerable<Mail> LoadFrom(string filePath)
        {
            var raw = Encoding.GetEncoding("latin1");
            using (StreamReader reader = new StreamReader(filePath, raw))
            {
                return LoadFrom(reader, filePath);
            }
        }

        /// <summary>
        /// Load mails from UNIX MBOX string reader
        /// </summary>
        /// <param name="reader">string reader</param>
        /// <returns>mails</returns>
        public static IEnumerable<Mail> LoadFrom(TextReader reader, string referenceFilePath)
        {
            String row;
            StringBuilder b = new StringBuilder();
            bool any = false;
            bool started = false;
            while (null != (row = reader.ReadLine()))
            {
                if (row.StartsWith("From "))
                {
                    if (any && started)
                    {
                        yield return new Mail { filePath = referenceFilePath, rawBody = b.ToString() };
                        any = false;
                    }
                    b.Length = 0;
                    started = true;
                }
                else if (row.StartsWith(">") && row.TrimStart('>').StartsWith("From"))
                {
                    b.Append(row.Substring(1) + "\n");
                    any = true;
                }
                else
                {
                    b.Append(row + "\n");
                    any = true;
                }
            }
            if (any)
            {
                yield return new Mail { filePath = referenceFilePath, rawBody = b.ToString() };
            }
        }
    }
}
