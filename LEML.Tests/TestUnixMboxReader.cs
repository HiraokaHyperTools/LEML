using kenjiuno.LEML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.IO;
using System.Linq;

namespace LEML.Tests
{
    public class TestUnixMboxReader
    {
        [Test]
        public void Splitter()
        {
            {
                var body = "\r\n"
                    + "\r\n"
                    + "\r\n"
                    + "From MAILER-DAEMON Fri Jul  8 12:08:34 2011" + "\r\n"
                    + "From: test@example.com" + "\r\n"
                    + "From MAILER-DAEMON Fri Jul  8 12:08:34 2011" + "\r\n"
                    + "From: test@example.net" + "\r\n"
                    + "From MAILER-DAEMON Fri Jul  8 12:08:34 2011" + "\r\n"
                    + "From: test@example.org" + "\r\n"
                    ;
                var mails = UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").ToArray();

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


        [Test]
        public void UnescapeOfFrom()
        {
            {
                var body = "\r\n"
                    + "\r\n"
                    + "\r\n"
                    + "From MAILER-DAEMON Fri Jul  8 12:08:34 2011" + "\r\n"
                    + "From: test@example.com" + "\r\n"
                    + "" + "\r\n"
                    + ">From me" + "\r\n"
                    + ">>From you" + "\r\n"
                    + ">>>From them" + "\r\n"
                    ;
                var mails = UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").ToArray();

                CollectionAssert.AreEqual(
                    expected: new string[]
                    {
                        "From: test@example.com\n"
                        + "\n"
                        + "From me\n"
                        + ">From you\n"
                        + ">>From them\n"
                        ,
                    },
                    actual: mails.Select(mail => mail.rawBody)
                );
            }
        }
    }
}
