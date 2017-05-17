using ClientManagerPlugin;
using FIVES;
using SIX;
using SIXCore.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            debugEntity = new Entity();
            debugEntity["location"]["position"].Suggest(new Vector(0, 0, 0));
            World.Instance.Add(debugEntity);
            server.createEntityCollectionDatapoint(worldUri, World.Instance);
            World.Instance.AddedEntity += createEntityDatapointForNewlyAddedEntities;
            Task.Factory.StartNew(fluctuate);
            debug();
        }

        private void fluctuate()
        {
            var vector = new Vector(0, 0, 0);
            var vector2 = new Vector(1, 1, 1);
            var i = 0;
            while (true)
            {
                i++;
                var suggestedValue = i % 2 == 0 ? vector : vector2;
                debugEntity["location"]["position"].Suggest(suggestedValue);
                Thread.Sleep(5000);
            }
        }

        private void createEntityDatapointForNewlyAddedEntities(object sender, EntityEventArgs e)
        {
            server.createEntityDatapoint(worldUri, e.Entity);
        }

        private void debug()
        {
            SIXClient<UpdateInfo> client = new SIXClient<UpdateInfo>(
                new Uri(worldUri.OriginalString + "/" + debugEntity.Guid + "/location/position"),
                new JsonSerialization()
            );
            client.Observe().Subscribe(onValue);
        }

        private void onValue(UpdateInfo updateInfo)
        {
            Console.WriteLine("Received UpdateInfo:");
            Console.WriteLine("Entity: " + updateInfo.entityGuid);
            Console.WriteLine("Component: " + updateInfo.componentName);
            Console.WriteLine("Attribute: " + updateInfo.attributeName);
            Console.WriteLine("Value: " + updateInfo.value);
        }

        public void Shutdown()
        {
            Console.WriteLine("[SIXstrichLD] shutdown");
        }

        private static Uri baseUri = new Uri("http://172.16.7.224:12345/");
        private static Uri worldUri = new Uri(baseUri.OriginalString + "world");
        private static Server server = new Server();
        private static Entity debugEntity;
    }
}
