using ClientManagerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SIXstrichLDPlugin
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
