using System;
using NUnit.Framework;
using Newtonsoft.Json;

namespace KIARAPlugin
{
    [TestFixture()]
    public class ProtocolUtilsTest
    {
        private Server serverConfig = JsonConvert.DeserializeObject<Server>(
            "{ services: '*', protocol: { name: 'test', port: 1234 } }");

        [Test()]
        public void ShouldRetrieveProtocolSetting()
        {
            Assert.AreEqual(ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "name", ""), "test");
            Assert.AreEqual(ProtocolUtils.retrieveProtocolSetting<int>(serverConfig, "port", 4321), 1234);
        }

        [Test()]
        public void ShouldUseDefaultValueWhenPropertyIsNotAvailable()
        {
            Assert.AreEqual(
                ProtocolUtils.retrieveProtocolSetting<string>(serverConfig, "host", "localhost"), "localhost");
        }
    }
}

