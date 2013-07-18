using System;
using System.Configuration;
using KIARA;
using System.Collections.Generic;
using FIVES;

public class PluginInitializer
{
    // service kiara {
    //   bool[] implements(string[] services);
    // }
    //
    // service clientsync {
    //   string[] listObjects();
    //   TODO: getObjectPosition
    // }

    private static readonly List<string> supportedServices = new List<string> {
        "kiara",
        "clientsync"
    };

    internal static List<bool> Implements(List<string> services)
    {
        return services.ConvertAll(service => supportedServices.Contains(service));
    }

    internal static List<string> ListObjects()
    {
        List<Guid> guids = EntityRegistry.GetAllGUIDs();
        return guids.ConvertAll(guid => guid.ToString());
    }

    internal static void RegisterMethods(Connection connection)
    {
        connection.RegisterFuncImplementation("kiara.implements", "...", (Func<List<string>, List<bool>>)Implements);
        connection.RegisterFuncImplementation("clientsync.listObjects", "...", (Func<List<string>>)ListObjects);
    }

    public PluginInitializer()
    {
        var context = new Context();
        string service = "http://localhost/projects/test-client/kiara/fives.json";
        context.AcceptClients(service, RegisterMethods);
    }
}

