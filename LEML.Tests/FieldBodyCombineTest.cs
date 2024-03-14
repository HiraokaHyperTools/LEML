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
        public void FileName(string expectedName, string body)
        {
            Assert.That(
                FieldBodyCombine.Combine(FieldBodyParser.Parse(body))
                    .Single(it => it.Key == "name")
                    .Value,
                Is.EqualTo(expectedName)
            );
        }
    }
}
