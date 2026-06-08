using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace kenjiuno.LEML
{
    /// <summary>
    /// Combine splitted field body like `name*0`, `name*1`, `name*2` to `name`
    /// </summary>
    public class FieldBodyCombine
    {
        private static readonly Regex _isMultiple = new Regex(@"^(?<name>[^\*]+)\*(?<index>\d+)(?<rfc5987>\*)?$");

        /// <summary>
        /// Combine splitted field body
        /// </summary>
        public static List<KeyValuePair<string, string>> Combine(
            IEnumerable<KeyValuePair<string, string>> sourcePairs
        )
        {
            var pairs = new List<KeyValuePair<string, string>>();
            var extendedPairs = new List<KeyValuePair<string, string>>();

            foreach (var sourcePair in sourcePairs)
            {
                var match = _isMultiple.Match(sourcePair.Key);
                if (match.Success)
                {
                    var newKey = match.Groups["name"].Value;
                    var isRfc5987 = match.Groups["rfc5987"].Success;
                    var thisPairs = isRfc5987 ? extendedPairs : pairs;
                    var y = 0;
                    var cy = thisPairs.Count;
                    for (; y < cy; y++)
                    {
                        if (thisPairs[y].Key == newKey)
                        {
                            thisPairs[y] = new KeyValuePair<string, string>(
                                newKey,
                                thisPairs[y].Value + sourcePair.Value
                            );
                            break;
                        }
                    }
                    if (y == cy)
                    {
                        thisPairs.Add(new KeyValuePair<string, string>(
                            newKey,
                            sourcePair.Value
                        ));
                    }
                }
                else
                {
                    pairs.Add(
                        new KeyValuePair<string, string>(
                            sourcePair.Key,
                            UtilDecodeRfc2231.Decode(sourcePair.Value, Encoding.UTF8)
                        )
                    );
                }
            }

            foreach (var pair in extendedPairs)
            {
                pairs.Add(
                    new KeyValuePair<string, string>(
                        pair.Key,
                        UtilDecodeRfc2231.Decode(pair.Value, Encoding.UTF8)
                    )
                );
            }

            return pairs;
        }
    }
}
