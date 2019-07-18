using kenjiuno.LEML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEMLTest
{
    [TestFixture]
    public class TestUtilDecodeRfc2047
    {
        [Test]
        public void Decodes()
        {
            Assert.AreEqual("abc", UtilDecodeRfc2047.Decode("abc"));

            // https://www.akanko.net/marimo/data/rfc/rfc2047-jp.txt
            Assert.AreEqual("this is some text", UtilDecodeRfc2047.Decode("=?iso-8859-1?q?this=20is=20some=20text?="));

            Assert.AreEqual(" this is some text ", UtilDecodeRfc2047.Decode(" =?iso-8859-1?q?this=20is=20some=20text?= "));

            Assert.AreEqual("If you can read this yo", UtilDecodeRfc2047.Decode("=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?="));
            Assert.AreEqual("If you can read this you understand the example.", UtilDecodeRfc2047.Decode("=?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?= \r\n\t=?ISO-8859-2?B?dSB1bmRlcnN0YW5kIHRoZSBleGFtcGxlLg==?="));
        }
    }
}
