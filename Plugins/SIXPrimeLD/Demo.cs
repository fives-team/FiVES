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
using ClientManagerPlugin;
using FIVES;
using SIXPrime.Server;
using SIXPrime.Core.Serialization;
using SIXPrime.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SIXPrimeLDPlugin
{
    static class Demo
    {
        private static Entity debugEntity;
        private static Entity debugEntity2;

        public static void startDemo(Server server, Uri worldUri)
        {
            startDemoDatapoints(server, worldUri);
            startDemoClients(worldUri);
        }

        private static void startDemoDatapoints(Server server, Uri worldUri)
        {
            debugEntity = new Entity();
            debugEntity["location"]["position"].Suggest(new Vector(0, 0, 0));
            World.Instance.Add(debugEntity);
            server.createEntityCollectionDatapoint(new JsonSerialization(), worldUri, World.Instance);
            Task.Factory.StartNew(fluctuate);

            debugEntity2 = new Entity();
            debugEntity2["location"]["position"].Suggest(new Vector(0, 0, 0));
            World.Instance.Add(debugEntity2);
            Task.Factory.StartNew(fluctuate2);
        }

        private static void startDemoClients(Uri worldUri)
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
            Aclient.Observe().Subscribe(Debug.onValueA);
            ECclient.Observe().Subscribe(Debug.onValueEC);
            Eclient.Observe().Subscribe(Debug.onValueE);
            Cclient.Observe().Subscribe(Debug.onValueC);
        }

        private static void fluctuate()
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

        private static void fluctuate2()
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
                debugEntity2["location"]["position"].Suggest(suggestedValue);
                debugEntity2["location"]["orientation"].Suggest(suggestedOrientation);
                Thread.Sleep(10000);
            }
        }
    }
}
