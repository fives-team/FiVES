using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    public interface IDiagnosablePlugin : IPluginInitializer
    {
        bool RegisterWidget();
        IPluginWidget Widget { get; }
    }
}
