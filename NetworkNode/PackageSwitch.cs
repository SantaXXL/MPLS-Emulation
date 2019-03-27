using NetworkNodes;
using System;
using System.Linq;
using Tools;
using static NetworkNodes.NetworkNode;

namespace NetworkNode
{
    /// <summary>
    /// Represents a switch inside an LSR
    /// </summary>
    public class PackageSwitch
    {
        private NetworkNodeRoutingTables RoutingTables { get; set; }
        private MainWindow Window { get; set; }

        public PackageSwitch(MainWindow window)
        {
            Window = window;
        }

        public MPLSPackage RouteMPLSPackage(MPLSPackage package, NetworkNodeRoutingTables networkNodeRoutingTables, MainWindow window)
        {
            Window = window;
            RoutingTables = networkNodeRoutingTables;

            MPLSPackage routedPackage = null;

            try
            {
                routedPackage = RoutePackage(package);
            }
            catch (TimeToLiveExpiredException)
            {
                AddLog($"Package {package} has expired (TTL = 0). Package discarded!", LogType.Error);
                return null;
            }
            catch (Exception e)
            {
                AddLog($"Exception: {e.StackTrace}", LogType.Error);
                return null;
            }

            if (routedPackage == null)
            {
                AddLog($"I don't know how to route package: {package}. Package discarded!", LogType.Error);
                return null;
            }
            return routedPackage;
        }

        private MPLSPackage RoutePackage(MPLSPackage package)
        {
            var packageNotLabeled = package.LabelStack.IsEmpty();
            MPLSPackage routedPackage = null;

            if (packageNotLabeled)
            {
                return PerformNotLabeledPackageAction(package, routedPackage);
            }
            else
            {
                routedPackage = RouteByMPLS(package);
            }

            if (routedPackage == null)
            {
                return null;
            }

            // the MPLS forwarding process has removed all the labels - we need to determine new FEC
            if (routedPackage.LabelStack.IsEmpty())
            {
                AddLog($"Label stack is empty, forwarding with IP-FIB table", LogType.Information, Window.ShowDetailedRoutingLogs);
                return RouteByIP(routedPackage);
            }

            // but if it hasn't, then return processed MPLS package with new labels
            return routedPackage;
        }


        private MPLSPackage PerformNotLabeledPackageAction(MPLSPackage package, MPLSPackage routedPackage)
        {
            // check, if FEC exists for such destination adress
            var FEC = RoutingTables.mplsFibTable.Rows.FirstOrDefault(row => row.DestAddress.Equals(package.DestAddress))?.FEC;
            AddLog($"Looking for FEC in MPLS-FIB table...", LogType.Information, Window.ShowDetailedRoutingLogs);
            if (FEC == null)
            {
                // if it doesn't, discard the package
                AddLog($"Couldn't find matching entry!", LogType.CantFindMatchingEntry, Window.ShowDetailedRoutingLogs);
                return null;
            }
            AddLog($"FEC for {package.DestAddress} is: {FEC}", LogType.Information, Window.ShowDetailedRoutingLogs);

            var shouldBeLabeled = FEC != 0;

            // if FEC is != 0, then route it using MPLS rules
            if (shouldBeLabeled)
            {
                AddLog($"Looking for NHLFE_ID in FTN table...", LogType.Information, Window.ShowDetailedRoutingLogs);

                var firstNHLFE_ID = RoutingTables.ftnTable.Rows.FirstOrDefault(row => row.FEC == FEC)?.NHLFE_ID;
                if (firstNHLFE_ID == null)
                {
                    AddLog($"Couldn't find matching entry!", LogType.CantFindMatchingEntry);
                    return null;
                }
                AddLog($"NHLFE_ID for FEC={FEC} is: {(int)firstNHLFE_ID}", LogType.Information, Window.ShowDetailedRoutingLogs);

                routedPackage = RouteByMPLS(package, (int)firstNHLFE_ID);
                AddLog($"Completed last action, forwarding to port {routedPackage.Port}", LogType.Information, Window.ShowDetailedRoutingLogs);
                return routedPackage;
            }
            else // If FEC == 0, then the package should not be labeled; will be used in such case: H1->R1->H2
            {
                AddLog("[IP] Decreasing TTL by 1", LogType.Information, Window.ShowDetailedTTLLogs);
                --package.TTL;
                return RouteByIP(package);
            }
        }

        private MPLSPackage RouteByMPLS(MPLSPackage unprocessedPackage, int firstNHLFEPointer = 0)
        {
            var processedPackage = unprocessedPackage;

            // an empty package, push labels
            if (processedPackage.LabelStack.IsEmpty())
            {
                AddLog("[IP] Decreasing TTL by 1", LogType.Information, Window.ShowDetailedTTLLogs);
                --processedPackage.TTL;
                if (processedPackage.TTL == 0) // IP's TTL == 0, discard the package
                {
                    throw new TimeToLiveExpiredException();
                }
                return PerformMplsEmptyPacketRouting(processedPackage, firstNHLFEPointer);
            }
            else // not an empty package
            {
                AddLog("[MPLS] Decreasing outermost's label TTL by 1", LogType.Information, Window.ShowDetailedTTLLogs);
                --processedPackage.PeekTopLabel().TTL;
                if (processedPackage.PeekTopLabel().TTL == 0) // MPLS's TTL == 0, discard the package
                {
                    throw new TimeToLiveExpiredException();
                }
                return PerformMplsNotEmptyPacketRouting(processedPackage);
            }
        }

