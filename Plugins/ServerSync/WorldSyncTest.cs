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
using FIVES;
using Moq;
using System.Collections.Generic;
using KIARAPlugin;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class WorldSyncTest
    {
        IServerSync originalServerSync = ServerSync.Instance;
        IComponentRegistry originalComponentRegistry = ComponentRegistry.Instance;
        IStringSerialization originalStringSerialization = StringSerialization.Instance;
        World originalWorld = World.Instance;

        Mock<IComponentRegistry> componentRegistryMock;
        Mock<Connection> remoteConnectionMock;
        Mock<IServiceImpl> localServiceMock;
        Mock<ILocalServer> localServerMock;
        Mock<IDomainOfInterest> doiMock;
        Mock<IDomainOfResponsibility> dorMock;
        Mock<IStringSerialization> serializationMock;
        Mock<IServerSync> serverSyncMock;

        // We use real RemoteServerImpl here instead of a Mock<IRemoteServer> as code in the DomainModel casts it to
        // RemoteServerImpl to be able to use internal setter for DoR and DoI. Adding an intermediate
        // IWritableRemoteServer wouldn't work as such an interface would have to be public to be mockable and thus we
        // would expose public access to the setters to DoR and DoI. As RemoveServerImpl's implementation is rather
        // trivial, we chose to use it here directly.
        RemoteServerImpl remoteServer;

        public interface IHandlers
        {
            IFuncCall AddEntity(params object[] args);
            IFuncCall RemoveEntity(params object[] args);
            IFuncCall ChangeAttributes(params object[] args);
        }

        public Mock<IHandlers> handlers;

        [SetUp]
        public void MockGlobalObjects()
        {
            handlers = new Mock<IHandlers>();
            remoteConnectionMock = new Mock<Connection>();
            remoteConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.addEntity", ""))
                .Returns((FuncWrapper)handlers.Object.AddEntity);
            remoteConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.removeEntity", ""))
                .Returns((FuncWrapper)handlers.Object.RemoveEntity);
            remoteConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.changeAttributes", ""))
                .Returns((FuncWrapper)handlers.Object.ChangeAttributes);

            doiMock = new Mock<IDomainOfInterest>();
            dorMock = new Mock<IDomainOfResponsibility>();
            doiMock.Setup(doi => doi.IsInterestedInEntity(It.IsAny<Entity>())).Returns(true);
            doiMock.Setup(doi => doi.IsInterestedInAttributeChange(It.IsAny<Entity>(), It.IsAny<string>(),
                It.IsAny<string>())).Returns(true);
            dorMock.Setup(dor => dor.IsResponsibleFor(It.IsAny<Entity>())).Returns(true);


            localServiceMock = new Mock<IServiceImpl>();
            localServerMock = new Mock<ILocalServer>();
            localServerMock.SetupGet(ls => ls.Service).Returns(localServiceMock.Object);
            localServerMock.SetupGet(ls => ls.DoI).Returns(doiMock.Object);
            localServerMock.SetupGet(ls => ls.DoR).Returns(dorMock.Object);

            remoteServer = new RemoteServerImpl(remoteConnectionMock.Object, doiMock.Object, dorMock.Object,
                Guid.NewGuid());

            serverSyncMock = new Mock<IServerSync>();
            serverSyncMock.Setup(ss => ss.RemoteServers).Returns(new List<IRemoteServer> { remoteServer });
            serverSyncMock.Setup(ss => ss.LocalServer).Returns(localServerMock.Object);

            var componentDefinition = new ComponentDefinition("test");
            componentDefinition.AddAttribute<int>("a", 42);

            componentRegistryMock = new Mock<IComponentRegistry>();
            componentRegistryMock.SetupGet(cr => cr.RegisteredComponents).Returns(
                new ReadOnlyCollection<ReadOnlyComponentDefinition>(new List<ReadOnlyComponentDefinition>()));
            componentRegistryMock.Setup(cr => cr.FindComponentDefinition("test"))
                .Returns(componentDefinition);

            serializationMock = new Mock<IStringSerialization>();

            ServerSync.Instance = serverSyncMock.Object;
            ComponentRegistry.Instance = componentRegistryMock.Object;
            StringSerialization.Instance = serializationMock.Object;
            World.Instance = new World();
        }

        [TearDown]
        public void RestoreGlobalObjects()
        {
            ServerSync.Instance = originalServerSync;
            ComponentRegistry.Instance = originalComponentRegistry;
            StringSerialization.Instance = originalStringSerialization;
            World.Instance = originalWorld;
        }

        [Test]
        public void ShouldRegisterWorldSyncAPI()
        {
            var worldSync = new WorldSync();
            localServiceMock.VerifySet(ls => ls["serverSync.addEntity"] = It.IsAny<Delegate>());
            localServiceMock.VerifySet(ls => ls["serverSync.removeEntity"] = It.IsAny<Delegate>());
            localServiceMock.VerifySet(ls => ls["serverSync.changeAttributes"] = It.IsAny<Delegate>());
        }

        [Test]
        public void ShouldSendUpdatesAccordingToDoI()
        {
            var entity1 = new Entity();
            var entity2 = new Entity();
            doiMock.Setup(doi => doi.IsInterestedInEntity(entity1)).Returns(true);
            doiMock.Setup(doi => doi.IsInterestedInEntity(entity2)).Returns(false);

            var worldSync = new WorldSync();
            worldSync.HandleLocalAddedEntity(this, new EntityEventArgs(entity1));
            worldSync.HandleLocalAddedEntity(this, new EntityEventArgs(entity2));

            handlers.Verify(h => h.AddEntity(entity1.Guid, It.Is<EntitySyncInfo>(
                esi => esi.Components.Count == 0)), Times.Once());
            handlers.Verify(h => h.AddEntity(entity2.Guid, It.Is<EntitySyncInfo>(
                esi => esi.Components.Count == 0)), Times.Never());
        }

        [Test]
        public void ShouldSendEntityAdditions()
        {
            var entity = new Entity();
            entity["test"]["a"].Suggest(33);

            var worldSync = new WorldSync();
            worldSync.HandleLocalAddedEntity(this, new EntityEventArgs(entity));

            handlers.Verify(h => h.AddEntity(entity.Guid, It.Is<EntitySyncInfo>(esi =>
                esi.Components.Count == 1 &&
                esi.Components["test"]["a"].LastValue.Equals(33))), Times.Once());
        }

        [Test]
        public void ShouldSendEntityRemovals()
        {
            var entity = new Entity();
            var worldSync = new WorldSync();
            worldSync.HandleLocalRemovedEntity(this, new EntityEventArgs(entity));

            handlers.Verify(h => h.RemoveEntity(entity.Guid), Times.Once());
        }

        [Test]
        public void ShouldSendAttributeChanges()
        {
            var entity = new Entity();
            entity["test"]["a"].Suggest(33);
            World.Instance.Add(entity);

            var worldSync = new WorldSync();
            worldSync.HandleLocalChangedAttribute(this, new ChangedAttributeEventArgs(entity["test"], "a", 33, 55));

            handlers.Verify(h => h.ChangeAttributes(entity.Guid, It.Is<EntitySyncInfo>(esi =>
                esi.Components.Count == 1 &&
                esi.Components["test"]["a"].LastValue.Equals(55))), Times.Once());
        }

        [Test]
        public void ShouldAddEntityOnUpdate()
        {
            var guid = Guid.NewGuid();
            var worldSync = new WorldSync();
            worldSync.HandleRemoteAddedEntity(remoteConnectionMock.Object, guid, new EntitySyncInfo());

            // Will throw exception if no such entity.
            World.Instance.FindEntity(guid);
        }

        [Test]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void ShouldRemoveEntityOnUpdate()
        {
            var entity = new Entity();
            World.Instance.Add(entity);

            var worldSync = new WorldSync();
            worldSync.HandleRemoteRemovedEntity(remoteConnectionMock.Object, entity.Guid);

            World.Instance.FindEntity(entity.Guid);
        }

        [Test]
        public void ShouldChangeAttributesOnUpdate()
        {
            var entity = new Entity();
            World.Instance.Add(entity);

            var changedAttributes = new EntitySyncInfo();
            changedAttributes["test"]["a"] = new AttributeSyncInfo(Guid.NewGuid(), 99);

            var worldSync = new WorldSync();
            worldSync.HandleRemoteChangedAttributes(remoteConnectionMock.Object, entity.Guid, changedAttributes);

            Assert.AreEqual(entity["test"]["a"].Value, 99);
        }

        [Test]
        public void ShouldCreateInitialSyncInfoForExistingEntities()
        {
            var entity = new Entity();
            World.Instance.Add(entity);
            var worldSync = new WorldSync();
            worldSync.syncInfo.ContainsKey(entity.Guid);
        }

        [Test]
        public void ShouldNotSendUpdatesWhenTheyResultFromRemoteUpdate()
        {
            var changedAttributes = new EntitySyncInfo();
            changedAttributes["test"]["a"] = new AttributeSyncInfo(Guid.NewGuid(), 99);

            var entity = new Entity();
            var worldSync = new WorldSync();
            worldSync.HandleRemoteAddedEntity(remoteConnectionMock.Object, entity.Guid, new EntitySyncInfo());
            worldSync.HandleRemoteChangedAttributes(remoteConnectionMock.Object, entity.Guid, new EntitySyncInfo());
            worldSync.HandleRemoteRemovedEntity(remoteConnectionMock.Object, entity.Guid);

            handlers.Verify(h => h.AddEntity(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Never());
            handlers.Verify(h => h.ChangeAttributes(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Never());
            handlers.Verify(h => h.RemoveEntity(entity.Guid), Times.Never());
        }


        [Test]
        public void ShouldForwardUpdatesToServersOtherThanTheSource()
        {
            var otherConnectionMock = new Mock<Connection>();
            var handlers2 = new Mock<IHandlers>();
            otherConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.addEntity", ""))
                .Returns((FuncWrapper)handlers2.Object.AddEntity);
            otherConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.removeEntity", ""))
                .Returns((FuncWrapper)handlers2.Object.RemoveEntity);
            otherConnectionMock.Setup(rc => rc.GenerateFuncWrapper("serverSync.changeAttributes", ""))
                .Returns((FuncWrapper)handlers2.Object.ChangeAttributes);

            var guid = Guid.NewGuid();
            RemoteServerImpl remoteServer2 = new RemoteServerImpl(otherConnectionMock.Object, doiMock.Object,
                dorMock.Object, guid);
            serverSyncMock.Setup(ss => ss.RemoteServers).Returns(
                new List<IRemoteServer> { remoteServer, remoteServer2 });

            var changedAttributes = new EntitySyncInfo();
            changedAttributes["test"]["a"] = new AttributeSyncInfo(Guid.NewGuid(), 99);

            var entity = new Entity();
            var worldSync = new WorldSync();
            worldSync.HandleRemoteAddedEntity(remoteConnectionMock.Object, entity.Guid, new EntitySyncInfo());
            worldSync.HandleRemoteChangedAttributes(remoteConnectionMock.Object, entity.Guid, changedAttributes);
            worldSync.HandleRemoteRemovedEntity(remoteConnectionMock.Object, entity.Guid);

            handlers.Verify(h => h.AddEntity(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Never());
            handlers.Verify(h => h.ChangeAttributes(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Never());
            handlers.Verify(h => h.RemoveEntity(entity.Guid), Times.Never());

            handlers2.Verify(h => h.AddEntity(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Once());
            handlers2.Verify(h => h.ChangeAttributes(entity.Guid, It.IsAny<EntitySyncInfo>()), Times.Once());
            handlers2.Verify(h => h.RemoveEntity(entity.Guid), Times.Once());
        }
    }
}

