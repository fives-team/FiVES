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
