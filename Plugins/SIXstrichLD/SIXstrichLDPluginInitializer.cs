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

            var baseUri = "http://172.16.7.224:12345/";
            var entity = new Entity();
            entity["location"]["position"].Suggest(new Vector(0, 0, 0));
            World.Instance.Add(entity);
            Server server = new Server();
            var entityUri = new Uri(baseUri + entity.Guid);
            server.CreateServerDatapoint(
                entityUri, new EntityDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), entity)
            );
            Console.WriteLine("created E datapoint: " + entityUri);
            foreach(var component in entity.Components)
            {
                var componentUri = new Uri(entityUri.OriginalString + "/" + component.Name);
                server.CreateServerDatapoint(
                    componentUri, new ComponentDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), entity[component.Name])
                );
                Console.WriteLine("created C datapoint: " + componentUri);

                foreach(var attribute in component.Definition.AttributeDefinitions)
                {
                    var attributeUri = new Uri(componentUri.OriginalString + "/" + attribute.Name);
                    server.CreateServerDatapoint(
                        attributeUri, new AttributeDatapointAdapter<ChangedAttributeEventArgs>(new JsonSerialization(), component[attribute.Name])
                    );
                    Console.WriteLine("created A datapoint: " + attributeUri);
                }
            }
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
    }
}
