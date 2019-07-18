using kenjiuno.LEML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEMLTest
{
    [TestFixture]
    public class TestFieldBodyParser
    {
        [Test]
        public void Parser()
        {
            CollectionAssert.AreEqual(
                expected: new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("", "text/plain"),
                },
                actual: FieldBodyParser.Parse("text/plain")
            );

            CollectionAssert.AreEqual(
                expected: new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("", "text/plain"),
                    new KeyValuePair<string, string>("charset", "utf-8"),
                },
                actual: FieldBodyParser.Parse("text/plain; charset=utf-8")
            );

            CollectionAssert.AreEqual(
                expected: new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("", "text/plain"),
                    new KeyValuePair<string, string>("charset", "utf-8"),
                    new KeyValuePair<string, string>("", ""),
                },
                actual: FieldBodyParser.Parse("text/plain; charset=utf-8; ")
            );

            CollectionAssert.AreEqual(
                expected: new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("", "text/plain"),
                    new KeyValuePair<string, string>("charset", "utf-8"),
                },
                actual: FieldBodyParser.Parse("text/plain; charset=\"utf-8\"")
            );
        }
    }
}
