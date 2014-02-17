using System;

namespace FIVES
{
    public struct Vector
    {
        public Vector(float x, float y, float z)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }
    }
}