        private MPLSPackage PerformMplsEmptyPacketRouting(MPLSPackage package, int firstNHLFEPointer)
        {
            AddLog($"Looking for NHLFE entry with NHLFE_ID={firstNHLFEPointer}...", LogType.Information, Window.ShowDetailedRoutingLogs);
            var firstNHLFE = RoutingTables.nhlfeTable.Rows.FirstOrDefault(row => row.ID == firstNHLFEPointer);

            if (firstNHLFE == null)
            {
                // routing information is missing
                AddLog($"Couldn't find matching entry!", LogType.CantFindMatchingEntry, Window.ShowDetailedRoutingLogs);
                return null;
            }

            var currentNHLFE = firstNHLFE;
            var labelTTL = (byte)package.TTL;
            string message = "Push first label on IP package";

            // apply the sequence of NHLFEs
            while (true)
            {
                var thereIsNextAction = currentNHLFE.NextID != null;
                if (Window.ShowDetailedRoutingLogs)
                {
                    string nextAction, outputPort, outputLabel;
                    nextAction = currentNHLFE.NextID == null ? "NULL" : currentNHLFE.NextID.ToString();
                    outputPort = currentNHLFE.OutPort == null ? "NULL" : currentNHLFE.OutPort.ToString();
                    outputLabel = currentNHLFE.OutLabel == null ? "NULL" : currentNHLFE.OutLabel.ToString();
                    AddLog($"Action for NHLFE_ID={currentNHLFE.ID} is {currentNHLFE.Action}, output port: {outputPort}, output label: {outputLabel}, next NHLFE_ID: {nextAction}", LogType.Information, Window.ShowDetailedRoutingLogs);
                }

                PerformLabelAction(package, currentNHLFE, labelTTL, ref message);
                if (!thereIsNextAction)
                {
                    break;
                }

                var nextID = currentNHLFE.NextID;
                currentNHLFE = RoutingTables.nhlfeTable.Rows.FirstOrDefault(row => row.ID == currentNHLFE.NextID);
                if (currentNHLFE == null)
                {
                    AddLog($"Couldn't find matching entry for NHLFE_ID={nextID}!", LogType.CantFindMatchingEntry, Window.ShowDetailedRoutingLogs);
                    return null;
                }
            }
            return package;
        }

        private MPLSPackage PerformMplsNotEmptyPacketRouting(MPLSPackage package)
        {
            var labelTTL = package.PeekTopLabel().TTL;
            string poppedLabelStack = "-";

            while (true)
            {
                if (package.LabelStack.IsEmpty()) // in case if we are egress LER
                {
                    return package;
                }
                AddLog($"Looking for NHLFE_ID in ILM table...", LogType.Information, Window.ShowDetailedRoutingLogs);

                var ilmRow = RoutingTables.ilmTable.Rows.FirstOrDefault(row => row.IncPort == package.Port && package.PeekTopLabel().Number == (short)row.IncLabel && poppedLabelStack.Equals(row.PoppedLabelStack));
                // After completing previous NHFLE action (with nextID==null), check, if there is any rule for such (incPort, incLabel, poppedLabelStack) entry
                // i.e. let's say that we popped one label in the previous action. We couldn't have NEXT_ID filled, because router can only see what is in the outermost label. 
                // But in order to disgunish, for example, label 17 that was nested from label 17 that was not, we must remember a stack of labels that have been popped.
                // See Tools/Table Rows/IlmTableRow.cs/incLabel description for more detailed explanation.
                if (ilmRow == null)
                {
                    AddLog($"Couldn't find matching entry!", LogType.CantFindMatchingEntry, Window.ShowDetailedRoutingLogs);
                    return null;
                }

                AddLog($"NHLFE_ID for incoming port={package.Port}, incoming label={package.PeekTopLabel().Number}, popped labels={(poppedLabelStack == "-" ? "NULL" : poppedLabelStack)}, is: {ilmRow.NHLFE_ID}", LogType.Information, Window.ShowDetailedRoutingLogs);

                var currentNHLFE = RoutingTables.nhlfeTable.Rows.FirstOrDefault(row => row.ID == ilmRow.NHLFE_ID);
                if (currentNHLFE == null)
                {
                    // routing information is missing
                    AddLog($"Couldn't find NHLFE entry!", LogType.Error, Window.ShowDetailedRoutingLogs);
                    return null;
                }

                while (true)
                {
                    if (Window.ShowDetailedRoutingLogs)
                    {
                        string nextAction, outputPort, outputLabel;
                        nextAction = currentNHLFE.NextID == null ? "NULL" : currentNHLFE.NextID.ToString();
                        outputPort = currentNHLFE.OutPort == null ? "NULL" : currentNHLFE.OutPort.ToString();
                        outputLabel = currentNHLFE.OutLabel == null ? "NULL" : currentNHLFE.OutLabel.ToString();
                        AddLog($"Action for NHLFE_ID={currentNHLFE.ID} is {currentNHLFE.Action}, output port: {outputPort}, output label: {outputLabel}, next NHLFE_ID: {nextAction}", LogType.Information, Window.ShowDetailedRoutingLogs);
                    }

                    var thereIsNextAction = currentNHLFE.NextID != null;
                    PerformLabelAction(package, currentNHLFE, labelTTL, ref poppedLabelStack);

                    // after doing SWAP, the next action will be delivered via NEXT_ID, not ILM entry. Same thing for PUSH.
                    // if there is no next action id, then it means that we can forward the packet
                    if (!thereIsNextAction && (currentNHLFE.Action.Equals("SWAP") || currentNHLFE.Action.Equals("PUSH")))
                    {
                        AddLog($"Completed last action, forwarding to port {package.Port}", LogType.Information, Window.ShowDetailedRoutingLogs);
                        return package;
                    }

                    // this means that we popped a label and the next action - push or swap - will be determined via ILM entry
                    if (!thereIsNextAction)
                    {
                        break;
                    }
                    currentNHLFE = RoutingTables.nhlfeTable.Rows.FirstOrDefault(row => row.ID == currentNHLFE.NextID);
                }
            }
        }

