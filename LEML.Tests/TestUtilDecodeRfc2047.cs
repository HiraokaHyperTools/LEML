using kenjiuno.LEML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEML.Tests
{
    public class TestUtilDecodeRfc2047
    {
        [Test]
        [TestCase("abc", "abc")]
        // https://www.akanko.net/marimo/data/rfc/rfc2047-jp.txt
        [TestCase("this is some text", "=?iso-8859-1?q?this=20is=20some=20text?=")]
        [TestCase(" this is some text ", " =?iso-8859-1?q?this=20is=20some=20text?= ")]
        [TestCase("If you can read this yo", "=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?=")]
        [TestCase("If you can read this you understand the example.", "=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?= \r\n\t=?ISO-8859-2?B?dSB1bmRlcnN0YW5kIHRoZSBleGFtcGxlLg==?=")]
        [TestCase("a b", "=?iso-8859-1?q?a_b?=")]
        public void Decodes(string expected, string input)
        {
            ClassicAssert.AreEqual(expected, UtilDecodeRfc2047.Decode(input));
        }
    }
}
