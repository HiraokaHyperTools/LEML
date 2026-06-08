using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
                foreach (var pair in FieldBodyCombine.Combine(FieldBodyParser.Parse(GetValue("Content-Disposition") ?? "")))
                {
                    if (pair.Key.Equals("FileName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return pair.Value;
                    }
                }
                foreach (var pair in FieldBodyCombine.Combine(FieldBodyParser.Parse(GetValue("Content-Type") ?? "")))
                {
                    if (pair.Key.Equals("Name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return UtilDecodeRfc2047.Decode(pair.Value);
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
}
