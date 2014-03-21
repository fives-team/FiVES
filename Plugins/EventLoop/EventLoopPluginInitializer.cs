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
            Scripting.RegisterGlobalObject("eventLoop", new EventLoopScriptingInterface());
            Scripting.NewContextCreated += InitializeEventLoopScriptingAPI;

        }

        private void InitializeEventLoopScriptingAPI(object sender, NewContextCreatedArgs e)
        {
            e.Context.Execute(@"
                eventLoop._handlers = [];

                eventLoop._triggerHandlers = function (elapsedMs) {
                    for (var handler in eventloop._handlers)
                        handler.call(null, elapsedMs);
                }

                eventLoop.addTickFiredHandler = function (handler) {
                    eventLoop._handlers.push(handler);
                }

                eventLoop.removeTickFiredHandler = function (handler) {
                    var handlerIndex = event._handlers.indexOf(handler);
                    if (handlerIndex != -1)
                        eventLoop._handlers.splice(handlerIndex, 1);
                }
            ");

            EventLoop.TickFired += delegate(object sender2, TickEventArgs args)
            {
                e.Context.Execute("eventLoop._triggerHandlers(" + args.TimeStamp.TotalMilliseconds + ")");
            };
        }

        public void Shutdown()
        {
        }
    }
}
