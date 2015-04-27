define(['kiara'], function(KIARA) {
    function JSONWebSocket(config) {
        var self = this;

        // Construct URL
        var url = "ws://";
        url += config.host ? config.host : "localhost";
        url += config.port ? ":" + config.port : "";
        if (config.path)
            url += config.path.substring(0, 1) == "/" ? config.path : "/" + config.path;
        else
            url += "/";

        KIARA.Protocol.call(self, 'fives-json');
        self.__url = url;

        self.__maxReconnectAttempts = 5;

        self.__funcs = {};
        self.__oneway = {};
        self.__activeCalls = {};
        self.__cachedCalls = [];
        self.__nextCallID = 0;
        self.__reconnectAttempts = 0;
        self.__callbacks = {};

        self.__connect();
    }

    KIARA.inherits(JSONWebSocket, KIARA.Protocol);

    JSONWebSocket.prototype.callMethod = function (callResponse, args) {
        var self = this;

        if (self.__ws.readyState == WebSocket.OPEN) {
            var callID = self.__nextCallID++;
            var argsWithCallbacks = self.__extractCallbacks(args);
            var request = [ "call", callID, callResponse.getMethodName() ].concat(argsWithCallbacks);
            self.__ws.send(JSON.stringify(request));
            if (!callResponse.isOneWay())
                self.__activeCalls[callID] = callResponse;
        } else {
            self.__cachedCalls.push([callResponse, args]);
        }
    }

    JSONWebSocket.prototype.registerFunc = function (methodDescriptor, nativeMethod) {
        var self = this;

        self.__funcs[methodDescriptor.methodName] = nativeMethod;
        self.__oneway[methodDescriptor.methodName] = methodDescriptor.isOneWay;
    }

    JSONWebSocket.prototype.__isFunction = function (functionToCheck) {
        var getType = {};
        return functionToCheck && getType.toString.call(functionToCheck) === '[object Function]';
    }

    JSONWebSocket.prototype.__guid = function() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);
            return v.toString(16);
        });
    }

    JSONWebSocket.prototype.__extractCallbacks = function(args) {
        var self = this;

        var newArgs = [];
        var callbacks = [];
        for (var i in args) {
            var arg = args[i];
            if (self.__isFunction(arg)) {
                if (!self.__callbacks.hasOwnProperty(arg)) {
                    var guid = self.__guid();
                    self.__callbacks[arg] = guid;
                    self.__funcs[guid] = arg;
                }

                callbacks.push(i);
                newArgs.push(self.__callbacks[arg]);
            } else {
                newArgs.push(arg);
            }
        }

        return [callbacks].concat(newArgs);
    }

    JSONWebSocket.prototype.__connect = function() {
        var self = this;

        self.__ws = new WebSocket(self.__url);
        self.__ws.onopen = self.__handleConnect.bind(self);
        self.__ws.onerror = self.__ws.onclose = self.__handleDisconnect.bind(self);
        self.__ws.onmessage = self.__handleMessage.bind(self);
    }

    JSONWebSocket.prototype.__handleCall = function(data) {
        var self = this;

        var callID = data[1];
        var methodName = data[2];
        if (methodName in self.__funcs) {
            var callbacks = data[3];
            var args = data.slice(4);
            for (var cbIndex in callbacks) {
                var remoteFuncName = args[cbIndex];
                args[cbIndex] = function() {
                    // This is a hack. Protocol.callMethod should accept a connection object, which should be passed
                    // down to the constructor of the protocol, but it isn't - so we just pass a null. Additionally,
                    // method descriptor should be created with correct type mapping string and one-way flag, which
                    // we can't know, because we don't know whether user cares about success status or not. We
                    // assume they do, which is why we pass `false` for one-way flag. Finally, since we can't tell
                    // the type of the callback argument, we just return CallResponse object and let users set up
                    // handlers as they like.
                    var methodDescriptor = self.createMethodDescriptor(remoteFuncName, "", false);
                    var callResponse = new KIARA.CallResponse(null, methodDescriptor);
                    self.callMethod(callResponse, arguments);
                    return callResponse;
                }
            }
            var response = [ 'call-reply', callID ];
            try {
                retVal = self.__funcs[methodName].apply(null, args);
                response.push(true);
                response.push(retVal);
            } catch (exception) {
                response.push(false);
                response.push(exception);
            }
            if (!self.__oneway[methodName])
                self.__ws.send(JSON.stringify(response));
        } else {
            self.__sendCallError(callID, "Method " + methodName + " is not registered");
        }
    }

    JSONWebSocket.prototype.__handleCallReply = function(data) {
        var self = this;

        var callID = data[1];
        if (callID in self.__activeCalls) {
            var callResponse = self.__activeCalls[callID];
            var success = data[2];
            var retValOrException = data[3];
            callResponse.setResult(retValOrException, success ? 'result' : 'exception');
            delete self.__activeCalls[callID];
        } else {
            self.__sendCallError(-1, "Invalid callID: " + callID);
        }
    }

    JSONWebSocket.prototype.__handleCallError = function(data) {
        var self = this;

        var callID = data[1];
        var reason = data[2];

        // Call error with callID=-1 means something we've sent something that was not understood by other side or
        // was malformed. This probably means that protocols aren't incompatible or incorrectly implemented on
        // either side.
        if (callID == -1)
            throw new KIARA.Error(KIARA.GENERIC_ERROR, reason);

        if (callID in self.__activeCalls) {
            self.__activeCalls[callID].setResult(reason, "error");
            // FIXME: Remove from production code. Users should use "error" handler instead.
            console.error("received call error: ", self.__activeCalls[callID], "reason:", reason);
        } else {
            self.__sendCallError(-1, "Invalid callID: " + callID);
        }
    }

    JSONWebSocket.prototype.__sendCallError = function(callID, reason) {
        var self = this;

        var message = ["call-error", callID, reason];
        self.__ws.send(JSON.stringify(message));
    }

    JSONWebSocket.prototype.__handleMessage = function (message) {
        var self = this;

        var data = JSON.parse(message.data);
        var msgType = data[0];
        if (msgType == 'call-reply') {
            self.__handleCallReply(data);
        } else if (msgType == 'call-error') {
            self.__handleCallError(data);
        } else if (msgType == 'call') {
            self.__handleCall(data);
        } else {
            self.__sendCallError(-1, "Unknown message type: " + msgType);
        }
    }

    JSONWebSocket.prototype.__handleConnect = function () {
        var self = this;

        for (var callIndex in self.__cachedCalls) {
            var call = self.__cachedCalls[callIndex];
            self.callMethod(call[0], call[1])
        }
        self.__cachedCalls = [];
    }

    JSONWebSocket.prototype.__handleDisconnect = function (event) {
        var self = this;

        self.__reconnectAttempts++;
        if (self.__reconnectAttempts <= self.__maxReconnectAttempts) {
            self.__connect();
        } else {
            for (var callID in self.__activeCalls) {
                var callResponse = self.__activeCalls[callID];
                callResponse.setResult(event, 'error');
            }
            self.__activeCalls = {};

            for (var callIndex in self.__cachedCalls) {
                var cachedCall = self.__cachedCalls[callIndex];
                cachedCall[0].setResult(event, 'error');
            }
            self.__cachedCalls = [];
        }
    }

    KIARA.registerProtocol('fives-json', JSONWebSocket);
});