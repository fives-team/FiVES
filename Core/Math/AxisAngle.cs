using System;

namespace FIVES
{
    public struct AxisAngle
    {
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

        public Vector axis { get; private set; }
        public float angle { get; private set; }
    }
}

