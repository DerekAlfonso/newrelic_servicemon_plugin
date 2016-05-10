using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using System.Configuration;
using System.ServiceProcess;
using System.Linq;

namespace newrelic_servicemon_plugin
{
    public class ServiceMonAgent : Agent
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
        private List<PluginConfig.ServiceMon> ServicesShouldBeRunning { get; set; }

        private static Logger logger = Logger.GetLogger(Program.ServiceName);

        public ServiceMonAgent(string name, List<Object> services)
        {
            Name = name;
            ServicesShouldBeRunning = ConvertFromObjectList(services);
        }

        private List<PluginConfig.ServiceMon> ConvertFromObjectList(List<Object> input)
        {
            List<PluginConfig.ServiceMon> ret = new List<PluginConfig.ServiceMon>();
            foreach(object item in input)
            {

                if(item is Dictionary<string,object>)
                {
                    Dictionary<string, object> ti = (Dictionary<string, object>)item;
                    ret.Add(new PluginConfig.ServiceMon { servicename = (string)ti["servicename"], displayname = (string)ti["displayname"] });
                }
            }
            return ret;
        }

        public override string GetAgentName()
        {
            return Name;
        }

        public override void PollCycle()
        {
            try
            {
                var runningSvcs = GetServicesByState(ServiceController.GetServices(), ServiceControllerStatus.Running);
                foreach(PluginConfig.ServiceMon svc in ServicesShouldBeRunning)
                {
                    if (!runningSvcs.ContainsKey(svc.servicename.ToLower()))
                    {
                        logger.Error("{0} ({1}) is not running on {2}", svc.displayname, svc.servicename, Name);
                        ReportMetric(svc.displayname, "running", 0);
                    }
                    else
                        ReportMetric(svc.displayname, "running", 1);
                }
            }
            catch (Exception e)
            {
                logger.Fatal("Unable to connect to \"{0}\". {1}", Name, e.Message);
            }
        }

        private static Dictionary<string, string> GetServicesByState(ServiceController[] services, ServiceControllerStatus stateToGet)
        {
            Dictionary<string, string> RunningServiceList = new Dictionary<string, string>();
            foreach (ServiceController svc in services.Where(x => x.Status == stateToGet))
                RunningServiceList.Add(svc.ServiceName.ToLower(), svc.DisplayName);
            return RunningServiceList;
        }
        private static Dictionary<string, string> GetServicesByNotState(ServiceController[] services, ServiceControllerStatus stateToNotGet)
        {
            Dictionary<string, string> RunningServiceList = new Dictionary<string, string>();
            foreach (ServiceController svc in services.Where(x => x.Status != stateToNotGet))
                RunningServiceList.Add(svc.ServiceName.ToLower(), svc.DisplayName);
            return RunningServiceList;
        }
    }

    class ServiceMonAgentFactory : AgentFactory
    {
        public override Agent CreateAgentWithConfiguration(IDictionary<string, object> properties)
        {
            string name = (string)properties["name"];
            List<Object> servicelist = (List<Object>)properties["servicelist"];

            if (servicelist.Count == 0)
                throw new Exception("'servicelist' is empty. Do you have a 'config/plugin.json' file?");

            return new ServiceMonAgent(name, servicelist);
        }
    }
}
