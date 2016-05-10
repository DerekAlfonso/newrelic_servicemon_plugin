using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using System.Management;
using System.Configuration;
using System.ServiceProcess;

namespace newrelic_servicemon_plugin
{
    class ServiceMonAgent : Agent
    {
        public override string Guid
        {
            get
            {
                if (ConfigurationManager.AppSettings.HasKeys())
                {
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["guid"].ToString()))
                        return ConfigurationManager.AppSettings["guid"].ToString();
                }
                return "com.automatedops" + Program.ServiceName;
            }
        }
        
        public override string Version
        {
            get
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return version;
            }
        }

        private string Name { get; set; }
        private List<PluginConfig.ServiceMon> Counters { get; set; }

        private static Logger logger = Logger.GetLogger(Program.ServiceName);

        public ServiceMonAgent(string name, List<PluginConfig.ServiceMon> services)
        {
            Name = name;
            Counters = services;
        }

        public override string GetAgentName()
        {
            return Name;
        }

        public override void PollCycle()
        {
            try
            {
                ServiceController sc = new ServiceController();

            }    
            catch (Exception e)
            {
                logger.Error("Unable to connect to \"{0}\". {1}", Name, e.Message);
            }
        }
    }

    class PerfmonAgentFactory : AgentFactory
    {
        public override Agent CreateAgentWithConfiguration(IDictionary<string, object> properties)
        {
            string name = (string)properties["name"];
            List<PluginConfig.ServiceMon> servicelist = (List<PluginConfig.ServiceMon>)properties["servicelist"];

            if (servicelist.Count == 0)
                throw new Exception("'servicelist' is empty. Do you have a 'config/plugin.json' file?");

            return new ServiceMonAgent(name, servicelist);
        }
    }
}
