using System;

namespace NativeClient
{
    public class EntityInfo 
    {
        public string Guid;
        public Vector Position;
        public Quat Orientation;
        public bool MovingBackward = false;
    }
}

