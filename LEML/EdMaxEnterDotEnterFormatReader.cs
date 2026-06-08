using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kenjiuno.LEML
{
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
}
