using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    public class ExternalProcessBuilder
    {

		private ExternalProcess externalProcess;
		private string parameters;

		public ExternalProcessBuilder() {
			externalProcess = new ExternalProcess();
			parameters = "";
		}

		public ExternalProcessBuilder ExecutableName(String name) {
			externalProcess.ExecutableName = name;
			return this;
		}

		public ExternalProcessBuilder AppendParameter(String parameter) {
			parameters += " " + parameter;
			return this;
		}

		public ExternalProcessBuilder WorkingDirectory(String workingDirectory) {
			externalProcess.WorkingDirectory = workingDirectory;
			return this;
		}

		public ExternalProcess Build() {
			externalProcess.Parameters = parameters;
			return externalProcess;
		}

    }
}
