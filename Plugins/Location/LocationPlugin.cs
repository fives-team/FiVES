using System;
using FIVES;
using System.Collections.Generic;

namespace Location
{
    /// <summary>
    /// Plugin that registers two components - position and orientation. Does not provide any associated functionality.
    /// </summary>
    public class LocationPlugin : IPluginInitializer
    {
        #region IPluginInitializer implementation

        public string getName()
        {
            return "Location";
        }

        public List<string> getDependencies()
        {
            return new List<string>();
        }

        public void initialize()
        {
            // Position is represented as a vector (x,y,z) from the default position, which is at (0,0,0).
            ComponentLayout positionLayout = new ComponentLayout();
            positionLayout["x"] = typeof(float);
            positionLayout["y"] = typeof(float);
            positionLayout["z"] = typeof(float);

            // Orientation is represented as a quaternion, where (x,y,z) is a vector part, and w is a scalar part. The 
            // orientation of the object is relative to the default orientation. In the default position and 
            // orientation, the viewer is on the Z-axis looking down the -Z-axis toward the origin with +X to the right 
            // and +Y straight up.
            ComponentLayout orientationLayout = new ComponentLayout();
            orientationLayout["x"] = typeof(float);
            orientationLayout["y"] = typeof(float);
            orientationLayout["z"] = typeof(float);
            orientationLayout["w"] = typeof(float);

            ComponentRegistry.Instance.defineComponent("position", pluginGUID, positionLayout);
            ComponentRegistry.Instance.defineComponent("orientation", pluginGUID, orientationLayout);
        }

        #endregion

        private readonly Guid pluginGUID = new Guid("90dd4c50-f09d-11e2-b778-0800200c9a66");
    }
}

