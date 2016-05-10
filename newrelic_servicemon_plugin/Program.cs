using System;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using Topshelf;
using System.Threading;

namespace newrelic_servicemon_plugin
{
    class Program
    {
        public const string ServiceName = "newrelic_servicemon_plugin";
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<PluginService>(sc =>
                {
                    sc.ConstructUsing(() => new PluginService());

                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });
                x.SetServiceName(ServiceName);
                x.SetDisplayName("NewRelic Windows Service Monitor Plugin");
                x.SetDescription("Sends Details to NewRelic About Running Services");
                x.StartAutomatically();
                x.RunAsPrompt();
            });
        }
    }

    class PluginService
    {
        Runner _runner;
        public Thread thread { get; set; }

        private static Logger logger = Logger.GetLogger(Program.ServiceName);

        public PluginService()
        {
            _runner = new Runner();
            _runner.Add(new PerfmonAgentFactory());
        }

        public void Start()
        {
            logger.Info("Starting service.");
            thread = new Thread(new ThreadStart(_runner.SetupAndRun));
            try
            {
                thread.Start();
            }
            catch (Exception e)
            {
                logger.Error("Exception occurred, unable to continue. {0}\r\n{1}", e.Message, e.StackTrace);
            }
        }

        public void Stop()
        {
            logger.Info("Stopping service.");
            System.Threading.Thread.Sleep(5000);
            
            if (thread.IsAlive)
            {
                _runner = null;
                thread.Abort();
            }            
        }
    }
}
