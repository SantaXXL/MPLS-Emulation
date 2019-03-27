using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static CableCloud.CableCloud;

namespace CableCloud
{

    /// <summary>
    /// Logika interakcji dla klasy CableCloud.xaml
    /// </summary>
    public partial class CableCloudGUI : Window
    {
        private CableCloud cableCloud;
        private List<CableBetweenPairOfNodes> connectedPairs;
        private const string Working = "WORKING";
        private const string OutOfOrder = "OUT-OF-ORDER";
        private static int flag = 0;

        public CableCloudGUI()
        {
            // display exceptions in english
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            InitializeComponent();
            logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);

            var args = Environment.GetCommandLineArgs();
            if (args.Length != 2)
            {
                MessageBox.Show("Wrong number of arguments!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            var filePath = args[1];
            cableCloud = new CableCloud(filePath);
            cableCloud.window = this;
            connectedPairs = new List<CableBetweenPairOfNodes>();

            foreach (var cable in cableCloud.Config.StatusOfCableBetweenNodes)
            {
                // e.g. H1:1111-R2:2222, WORKING
                var node1 = cable.Key.Split(':')[0];
                var port1 = cable.Key.Split('-')[0].Split(':')[1];
                var node2 = cable.Key.Split(':')[1].Split('-')[1];
                var port2 = cable.Key.Split(':')[2];
                var status = cable.Value;
                connectedPairs.Add(new CableBetweenPairOfNodes(node1, port1, node2, port2, status));
            }

            connectedPairs.Sort((x, y) =>
            {
                int compare = x.Node1.CompareTo(y.Node1);
                if (compare != 0)
                {
                    return compare;
                }
                return x.Node2.CompareTo(y.Node2);
            });
            tableOfCables.ItemsSource = connectedPairs;
        }

        private void ChangeCableStatus_DoubleClick(object sender, RoutedEventArgs e)
        {
            int index = tableOfCables.SelectedIndex;
            if (index == -1) // selected empty entry
            {
                return;
            }
            var selectedEntry = tableOfCables.SelectedItems[0] as CableBetweenPairOfNodes;
            var node1 = selectedEntry.Node1;
            var port1 = selectedEntry.Port1;
            var node2 = selectedEntry.Node2;
            var port2 = selectedEntry.Port2;
            var cableStatus = selectedEntry.Status;
            if (cableStatus == Working)
            {
                cableStatus = OutOfOrder;
            }
            else
            {
                cableStatus = Working;
            }
            var connectedNodes = $"{selectedEntry.Node1}:{selectedEntry.Port1}-{ selectedEntry.Node2}:{ selectedEntry.Port2}";
            cableCloud.Config.StatusOfCableBetweenNodes[connectedNodes] = cableStatus;
            var connectedPair = connectedPairs.Find(x => x.Node1.Equals(node1) && x.Node2.Equals(node2) && x.Port1.Equals(port1) && x.Port2.Equals(port2));
            connectedPair.Status = cableStatus;
            tableOfCables.Items.Refresh();
        }

        public void AddLog(string log, LogType logType)
        {
            Run run = new Run(log);
            Color color;

            switch (logType)
            {
                case LogType.Connected:
                    color = flag++ % 2 == 0 ? Colors.BurlyWood : Colors.OldLace;
                    break;
                case LogType.discarded:
                    color = Color.FromRgb(222, 141, 135);
                    break;
                case LogType.Received:
                    color = Colors.LightGreen;
                    break;
                case LogType.Sent:
                    color = Colors.Yellow;
                    break;
                default:
                    color = Colors.White;
                    break;
            }

            run.Foreground = new SolidColorBrush(Colors.Black);
            Paragraph paragraph = new Paragraph(run);
            paragraph.Background = new SolidColorBrush(color);

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

        private void ChangeFontSizeKeyboard(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.OemPlus)
                {
                    ++logTextBox.FontSize;
                    ++tableOfCables.FontSize;
                }
                else if (e.Key == Key.OemMinus)
                {
                    --logTextBox.FontSize;
                    --tableOfCables.FontSize;
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
                ++tableOfCables.FontSize;
            }
            else
            {
                --logTextBox.FontSize;
                --tableOfCables.FontSize;
            }
        }


    }
}
