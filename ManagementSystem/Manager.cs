using NetworkNodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using static ManagementSystem.MainWindow;

namespace ManagementSystem
{
    class StateObject
    {
        // Client  socket
        public Socket WorkSocket { get; set; } = null;
        // Size of receive buffer
        public const int BufferSize = 1024;
        // Receive buffer
        public byte[] Buffer { get; set; } = new byte[BufferSize];
        // Received data string
        public StringBuilder sb { get; set; } = new StringBuilder();
    }

    class Manager
    {
        private ManualResetEvent AllDone = new ManualResetEvent(false);
        private const int Backlog = 100;
        private IPAddress ManagementSystemAddress = IPAddress.Parse("127.0.0.1");
        private Socket Server;
        private const int ManagementSystemPort = 62570;
        private ConcurrentDictionary<string, Socket> RouterNameToSocket;
        public ConcurrentDictionary<Socket, string> SocketToRouterName { get; set; }
        public Dictionary<string, List<FtnTableRow>> RouterNameToFTN_Table { get; set; } = new Dictionary<string, List<FtnTableRow>>();
        public Dictionary<string, List<MplsFibTableRow>> RouterNameToMPLS_FIB_Table { get; set; } = new Dictionary<string, List<MplsFibTableRow>>();
        public Dictionary<string, List<IpFibTableRow>> RouterNameToIP_FIB_Table { get; set; } = new Dictionary<string, List<IpFibTableRow>>();
        public Dictionary<string, List<IlmTableRow>> RouterNameToILMTable { get; set; } = new Dictionary<string, List<IlmTableRow>>();
        public Dictionary<string, List<NHLFETableRow>> RouterNameToNHLFE_Table { get; set; } = new Dictionary<string, List<NHLFETableRow>>();
        public MainWindow Window;

        public Manager()
        {
            RouterNameToSocket = new ConcurrentDictionary<string, Socket>();
            SocketToRouterName = new ConcurrentDictionary<Socket, string>();
        }

        public void Start(MainWindow window)
        {
            Window = window;
            AddLog("Server started...", LogType.Information);
            Task.Run(action: () => StartServer());
        }

