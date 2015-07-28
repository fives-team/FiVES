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
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class EntityCollectionTest
    {
        public interface IMockHandlers
        {
            void Added(object sender, EntityEventArgs e);
            void Removed(object sender, EntityEventArgs e);
        }

        EntityCollection collection;
        Mock<IMockHandlers> mockHandlers;

        [SetUp()]
        public void Init()
        {
            collection = new EntityCollection();
            mockHandlers = new Mock<IMockHandlers>();
        }

        [Test()]
        public void ShouldCorrectlyAddRemoveAndCountEntities()
        {
            Assert.AreEqual(0, collection.Count);

            var entity = new Entity();
            collection.Add(entity);
            collection.Add(new Entity());

            Assert.AreEqual(2, collection.Count);

            collection.Remove(entity);

            Assert.AreEqual(1, collection.Count);

            collection.Add(new Entity());
            collection.Clear();

            Assert.AreEqual(0, collection.Count);
        }

        [Test()]
        public void ShouldCorrectVerifyIfCollectionContainsAnEntity()
        {
            var entity1 = new Entity();
            var entity2 = new Entity();

            collection.Add(entity1);

            Assert.IsTrue(collection.Contains(entity1));
            Assert.IsFalse(collection.Contains(entity2));
        }

        [Test()]
        public void ShouldFindEntityByGuid()
        {
            var entity = new Entity();

            collection.Add(entity);
            collection.Add(new Entity());
            collection.Add(new Entity());

            Assert.AreEqual(entity, collection.FindEntity(entity.Guid));
        }

        [Test()]
        [ExpectedException(typeof(EntityNotFoundException))]
        public void ShouldFailToFindNonExistingEntities()
        {
            var entity1 = new Entity();
            var entity2 = new Entity();

            collection.Add(entity1);
            collection.FindEntity(entity2.Guid);
        }

        [Test()]
        public void ShouldFireAddedEvents()
        {
            collection.AddedEntity += mockHandlers.Object.Added;
            collection.Add(new Entity());
            mockHandlers.Verify(h => h.Added(It.IsAny<object>(), It.IsAny<EntityEventArgs>()), Times.Once);
        }

        [Test()]
        public void ShouldNotBeReadOnly()
        {
            Assert.AreEqual(false, collection.IsReadOnly);
        }

        [Test()]
        public void ShouldCopyCollectionToArray()
        {
            var entity = new Entity();
            collection.Add(entity);

            Entity[] array = new Entity[1];
            collection.CopyTo(array, 0);
            Assert.AreEqual(entity, array[0]);
        }

        [Test()]
        public void ShouldReturnCorrectEnumerator()
        {
            var entity = new Entity();
            collection.Add(entity);

            IEnumerator enumerator = collection.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(entity, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(entity, enumerator.Current);
        }

        [Test()]
        public void ShouldFireRemovedEvents()
        {
            var entity = new Entity();

            collection.Add(entity);
            collection.Add(new Entity());
            collection.Add(new Entity());

            collection.RemovedEntity += mockHandlers.Object.Removed;

            collection.Remove(entity);
            collection.Clear();

            mockHandlers.Verify(h => h.Removed(It.IsAny<object>(), It.IsAny<EntityEventArgs>()), Times.Exactly(3));
        }
    }
}
