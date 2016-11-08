using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;
using RESTServicePlugin;

namespace DiagnosisPlugin
{
    public class DiagnosisPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get
            {
                return "Diagnosis";
            }
        }

        public List<string> PluginDependencies
        {
            get
            {
                return new List<String>() { "RESTService" };
            }
        }

        public List<string> ComponentDependencies
        {
            get
            {
                return new List<String>();
            }
        }

        public void Initialize()
        {
            Console.WriteLine("[Diagnosis] Diagnosis Website available at :8081/diagnosis/");
            RequestDispatcher.Instance.RegisterHandler(new DiagnosisRequestHandler());
        }

        public void Shutdown()
        {
            Console.WriteLine("[Diagnosis] shutting down");
        }

        private void helloWorld()
        {
            Console.WriteLine("[DIAGNOSIS RPC] HELLO WORLD!");
        }
    }
}
