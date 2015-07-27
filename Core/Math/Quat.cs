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
    public class Quat
    {
        public Quat() { }

        public Quat(float x, float y, float z, float w)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
        {
            return "quat(" + this.x + " " + this.y + " " + this.z + " " + this.w + ")";
        }

        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }
        public float w { get; private set; }
    }
}

