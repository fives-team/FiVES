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
    public class DomainSyncTest
    {
        IServerSync originalServerSync = ServerSync.Instance;
        IComponentRegistry originalComponentRegistry = ComponentRegistry.Instance;
        IStringSerialization originalStringSerialization = StringSerialization.Instance;

        Mock<IComponentRegistry> componentRegistryMock;
        Mock<Connection> remoteConnectionMock;
        Mock<ServiceImplementation> localServiceMock;
        Mock<ILocalServer> localServerMock;
        Mock<IDomainOfInterest> doiMock;
        Mock<IDomainOfResponsibility> dorMock;
        Mock<IStringSerialization> serializationMock;

        // We use real RemoteServerImpl here instead of a Mock<IRemoteServer> as code in the DomainModel casts it to
        // RemoteServerImpl to be able to use internal setter for DoR and DoI. Adding an intermediate
        // IWritableRemoteServer wouldn't work as such an interface would have to be public to be mockable and thus we
        // would expose public access to the setters to DoR and DoI. As RemoveServerImpl's implementation is rather
        // trivial, we chose to use it here directly.
        RemoteServerImpl remoteServer;

        public interface IHandlers
        {
            IClientFunctionCall UpdateDoI(params object[] args);
            IClientFunctionCall UpdateDoR(params object[] args);
        }

        public Mock<IHandlers> handlers;

        [SetUp]
        public void MockGlobalObjects()
        {
            handlers = new Mock<IHandlers>();
            remoteConnectionMock = new Mock<Connection>();
            remoteConnectionMock.Setup(rc => rc.GenerateClientFunction("serverSync", "updateDoI"))
                .Returns((ClientFunction)handlers.Object.UpdateDoI);
            remoteConnectionMock.Setup(rc => rc.GenerateClientFunction("serverSync", "updateDoR"))
                .Returns((ClientFunction)handlers.Object.UpdateDoR);

            doiMock = new Mock<IDomainOfInterest>();
            dorMock = new Mock<IDomainOfResponsibility>();
            localServiceMock = new Mock<ServiceImplementation>();
            localServerMock = new Mock<ILocalServer>();
            localServerMock.SetupGet(ls => ls.Service).Returns(localServiceMock.Object);
            localServerMock.SetupGet(ls => ls.DoI).Returns(doiMock.Object);
            localServerMock.SetupGet(ls => ls.DoR).Returns(dorMock.Object);

            remoteServer = new RemoteServerImpl(remoteConnectionMock.Object, doiMock.Object, dorMock.Object,
                Guid.NewGuid());

            var serverSyncMock = new Mock<IServerSync>();
            serverSyncMock.Setup(ss => ss.RemoteServers).Returns(new List<IRemoteServer> { remoteServer });
            serverSyncMock.Setup(ss => ss.LocalServer).Returns(localServerMock.Object);

            componentRegistryMock = new Mock<IComponentRegistry>();
            componentRegistryMock.SetupGet(cr => cr.RegisteredComponents).Returns(
                new ReadOnlyCollection<ReadOnlyComponentDefinition>(new List<ReadOnlyComponentDefinition>()));

            serializationMock = new Mock<IStringSerialization>();

            ServerSync.Instance = serverSyncMock.Object;
            ComponentRegistry.Instance = componentRegistryMock.Object;
            StringSerialization.Instance = serializationMock.Object;
        }

        [TearDown]
        public void RestoreGlobalObjects()
        {
            ServerSync.Instance = originalServerSync;
            ComponentRegistry.Instance = originalComponentRegistry;
            StringSerialization.Instance = originalStringSerialization;
        }

        [Test]
        public void ShouldRegisterDomainSyncAPI()
        {
            var domainSync = new DomainSync();
            localServiceMock.VerifySet(ls => ls["serverSync.getDoR"] = It.IsAny<Delegate>());
            localServiceMock.VerifySet(ls => ls["serverSync.getDoI"] = It.IsAny<Delegate>());
            localServiceMock.VerifySet(ls => ls["serverSync.updateDoI"] = It.IsAny<Delegate>());
            localServiceMock.VerifySet(ls => ls["serverSync.updateDoR"] = It.IsAny<Delegate>());
        }

        [Test]
        public void ShouldSendDoRUpdates()
        {
            serializationMock.Setup(s => s.SerializeObject<IDomainOfResponsibility>(dorMock.Object))
                .Returns("serializedTestDoR");

            var domainSync = new DomainSync();
            domainSync.HandleLocalDoRChanged(this, new EventArgs());

            handlers.Verify(h => h.UpdateDoR("serializedTestDoR"), Times.Once());
        }

        [Test]
        public void ShouldSendDoIUpdates()
        {
            serializationMock.Setup(s => s.SerializeObject<IDomainOfInterest>(doiMock.Object))
                .Returns("serializedTestDoI");

            var domainSync = new DomainSync();
            domainSync.HandleLocalDoIChanged(this, new EventArgs());

            handlers.Verify(h => h.UpdateDoI("serializedTestDoI"), Times.Once());
        }

        [Test]
        public void ShouldChangeRemoteDoIOnUpdate()
        {
            var remoteDoIMock = new Mock<IDomainOfInterest>();
            serializationMock.Setup(s => s.DeserializeObject<IDomainOfInterest>("serializedTestDoI"))
                .Returns(remoteDoIMock.Object);

            var domainSync = new DomainSync();
            domainSync.HandleRemoteDoIChanged(remoteConnectionMock.Object, "serializedTestDoI");

            Assert.AreSame(remoteServer.DoI, remoteDoIMock.Object);
        }

        [Test]
        public void ShouldChangeRemoteDoROnUpdate()
        {
            var remoteDoRMock = new Mock<IDomainOfResponsibility>();
            serializationMock.Setup(s => s.DeserializeObject<IDomainOfResponsibility>("serializedTestDoR"))
                .Returns(remoteDoRMock.Object);

            var domainSync = new DomainSync();
            domainSync.HandleRemoteDoRChanged(remoteConnectionMock.Object, "serializedTestDoR");

            Assert.AreSame(remoteServer.DoR, remoteDoRMock.Object);
        }
    }
}

