using System;
using System.Collections.Generic;

namespace FIVES
{
    // Plugins must have one class implementing this interface. This plugin manager will find this class and create its
    // instance. Plugin authors may use this to perform initialization of the plugin-specific datastructures. Later we
    // may add more functions into this interface, so that all plugins can provide us with some extra meta-information,
    // e.g. a name, a list of dependencies etc.
    //
    // IMPORTANT: Plugin authors must make sure that an object implementing this interface can be constructed via a
    // parameterless constructor.
    public interface IPluginInitializer
    {
        // Should return a name of the plugin.
        string getName();

        // Should return a list of dependencies of the plugin.
        List<string> getDependencies();

        // Will be run when all dependencies have been initialized.
        void initialize();

    }
}

