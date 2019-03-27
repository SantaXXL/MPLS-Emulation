using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using static NetworkNodes.NetworkNode;

namespace NetworkNodes
{
    public class ManagementAgent
    {
        public NetworkNodeConfig Config { get; set; }

        public event EventHandler<ForwardingTableEventArgs> ModfifyForwardingTable;

        public Socket ConnectedSocket { get; set; }

        public static int flag = 0;

        public MainWindow Window { get; set; }


        public ManagementAgent(NetworkNodeConfig networkNodeConfig, MainWindow window)
        {
            Config = networkNodeConfig;
            Window = window;
        }

        public void Start()
        {
            while (true)
            {
                AddLog("Management agent starting...", LogType.Information);

                ConnectToMS();

                if (ConnectedSocket == null)
                {
                    // connection failed
                    // start over
                    continue;
                }

                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[512];
                        int bytes = ConnectedSocket.Receive(buffer);

                        var message = Encoding.ASCII.GetString(buffer, 0, bytes);

                        Task.Run(() => HandleMessage(message));
                    }
                    catch (SocketException e)
                    {
                        // ignore timeout exceptions
                        if (e.SocketErrorCode != SocketError.TimedOut)
                        {
                            if (e.SocketErrorCode == SocketError.Shutdown || e.SocketErrorCode == SocketError.ConnectionReset)
                            {
                                AddLog("Connection to MS broken!", LogType.Error);
                                break;
                            }

                            else
                            {
                                AddLog($"{e.Source}: {e.SocketErrorCode}", LogType.Error);
                            }
                        }
                    }
                }
            }
        }

        private void ConnectToMS()
        {
            AddLog($"Connecting to MS at {Config.ManagementSystemAddress}:{Config.ManagementSystemPort}", LogType.Information);
            while (true)
            {
                Socket socket = new Socket(Config.ManagementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 20000;

                try
                {
                    var result = socket.BeginConnect(new IPEndPoint(Config.ManagementSystemAddress, Config.ManagementSystemPort), null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                    if (success)
                    {
                        socket.EndConnect(result);
                    }
                    else
                    {
                        socket.Close();
                        AddLog("Connection to MS not established - timeout...", LogType.Error);
                        continue;
                    }
                }
                catch (Exception)
                {
                    AddLog("Retrying...", LogType.Information);
                }

                try
                {
                    AddLog($"Sending hello to MS...", LogType.Information);
                    socket.Send(Encoding.ASCII.GetBytes($"{ManagementActions.HELLO} {Config.NodeName}"));

                    byte[] buffer = new byte[256];
                    int bytes = socket.Receive(buffer);

                    var message = Encoding.ASCII.GetString(buffer, 0, bytes);

                    if (message.Contains(ManagementActions.HELLO))
                    {
                        AddLog("Estabilished connection with MS", LogType.Information);
                        ConnectedSocket = socket;

                        break;
                    }
                }
                catch (Exception)
                {
                    AddLog("Couldn't connect to MS!", LogType.Error);
                }
            }

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
                        AddLog("Connection to MS broken!", LogType.Error);

                        break;
                    }
                }
            });
        }


        private void HandleMessage(string message)
        {
            AddLog($"Message received from MS: {message}", LogType.Information);

            // first word in message is the action type
            var action = message.Split(null)[0];
            var data = message.Replace($"{action} ", "");

            switch (action)
            {
                case ManagementActions.SHOW_LESS_ROUTING_LOGS:
                    Window.ShowDetailedRoutingLogs = false;
                    break;

                case ManagementActions.SHOW_MORE_ROUTING_LOGS:
                    Window.ShowDetailedRoutingLogs = true;
                    break;

                case ManagementActions.SHOW_LESS_TTL_LOGS:
                    Window.ShowDetailedTTLLogs = false;
                    break;

                case ManagementActions.SHOW_MORE_TTL_LOGS:
                    Window.ShowDetailedTTLLogs = true;
                    break;

                case ManagementActions.ADD_FTN_ENTRY:
                case ManagementActions.REMOVE_FTN_ENTRY:
                    var ftnArgs = new ForwardingTableEventArgs(action, new FtnTableRow(data));
                    OnModifyForwardingTable(ftnArgs);
                    break;

                case ManagementActions.ADD_ILM_ENTRY:
                case ManagementActions.REMOVE_ILM_ENTRY:
                    var ilmArgs = new ForwardingTableEventArgs(action, new IlmTableRow(data));
                    OnModifyForwardingTable(ilmArgs);
                    break;

                case ManagementActions.ADD_MPLS_FIB_ENTRY:
                case ManagementActions.REMOVE_MPLS_FIB_ENTRY:
                    var mplsFibArgs = new ForwardingTableEventArgs(action, new MplsFibTableRow(data));
                    OnModifyForwardingTable(mplsFibArgs);
                    break;

                case ManagementActions.ADD_NHLFE_ENTRY:
                case ManagementActions.REMOVE_NHLFE_ENTRY:
                    var nhlfeArgs = new ForwardingTableEventArgs(action, new NHLFETableRow(data));
                    OnModifyForwardingTable(nhlfeArgs);
                    break;

                case ManagementActions.ADD_IP_FIB_ENTRY:
                case ManagementActions.REMOVE_IP_FIB_ENTRY:
                    var ipFibArgs = new ForwardingTableEventArgs(action, new IpFibTableRow(data));
                    OnModifyForwardingTable(ipFibArgs);
                    break;
            }
        }

        protected virtual void OnModifyForwardingTable(ForwardingTableEventArgs e)
        {
            ModfifyForwardingTable?.Invoke(this, e);
        }

        private void AddLog(string log, LogType logType)
        {
            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Window.Dispatcher.Invoke(() => Window.AddLog(log, logType));
        }
    }
}
