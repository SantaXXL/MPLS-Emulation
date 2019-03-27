using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Tools;

namespace Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int flag = 0;
        public HostConfig Config { get; set; }

        // even = not sending
        // odd = sending
        public int IsSending { get; set; }

        public int NextID = 0;

        public RemoteHostData CurrentDestination { get; set; }

        public double CurrentPeriod { get; set; }

        public MPLSSocket ConnectedSocket { get; set; }

        public enum LogType { Information, BrokenConnection }

        public MainWindow()
        {
            // display exceptions in english
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            InitializeComponent();
            logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);
            IsSending = 0;
            UpdatePeriodsComboList();
            periodsComboBox.SelectedIndex = 3;
            UpdateComponents();
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                try
                {
                    Config = HostConfig.ReadConfig(args[1]);
                    Title = Config.HostName;
                    UpdateDestinationsComboBoxList();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Wrong config", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                }
            }
            Task.Run(() => ConnectToCloud());
        }

        private void ConnectToCloud()
        {
            Dispatcher.Invoke(() => AddLog($"Connecting to cable cloud at {Config.CloudAddress}:{Config.CloudPort}"));

            try
            {
                ConnectedSocket =
                    new MPLSSocket(Config.CloudAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ConnectedSocket.Connect(new IPEndPoint(Config.CloudAddress, Config.CloudPort));
                ConnectedSocket.Send(Encoding.ASCII.GetBytes($"{ManagementActions.HELLO} {Config.HostName}"));

                Dispatcher.Invoke(() => AddLog($"Estabilished connection with cable cloud"));
                Task.Run(() => { Listen(); });
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() => AddLog("Failed to connect to cable cloud", LogType.BrokenConnection));
            }
        }

        public void Listen()
        {
            while (true)
            {
                while (ConnectedSocket == null || !ConnectedSocket.Connected)
                {
                    Dispatcher.Invoke(() => AddLog("Retrying connection to cable cloud"));
                    ConnectToCloud();
                }

                try
                {
                    MPLSPackage package = ConnectedSocket.Receive();

                    if (package != null)
                    {
                        Dispatcher.Invoke(() => AddLog($"Received package: {package}"));
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.TimedOut)
                    {
                        if (e.SocketErrorCode == SocketError.Shutdown || e.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            Dispatcher.Invoke(() => AddLog("Connection to cloud broken!", LogType.BrokenConnection));
                            continue;
                        }

                        else
                        {
                            Dispatcher.Invoke(() => AddLog($"Couldn't connect to cable cloud!", LogType.BrokenConnection));
                        }
                    }
                }

            }
        }

        private void periodsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = periodsComboBox.SelectedItem.ToString();
            double.TryParse(selection, out double periodInSeconds);
            CurrentPeriod = periodInSeconds * 1000;
        }

        private void startSendingButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Sending button clicked. Sending to: " + CurrentDestination);

            IsSending++;

            Task.Run(async () =>
            {
                while (IsSendingOn())
                {
                    var lastSendingFlag = IsSending;

                    SendMessage();

                    await Task.Delay(TimeSpan.FromMilliseconds(CurrentPeriod));

                    // check if cancelled in the meantime
                    if (lastSendingFlag != IsSending)
                    {
                        break;
                    }
                }
            });

            UpdateComponents();
        }

        private void SendMessage()
        {
            if (ConnectedSocket == null || !ConnectedSocket.Connected)
            {
                return;
            }

            // construct the MPLS package
            MPLSPackage package = new MPLSPackage();

            package.ID = NextID++;
            package.SourceAddress = Config.IpAddress;
            package.Port = Config.OutPort;
            package.TTL = --package.TTL;

            Dispatcher.Invoke(() =>
            {
                package.Payload = messageTextBox.Text;
                package.DestAddress = ((RemoteHostData)destinationsComboBox.SelectedItem).IPAddress;
            });

            try
            {
                ConnectedSocket.Send(package);
                Dispatcher.Invoke(() => { AddLog($"Package sent: {package}"); });
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => AddLog(e.Message));
            }
        }

        private void stopSendingButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("Stop sending button clicked");
            IsSending++;
            UpdateComponents();
        }

        private void UpdateComponents()
        {
            startSendingButton.IsEnabled = Config != null && CurrentDestination != null && !IsSendingOn();
            stopSendingButton.IsEnabled = Config != null && IsSendingOn();
        }

        private void UpdatePeriodsComboList()
        {
            string[] periods = { "0.1", "0.5", "1", "2", "5", "15", "30", "60" };
            foreach (var period in periods)
            {
                periodsComboBox.Items.Add(period);
            }
        }

        private void UpdateDestinationsComboBoxList()
        {
            destinationsComboBox.Items.Clear();

            if (Config != null)
            {
                Config.RemoteHosts.ForEach(remoteHost => destinationsComboBox.Items.Add(remoteHost));
            }
        }

        private void destinationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentDestination = (RemoteHostData)destinationsComboBox.SelectedItem;
            UpdateComponents();
        }

        private bool IsSendingOn()
        {
            return IsSending % 2 == 1;
        }

        public void AddLog(string log, LogType logType = LogType.Information)
        {
            Run run = new Run($"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond.ToString().PadLeft(3, '0')}] {log}");
            Color foregroundColor = Colors.Black;
            Color backgroundColor = Colors.White;
            switch (logType)
            {
                case LogType.Information:
                    backgroundColor = flag++ % 2 == 0 ? Colors.BurlyWood : Color.FromRgb(239, 220, 195);
                    break;
                case LogType.BrokenConnection:
                    backgroundColor = Color.FromRgb(222, 141, 135);
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


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.OemPlus)
                {
                    ++logTextBox.FontSize;
                }
                else if (e.Key == Key.OemMinus)
                {
                    --logTextBox.FontSize;
                }
            }
        }

        private void LogTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            e.Handled = true;
            if (e.Delta > 0)
            {
                ++logTextBox.FontSize;
            }
            else
            {
                --logTextBox.FontSize;
            }
        }
    }
}
