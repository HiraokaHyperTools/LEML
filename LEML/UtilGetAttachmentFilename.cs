using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace kenjiuno.LEML
{
    public class UtilGetAttachmentFilename
    {
        public static GetAttachmentFilenameResult GetAttachmentFilename(IDictionary<string, string> dictHeaders)
        {
            var result = new GetAttachmentFilenameResult();

            {
                if (true
                    && dictHeaders.TryGetValue("Content-Disposition", out string contentDisposition)
                    && !string.IsNullOrEmpty(contentDisposition)
                )
                {
                    var list1 = FieldBodyParser.Parse(contentDisposition);
                    var list2 = FieldBodyCombine.Combine(list1);

                    {
                        foreach (var pair in list2)
                        {
                            if (false) { }
                            else if (pair.Key == "filename")
                            {
                                result.ContentDispositionFilename = pair.Value;
                                break;
                            }
                        }
                    }
                }
            }

            {
                if (true
                    && dictHeaders.TryGetValue("Content-Type", out string contentType)
                    && !string.IsNullOrEmpty(contentType)
                )
                {
                    var elements = FieldBodyParser.Parse(contentType);

                    foreach (var pair in elements)
                    {
                        if (false) { }
                        else if (pair.Key == "name")
                        {
                            result.ContentTypeName = UtilDecodeRfc2047.Decode(pair.Value);
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }
}
