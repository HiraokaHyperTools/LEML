using kenjiuno.LEML;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace LEMLTest
{
    [TestFixture]
    public class TestEML
    {
        [Test]
        public void AsciiFrom()
        {
            var body = "From: test@example.com";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            Assert.AreEqual(mail.From, "test@example.com");
        }

        [Test]
        public void AsciiMessageBody()
        {
            var body = "From: test@example.com" + "\r\n"
                + "" + "\r\n"
                + "This is message body." + "\r\n"
                ;
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            Assert.AreEqual("This is message body.\n", mail.MessageBody);
        }
    }
}
