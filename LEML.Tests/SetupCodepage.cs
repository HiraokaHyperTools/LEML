using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEML.Tests
{
    [SetUpFixture]
    public class SetupCodepage
    {
        [OneTimeSetUp]
        public void Setup()
        {
#if NET6_0
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }
    }
}
