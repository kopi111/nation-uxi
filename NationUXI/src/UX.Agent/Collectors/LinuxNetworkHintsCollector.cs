using System.Net.NetworkInformation;
using System.Net.Sockets;
using UX.Shared.Contracts;

namespace UX.Agent.Collectors;

public class LinuxNetworkHintsCollector : INetworkHintsCollector
{
    public Task<NetworkHintsDto?> CollectAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Collect());

    private static NetworkHintsDto Collect()
    {
        var ips = new List<string>();
        var dns = new List<string>();

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            var ipProps = ni.GetIPProperties();

            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    ips.Add(ua.Address.ToString());
            }

            foreach (var d in ipProps.DnsAddresses)
            {
                if (d.AddressFamily == AddressFamily.InterNetwork)
                    dns.Add(d.ToString());
            }
        }

        return new NetworkHintsDto
        {
            IpAddresses = ips.Distinct().Take(10).ToList(),
            DefaultGateway = null,
            DnsServers = dns.Distinct().Take(10).ToList(),
            ConnectedSsid = null,
            ConnectedBssid = null
        };
    }
}
