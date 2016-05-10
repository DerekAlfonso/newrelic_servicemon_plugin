using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
namespace newrelic_plugin_tests
{
    [TestClass]
    public class ServiceMon
    {
        ServiceController[] services = ServiceController.GetServices();
        [TestMethod]
        public void ListServices()
        {
            Assert.IsTrue(services.Length > 0, "No services returned");
            Console.WriteLine("Running Services:");
            var runningSvc = GetServicesByState(services, ServiceControllerStatus.Running);
            foreach (string svcName in runningSvc.Keys)
                Console.WriteLine("{0} ({1})", runningSvc[svcName], svcName);
            Console.WriteLine();
            Console.WriteLine("Services Not Running:");
            var notRunningSvc = GetServicesByNotState(services, ServiceControllerStatus.Running);
            foreach (string svcName in notRunningSvc.Keys)
                Console.WriteLine("{0} ({1})", notRunningSvc[svcName], svcName);

        }
        [TestMethod]
        public void GetServices()
        {
            Assert.IsTrue(services.Length > 0, "No services returned");
            Console.WriteLine("Returned {0} services", services.Length);
            Dictionary<string, string> RunningServiceList = GetServicesByState(services, ServiceControllerStatus.Running);
            Console.WriteLine("There are {0} running services", RunningServiceList.Count);
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


        [TestMethod]
        public void IsRunningTest()
        {
            var svcList = GetServicesByState(services, ServiceControllerStatus.Running);
            foreach(string checkSvc in Properties.Settings.Default.ShouldBeRunning)
                Assert.IsTrue(svcList.ContainsKey(checkSvc.ToLower()), "{0} is not running", checkSvc);
        }

        [TestMethod]
        public void BuildConfig()
        {
            newrelic_servicemon_plugin.PluginConfig pc = new newrelic_servicemon_plugin.PluginConfig();
            pc.agents.Add(new newrelic_servicemon_plugin.PluginConfig.Agent
            {
                name = Environment.MachineName,
                servicelist = new List<newrelic_servicemon_plugin.PluginConfig.ServiceMon>
                  {
                      new newrelic_servicemon_plugin.PluginConfig.ServiceMon
                      {
                           servicename = "aspnet_state",
                           displayname = "ASP.net State Service"
                      },
                      new newrelic_servicemon_plugin.PluginConfig.ServiceMon
                      {
                          servicename = "timebroker",
                          displayname = "Time Broker"
                      }
                  }
            });
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(pc, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText("config/plugin.json", json);
            Console.Write(json);
        }

        [TestMethod]
        public void RunService()
        {
            newrelic_servicemon_plugin.PluginConfig pc = new newrelic_servicemon_plugin.PluginConfig();
            pc.agents.Add(new newrelic_servicemon_plugin.PluginConfig.Agent
            {
                name = Environment.MachineName,
                servicelist = new List<newrelic_servicemon_plugin.PluginConfig.ServiceMon>
                  {
                      new newrelic_servicemon_plugin.PluginConfig.ServiceMon
                      {
                           servicename = "aspnet_state",
                           displayname = "ASP.net State Service"
                      },
                      new newrelic_servicemon_plugin.PluginConfig.ServiceMon
                      {
                          servicename = "timebroker",
                          displayname = "Time Broker"
                      }
                  }
            });
            newrelic_servicemon_plugin.ServiceMonAgent agent = new newrelic_servicemon_plugin.ServiceMonAgent(pc.agents[0].name, pc.agents[0].servicelist);
            agent.PollCycle();
        }
    }
}
