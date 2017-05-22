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
            Task.Factory.StartNew(fluctuate);
            debug();
        }

        private void fluctuate()
        {
            var vector = new Vector(0, 0, 0);
            var vector2 = new Vector(1, 1, 1);
            var orientation = new Quat(0, 0, 0, 1);
            var orientation2 = new Quat(1, 1, 1, 1);
            var i = 0;
            while (true)
            {
                i++;
                var suggestedValue = i % 2 == 0 ? vector : vector2;
                var suggestedOrientation = i % 2 == 0 ? orientation : orientation2;
                debugEntity["location"]["position"].Suggest(suggestedValue);
                debugEntity["location"]["orientation"].Suggest(suggestedOrientation);
                Thread.Sleep(5000);
            }
        }

        {
        }

        private void debug()
        {
            SIXClient<UpdateInfo> Aclient = new SIXClient<UpdateInfo>(
                new Uri(worldUri.OriginalString + "/" + debugEntity.Guid + "/location/position"),
                new JsonSerialization()
            );
            SIXClient<UpdateInfo> ECclient = new SIXClient<UpdateInfo>(
                new Uri(worldUri.OriginalString),
                new JsonSerialization()
            );
            SIXClient<UpdateInfo> Eclient = new SIXClient<UpdateInfo>(
                new Uri(worldUri.OriginalString + "/" + debugEntity2.Guid),
                new JsonSerialization()
            );
            SIXClient<UpdateInfo> Cclient = new SIXClient<UpdateInfo>(
                new Uri(worldUri.OriginalString + "/" + debugEntity2.Guid + "/location"),
                new JsonSerialization()
            );
            Aclient.Observe().Subscribe(onValueA);
            ECclient.Observe().Subscribe(onValueEC);
            Eclient.Observe().Subscribe(onValueE);
            Cclient.Observe().Subscribe(onValueC);
        }

        private void onValueA(UpdateInfo updateInfo)
        {
            Console.Write("A received update");
            logUpdateInfo(updateInfo);
        }

        private void onValueC(UpdateInfo updateInfo)
        {
            Console.Write("C received update");
            logUpdateInfo(updateInfo);
        }

        private void onValueE(UpdateInfo updateInfo)
        {
            Console.Write("E received update");
            logUpdateInfo(updateInfo);
        }

        private void onValueEC(UpdateInfo updateInfo)
        {
            Console.Write("EC received update");
            logUpdateInfo(updateInfo);
        }

        private void logUpdateInfo(UpdateInfo updateInfo)
        {
            Console.Write("Received UpdateInfo:");
            Console.Write(" / Entity: " + updateInfo.entityGuid);
            Console.Write(" / Component: " + updateInfo.componentName);
            Console.Write(" / Attribute: " + updateInfo.attributeName);
            Console.WriteLine(" / Value: " + updateInfo.value);
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
