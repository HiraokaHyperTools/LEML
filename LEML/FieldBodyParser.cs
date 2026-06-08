using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kenjiuno.LEML
{
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
}
