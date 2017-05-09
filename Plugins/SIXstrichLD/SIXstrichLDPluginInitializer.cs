using FIVES;
using SIX;
using SIXCore.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIXstrichLDPlugin
{
    public class SIXstrichLDPluginInitializer : IPluginInitializer
    {
        public List<string> ComponentDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public string Name
        {
            get
            {
                return "SIXstrichLD";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<string>();
            }
        }

        public void Initialize()
        {
            debug();

            var entity = new Entity();
            entity["location"]["position"].Suggest(new Vector(0, 0, 0));
            World.Instance.Add(entity);
            server.createEntityCollectionDatapoint(worldUri, World.Instance);
        }

        private void debug()
        {
            var eventP = typeof(ChangedAttributeEventArgs).GetProperties();
            var entityP = typeof(Entity).GetProperties();
            var componentP = typeof(Component).GetProperties();
            var attributeP = typeof(FIVES.Attribute).GetProperties();
            var guidP = typeof(Guid).GetProperties();
        }

        public void Shutdown()
        {
            Console.WriteLine("[SIXstrichLD] shutdown");
        }

        private static Uri baseUri = new Uri("http://172.16.7.224:12345/");
        private static Uri worldUri = new Uri(baseUri.OriginalString + "world");
        private static Server server = new Server();
    }
}
