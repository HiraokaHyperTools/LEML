using kenjiuno.LEML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEML.Tests
{
    public class UtilGetAttachmentFilenameTest
    {
        [Test]
        // Thunderbird 152.0b2
        [TestCase(
            11,
            "新しいリッチ テキスト ドキュメント.rtf",
            "新しいリッチ テキスト ドキュメント.rtf", 
            "application/msword;\r\n name=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6U=?=\r\n =?UTF-8?B?44Oh44Oz44OILnJ0Zg==?=\"", 
            "attachment;\r\n filename*0*=UTF-8''%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81;\r\n filename*1*=%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD;\r\n filename*2*=%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66"
        )]
        // Thunderbird 152.0b2
        [TestCase(
            12,
            "File.rtf",
            "File.rtf",
            "application/msword; name=\"File.rtf\"",
            "attachment; filename=\"File.rtf\""
        )]
        // Sylpheed 3.7.0
        [TestCase(
            21,
            "新しいリッチ テキスト ドキュメント.rtf",
            "新しいリッチ テキスト ドキュメント.rtf",
            "text/rtf;\r\n name=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhC?=\r\n =?ISO-2022-JP?B?IBskQiVJJS0lZSVhJXMlSBsoQi5ydGY=?=\"",
            "attachment;\r\n filename=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhC?=\r\n =?ISO-2022-JP?B?IBskQiVJJS0lZSVhJXMlSBsoQi5ydGY=?=\""
        )]
        // Sylpheed 3.7.0
        [TestCase(
            22,
            "File.rtf",
            "File.rtf",
            "text/rtf;\r\n name=\"File.rtf\"",
            "attachment;\r\n filename=\"File.rtf\""
        )]
        // Outlook 2013
        [TestCase(
            31,
            "新しいリッチ テキスト ドキュメント.rtf",
            "新しいリッチ テキスト ドキュメント.rtf",
            "application/msword;\r\n\tname=\"=?iso-2022-jp?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIA==?=\r\n\t=?iso-2022-jp?B?GyRCJUklLSVlJWElcyVIGyhCLnJ0Zg==?=\"",
            "attachment;\r\n\tfilename=\"=?iso-2022-jp?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIA==?=\r\n\t=?iso-2022-jp?B?GyRCJUklLSVlJWElcyVIGyhCLnJ0Zg==?=\""
        )]
        // Outlook 2013
        [TestCase(
            32,
            "File.rtf",
            "File.rtf",
            "application/msword;\r\n\tname=\"File.rtf\"",
            "attachment;\r\n\tfilename=\"File.rtf\""
        )]
        // GMail.com
        [TestCase(
            41,
            "新しいリッチ テキスト ドキュメント.rtf",
            "新しいリッチ テキスト ドキュメント.rtf",
            "application/rtf; \r\n\tname=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6Xjg6Hjg7Pjg4gucnRm?=\"",
            "attachment; \r\n\tfilename=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6Xjg6Hjg7Pjg4gucnRm?=\""
        )]
        // GMail.com
        [TestCase(
            42,
            "File.rtf",
            "File.rtf",
            "application/rtf; name=\"File.rtf\"",
            "attachment; filename=\"File.rtf\""
        )]
        // EdMax 2.86.3F
        [TestCase(
            51,
            "新しいリッチ テキスト ドキュメント.rtf",
            "新しいリッチ テキスト ドキュメント.rtf",
            "application/octet-stream; name=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIBskQiVJJS0lZSVhJXMlSBsoQg==?=.rtf\"",
            "attachment; filename=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIBskQiVJJS0lZSVhJXMlSBsoQg==?=.rtf\""
        )]
        // EdMax 2.86.3F
        [TestCase(
            52,
            "File.rtf",
            "File.rtf",
            "application/octet-stream; name=\"File.rtf\"",
            "attachment; filename=\"File.rtf\""
        )]
        public void Decodes(
            int number,
            string expectedContentTypeName,
            string expectedContentDispositionFilename,
            string contentType,
            string contentDisposition
        )
        {
            var dictHeaders = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (contentType != null)
            {
                dictHeaders["Content-Type"] = contentType;
            }
            if (contentDisposition != null)
            {
                dictHeaders["Content-Disposition"] = contentDisposition;
            }

            var result = UtilGetAttachmentFilename.GetAttachmentFilename(dictHeaders);

            ClassicAssert.AreEqual(expectedContentTypeName, result.ContentTypeName);
            ClassicAssert.AreEqual(expectedContentDispositionFilename, result.ContentDispositionFilename);
        }
    }
}
