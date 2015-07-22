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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
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
            return new Vector(
                vector1.y * vector2.z - vector1.z * vector2.y,
                vector1.z * vector2.x - vector1.x * vector2.z,
                vector1.x * vector2.y - vector1.y * vector2.x
            );
        }

        public static Vector ScaleVector(Vector vector, float scalar)
        {
            return new Vector(
                vector.x * scalar,
                vector.y * scalar,
                vector.z * scalar
            );
        }

        public static Vector AddVectors(Vector vector1, Vector vector2)
        {
            return new Vector(
                vector1.x + vector2.x,
                vector1.y + vector2.y,
                vector1.z + vector2.z
            );
        }

        public static Vector AxisFromQuaternion(Quat q)
        {
            if (q.w * q.w != 1)
            {
                return new Vector(
                    q.x / ((float)System.Math.Sqrt(1 - q.w * q.w)),
                    q.y / ((float)System.Math.Sqrt(1 - q.w * q.w)),
                    q.z / ((float)System.Math.Sqrt(1 - q.w * q.w))
                );
            }
            else
            {
                return new Vector(0, 0, 0);
            }
        }

        public static float AngleFromQuaternion(Quat q)
        {
            return 2.0f * (float)System.Math.Acos(q.w);
        }

        public static Quat QuaternionFromAxisAngle(Vector axis, float r)
        {
            return new Quat(
                axis.x * (float)System.Math.Sin(0.5 * r),
                axis.y * (float)System.Math.Sin(0.5 * r),
                axis.z * (float)System.Math.Sin(0.5 * r),
                (float)System.Math.Cos(0.5 * r)
            );
        }

        public static Quat MultiplyQuaternions(Quat p, Quat q)
        {
            return new Quat(
                p.w * q.x + p.x * q.w + p.y * q.z - p.z * q.y,
                p.w * q.y - p.x * q.z + p.y * q.w + p.z * q.x,
                p.w * q.z + p.x * q.y - p.y * q.x + p.z * q.w,
                p.w * q.w - p.x * q.x - p.y * q.y - p.z * q.z
            );
        }
    }
}
