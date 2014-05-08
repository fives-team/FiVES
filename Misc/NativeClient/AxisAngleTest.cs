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

namespace NativeClient
{
    [TestFixture()]
    public class AxisAngleTest
    {
        private const double EPS = 1e-7;

        [Test()]
        public void ShouldConvertToQuaternionCorrectly()
        {
            AxisAngle aa = new AxisAngle { Axis = new Vector { x = 1, y = 2, z = 3 }, Angle = 4 };
            Quat q = aa.ToQuaternion();

            Assert.That(q.x, Is.EqualTo(0.2430199533700943).Within(EPS));
            Assert.That(q.y, Is.EqualTo(0.4860399067401886).Within(EPS));
            Assert.That(q.z, Is.EqualTo(0.7290598750114441).Within(EPS));
            Assert.That(q.w, Is.EqualTo(-0.416146844625473).Within(EPS));
        }

        [Test()]
        public void ShouldHandleInvalidAxisAngleOnConversionToQuaternionCorrectly()
        {
            AxisAngle aa = new AxisAngle { Axis = new Vector { x = 0, y = 0, z = 0 }, Angle = 1 };
            Quat q = aa.ToQuaternion();

            Assert.That(q.x, Is.EqualTo(0).Within(EPS));
            Assert.That(q.y, Is.EqualTo(0).Within(EPS));
            Assert.That(q.z, Is.EqualTo(0).Within(EPS));
            Assert.That(q.w, Is.EqualTo(1).Within(EPS));
        }

        [Test()]
        public void ShouldConvertFromQuaternionCorrectly()
        {
            Quat q = new Quat { x = 0.2430199533700943, y = 0.4860399067401886, z = 0.7290598750114441, 
                                w = -0.416146844625473 };
            AxisAngle aa = new AxisAngle();
            aa.FromQuaternion(q);

            Assert.That(aa.Axis.x, Is.EqualTo(0.26726123690605164).Within(EPS));
            Assert.That(aa.Axis.y, Is.EqualTo(0.5345224738121033).Within(EPS));
            Assert.That(aa.Axis.z, Is.EqualTo(0.8017837405204773).Within(EPS));
            Assert.That(aa.Angle, Is.EqualTo(4.000000017768291).Within(EPS));
        }

        [Test()]
        public void ShouldHandleInvalidQuaternionCorrectly()
        {
            Quat q = new Quat { x = 0, y = 0, z = 0, w = 10 };
            AxisAngle aa = new AxisAngle();
            aa.FromQuaternion(q);

            Assert.That(aa.Axis.x, Is.EqualTo(0).Within(EPS));
            Assert.That(aa.Axis.y, Is.EqualTo(0).Within(EPS));
            Assert.That(aa.Axis.z, Is.EqualTo(1).Within(EPS));
            Assert.That(aa.Angle, Is.EqualTo(0).Within(EPS));
        }
    }
}

