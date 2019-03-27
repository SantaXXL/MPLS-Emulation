using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static NetworkNodes.NetworkNode;

namespace NetworkNodes
{
    /// <summary>
    /// Interaction logic for NetworkNode.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkNode networkNode;
        public bool ShowDetailedRoutingLogs { get; set; } = false;
        public bool ShowDetailedTTLLogs { get; set; } = false;

        public MainWindow()
        {
            // display exceptions in english
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            InitializeComponent();
            logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);

            networkNode = new NetworkNode(this);
            var args = Environment.GetCommandLineArgs();
            if (args.Length != 2)
            {
                MessageBox.Show("Wrong number of arguments!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            Task.Run(() => networkNode.Start(args[1]));
        }

        public void AddLog(string log, LogType logType)
        {
            Color backgroundColor = Colors.White;

            switch (logType)
            {
                case LogType.CantFindMatchingEntry:
                case LogType.Error:
                    backgroundColor = Color.FromRgb(222, 141, 135);
                    break;
                case LogType.Information:
                    backgroundColor = ColorFlag++ % 2 == 0 ? Colors.BurlyWood : Color.FromRgb(239, 220, 195);
                    break;
                case LogType.Received:
                    backgroundColor = Colors.LightGreen;
                    break;
                case LogType.Sent:
                    backgroundColor = Colors.Yellow;
                    break;

                default:
                    backgroundColor = Colors.White;
                    break;
            }

            Color foregroundColor = Colors.Black; // text color
            Run run = new Run(log);
            run.Foreground = new SolidColorBrush(foregroundColor);

            Paragraph paragraph = new Paragraph(run);
            paragraph.Background = new SolidColorBrush(backgroundColor);

            // if we've reached 101 blocks of text, then remove the first block
            var numberOfBlocks = logTextBox.Document.Blocks.Count;
            const int MaxNumberOfBlocks = 100;
            if (numberOfBlocks > MaxNumberOfBlocks)
            {
                logTextBox.Document.Blocks.Remove(logTextBox.Document.Blocks.FirstBlock);
            }

            logTextBox.Document.PagePadding = new Thickness(0);
            logTextBox.Document.Blocks.Add(paragraph);
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
