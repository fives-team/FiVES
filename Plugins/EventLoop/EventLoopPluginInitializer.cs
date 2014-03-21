using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIVES;
using ScriptingPlugin;

namespace EventLoopPlugin
{
    public class EventLoopPluginInitializer : IPluginInitializer
    {
        public string Name
        {
            get { return "EventLoop"; }
        }

        public List<string> PluginDependencies
        {
            get { return new List<string> { }; }
        }

        public List<string> ComponentDependencies
        {
            get { return new List<string> { }; }
        }

        public void Initialize()
        {
            EventLoop.Instance = new EventLoopImpl();

            PluginManager.Instance.AddPluginLoadedHandler("Scripting", RegisterScriptingInterface);
        }

        private void RegisterScriptingInterface()
        {
            eventLoopObj = new EventLoopScriptingInterface();
            Scripting.NewContextCreated += InitializeEventLoopScriptingAPI;
        }

        private void InitializeEventLoopScriptingAPI(object sender, NewContextCreatedArgs e)
        {
            e.Context.Execute(@"
                eventLoop = arguments[0];
                eventLoop._handlers = [];

                eventLoop._triggerHandlers = function (elapsedMs) {
                    for (var index in this._handlers)
                        this._handlers[index].call(null, elapsedMs);
                }

                eventLoop.addTickFiredHandler = function (handler) {
                    this._handlers.push(handler);
                }

                eventLoop.removeTickFiredHandler = function (handler) {
                    var handlerIndex = this._handlers.indexOf(handler);
                    if (handlerIndex != -1)
                        this._handlers.splice(handlerIndex, 1);
                }
            ", eventLoopObj);

            EventLoop.TickFired += delegate(object sender2, TickEventArgs args)
            {
                e.Context.Execute("eventLoop._triggerHandlers(" + args.TimeStamp.TotalMilliseconds + ")");
            };
        }

        EventLoopScriptingInterface eventLoopObj;

        public void Shutdown()
        {
        }
    }
}
