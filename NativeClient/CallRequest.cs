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
	class CallRequest
	{
        public string Message;
        public int CallID;
        public string FuncName;
        public List<int> Callbacks;
        public List<JToken> Args;
	}
}

