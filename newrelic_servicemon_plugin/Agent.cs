using System;
using System.Collections.Generic;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
//using NewRelic.Api.Agent;
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
                return "com.DerekAlfonso." + Program.ServiceName;
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

        private static Logger logger = Logger.GetLogger("ServiceMonAgent");

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

        private int CountState(ServiceController[] svcGet, ServiceControllerStatus status, ref int RunningTotal)
        {
            var ret = GetServicesByState(svcGet, status).Count;
            RunningTotal += ret;
            return ret;
        }

        public override void PollCycle()
        {
            try
            {
                Dictionary<string, Object> servicesNotRunning = new Dictionary<string, object>();

                var svcGet = ServiceController.GetServices();
                var runningSvcs = GetServicesByState(svcGet, ServiceControllerStatus.Running);
                ReportMetric("Total/Running", "services", runningSvcs.Count);
                int tsc = runningSvcs.Count;
                ReportMetric("Total/Stopped", "services", CountState(svcGet, ServiceControllerStatus.Stopped, ref tsc));
                ReportMetric("Total/Starting", "services", CountState(svcGet, ServiceControllerStatus.StartPending, ref tsc));
                ReportMetric("Total/Stopping", "services", CountState(svcGet, ServiceControllerStatus.StopPending, ref tsc));
                ReportMetric("Total/Paused", "services", CountState(svcGet, ServiceControllerStatus.Paused, ref tsc));
                ReportMetric("Total/Pausing", "services", CountState(svcGet, ServiceControllerStatus.PausePending, ref tsc));
                ReportMetric("Total/Continue Pending", "services", CountState(svcGet, ServiceControllerStatus.ContinuePending, ref tsc));
                ReportMetric("Total/Service Count", "services", tsc);
                int IsRunning = 0;
                foreach(PluginConfig.ServiceMon svc in ServicesShouldBeRunning)
                {
                    if (!runningSvcs.ContainsKey(svc.servicename.ToLower()))
                    {
                        logger.Error("{0} ({1}) is not running on {2}", svc.displayname, svc.servicename, Name);
                        ReportMetric("Service/" + svc.displayname, "isRunning", 0);
                        servicesNotRunning.Add(svc.servicename, svc.displayname);
                    }
                    else
                    {
                        ReportMetric("Service/" + svc.displayname, "isRunning", 1);
                        IsRunning++;
                    }
                }
                //if(servicesNotRunning.Count>0)
                    //NewRelic.Api.Agent.NewRelic.RecordCustomEvent("Required Services Not Running", servicesNotRunning);

                ReportMetric("Required/Count/Running", "services", IsRunning);
                ReportMetric("Required/Count/Not Running", "services", servicesNotRunning.Count);
                ReportMetric("Required/Count/Total", "services", ServicesShouldBeRunning.Count);

                ReportPercentage("Required/Prct/Not Running", servicesNotRunning.Count, ServicesShouldBeRunning.Count);
                ReportPercentage("Required/Prct/Running", IsRunning, ServicesShouldBeRunning.Count);
            }
            catch (Exception e)
            {
                logger.Fatal("Unable to connect to \"{0}\". {1}", Name, e.Message);
            }
        }

        private void ReportPercentage(string Name, float Numerator, float Denominator)
        {
            if (Denominator > 0)
                ReportMetric(Name, "%", (Numerator / Denominator) * 100);
            else
                ReportMetric(Name, "%", 0);
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
