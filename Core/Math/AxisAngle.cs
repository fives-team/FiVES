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

namespace FIVES
{
    public class AxisAngle
    {
        public AxisAngle() { }
        public AxisAngle(float x, float y, float z, float angle)
            : this(new Vector(x, y, z), angle)
        {
        }

        public AxisAngle(Vector axis, float angle)
            : this()
        {
            this.axis = axis;
            this.angle = angle;
        }

        public override string ToString()
        {
            return "axisangle(" + axis.ToString() + ", " + angle + ")";
        }

        public Vector axis { get; private set; }
        public float angle { get; private set; }
    }
}

