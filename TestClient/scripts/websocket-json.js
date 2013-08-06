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

        KIARA.Protocol.call(self, 'websocket-json');
        self.__url = url;

        self.__maxReconnectAttempts = 5;

        self.__funcs = {};
        self.__oneway = {};
        self.__activeCalls = {};
        self.__cachedCalls = [];
        self.__nextCallID = 0;
        self.__reconnectAttempts = 0;

        self.__connect();
    }

    KIARA.inherits(JSONWebSocket, KIARA.Protocol);

    JSONWebSocket.prototype.callMethod = function (callResponse, args) {
        var self = this;

        if (self.__ws.readyState == WebSocket.OPEN) {
            var callID = self.__nextCallID++;
            var argsArray = Array.prototype.slice.call(args);
            var request = [ "call", callID, callResponse.getMethodName() ].concat(argsArray);
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

    JSONWebSocket.prototype.__connect = function() {
        var self = this;

        self.__ws = new WebSocket(self.__url);
        self.__ws.onopen = self.__handleConnect.bind(self);
        self.__ws.onerror = self.__ws.onclose = self.__handleDisconnect.bind(self);
        self.__ws.onmessage = self.__handleMessage.bind(self);
    }

    JSONWebSocket.prototype.__handleMessage = function (message) {
        var self = this;

        var data = JSON.parse(message.data);
        var msgType = data[0];
        if (msgType == 'call-reply') {
            var callID = data[1];
            if (callID in self.__activeCalls) {
                var callResponse = self.__activeCalls[callID];
                var success = data[2];
                var retValOrException = data[3];
                callResponse.setResult(retValOrException, success ? 'result' : 'exception');
                delete self.__activeCalls[callID];
            } else {
                throw new KIARA.Error(KIARA.CONNECTION_ERROR,
                    "Received a response for an unrecognized call id: " + callID);
            }
        } else if (msgType == 'call') {
            var callID = data[1];
            var methodName = data[2];
            if (methodName in self.__funcs) {
                var args = data.slice(3);
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
                throw new KIARA.Error(KIARA.CONNECTION_ERROR,
                    "Received a call for an unregistered method: " + methodName);
            }
        } else {
            throw new KIARA.Error(KIARA.CONNECTION_ERROR, "Unknown message type: " + msgType);
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

    KIARA.registerProtocol('websocket-json', JSONWebSocket);
});