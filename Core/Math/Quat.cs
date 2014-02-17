using System;

namespace FIVES
{
    public struct Quat
    {
        public Quat(float x, float y, float z, float w)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }
        public float w { get; private set; }
    }
}

