using System.Collections.Generic;

namespace newrelic_servicemon_plugin
{
    public class PluginConfig
    {
        public class ServiceMon
        {
            public string servicename { get; set; }
            public string displayname { get; set; }
        }
        public class Agent
        {
            public Agent()
            {
                servicelist = new List<ServiceMon>();
            }
            public string name { get; set; }
            public List<ServiceMon> servicelist { get; set; }
        }
        public List<Agent> agents { get; set; }
        public PluginConfig()
        {
            agents = new List<Agent>();
        }
    }
}