        private void StartServer()
        {
            Server = new Socket(ManagementSystemAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            AddLog("Waiting for a connection...", LogType.Information);
            try
            {
                Server.Bind(new IPEndPoint(ManagementSystemAddress, ManagementSystemPort));
                Server.Listen(Backlog);
                while (true)
                {
                    AllDone.Reset();
                    Server.BeginAccept(new AsyncCallback(AcceptCallback), Server);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                AddLog(e.Source + " " + e.Message, LogType.Error);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            AllDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            // Read data from the client socket.
            int bytesRead;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception)
            {
                Socket outSocket;
                string outString;

                var routerName = SocketToRouterName[handler];
                RouterNameToSocket.TryRemove(routerName, out outSocket);
                SocketToRouterName.TryRemove(handler, out outString);
                // if the client has been shutdown, then close the connection
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }
            state.sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
            var content = state.sb.ToString().Split(' ');
            if (content[0].StartsWith("KEEPALIVE"))
            {
                // do nothing
            }
            else if (content[0].Equals("HELLO"))
            {
                var routerName = content[1];

                while (true)
                {
                    var success = RouterNameToSocket.TryAdd(routerName, handler);
                    if (success)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }

                while (true)
                {
                    var success = SocketToRouterName.TryAdd(handler, routerName);
                    if (success)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                AddLog($"Message received from {content[1]}: {content[0]}", LogType.Information);
                SendResponse(routerName, handler);
            }
            else
            {
                AddLog("Invalid request", LogType.Error);
                return;
            }
            state.sb.Clear();
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void SendResponse(string routerName, Socket handler)
        {
            AddLog($"Sending to {routerName}: HELLO", LogType.Information);
            byte[] responseMessage = Encoding.ASCII.GetBytes("HELLO");
            handler.Send(responseMessage);

            RouterNameToFTN_Table.Keys.ToList().FindAll(key => key.Equals(routerName)).ForEach(key =>
            {
                RouterNameToFTN_Table[key].ForEach(rule => SendRow(routerName, rule, ManagementActions.ADD_FTN_ENTRY));
            });

            RouterNameToILMTable.Keys.ToList().FindAll(key => key.Equals(routerName)).ForEach(key =>
            {
                RouterNameToILMTable[key].ForEach(rule => SendRow(routerName, rule, ManagementActions.ADD_ILM_ENTRY));
            });

            RouterNameToIP_FIB_Table.Keys.ToList().FindAll(key => key.Equals(routerName)).ForEach(key =>
            {
                RouterNameToIP_FIB_Table[key].ForEach(rule => SendRow(routerName, rule, ManagementActions.ADD_IP_FIB_ENTRY));
            });

            RouterNameToMPLS_FIB_Table.Keys.ToList().FindAll(key => key.Equals(routerName)).ForEach(key =>
            {
                RouterNameToMPLS_FIB_Table[key].ForEach(rule => SendRow(routerName, rule, ManagementActions.ADD_MPLS_FIB_ENTRY));
            });

            RouterNameToNHLFE_Table.Keys.ToList().FindAll(key => key.Equals(routerName)).ForEach(key =>
            {
                RouterNameToNHLFE_Table[key].ForEach(rule => SendRow(routerName, rule, ManagementActions.ADD_NHLFE_ENTRY));
            });
        }

        public void SendRow(string routerName, ITableRow row, string action)
        {
            var socket = RouterNameToSocket[routerName];
            byte[] data = Encoding.ASCII.GetBytes($"{action} {row.Serialize()}");
            socket.Send(data);
            Thread.Sleep(100);
            AddLog($"Sending to {routerName}: {action} {row}", LogType.Information);
        }

        public void ReadConfig()
        {
            var content = File.ReadAllLines(Environment.GetCommandLineArgs()[1]).ToList();

            var actionsToTypes = new Dictionary<string, Type>()
            {
                {ManagementActions.ADD_FTN_ENTRY, typeof(FtnTableRow)},
                {ManagementActions.ADD_ILM_ENTRY, typeof(IlmTableRow)},
                {ManagementActions.ADD_MPLS_FIB_ENTRY, typeof(MplsFibTableRow)},
                {ManagementActions.ADD_IP_FIB_ENTRY, typeof(IpFibTableRow)},
                {ManagementActions.ADD_NHLFE_ENTRY, typeof(NHLFETableRow)}
            };

            var actionsToDictionaries = new Dictionary<string, Dictionary<string, List<object>>>();

            // read all tables
            foreach (var key in actionsToTypes.Keys.ToList())
            {
                actionsToDictionaries[key] = content.FindAll(line => line.Contains(key))
                    .GroupBy(line => line.Substring(0, line.IndexOf(' '))) // group by router name
                    .ToDictionary(line => line.Key, lines => lines.Select(line => // put in a dictionary with router name as key
                    {
                        var parts = line.Split(null);

                        return actionsToTypes[key].GetConstructors()[0]
                            .Invoke(new object[] { line.Replace($"{parts[0]} ", "").Replace($"{parts[1]} ", "") }); // get type from dictionary and instantiate a suitable TableRow
                    }).ToList());
            }

            // assign the dictionaries to object properties while casting the List<object> to List<proper type>
            RouterNameToFTN_Table = actionsToDictionaries[ManagementActions.ADD_FTN_ENTRY]
                .ToDictionary(pair => pair.Key, pair => pair.Value.Select(val => (FtnTableRow)val).ToList());

            RouterNameToILMTable = actionsToDictionaries[ManagementActions.ADD_ILM_ENTRY]
                .ToDictionary(pair => pair.Key, pair => pair.Value.Select(val => (IlmTableRow)val).ToList());

            RouterNameToIP_FIB_Table = actionsToDictionaries[ManagementActions.ADD_IP_FIB_ENTRY]
                .ToDictionary(pair => pair.Key, pair => pair.Value.Select(val => (IpFibTableRow)val).ToList());

            RouterNameToMPLS_FIB_Table = actionsToDictionaries[ManagementActions.ADD_MPLS_FIB_ENTRY]
                .ToDictionary(pair => pair.Key, pair => pair.Value.Select(val => (MplsFibTableRow)val).ToList());

            RouterNameToNHLFE_Table = actionsToDictionaries[ManagementActions.ADD_NHLFE_ENTRY]
                .ToDictionary(pair => pair.Key, pair => pair.Value.Select(val => (NHLFETableRow)val).ToList());
        }

        public static object ConvertList(List<object> values, Type type)
        {
            return values.Select(item => Convert.ChangeType(item, type)).ToList();
        }

        private void AddLog(string log, LogType logType)
        {
            Window.Dispatcher.Invoke(() => Window.AddLog(log, logType));
        }

        private void SkipLines(int howManyLines, StreamReader streamReader)
        {
            for (int i = 0; i < howManyLines; i++)
            {
                streamReader.ReadLine();
            }
        }
    }
}
