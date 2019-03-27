using NetworkNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Tools;

namespace ManagementSystem
{
    public partial class MainWindow : Window
    {
        private Manager Manager { get; set; }

        public List<MplsFibTableRow> MPLS_FIB_Rows { get; set; }

        public List<IpFibTableRow> IP_FIB_Rows { get; set; }

        public List<FtnTableRow> FTN_Rows { get; set; }

        public List<IlmTableRow> ILM_Rows { get; set; }

        public List<NHLFETableRow> NHLFE_Rows { get; set; }

        private static int flag = 0;

        public enum LogType { Error, Information };

        public MainWindow()
        {
            // display exceptions in english
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Manager = new Manager();
            MPLS_FIB_Rows = new List<MplsFibTableRow>();
            IP_FIB_Rows = new List<IpFibTableRow>();
            FTN_Rows = new List<FtnTableRow>();
            ILM_Rows = new List<IlmTableRow>();
            NHLFE_Rows = new List<NHLFETableRow>();

            InitializeComponent();
            logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);

            try
            {
                Manager.ReadConfig();
            }
            catch (Exception e)
            {
                AddLog($"Exception: {e.Message}", LogType.Error);
            }

            foreach (var entry in Manager.RouterNameToFTN_Table)
            {
                entry.Value.ForEach(value => value.RouterName = entry.Key);
            }

            foreach (var entry in Manager.RouterNameToILMTable)
            {
                entry.Value.ForEach(value => value.RouterName = entry.Key);
            }

            foreach (var entry in Manager.RouterNameToIP_FIB_Table)
            {
                entry.Value.ForEach(value => value.RouterName = entry.Key);
            }

            foreach (var entry in Manager.RouterNameToMPLS_FIB_Table)
            {
                entry.Value.ForEach(value => value.RouterName = entry.Key);
            }

            foreach (var entry in Manager.RouterNameToNHLFE_Table)
            {
                entry.Value.ForEach(value => value.RouterName = entry.Key);
            }

            MPLS_FIB_Rows = Manager.RouterNameToMPLS_FIB_Table.Values.SelectMany(x => x).ToList();
            IP_FIB_Rows = Manager.RouterNameToIP_FIB_Table.Values.SelectMany(x => x).ToList();
            FTN_Rows = Manager.RouterNameToFTN_Table.Values.SelectMany(x => x).ToList();
            ILM_Rows = Manager.RouterNameToILMTable.Values.SelectMany(x => x).ToList();
            NHLFE_Rows = Manager.RouterNameToNHLFE_Table.Values.SelectMany(x => x).ToList();
            SortMplsFibRows();
            SortIpFibRows();
            SortFtnRows();
            SortIlmRows();
            SortNhlfeRows();

            MPLS_FIB_Table.ItemsSource = MPLS_FIB_Rows;
            IP_FIB_Table.ItemsSource = IP_FIB_Rows;
            FTN_Table.ItemsSource = FTN_Rows;
            ILM_Table.ItemsSource = ILM_Rows;
            NHLFE_Table.ItemsSource = NHLFE_Rows;

            Manager.Start(this);
        }

        private void SortMplsFibRows()
        {
            MPLS_FIB_Rows.Sort((x, y) =>
            {
                int compare = x.RouterName.CompareTo(y.RouterName);
                if (compare != 0)
                {
                    return compare;
                }
                compare = x.FEC.ToString().CompareTo(y.FEC.ToString());
                if (compare != 0)
                {
                    return compare;
                }
                return x.DestAddress.ToString().CompareTo(y.DestAddress.ToString());
            });
        }

        private void SortIpFibRows()
        {
            IP_FIB_Rows.Sort((x, y) =>
            {
                int compare = x.RouterName.CompareTo(y.RouterName);
                if (compare != 0)
                {
                    return compare;
                }
                return x.OutPort.ToString().CompareTo(y.OutPort.ToString());
            });
        }

        private void SortFtnRows()
        {
            FTN_Rows.Sort((x, y) =>
            {
                int compare = x.RouterName.CompareTo(y.RouterName);
                if (compare != 0)
                {
                    return compare;
                }
                compare = x.FEC.ToString().CompareTo(y.FEC.ToString());
                if (compare != 0)
                {
                    return compare;
                }
                return x.NHLFE_ID.ToString().CompareTo(y.NHLFE_ID.ToString());
            });
        }

