﻿using kenjiuno.LEML;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace LEMLTest
{
    [TestFixture]
    public class TestEdMaxEnterDotEnterReader
    {
        [Test]
        public void Splitter()
        {
            var reader = new EdMaxEnterDotEnterReader();
            {
                var body = ""
                    + "From: test@example.com" + "\r\n"
                    + "." + "\r\n"
                    + "From: test@example.net" + "\r\n"
                    + "." + "\r\n"
                    + "From: test@example.org" + "\r\n"
                    ;
                var mails = reader.LoadFrom(new StringReader(body), "test.eml").ToArray();

                CollectionAssert.AreEqual(
                    expected: new string[]
                    {
                        "From: test@example.com\n",
                        "From: test@example.net\n",
                        "From: test@example.org\n",
                    },
                    actual: mails.Select(mail => mail.rawBody)
                );
            }
        }

    }
}
