using System;
using System.Configuration;

public class PluginInitializer
{
    public PluginInitializer()
    {
        int port = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);

        // TODO: Start listing for client connections via KIARA on port 34837 (FIVES on phone keypad)
        // TODO: For each connection - basic interface discovery mechanism (IDM) and a basic service returning a list of objects
    }
}

