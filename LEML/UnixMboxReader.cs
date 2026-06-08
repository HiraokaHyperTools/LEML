using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kenjiuno.LEML
{
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
