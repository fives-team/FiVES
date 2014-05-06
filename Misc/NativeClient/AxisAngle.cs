// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using WebSocket4Net;
using NLog;
using SuperSocket.ClientEngine;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace NativeClient
{
    class AxisAngle
    {
        public Vector Axis = new Vector();
        public double Angle = 0.0;

        public void FromQuaternion(Quat quat) {
            double s = Math.Sqrt(1 - quat.w * quat.w);
            if (s < 0.001 || double.IsNaN(s)) {
                Axis.x = 0;
                Axis.y = 0;
                Axis.z = 1;
                Angle = 0;
            } else {
                s = 1 / s;
                Axis.x = quat.x * s;
                Axis.y = quat.y * s;
                Axis.z = quat.z * s;
                Angle = 2 * Math.Acos(quat.w);
            }
        }

        public Quat ToQuaternion() {
            var quat = new Quat();
            var l = Math.Sqrt(Axis.x * Axis.x + Axis.y * Axis.y + Axis.z * Axis.z);
            if (l > 0.00001) {
                double s = Math.Sin(Angle / 2) / l;
                quat.x = Axis.x * s;
                quat.y = Axis.y * s;
                quat.z = Axis.z * s;
                quat.w = Math.Cos(Angle / 2);
            } else {
                quat.x = quat.y = quat.z = 0;
                quat.w = 1;
            }
            return quat;
        }
    }
}

