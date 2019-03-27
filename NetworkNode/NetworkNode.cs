using NetworkNode;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tools;

namespace NetworkNodes
{
    public partial class NetworkNode
    {
        public NetworkNodeConfig Config { get; set; }

        public MPLSSocket ConnectedSocket { get; set; }

        public static int ColorFlag { get; set; } = 0;

        public MainWindow Window { get; set; }

        public PackageSwitch packageSwitch { get; set; }

        public NetworkNodeRoutingTables networkNodeRoutingTables { get; set; }

        public NetworkNode(MainWindow window)
        {
            Window = window;
            packageSwitch = new PackageSwitch(Window);
            networkNodeRoutingTables = new NetworkNodeRoutingTables(Window);
        }

        public void Start(string configPath)
        {
            AddLog("Starting a node...", LogType.Information);

            try
            {
                Config = NetworkNodeConfig.ParseConfig(configPath);
            }
            catch (Exception e)
            {
                AddLog($"Exception: {e.Message}", LogType.Error);
                return;
            }

            networkNodeRoutingTables.LoadTablesFromConfig(Config, Window);
            networkNodeRoutingTables.StartManagementAgent();

            ConnectToCloud();

            while (true)
            {
                while (ConnectedSocket == null || !ConnectedSocket.Connected)
                {
                    AddLog("Retrying connection to cable cloud...", LogType.Information);
                    ConnectToCloud();
                }

                try
                {
                    var package = ConnectedSocket.Receive();

                    AddLog($"Received package: {package} at port {package.Port}", LogType.Received);

                    Task.Run(() => HandlePackage(package));
                }
                catch (InvalidMPLSPackageException)
                {
                    AddLog("Received package was not a valid package.", LogType.Error);
                }
                catch (SocketException e)
                {
                    // ignore timeout exceptions
                    if (e.SocketErrorCode != SocketError.TimedOut)
                    {
                        if (e.SocketErrorCode == SocketError.Shutdown || e.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            AddLog("Connection to Cloud broken!", LogType.Error);
                            continue;
                        }

                        else
                        {
                            AddLog($"{e.Source}: {e.SocketErrorCode}", LogType.Error);
                        }
                    }
                }
            }
        }

        private void ConnectToCloud()
        {
            AddLog($"Connecting to cable cloud at {Config.CloudAddress}:{Config.CloudPort}", LogType.Information);

            try
            {
                MPLSSocket socket = new MPLSSocket(Config.ManagementSystemAddress.AddressFamily, SocketType.Stream,
                    ProtocolType.Tcp);

                socket.Connect(new IPEndPoint(Config.CloudAddress, Config.CloudPort));

                socket.Send(Encoding.ASCII.GetBytes($"{ManagementActions.HELLO} {Config.NodeName}"));
                AddLog("Estabilished connection with cable cloud", LogType.Information);

                ConnectedSocket = socket;

                Task.Run(async () =>
                {
                    while (true)
                    {
                        var connectionGood = ConnectedSocket != null && ConnectedSocket.Connected;
                        if (connectionGood)
                        {
                            ConnectedSocket.Send(Encoding.ASCII.GetBytes("KEEPALIVE"));

                            await Task.Delay(5000);
                        }
                        else
                        {
                            AddLog("Connection to cable cloud broken!", LogType.Error);

                            break;
                        }
                    }
                });
            }
            catch (Exception)
            {
                AddLog("Failed to connect to cable cloud", LogType.Error);
            }
        }

        private void HandlePackage(MPLSPackage package)
        {
            MPLSPackage routedPackage = null;
            routedPackage = packageSwitch.RouteMPLSPackage(package, networkNodeRoutingTables, Window);
            if (routedPackage == null)
            {
                return;
            }
            try
            {
                ConnectedSocket.Send(routedPackage);
                AddLog(
                    $"Package {package} routed to port {routedPackage.Port} with label {routedPackage.PeekTopLabel()?.Number.ToString() ?? "None"}",
                    LogType.Sent);
            }
            catch (Exception e)
            {
                AddLog($"Package {routedPackage} not sent correctly: {e.Message}", LogType.Error);
            }
        }

        private void AddLog(string log, LogType logType, bool showDetailedLogs = true)
        {
            if (showDetailedLogs == false)
            {
                return;
            }
            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Window.Dispatcher.Invoke(() => Window.AddLog(log, logType));
        }
    }
}