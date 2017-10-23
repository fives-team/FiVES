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
using ClientManagerPlugin;
using System;
using System.Web.Script.Serialization;

namespace SIXPrimeLDPlugin
{
    static class Debug
    {
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        public static string logUpdateInfo(UpdateInfo updateInfo)
        {
            return "received UpdateInfo: " +
                updateInfo.entityGuid +
                " / " + updateInfo.componentName +
                " / " + updateInfo.attributeName +
                " / " + serializer.Serialize(updateInfo.value);
        }

        private static void onValue(string datapointType, UpdateInfo updateInfo)
        {
            Console.WriteLine(datapointType + " " + logUpdateInfo(updateInfo));
        }

        public static void onValueA(UpdateInfo updateInfo)
        {
            onValue("A", updateInfo);
        }

        public static void onValueC(UpdateInfo updateInfo)
        {
            onValue("C", updateInfo);
        }

        public static void onValueE(UpdateInfo updateInfo)
        {
            onValue("E", updateInfo);
        }

        public static void onValueEC(UpdateInfo updateInfo)
        {
            onValue("EC", updateInfo);
        }
    }
}
