// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using NUnit.Framework;
using FIVES;
using Moq;
using System.Collections.Generic;
using SINFONI;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class ComponentSyncTest
    {
        IServerSync originalServerSync = ServerSync.Instance;
        IComponentRegistry originalComponentRegistry = ComponentRegistry.Instance;

        Mock<IComponentRegistry> componentRegistryMock;
        Mock<Connection> remoteConnectionMock;
        Mock<ServiceImplementation> localServiceMock;

        ComponentDefinition testComponentDefinition;

        public interface IHandlers
        {
            IClientFunctionCall RegisterComponentDefinition(params object[] args);
        }

        public Mock<IHandlers> handlers;

        [SetUp]
        public void MockGlobalObjects()
        {
            handlers = new Mock<IHandlers>();
            remoteConnectionMock = new Mock<Connection>();
            remoteConnectionMock.Setup(rc => rc.GenerateClientFunction("serverSync", "registerComponentDefinition"))
                .Returns((ClientFunction)handlers.Object.RegisterComponentDefinition);

            var remoteServerMock = new Mock<IRemoteServer>();
            remoteServerMock.Setup(rs => rs.Connection).Returns(remoteConnectionMock.Object);

            localServiceMock = new Mock<ServiceImplementation>();
            var localServerMock = new Mock<ILocalServer>();
            localServerMock.SetupGet(ls => ls.Service).Returns(localServiceMock.Object);

            var serverSyncMock = new Mock<IServerSync>();
            serverSyncMock.Setup(ss => ss.RemoteServers).Returns(new List<IRemoteServer> { remoteServerMock.Object });
            serverSyncMock.Setup(ss => ss.LocalServer).Returns(localServerMock.Object);

            componentRegistryMock = new Mock<IComponentRegistry>();
            componentRegistryMock.SetupGet(cr => cr.RegisteredComponents).Returns(
                new ReadOnlyCollection<ReadOnlyComponentDefinition>(new List<ReadOnlyComponentDefinition>()));

            ServerSync.Instance = serverSyncMock.Object;
            ComponentRegistry.Instance = componentRegistryMock.Object;

            testComponentDefinition = new ComponentDefinition("test");
            testComponentDefinition.AddAttribute<float>("f", 3.14f);
            testComponentDefinition.AddAttribute<int>("i", 42);
        }

        [TearDown]
        public void RestoreGlobalObjects()
        {
            ServerSync.Instance = originalServerSync;
            ComponentRegistry.Instance = originalComponentRegistry;
        }

        [Test]
        public void ShouldRegisterComponentSyncAPI()
        {
            var componentSync = new ComponentSync();
            localServiceMock.VerifySet(ls => ls["serverSync.registerComponentDefinition"] = It.IsAny<Delegate>());
        }

        [Test]
        public void ShouldSendComponentRegistrationUpdates()
        {
            var componentSync = new ComponentSync();
            componentSync.HandleLocalRegisteredComponent(this,
                new RegisteredComponentEventArgs(testComponentDefinition));

            handlers.Verify(h => h.RegisterComponentDefinition(It.Is<ComponentDef>(cd =>
                cd.Name == "test" &&
                cd.AttributeDefs.Count == 2 &&
                cd.AttributeDefs[0].Name == "f" &&
                cd.AttributeDefs[0].DefaultValue.Equals(3.14f) &&
                cd.AttributeDefs[0].Type == typeof(float).AssemblyQualifiedName &&
                cd.AttributeDefs[1].Name == "i" &&
                cd.AttributeDefs[1].DefaultValue.Equals(42) &&
                cd.AttributeDefs[1].Type == typeof(int).AssemblyQualifiedName)), Times.Once());
        }

        [Test]
        public void ShouldRegisterComponentsWhenReceiveingAnUpdate()
        {
            var componentSync = new ComponentSync();
            componentSync.HandleRemoteRegisteredComponentDefinition(remoteConnectionMock.Object,
                (ComponentDef)testComponentDefinition);

            componentRegistryMock.Verify(cr => cr.Register(It.Is<ComponentDefinition>(cd =>
                cd.Name == "test" &&
                cd.AttributeDefinitions.Count == 2 &&
                cd.ContainsAttributeDefinition("f") &&
                cd["f"].DefaultValue.Equals(3.14f) &&
                cd["f"].Type == typeof(float) &&
                cd.ContainsAttributeDefinition("i") &&
                cd["i"].DefaultValue.Equals(42) &&
                cd["i"].Type == typeof(int))), Times.Once());
        }
    }
}