        private void SortIlmRows()
        {
            ILM_Rows.Sort((x, y) =>
            {
                int compare = x.RouterName.CompareTo(y.RouterName);
                if (compare != 0)
                {
                    return compare;
                }
                compare = x.IncPort.ToString().CompareTo(y.IncPort.ToString());
                if (compare != 0)
                {
                    return compare;
                }
                compare = x.IncLabel.ToString().CompareTo(y.IncLabel.ToString());
                if (compare != 0)
                {
                    return compare;

                }
                return x.PoppedLabelStack.ToString().CompareTo(y.PoppedLabelStack.ToString());
            });
        }

        private void SortNhlfeRows()
        {
            NHLFE_Rows.Sort((x, y) =>
            {
                int compare = x.RouterName.CompareTo(y.RouterName);
                if (compare != 0)
                {
                    return compare;
                }
                compare = x.ID.ToString().CompareTo(y.ID.ToString());
                if (compare != 0)
                {
                    return compare;
                }
                return x.OutLabel.ToString().CompareTo(y.OutLabel.ToString());
            });
        }


        public void AddLog(string log, LogType logType)
        {
            Run run = new Run($"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}");
            Color backgroundColor = Colors.White;
            Color foregroundColor = Colors.White;
            switch (logType)
            {
                case LogType.Information:
                    backgroundColor = flag++ % 2 == 0 ? Colors.BurlyWood : Color.FromRgb(239, 220, 195);
                    foregroundColor = Colors.Black;
                    break;
                case LogType.Error:
                    backgroundColor = Color.FromRgb(222, 141, 135);
                    foregroundColor = Colors.Black;
                    break;
            }

            run.Foreground = new SolidColorBrush(foregroundColor);
            Paragraph paragraph = new Paragraph(run);
            paragraph.Background = new SolidColorBrush(backgroundColor);
            var numberOfBlocks = logTextBox.Document.Blocks.Count;
            const int MaxNumberOfBlocks = 100;

            if (numberOfBlocks > MaxNumberOfBlocks)
            {
                logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);
            }
            logTextBox.Document.Blocks.Add(paragraph);
            logTextBox.Document.PagePadding = new Thickness(0);
            logTextBox.ScrollToEnd();
        }

        private bool CheckIfIsProperMPLS_FIB_Entry(string destAddress, string FEC)
        {
            var isNumeric = int.TryParse(FEC, out int n);
            var isIPaddress = IPAddress.TryParse(destAddress, out IPAddress address);
            if (isNumeric)
            {
                if (n < 0)
                {
                    return false;
                }
            }
            return isNumeric && isIPaddress;
        }

        private void MPLS_FIB_AddButton_Click(object sender, RoutedEventArgs e)
        {
            var routerName = MPLS_FIB_RouterName.Text;
            var destAddress = MPLS_FIB_DestAddress.Text;
            var FEC = MPLS_FIB_FEC.Text;
            if (routerName.Length == 0 || destAddress.Length == 0 || FEC.Length == 0)
            {
                return;
            }

            var isProperMPLS_FIB_Rule = CheckIfIsProperMPLS_FIB_Entry(destAddress, FEC);
            if (!isProperMPLS_FIB_Rule || !Manager.RouterNameToMPLS_FIB_Table.ContainsKey(routerName))
            {
                AddLog("You are trying to add an incorrect rule!", LogType.Error);
                return;
            }

            var foundRow = MPLS_FIB_Rows.Find(x => x.RouterName.Equals(routerName) &&
            x.DestAddress.Equals(IPAddress.Parse(destAddress)));
            if (foundRow != null)
            {
                AddLog("FEC for such destination adress already exists!", LogType.Error);
                return;
            }

            var rule = new MplsFibTableRow(destAddress + " " + FEC);
            rule.RouterName = routerName;

            Manager.RouterNameToMPLS_FIB_Table[routerName].Add(rule);
            MPLS_FIB_Rows.Add(rule);
            Manager.SendRow(routerName, rule, ManagementActions.ADD_MPLS_FIB_ENTRY);
            SortMplsFibRows();
            MPLS_FIB_Table.Items.Refresh();
            MPLS_FIB_RouterName.Clear();
            MPLS_FIB_DestAddress.Clear();
            MPLS_FIB_FEC.Clear();
            MPLS_FIB_RouterName.Focus();
        }

        private void MPLS_FIB_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = MPLS_FIB_Table.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }
            var selectedEntry = MPLS_FIB_Table.SelectedItems[0] as MplsFibTableRow;
            var routerName = selectedEntry.RouterName;
            var destAddress = selectedEntry.DestAddress;
            var FEC = selectedEntry.FEC;

            if (Manager.RouterNameToMPLS_FIB_Table.ContainsKey(routerName))
            {
                try
                {
                    // remove item from the list that is binded to GUI's listView
                    var foundRow = MPLS_FIB_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.DestAddress.Equals(destAddress) && x.FEC.Equals(FEC));
                    Manager.SendRow(routerName, foundRow, ManagementActions.REMOVE_MPLS_FIB_ENTRY);
                    MPLS_FIB_Rows.Remove(foundRow);

                    // remove item from the list that is in dictionary
                    var row = Manager.RouterNameToMPLS_FIB_Table[routerName].Find(x => x.RouterName.Equals(routerName) &&
                    x.DestAddress.Equals(destAddress) && x.FEC.Equals(FEC));
                    Manager.RouterNameToMPLS_FIB_Table[routerName].Remove(row);
                    MPLS_FIB_Table.Items.Refresh();
                }
                catch (Exception)
                {
                    AddLog($"Router {routerName} is not connected to MS!\n", LogType.Error);
                }
            }
            else
            {
                AddLog("Network topology does not contain such router!\n", LogType.Error);
            }
        }

        private bool CheckIfIsProperIP_FIB_Entry(string destAddress, string outPort)
        {
            var isNumeric = int.TryParse(outPort, out int n);
            var isIPaddress = IPAddress.TryParse(destAddress, out IPAddress address);
            return isNumeric && isIPaddress;
        }

        private void IP_FIB_AddButton_Click(object sender, RoutedEventArgs e)
        {
            var routerName = IP_FIB_RouterName.Text;
            var destAddress = IP_FIB_DestAddress.Text;
            var outPort = IP_FIB_OutPort.Text;
            if (routerName.Length == 0 || destAddress.Length == 0 || outPort.Length == 0)
            {
                return;
            }
            var isProperRule = CheckIfIsProperIP_FIB_Entry(destAddress, outPort);
            if (!isProperRule || !Manager.RouterNameToIP_FIB_Table.ContainsKey(routerName))
            {
                AddLog("You are trying to add an incorrect rule!", LogType.Error);
                return;
            }

            var foundRow = IP_FIB_Rows.Find(x => x.RouterName.Equals(routerName) &&
            x.DestAddress.Equals(IPAddress.Parse(destAddress)));
            if (foundRow != null)
            {
                AddLog("Output port for such destination address already exists!", LogType.Error);
                return;
            }

            var rule = new IpFibTableRow(destAddress + " " + outPort);
            rule.RouterName = routerName;
            Manager.RouterNameToIP_FIB_Table[routerName].Add(rule);
            IP_FIB_Rows.Add(rule);
            Manager.SendRow(routerName, rule, ManagementActions.ADD_IP_FIB_ENTRY);
            SortIpFibRows();
            IP_FIB_Table.Items.Refresh();
            IP_FIB_RouterName.Clear();
            IP_FIB_DestAddress.Clear();
            IP_FIB_OutPort.Clear();
            IP_FIB_RouterName.Focus();
        }

        private void IP_FIB_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = IP_FIB_Table.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }
            var selectedEntry = IP_FIB_Table.SelectedItems[0] as IpFibTableRow;
            var routerName = selectedEntry.RouterName;
            var destAddress = selectedEntry.DestAddress;
            var outPort = selectedEntry.OutPort;
            if (Manager.RouterNameToIP_FIB_Table.ContainsKey(routerName))
            {
                try
                {
                    // remove item from the list that is binded to GUI's listView
                    var foundRow = IP_FIB_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.DestAddress.Equals(destAddress) && x.OutPort.Equals(outPort));
                    Manager.SendRow(routerName, foundRow, ManagementActions.REMOVE_IP_FIB_ENTRY);
                    IP_FIB_Rows.Remove(foundRow);

                    // remove item from the list that is in dictionary
                    var item = Manager.RouterNameToIP_FIB_Table[routerName].Find(x => x.RouterName.Equals(routerName) &&
                    x.DestAddress.Equals(destAddress) && x.OutPort.Equals(outPort));
                    Manager.RouterNameToIP_FIB_Table[routerName].Remove(item);
                    IP_FIB_Table.Items.Refresh();
                }
                catch (Exception)
                {
                    AddLog($"Router {routerName} is not connected to MS!\n", LogType.Error);
                }
            }
            else
            {
                AddLog("Network topology does not contain such router!\n", LogType.Error);
            }
        }

        private bool CheckIfIsProperFTN_Entry(string FEC, string NHLFE_ID)
        {
            var isNumeric1 = int.TryParse(FEC, out int n1);
            var isNumeric2 = int.TryParse(NHLFE_ID, out int n2);
            if (isNumeric1 && isNumeric2)
            {
                if (n1 >= 0 && n2 >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void FTN_AddButton_Click(object sender, RoutedEventArgs e)
        {
            var routerName = FTN_RouterName.Text;
            var FEC = FTN_FEC.Text;
            var NHLFE_ID = FTN_NHLFE_ID.Text;
            if (routerName.Length == 0 || FEC.Length == 0 || NHLFE_ID.Length == 0)
            {
                return;
            }
            var isProperRule = CheckIfIsProperFTN_Entry(FEC, NHLFE_ID);
            if (!isProperRule || !Manager.RouterNameToFTN_Table.ContainsKey(routerName))
            {
                AddLog("You are trying to add an incorrect rule!", LogType.Error);
                return;
            }

            var foundRow = FTN_Rows.Find(x => x.RouterName.Equals(routerName) &&
                   x.FEC.Equals(int.Parse(FEC)));

            if (foundRow != null)
            {
                AddLog("NHLFE ID for such FEC already exists!", LogType.Error);
                return;
            }

            var rule = new FtnTableRow(FEC + " " + NHLFE_ID);
            rule.RouterName = routerName;
            Manager.RouterNameToFTN_Table[routerName].Add(rule);
            FTN_Rows.Add(rule);
            Manager.SendRow(routerName, rule, ManagementActions.ADD_FTN_ENTRY);
            SortFtnRows();

            FTN_Table.Items.Refresh();
            FTN_RouterName.Clear();
            FTN_FEC.Clear();
            FTN_NHLFE_ID.Clear();
            FTN_RouterName.Focus();
        }

        private void FTN_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = FTN_Table.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }

            var selectedEntry = FTN_Table.SelectedItems[0] as FtnTableRow;
            var routerName = selectedEntry.RouterName;
            var FEC = selectedEntry.FEC;
            var NHLFE_ID = selectedEntry.NHLFE_ID;

            if (Manager.RouterNameToFTN_Table.ContainsKey(routerName))
            {
                try
                {
                    // remove item from the list that is binded to GUI's listView
                    var foundRow = FTN_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.FEC.Equals(FEC) && x.NHLFE_ID.Equals(NHLFE_ID));
                    Manager.SendRow(routerName, foundRow, ManagementActions.REMOVE_FTN_ENTRY);
                    FTN_Rows.Remove(foundRow);

                    // remove item from the list that is in dictionary
                    var row = Manager.RouterNameToFTN_Table[routerName].Find(x => x.RouterName.Equals(routerName) &&
                    x.FEC.Equals(FEC) && x.NHLFE_ID.Equals(NHLFE_ID));
                    Manager.RouterNameToFTN_Table[routerName].Remove(row);
                    FTN_Table.Items.Refresh();
                }
                catch (Exception)
                {
                    AddLog($"Router {routerName} is not connected to MS!\n", LogType.Error);
                }
            }
            else
            {
                AddLog("Network topology does not contain such router!\n", LogType.Error);
            }
        }

        private bool CheckIfIsProperILM_Entry(string incPort, string incLabel, string poppedLabels, string NHLFE_ID)
        {
            var isNumeric1 = int.TryParse(incLabel, out int n1);
            var isNumeric2 = int.TryParse(incPort, out int n2);
            var isNumeric3 = int.TryParse(NHLFE_ID, out int n3);
            if (!(isNumeric1 && isNumeric2 && isNumeric3))
            {
                return false;
            }
            if (n1 < 16 || n2 < 1 || n3 < 1)
            {
                return false;
            }
            if (poppedLabels.Equals("-"))
            {
                return true;
            }

            var parts = poppedLabels.Split(',');
            if (parts == null)
            {
                return false;
            }
            foreach (var label in parts)
            {
                if (int.TryParse(label, out int x) == false)
                {
                    return false;
                }
                if (x < 15)
                {
                    return false;
                }
            }
            return true;
        }

        private void ILM_AddButton_Click(object sender, RoutedEventArgs e)
        {
            var routerName = ILM_RouterName.Text;
            var incPort = ILM_IncPort.Text;
            var incLabel = ILM_IncLabel.Text;
            var poppedLabels = ILM_PoppedLabels.Text;
            var NHLFE_ID = ILM_NHLFE_ID.Text;

            if (routerName.Length == 0 || incPort.Length == 0 || incLabel.Length == 0 || poppedLabels.Length == 0 || NHLFE_ID.Length == 0)
            {
                return;
            }
            var isProperRule = CheckIfIsProperILM_Entry(incPort, incLabel, poppedLabels, NHLFE_ID);
            if (!isProperRule || !Manager.RouterNameToILMTable.ContainsKey(routerName))
            {
                AddLog("You are trying to add an incorrect rule!", LogType.Error);
                return;
            }

            var foundRow = ILM_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.IncLabel.Equals(int.Parse(incLabel)) && x.IncPort.Equals(ushort.Parse(incPort)) && x.PoppedLabelStack.Equals(poppedLabels));

            if(foundRow != null)
            {
                AddLog("NHLFE ID for such parameters already exists!", LogType.Error);
                return;
            }

            var rule = new IlmTableRow(incPort + " " + incLabel + " " + poppedLabels + " " + NHLFE_ID);
            rule.RouterName = routerName;
            Manager.RouterNameToILMTable[routerName].Add(rule);
            ILM_Rows.Add(rule);
            Manager.SendRow(routerName, rule, ManagementActions.ADD_ILM_ENTRY);
            SortIlmRows();

            ILM_Table.Items.Refresh();
            ILM_RouterName.Clear();
            ILM_IncPort.Clear();
            ILM_IncLabel.Clear();
            ILM_PoppedLabels.Clear();
            ILM_NHLFE_ID.Clear();
            ILM_RouterName.Focus();
        }

        private void ILM_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = ILM_Table.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }

            var selectedEntry = ILM_Table.SelectedItems[0] as IlmTableRow;
            var routerName = selectedEntry.RouterName;
            var incLabel = selectedEntry.IncLabel;
            var incPort = selectedEntry.IncPort;
            var poppedLabelStack = selectedEntry.PoppedLabelStack;
            var NHLFE_ID = selectedEntry.NHLFE_ID;

            if (Manager.RouterNameToILMTable.ContainsKey(routerName))
            {
                try
                {
                    // remove item from the list that is binded to GUI's listView
                    var foundRow = ILM_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.IncLabel.Equals(incLabel) && x.IncPort.Equals(incPort) && x.PoppedLabelStack.Equals(poppedLabelStack) && x.NHLFE_ID.Equals(NHLFE_ID));
                    Manager.SendRow(routerName, foundRow, ManagementActions.REMOVE_ILM_ENTRY);
                    ILM_Rows.Remove(foundRow);

                    // remove item from the list that is in dictionary
                    var row = Manager.RouterNameToILMTable[routerName].Find(x => x.RouterName.Equals(routerName) &&
                    x.IncLabel.Equals(incLabel) && x.IncPort.Equals(incPort) && x.PoppedLabelStack.Equals(poppedLabelStack) && x.NHLFE_ID.Equals(NHLFE_ID));
                    Manager.RouterNameToILMTable[routerName].Remove(row);
                    ILM_Table.Items.Refresh();
                }
                catch (Exception)
                {
                    AddLog($"Router {routerName} is not connected to MS!\n", LogType.Error);
                }
            }
            else
            {
                AddLog("Network topology does not contain such router!\n", LogType.Error);
            }
        }

        private bool CheckIfIsProperNHLFE_Entry(string routerName, string NHLFE_ID, string action, string outLabel, string outPort, string nextID)
        {
            var isNumeric1 = int.TryParse(NHLFE_ID, out int n1);
            var isNumeric2 = int.TryParse(outLabel, out int n2);
            var isNumeric3 = int.TryParse(outPort, out int n3);
            var isNumeric4 = int.TryParse(nextID, out int n4);
            if (!isNumeric1)
            {
                return false;
            }
            if (n1 < 1)
            {
                return false;
            }
            bool alreadyExistsSuchNhlfe = false;
            NHLFE_Rows.ForEach((row) =>
            {
                if (row.RouterName.Equals(routerName))
                {
                    if (row.ID == n1)
                    {
                        alreadyExistsSuchNhlfe = true;
                    }
                }
            });

            if (alreadyExistsSuchNhlfe)
            {
                return false;
            }

            if (!(action.Equals("PUSH") || action.Equals("POP") || action.Equals("SWAP")))
            {
                return false;
            }
            if (action.Equals("PUSH") && outPort.Equals("-") & nextID.Equals("-"))
            {
                return false;
            }
            if (!isNumeric2)
            {
                if (!outLabel.Equals("-"))
                {
                    return false;
                }
            }
            if (!isNumeric3)
            {
                if (!outPort.Equals("-"))
                {
                    return false;
                }
            }
            if (!isNumeric4)
            {
                if (!nextID.Equals("-"))
                {
                    return false;
                }
            }
            if (isNumeric1 && isNumeric4)
            {
                if (n1 == n4)
                {
                    return false;
                }
            }
            if (isNumeric2)
            {
                if (n2 < 16)
                {
                    return false;
                }
            }
            if (isNumeric3 && isNumeric4)
            {
                return false;
            }
            if (isNumeric3)
            {
                if (n3 < 1)
                {
                    return false;
                }
            }
            if (isNumeric4)
            {
                if (n4 < 1)
                {
                    return false;
                }
            }
            return true;
        }

        private void NHLFE_AddButton_Click(object sender, RoutedEventArgs e)
        {
            var routerName = NHLFE_RouterName.Text;
            var NHLFE_ID = NHLFE_NHLFE_ID.Text;
            var action = NHLFE_Action.Text;
            var outLabel = NHLFE_OutLabel.Text;
            var outPort = NHLFE_OutPort.Text;
            var nextID = NHLFE_NextID.Text;

            if (routerName.Length == 0 || NHLFE_ID.Length == 0 || action.Length == 0 || outLabel.Length == 0 || outPort.Length == 0 || nextID.Length == 0)
            {
                return;
            }
            var isProperRule = CheckIfIsProperNHLFE_Entry(routerName, NHLFE_ID, action, outLabel, outPort, nextID);
            if (!isProperRule || !Manager.RouterNameToNHLFE_Table.ContainsKey(routerName))
            {
                AddLog("You are trying to add an incorrect rule!", LogType.Error);
                return;
            }

            var foundRow = NHLFE_Rows.Find(x => x.RouterName.Equals(routerName) &&
                   x.ID.Equals(int.Parse(NHLFE_ID)));
            if(foundRow != null)
            {
                AddLog("NHLFE with given ID already exists!", LogType.Error);
                return;
            }

            var rule = new NHLFETableRow(NHLFE_ID + " " + action + " " + outLabel + " " + outPort + " " + nextID);
            rule.RouterName = routerName;
            Manager.RouterNameToNHLFE_Table[routerName].Add(rule);
            NHLFE_Rows.Add(rule);
            Manager.SendRow(routerName, rule, ManagementActions.ADD_NHLFE_ENTRY);
            SortNhlfeRows();

            NHLFE_Table.Items.Refresh();
            NHLFE_RouterName.Clear();
            NHLFE_NHLFE_ID.Clear();
            NHLFE_Action.Clear();
            NHLFE_OutLabel.Clear();
            NHLFE_OutPort.Clear();
            NHLFE_NextID.Clear();
            NHLFE_RouterName.Focus();
        }

        private void NHLFE_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = NHLFE_Table.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }

            var selectedEntry = NHLFE_Table.SelectedItems[0] as NHLFETableRow;
            var routerName = selectedEntry.RouterName;
            var NHLFE_ID = selectedEntry.ID;
            var action = selectedEntry.Action;
            var outLabel = selectedEntry.OutLabel;
            var outPort = selectedEntry.OutPort;
            var NHLFE_next_ID = selectedEntry.NextID;

            if (Manager.RouterNameToNHLFE_Table.ContainsKey(routerName))
            {
                try
                {
                    // remove item from the list that is binded to GUI's listView
                    var foundRow = NHLFE_Rows.Find(x => x.RouterName.Equals(routerName) &&
                    x.ID.Equals(NHLFE_ID) && x.Action.Equals(action) && x.OutLabel.Equals(outLabel) &&
                    x.OutPort.Equals(outPort) && x.NextID.Equals(NHLFE_next_ID));
                    Manager.SendRow(routerName, foundRow, ManagementActions.REMOVE_NHLFE_ENTRY);
                    NHLFE_Rows.Remove(foundRow);

                    // remove item from the list that is in dictionary
                    var row = Manager.RouterNameToNHLFE_Table[routerName].Find(x => x.RouterName.Equals(routerName) &&
                    x.ID.Equals(NHLFE_ID) && x.Action.Equals(action) && x.OutLabel.Equals(outLabel) &&
                    x.OutPort.Equals(outPort) && x.NextID.Equals(NHLFE_next_ID));
                    Manager.RouterNameToNHLFE_Table[routerName].Remove(row);
                    NHLFE_Table.Items.Refresh();
                }
                catch (Exception)
                {
                    AddLog($"Router {routerName} is not connected to MS!\n", LogType.Error);
                }
            }
            else
            {
                AddLog("Network topology does not contain such router!\n", LogType.Error);
            }
        }

        private void ShowDetailedRoutingLogs_Checked(object sender, RoutedEventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(ManagementActions.SHOW_MORE_ROUTING_LOGS);

            Task.Run(action: () =>
            {
                foreach (var entry in Manager.SocketToRouterName)
                {
                    var socket = entry.Key;
                    socket.Send(message);
                }
            });
            AddLog($"Sending to ALL: {ManagementActions.SHOW_MORE_ROUTING_LOGS}", LogType.Information);
        }

        private void ShowDetailedRoutingLogs_Unchecked(object sender, RoutedEventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(ManagementActions.SHOW_LESS_ROUTING_LOGS);

            Task.Run(action: () =>
            {
                foreach (var entry in Manager.SocketToRouterName)
                {
                    var socket = entry.Key;
                    socket.Send(message);
                }
            });
            AddLog($"Sending to ALL: {ManagementActions.SHOW_LESS_ROUTING_LOGS}", LogType.Information);
        }

        private void ShowDetailedTTLLogs_Checked(object sender, RoutedEventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(ManagementActions.SHOW_MORE_TTL_LOGS);

            Task.Run(action: () =>
            {
                foreach (var entry in Manager.SocketToRouterName)
                {
                    var socket = entry.Key;
                    socket.Send(message);
                }
            });
            AddLog($"Sending to ALL: {ManagementActions.SHOW_MORE_ROUTING_LOGS}", LogType.Information);
        }

        private void ShowDetailedTTLLogs_Unchecked(object sender, RoutedEventArgs e)
        {
            byte[] message = Encoding.ASCII.GetBytes(ManagementActions.SHOW_LESS_TTL_LOGS);

            Task.Run(action: () =>
            {
                foreach (var entry in Manager.SocketToRouterName)
                {
                    var socket = entry.Key;
                    socket.Send(message);
                }
            });
            AddLog($"Sending to ALL: {ManagementActions.SHOW_LESS_ROUTING_LOGS}", LogType.Information);
        }

        private void ChangeFontSizeKeyboard(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.OemPlus)
                {
                    ++logTextBox.FontSize;
                    ++MPLS_FIB_Table.FontSize;
                    ++IP_FIB_Table.FontSize;
                    ++FTN_Table.FontSize;
                    ++ILM_Table.FontSize;
                    ++NHLFE_Table.FontSize;
                }
                else if (e.Key == Key.OemMinus)
                {
                    --logTextBox.FontSize;
                    --MPLS_FIB_Table.FontSize;
                    --IP_FIB_Table.FontSize;
                    --FTN_Table.FontSize;
                    --ILM_Table.FontSize;
                    --NHLFE_Table.FontSize;
                }
            }
        }

        private void ChangeFontSizeMouse(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            e.Handled = true;
            if (e.Delta > 0)
            {
                ++logTextBox.FontSize;
                ++MPLS_FIB_Table.FontSize;
                ++IP_FIB_Table.FontSize;
                ++FTN_Table.FontSize;
                ++ILM_Table.FontSize;
                ++NHLFE_Table.FontSize;
            }
            else
            {
                --logTextBox.FontSize;
                --MPLS_FIB_Table.FontSize;
                --IP_FIB_Table.FontSize;
                --FTN_Table.FontSize;
                --ILM_Table.FontSize;
                --NHLFE_Table.FontSize;
            }
        }


    }
}