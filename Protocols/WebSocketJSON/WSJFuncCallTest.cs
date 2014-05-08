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
using NUnit.Framework;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJFuncCallTest : WSJFuncCall
    {
        [Test()]
        public void ShouldConvertResults()
        {
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("42"), typeof(int)), 42);
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("'abc'"), typeof(string)), "abc");
            Assert.AreEqual(base.ConvertResult(JsonConvert.DeserializeObject<JToken>("3.14"), typeof(float)), 3.14f);
        }
    }
}