        private MPLSPackage RouteByIP(MPLSPackage package)
        {
            if (package.TTL == 0)
            {
                throw new TimeToLiveExpiredException();
            }

            var matchingRow = RoutingTables.ipFibTable.Rows.FirstOrDefault(row => row.DestAddress.Equals(package.DestAddress));
            AddLog($"Looking for output port in IP-FIB table...", LogType.Information, Window.ShowDetailedRoutingLogs);

            if (matchingRow == null)
            {
                AddLog($"Couldn't find matching entry!", LogType.CantFindMatchingEntry, Window.ShowDetailedRoutingLogs);
                return null;
            }

            package.Port = matchingRow.OutPort;
            AddLog($"The output port for {package.DestAddress} is {package.Port}", LogType.Information, Window.ShowDetailedRoutingLogs);
            return package;
        }

        private void PerformLabelAction(MPLSPackage package, NHLFETableRow NHLFE, byte TTL, ref string poppedLabelStack)
        {
            if (NHLFE.OutPort != null)
            {
                package.Port = (ushort)NHLFE.OutPort;
            }

            switch (NHLFE.Action)
            {
                case LabelActions.POP:
                    if (poppedLabelStack.Equals("-"))
                    {
                        poppedLabelStack = package.PeekTopLabel().Number.ToString();
                    }
                    else
                    {
                        poppedLabelStack += "," + package.PeekTopLabel().Number.ToString();
                    }

                    AddLog($"Popped top label: {package.PeekTopLabel().Number}", LogType.Information, Window.ShowDetailedRoutingLogs);
                    package.PopTopLabel();
                    if (package.LabelStack.IsEmpty())
                    {
                        AddLog("[MPLS->IP] Copying TTL from MPLS level 1 label to IP header", LogType.Information, Window.ShowDetailedTTLLogs);
                        package.TTL = TTL;
                    }
                    else
                    {
                        AddLog("[MPLS] Copying TTL from the previous outermost label to the new [lower level] outermost one", LogType.Information, Window.ShowDetailedTTLLogs);
                        package.PeekTopLabel().TTL = TTL;
                    }
                    break;

                case LabelActions.SWAP:
                    AddLog($"Swapped top label: {package.PeekTopLabel().Number} to {(short)NHLFE.OutLabel}", LogType.Information, Window.ShowDetailedRoutingLogs);
                    package.PopTopLabel();
                    package.PushLabel(new Label((short)NHLFE.OutLabel));
                    package.PeekTopLabel().TTL = TTL;
                    break;

                case LabelActions.PUSH:
                    AddLog($"Pushed new label: {(short)NHLFE.OutLabel}", LogType.Information, Window.ShowDetailedRoutingLogs);
                    package.PushLabel(new Label((short)NHLFE.OutLabel));
                    if (poppedLabelStack.Equals("Push first label on IP package"))
                    {
                        AddLog("[MPLS] Copying TTL from IP header", LogType.Information, Window.ShowDetailedTTLLogs);
                        poppedLabelStack = "-";
                    }
                    else
                    {
                        AddLog("[MPLS] Copying TTL from the previous outermost label to the new [higher level] outermost one", LogType.Information, Window.ShowDetailedTTLLogs);
                    }
                    package.PeekTopLabel().TTL = TTL;
                    break;

                default:
                    break;
            }
        }

        private void AddLog(string log, LogType logType, bool showLog = true)
        {
            if (showLog == false)
            {
                return;
            }

            log = $"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}";
            Window.Dispatcher.Invoke(() => Window.AddLog(log, logType));
        }
    }
}
