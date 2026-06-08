using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kenjiuno.LEML
{
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
}
