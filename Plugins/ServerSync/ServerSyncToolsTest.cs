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
using KIARAPlugin;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class ServerSyncToolsTest
    {
        [Test]
        public void ShouldSetTypeNameHandlingToAutoWhenConfiguringJsonSerializer()
        {
            Mock<Connection> conn = new Mock<Connection>();
            object settings = new JsonSerializerSettings();
            conn.Setup(c => c.GetProperty("JsonSerializerSettings", out settings)).Returns(true);

            ServerSyncTools.ConfigureJsonSerializer(conn.Object);

            conn.Verify(c => c.SetProperty("JsonSerializerSettings",
                It.Is<JsonSerializerSettings>(jss => jss.TypeNameHandling == TypeNameHandling.Auto)));
        }
    }
}
