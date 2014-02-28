using KIARAPlugin;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class ToolsTest
    {
        [Test]
        public void ShouldSetTypeNameHandlingToAutoWhenConfiguringJsonSerializer()
        {
            Mock<Connection> conn = new Mock<Connection>();
            object settings = new JsonSerializerSettings();
            conn.Setup(c => c.GetProperty("JsonSerializerSettings", out settings)).Returns(true);

            Tools.ConfigureJsonSerializer(conn.Object);

            conn.Verify(c => c.SetProperty("JsonSerializerSettings",
                It.Is<JsonSerializerSettings>(jss => jss.TypeNameHandling == TypeNameHandling.Auto)));
        }
    }
}
