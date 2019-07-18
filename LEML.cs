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
        /// Mail or multipart entity
        /// </summary>
        public Mail entity { get; internal set; }
        /// <summary>
        /// All headers that allow same key names
        /// </summary>
        public List<KeyValuePair<string, string>> allHeaders { get; } = new List<KeyValuePair<string, string>>();
        /// <summary>
        /// Some headers: last one captured by key name
        /// </summary>
        public SortedDictionary<string, string> dictHeaders { get; } = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Obtain last header value by key name
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
        /// multipart entites
        /// </summary>
        public List<EML> multiparts = new List<EML>();

        /// <summary>
        /// Consturct from mail entity
        /// </summary>
        /// <param name="entity"></param>
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
        /// Content-Type, or string.Empty
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
        /// Content-Disposition, or string.Empty
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
        /// Content-Disposition.FileName, or Content-Type.Name, or string.Empty
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
        /// From, or string.Empty
        /// </summary>
        public String From
        {
            get
            {
                return GetValue("From") ?? "";
            }
        }

        /// <summary>
        /// To, or string.Empty
        /// </summary>
        public String To
        {
            get
            {
                return GetValue("To") ?? "";
            }
        }

        /// <summary>
        /// Subject, or string.Empty
        /// </summary>
        public String Subject
        {
            get
            {
                return GetValue("Subject") ?? "";
            }
        }

        /// <summary>
        /// PerceivedType of ContentType, or string.Empty
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
        /// boundary in Content-Type, or string.Empty
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
        /// charset in Content-Type, or string.Empty
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
        /// Content-Transfer-Encoding or string.Empty
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
        /// Main contents text body in readable Unicode (decodes ContentTransferEncoding)
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
        /// Date or string.Empty
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
        /// Get bytes from EML
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
    public static class UtilDecodeRfc2047
    {
        public static string Decode(String text)
        {
            String r = Regex.Replace(text, "=\\?(?<c>[^\\?]+)\\?(?<e>[BbQq])\\?(?<b>[^\\?]+)\\?=", Repl);
            return r;
        }

        static string Repl(Match M)
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
    /// key = value
    /// <list type="bullet">
    /// <item>&quot;&quot; = &quot;text/plain&quot;</item>
    /// <item>&quot;charset&quot; = &quot;utf-8&quot;</item>
    /// </list>
    /// </remarks>
    public static class FieldBodyParser
    {
        /// <summary>
        /// Parse field body
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> Parse(String s)
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            for (int x = 0, cx = s.Length; x < cx;)
            {
                while (x < cx && char.IsWhiteSpace(s[x]))
                {
                    x++;
                }
                String key = "";
                bool isPair = false;
                while (x < cx)
                {
                    if (s[x] == ';')
                    {
                        x++;
                        break;
                    }
                    else if (s[x] == '=')
                    {
                        x++;
                        isPair = true;
                        break;
                    }
                    else
                    {
                        key += s[x];
                        x++;
                    }
                }
                if (!isPair)
                {
                    pairs.Add(new KeyValuePair<string, string>("", key));
                    continue;
                }
                while (x < cx && char.IsWhiteSpace(s[x]))
                {
                    x++;
                }
                if (x < cx)
                {
                    String value = "";
                    if (s[x] == '"')
                    {
                        x++;
                        while (x < cx)
                        {
                            if (s[x] == '"')
                            {
                                x++;
                                break;
                            }
                            else
                            {
                                value += s[x];
                                x++;
                            }
                        }
                    }
                    else
                    {
                        while (x < cx)
                        {
                            if (s[x] == ';')
                            {
                                x++;
                                break;
                            }
                            else
                            {
                                value += s[x];
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
    /// EdMax MBOX file Reader
    /// </summary>
    public class EdMaxEnterDotEnterReader
    {
        static readonly Encoding raw = Encoding.GetEncoding("latin1");
        static readonly Encoding jis = Encoding.GetEncoding("iso-2022-jp");
        static readonly Encoding sjis = Encoding.GetEncoding(932);
        static readonly Encoding eucjp = Encoding.GetEncoding("euc-jp");

        /// <summary>
        /// Load EdMax MBOX file
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
        /// Load EdMax `(Enter).(Enter)` format string reader
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
    /// UNIX MBOX file decoder
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
