using kenjiuno.LEML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Linq;

namespace LEML.Tests
{
    public class EMLTest
    {
        [Test]
        public void AsciiFrom()
        {
            var body = "From: test@example.com";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            ClassicAssert.AreEqual(mail.From, "test@example.com");
        }

        [Test]
        public void AsciiMessageBody()
        {
            var body = "From: test@example.com" + "\r\n"
                + "" + "\r\n"
                + "This is message body." + "\r\n"
                ;
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            ClassicAssert.AreEqual("This is message body.\n", mail.MessageBody);
        }

        private void DisplayContentTypeAndDisposition(EML mail)
        {
            Console.WriteLine("--- Content-Type");
            mail.dictHeaders.TryGetValue("Content-Type", out string contentType);
            Console.WriteLine(contentType);

            Console.WriteLine("--- Content-Disposition");
            mail.dictHeaders.TryGetValue("Content-Disposition", out string contentDisposition);
            Console.WriteLine(contentDisposition);
        }

        [Test]
        public void TestTB1()
        {
            // 新しいリッチ テキスト ドキュメント.rtf
            var body = "Content-Type: application/msword;\r\n name=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6U=?=\r\n =?UTF-8?B?44Oh44Oz44OILnJ0Zg==?=\"\r\nContent-Disposition: attachment;\r\n filename*0*=UTF-8''%E6%96%B0%E3%81%97%E3%81%84%E3%83%AA%E3%83%83%E3%83%81;\r\n filename*1*=%20%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%20%E3%83%89%E3%82%AD;\r\n filename*2*=%E3%83%A5%E3%83%A1%E3%83%B3%E3%83%88%2E%72%74%66\r\nContent-Transfer-Encoding: base64\r\n\r\ne1xydGYxfQ==";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestTB2()
        {
            // File.rtf
            var body = "Content-Type: application/msword; name=\"File.rtf\"\r\nContent-Disposition: attachment; filename=\"File.rtf\"\r\nContent-Transfer-Encoding: base64\r\n\r\ne1xydGYxfQ==\r\n";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestSyl1()
        {
            // 新しいリッチ テキスト ドキュメント.rtf
            var body = "Content-Type: text/rtf;\r\n name=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhC?=\r\n =?ISO-2022-JP?B?IBskQiVJJS0lZSVhJXMlSBsoQi5ydGY=?=\"\r\nContent-Disposition: attachment;\r\n filename=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhC?=\r\n =?ISO-2022-JP?B?IBskQiVJJS0lZSVhJXMlSBsoQi5ydGY=?=\"\r\nContent-Transfer-Encoding: 7bit\r\n\r\n{\\rtf1}";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestSyl2()
        {
            // File.rtf
            var body = "Content-Type: text/rtf;\r\n name=\"File.rtf\"\r\nContent-Disposition: attachment;\r\n filename=\"File.rtf\"\r\nContent-Transfer-Encoding: 7bit\r\n\r\n{\\rtf1}";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestOL1()
        {
            // 新しいリッチ テキスト ドキュメント.rtf
            var body = "Content-Type: application/msword;\r\n\tname=\"=?iso-2022-jp?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIA==?=\r\n\t=?iso-2022-jp?B?GyRCJUklLSVlJWElcyVIGyhCLnJ0Zg==?=\"\r\nContent-Transfer-Encoding: 7bit\r\nContent-Disposition: attachment;\r\n\tfilename=\"=?iso-2022-jp?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIA==?=\r\n\t=?iso-2022-jp?B?GyRCJUklLSVlJWElcyVIGyhCLnJ0Zg==?=\"\r\n\r\n{\\rtf1}";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestOL2()
        {
            // File.rtf
            var body = "Content-Type: application/msword;\r\n\tname=\"File.rtf\"\r\nContent-Transfer-Encoding: 7bit\r\nContent-Disposition: attachment;\r\n\tfilename=\"File.rtf\"\r\n\r\n{\\rtf1}";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestGM1()
        {
            // 新しいリッチ テキスト ドキュメント.rtf
            var body = "Content-Type: application/rtf; \r\n\tname=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6Xjg6Hjg7Pjg4gucnRm?=\"\r\nContent-Disposition: attachment; \r\n\tfilename=\"=?UTF-8?B?5paw44GX44GE44Oq44OD44OBIOODhuOCreOCueODiCDjg4njgq3jg6Xjg6Hjg7Pjg4gucnRm?=\"\r\nContent-Transfer-Encoding: base64\r\nX-Attachment-Id: f_mq53eb2w0\r\nContent-ID: <f_mq53eb2w0>\r\n\r\ne1xydGYxfQ==\r\n";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestGM2()
        {
            // File.rtf
            var body = "Content-Type: application/rtf; name=\"File.rtf\"\r\nContent-Disposition: attachment; filename=\"File.rtf\"\r\nContent-Transfer-Encoding: base64\r\nX-Attachment-Id: f_mq53e0sl0\r\nContent-ID: <f_mq53e0sl0>\r\n\r\ne1xydGYxfQ==";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestEM1()
        {
            // 新しいリッチ テキスト ドキュメント.rtf
            var body = "Content-Type: application/octet-stream; name=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIBskQiVJJS0lZSVhJXMlSBsoQg==?=.rtf\"\r\nContent-Transfer-Encoding: base64\r\nContent-Disposition: attachment; filename=\"=?ISO-2022-JP?B?GyRCPzckNyQkJWolQyVBGyhCIBskQiVGJS0lOSVIGyhCIBskQiVJJS0lZSVhJXMlSBsoQg==?=.rtf\"\r\n\r\ne1xydGYxfQ==";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

        [Test]
        public void TestEM2()
        {
            // File.rtf
            var body = "Content-Type: application/octet-stream; name=\"File.rtf\"\r\nContent-Transfer-Encoding: base64\r\nContent-Disposition: attachment; filename=\"File.rtf\"\r\n\r\ne1xydGYxfQ==";
            var mail = new EML(UnixMboxReader.LoadFrom(new StringReader(body), "test.eml").Single());
            DisplayContentTypeAndDisposition(mail);
        }

    }
}
