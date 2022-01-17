using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ActivityLogger.events;

 namespace ActivityLogger
{
    public class App : Form
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ToolStrips _strips;
        private readonly Logger _logger;

        public App()
        {
            this.WindowState = FormWindowState.Minimized;
            
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon($@"{Logger.ActivityLoggerPath}\icon.ico"),
                Text = "Activities",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };

            _strips = new ToolStrips
            {
                RestartLogger = new ToolStripMenuItem("Restart", null, Restart),
                ExitLogger = new ToolStripMenuItem("Exit", null, Exit),

                StartFirebaseClient = new ToolStripMenuItem("Start Firebase Client", null, StartFirebaseClient),
                RestartFirebaseClient = new ToolStripMenuItem("Restart Firebase Client", null, RestartFirebaseClient),
                ExitFirebaseClient = new ToolStripMenuItem("Stop Firebase Client", null, ExitFirebaseClient),
            };

            _logger = new Logger();

            _logger.OnInitializedLogger += OnLoggerInitialized;
            _logger.OnInitializedLoggerFailed += OnLoggerInitializedError;

            _notifyIcon.Click += (sender, _) => ReloadNotifyIcon();

            Task.Run(_logger.InitializeLogger);
        }

        protected override void OnLoad(EventArgs e)
        {
            ShowInTaskbar = false;
            Visible = false;
            Opacity = 0;
            base.OnLoad(e);
        }
        
        private void OnLoggerInitialized(object sender, InitializedLoggerEventArgs args) => ReloadNotifyIcon();

        private void ReloadNotifyIcon()
        {
            _notifyIcon.ContextMenuStrip.Items.Clear();

            _notifyIcon.ContextMenuStrip.Items.Add(_strips.RestartLogger);
            _notifyIcon.ContextMenuStrip.Items.Add(_strips.ExitLogger);

            if (_logger.FirebaseClient == null || _logger.FirebaseClient.HasExited)
            {
                _notifyIcon.ContextMenuStrip.Items.Add(_strips.StartFirebaseClient);
                _notifyIcon.ContextMenuStrip.Update();
                return;
            }

            _notifyIcon.ContextMenuStrip.Items.Add(_strips.RestartFirebaseClient);
            _notifyIcon.ContextMenuStrip.Items.Add(_strips.ExitFirebaseClient);

            _notifyIcon.ContextMenuStrip.Update();
        }

        private static void OnLoggerInitializedError(object sender, InitializedLoggerFailedEventArgs args) => Application.Exit();

        private void Restart(object sender, EventArgs args) => _logger.Restart();

        private void Exit(object sender, EventArgs args) {
            _logger.ExitFirebaseClient();
            Application.Exit();
        }

        private void ExitFirebaseClient(object sender, EventArgs args) => _logger.ExitFirebaseClient();

        private void RestartFirebaseClient(object sender, EventArgs args)
        {
            _logger.ExitFirebaseClient();
            _logger.LaunchFirebaseClient();
        }

        private void StartFirebaseClient(object sender, EventArgs args) => _logger.LaunchFirebaseClient();

        private class ToolStrips
        {
            // Activity Logger
            public ToolStripMenuItem RestartLogger { get; set; }
            public ToolStripMenuItem ExitLogger { get; set; }

            // FirebaseClient
            public ToolStripMenuItem StartFirebaseClient { get; set; }
            public ToolStripMenuItem RestartFirebaseClient { get; set; }
            public ToolStripMenuItem ExitFirebaseClient { get; set; }
        }
    }
}