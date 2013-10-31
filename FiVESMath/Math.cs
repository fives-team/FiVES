using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiVESMath
{
    public static class Math
    {
        /// <summary>
        /// Rotates a vector v around an axis n by an angle a by the formula
        /// result = x*cos a + n(n.v)(1-cos a) + (v x n)sin a 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector RotateVectorByAxisAngle(Vector vector, Vector axis, float angle)
        {
            Vector result = new Vector();

            float cosAngle =  (float)System.Math.Cos(angle);
            float sinAngle = (float)System.Math.Sin(angle);

            Vector scaledVector = ScaleVector(vector, cosAngle); // v*cos a 

            float axisFactor = ScalarProduct(vector, axis)*(1 - cosAngle); // (n.v)(1 - cos a)
            Vector scaledAxis = ScaleVector(axis, axisFactor); // n(n.v)(1-cos a)

            Vector scaledCross = ScaleVector(CrossProduct(vector, axis), sinAngle); // (v x n)sin a

            result = AddVectors(AddVectors(scaledAxis , scaledVector), scaledCross);

            return result;
        }

        public static float VectorLength(Vector vector)
        {
            return (float)System.Math.Sqrt(ScalarProduct(vector, vector));
        }

        public static float ScalarProduct(Vector vector1, Vector vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
        }

        public static Vector CrossProduct(Vector vector1, Vector vector2)
        {
            Vector result = new Vector();
            result.x = vector1.y*vector2.z - vector1.z *vector2.y;
            result.y = vector1.z*vector2.x - vector1.x * vector2.z;
            result.z = vector1.x*vector2.y - vector1.y * vector2.x;
            return result;
        }

        public static Vector ScaleVector(Vector vector, float scalar)
        {
            Vector result = new Vector();
            result.x = vector.x * scalar;
            result.y = vector.y * scalar;
            result.z = vector.z * scalar;
            return result;
        }

        public static Vector AddVectors(Vector vector1, Vector vector2)
        {
            Vector result = new Vector();
            result.x = vector1.x + vector2.x;
            result.y = vector1.y + vector2.y;
            result.z = vector1.z + vector2.z;
            return result;
        }

        public static Vector AxisFromQuaternion(Quat q)
        {
            Vector axis = new Vector();
            if (q.w * q.w != 1)
            {
                axis.x = q.x / ((float)System.Math.Sqrt(1 - q.w * q.w));
                axis.y = q.y / ((float)System.Math.Sqrt(1 - q.w * q.w));
                axis.z = q.z / ((float)System.Math.Sqrt(1 - q.w * q.w));
            }
            else
            {
                axis.x = axis.y = axis.z = 0;
            }
            return axis;
        }

        public static float AngleFromQuaternion(Quat q)
        {
            return 2.0f * (float)System.Math.Acos(q.w);
        }
    }
}
