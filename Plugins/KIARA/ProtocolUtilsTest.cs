// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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

