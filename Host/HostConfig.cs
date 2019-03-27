using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Host
{
    public class HostConfig
    {
        public List<RemoteHostData> RemoteHosts { get; set; }

        public IPAddress CloudAddress { get; set; }

        public ushort CloudPort { get; set; }

        public IPAddress IpAddress { get; set; }

        public string HostName { get; set; }

        public ushort OutPort { get; set; }

        public HostConfig()
        {
            RemoteHosts = new List<RemoteHostData>();
        }

        public static HostConfig ReadConfig(String configFile)
        {
            HostConfig hostConfig = new HostConfig();
            var lines = File.ReadAllLines(configFile).ToList();

            // 1. wczytaj dane klientów z którymi można się połączyć        
            hostConfig.RemoteHosts = lines.FindAll(line => line.StartsWith("REMOTEHOST"))
                .Select(line => line.Replace("REMOTEHOST ", ""))
                .Select(line => new RemoteHostData(line)).ToList();

            hostConfig.CloudAddress = IPAddress.Parse(GetProperty(lines, "CLOUDADDRESS"));
            hostConfig.CloudPort = ushort.Parse(GetProperty(lines, "CLOUDPORT"));
            hostConfig.IpAddress = IPAddress.Parse(GetProperty(lines, "IPADDRESS"));
            hostConfig.HostName = GetProperty(lines, "HOSTNAME");
            hostConfig.OutPort = ushort.Parse(GetProperty(lines, "OUTPORT"));

            return hostConfig;
        }

        private static string GetProperty(List<string> content, string propertyName)
        {
            return content.Find(line => line.StartsWith(propertyName)).Replace($"{propertyName} ", "");
        }
    }
}
