using System.Windows;

namespace TCPSample {

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

            Prepare(@"./Setting.config");
            TestMethodEntry();
        }

        private SAXWrapper.SettingReader setting;

        private Logger.LogSpooler log;

        private TCPInfrastructure.Client client;

        private TCPInfrastructure.Dispatcher dispatcher;

        private void Prepare(string fileName) {
            setting = new SAXWrapper.SettingReader();
            setting.SetDirectory(System.IO.Path.GetDirectoryName(fileName));
            setting.SetFileName(System.IO.Path.GetFileName(fileName));
            setting.Parse();
            log = new Logger.LogSpooler();
            log.SetSafe(60);
            log.Start();
        }

        private void TestMethodEntry() {
            if (setting.GetNode().SubCategory(@"Work") != null && setting.GetNode().SubCategory(@"Work").Attr(@"As").GetNodeValue().Equals(@"Client")) {
                client = new TCPInfrastructure.Client(@"./Setting.config", @"ClientDefault");
                client.SetLog(log);
                client.Start();
            } else {
                dispatcher = new TCPInfrastructure.Dispatcher(@"./Setting.config", @"DispatcherDefault");
                dispatcher.SetLog(log);
                dispatcher.Start();
            }
        }
    }
}