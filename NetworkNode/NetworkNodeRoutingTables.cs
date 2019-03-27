using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Tools;
using static NetworkNodes.NetworkNode;

namespace NetworkNodes
{
    public class NetworkNodeRoutingTables
    {
        public IpFibTable ipFibTable { get; set; }

        public MplsFibTable mplsFibTable { get; set; }

        public FTNTable ftnTable { get; set; }

        public NHLFETable nhlfeTable { get; set; }

        public ILMTable ilmTable { get; set; }

        private ManagementAgent managementAgent { get; set; }

        private MainWindow Window { get; set; }

        public NetworkNodeRoutingTables(MainWindow window)
        {
            Window = window;
            mplsFibTable = new MplsFibTable();
            ftnTable = new FTNTable();
            nhlfeTable = new NHLFETable();
            ilmTable = new ILMTable();
            ipFibTable = new IpFibTable();
        }

        public void LoadTablesFromConfig(NetworkNodeConfig Config, MainWindow window)
        {
            managementAgent = new ManagementAgent(Config, window);
            managementAgent.ModfifyForwardingTable += HandleModifyForwardingTable;
            window.Dispatcher.Invoke(() => window.Title = Config.NodeName);
        }

        public void StartManagementAgent()
        {
            Task.Run(() => managementAgent.Start());
        }

        private void HandleModifyForwardingTable(object sender, ForwardingTableEventArgs e)
        {
            switch (e.Action)
            {
                case ManagementActions.ADD_FTN_ENTRY:
                    AddLog($"Adding new FTN entry: {(FtnTableRow)e.Row}", LogType.Information);
                    ftnTable.Rows.Add((FtnTableRow)e.Row);
                    break;

                case ManagementActions.REMOVE_FTN_ENTRY:
                    AddLog($"Removing FTN entry: {(FtnTableRow)e.Row}", LogType.Information);
                    ftnTable.Rows.Remove((FtnTableRow)e.Row);
                    break;

                case ManagementActions.ADD_ILM_ENTRY:
                    AddLog($"Adding new ILM entry: {(IlmTableRow)e.Row}", LogType.Information);
                    ilmTable.Rows.Add((IlmTableRow)e.Row);
                    break;

                case ManagementActions.REMOVE_ILM_ENTRY:
                    AddLog($"Removing ILM entry: {(IlmTableRow)e.Row}", LogType.Information);
                    ilmTable.Rows.Remove((IlmTableRow)e.Row);
                    break;

                case ManagementActions.ADD_MPLS_FIB_ENTRY:
                    AddLog($"Adding new MPLS-FIB entry: {(MplsFibTableRow)e.Row}", LogType.Information);
                    mplsFibTable.Rows.Add((MplsFibTableRow)e.Row);
                    break;

                case ManagementActions.REMOVE_MPLS_FIB_ENTRY:
                    AddLog($"Removing MPLS-FIB entry: {(MplsFibTableRow)e.Row}", LogType.Information);
                    mplsFibTable.Rows.Remove((MplsFibTableRow)e.Row);
                    break;

                case ManagementActions.ADD_NHLFE_ENTRY:
                    AddLog($"Adding new NHLFE entry: {(NHLFETableRow)e.Row}", LogType.Information);
                    nhlfeTable.Rows.Add((NHLFETableRow)e.Row);
                    break;

                case ManagementActions.REMOVE_NHLFE_ENTRY:
                    AddLog($"Removing NHLFE entry: {(NHLFETableRow)e.Row}", LogType.Information);
                    nhlfeTable.Rows.Remove((NHLFETableRow)e.Row);
                    break;

                case ManagementActions.ADD_IP_FIB_ENTRY:
                    AddLog($"Adding new IP-FIB entry: {(IpFibTableRow)e.Row}", LogType.Information);
                    ipFibTable.Rows.Add((IpFibTableRow)e.Row);
                    break;

                case ManagementActions.REMOVE_IP_FIB_ENTRY:
                    AddLog($"Removing IP-FIB entry: {(IpFibTableRow)e.Row}", LogType.Information);
                    ipFibTable.Rows.Remove((IpFibTableRow)e.Row);
                    break;
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
