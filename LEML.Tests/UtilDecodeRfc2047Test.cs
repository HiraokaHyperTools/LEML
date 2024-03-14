using kenjiuno.LEML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEML.Tests
{
    public class UtilDecodeRfc2047Test
    {
        [Test]
        [TestCase("abc", "abc")]
        // https://www.akanko.net/marimo/data/rfc/rfc2047-jp.txt
        [TestCase("this is some text", "=?iso-8859-1?q?this=20is=20some=20text?=")]
        [TestCase("this is some text", " =?iso-8859-1?q?this=20is=20some=20text?= ")]
        [TestCase("If you can read this yo", "=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?=")]
        [TestCase("If you can read this you understand the example.", "=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?= \r\n\t=?ISO-8859-2?B?dSB1bmRlcnN0YW5kIHRoZSBleGFtcGxlLg==?=")]
        [TestCase("a b", "=?iso-8859-1?q?a_b?=")]
        [TestCase("This is a long1 long2 long3 long4 long5 long6 long7 long8 long9 long10 subject", "This is a long1 long2 long3 long4 long5 long6 long7 long8 long9\r\n long10 subject")]
        [TestCase("これは長い長い長い長い長い件名", " =?UTF-8?B?44GT44KM44Gv6ZW344GE6ZW344GE6ZW344GE6ZW344GE6ZW344GE5Lu2?=\r\n =?UTF-8?B?5ZCN?=")]
        [TestCase("これは長い long long 長い件名です", " =?UTF-8?B?44GT44KM44Gv6ZW344GEIGxvbmcgbG9uZyDplbfjgYTku7blkI3jgac=?=\r\n =?UTF-8?B?44GZ?=")]
        [TestCase("これは 1234567890 件名", " =?ISO-2022-JP?B?GyRCJDMkbCRPGyhC?= 1234567890\r\n =?ISO-2022-JP?B?GyRCN29MPhsoQg==?=")]
        public void Decodes(string expected, string input)
        {
            ClassicAssert.AreEqual(expected, UtilDecodeRfc2047.Decode(input));
        }
    }
}
