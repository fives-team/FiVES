using System;

namespace NativeClient
{
    class EntityInfo
    {
        public string Guid;
        public Vector Position;
        public Quat Orientation;
        public bool IsLocallyCreated = false;
    }
}

