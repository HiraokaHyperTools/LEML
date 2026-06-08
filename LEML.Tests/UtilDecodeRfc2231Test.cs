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
    public class UtilDecodeRfc2231Test
    {
        [Test]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("新しいリッチ テキスト ドキュメント.rtf", "UTF-8''%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66")]
        [TestCase("新しいリッチ テキスト ドキュメント.rtf", "UTF-8'ja'%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66")]
        [TestCase("新しいリッチ テキスト ドキュメント.rtf", "UTF-8'ja-jp'%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66")]
        [TestCase("This is ***fun***", "us-ascii''This%20is%20%2A%2A%2Afun%2A%2A%2A")]
        [TestCase("This is ***fun***", "us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A")]
        public void Decodes(string expected, string input)
        {
            ClassicAssert.AreEqual(expected, UtilDecodeRfc2231.Decode(input, Encoding.UTF8));
        }
    }
}
