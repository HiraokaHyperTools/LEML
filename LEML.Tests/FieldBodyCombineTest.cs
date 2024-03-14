using kenjiuno.LEML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEML.Tests
{
    public class FieldBodyCombineTest
    {
        [TestCase("JP_ORDERCONFIRMATION_7400005424302_1002503338_3535.OC_2024_03_14.pdf", "application/pdf; \n\tname*0=JP_ORDERCONFIRMATION_7400005424302_1002503338_3535.OC_2024_0; \n\tname*1=3_14.pdf")]
        [TestCase("新規 Microsoft PowerPoint Presentation.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation;\r\n name=\"=?UTF-8?Q?=E6=96=B0=E8=A6=8F_Microsoft_PowerPoint_Presentation=2Eppt?=\r\n =?UTF-8?Q?x?=\"")]
        public void Name(string expectedName, string body)
        {
            Assert.That(
                UtilDecodeRfc2047.Decode(
                    FieldBodyCombine.Combine(FieldBodyParser.Parse(body))
                        .Single(it => it.Key == "name")
                        .Value
                ),
                Is.EqualTo(expectedName)
            );
        }

        [TestCase("新規 Microsoft PowerPoint Presentation.pptx", "attachment;\r\n filename*0*=UTF-8''%E6%96%B0%E8%A6%8F%20%4D%69%63%72%6F%73%6F%66%74%20%50;\r\n filename*1*=%6F%77%65%72%50%6F%69%6E%74%20%50%72%65%73%65%6E%74%61%74%69;\r\n filename*2*=%6F%6E%2E%70%70%74%78")]
        public void FileName(string expectedName, string body)
        {
            Assert.That(
                FieldBodyCombine.Combine(FieldBodyParser.Parse(body))
                    .Single(it => it.Key == "filename")
                    .Value,
                Is.EqualTo(expectedName)
            );
        }
    }
}
