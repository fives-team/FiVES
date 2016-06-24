using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES {
	public class ExternalProcess {

		private String _parameters;
		public String Parameters {
			get {
				return _parameters;
			}
			set {
				if (!isRunning) {
					_parameters = value;
				}
			}
		}

		private String _workingDirectory;
		public String WorkingDirectory {
			get {
				return _workingDirectory;
			}
			set {
				if (!isRunning) {
					_workingDirectory = value;
				}
			}
		}

		private String _executableName;
		public String ExecutableName {
			get {
				return _executableName;
			}
			set {
				if (!isRunning) {
					_executableName = value;
				}
			}
		}

		private Process process;

		private ProcessStartInfo startInfo;

		private bool isRunning;

		public ExternalProcess() {
			isRunning = false;
		}

		public void Start() {
			startInfo = new ProcessStartInfo(ExecutableName, Parameters);
			startInfo.WorkingDirectory = WorkingDirectory;
			startInfo.UseShellExecute = false;
			isRunning = true;
			process = System.Diagnostics.Process.Start(startInfo);
		}

		public void Kill() {
			if (!process.HasExited) {
				process.Kill();
			}
			isRunning = false;
		}
	}
}
