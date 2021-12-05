﻿using System;
 using System.Drawing;
 using System.Threading;
 using System.Threading.Tasks;
 using System.Windows.Forms;
 using ActivityLogger.events;

 namespace ActivityLogger
{
    public class App : Form
    {
        private readonly NotifyIcon notifyIcon;
        private readonly ToolStrips strips;
        private readonly Logger logger;

        public App()
        {
            this.WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon($@"{Logger.ActivityLoggerPath}\icon.ico"),
                Text = "Activities",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };

            strips = new ToolStrips
            {
                RestartLogger = new ToolStripMenuItem("Restart", null, Restart),
                ExitLogger = new ToolStripMenuItem("Exit", null, Exit),

                StartFirebaseClient = new ToolStripMenuItem("Start Firebase Client", null, StartFirebaseClient),
                RestartFirebaseClient = new ToolStripMenuItem("Restart Firebase Client", null, RestartFirebaseClient),
                ExitFirebaseClient = new ToolStripMenuItem("Stop Firebase Client", null, ExitFirebaseClient),
            };

            logger = new Logger();

            logger.OnInitializedLogger += OnLoggerInitialized;
            logger.OnInitializedLoggerFailed += OnLoggerInitializedError;

            notifyIcon.Click += (sender, _) => ReloadNotifyIcon();

            Task.Run(logger.InitializeLogger);
        }

        private void OnLoggerInitialized(object sender, InitializedLoggerEventArgs args) => ReloadNotifyIcon();

        private void ReloadNotifyIcon()
        {
            notifyIcon.ContextMenuStrip.Items.Clear();

            notifyIcon.ContextMenuStrip.Items.Add(strips.RestartLogger);
            notifyIcon.ContextMenuStrip.Items.Add(strips.ExitLogger);

            if (logger.FirebaseClient == null || logger.FirebaseClient.HasExited)
            {
                notifyIcon.ContextMenuStrip.Items.Add(strips.StartFirebaseClient);
                notifyIcon.ContextMenuStrip.Update();
                return;
            }

            notifyIcon.ContextMenuStrip.Items.Add(strips.RestartFirebaseClient);
            notifyIcon.ContextMenuStrip.Items.Add(strips.ExitFirebaseClient);

            notifyIcon.ContextMenuStrip.Update();
        }

        private static void OnLoggerInitializedError(object sender, InitializedLoggerFailedEventArgs args) => Application.Exit();

        private void Restart(object sender, EventArgs args) => logger.Restart();

        private void Exit(object sender, EventArgs args) {
            logger.ExitFirebaseClient();
            Application.Exit();
        }

        private void ExitFirebaseClient(object sender, EventArgs args) => logger.ExitFirebaseClient();

        private void RestartFirebaseClient(object sender, EventArgs args)
        {
            logger.ExitFirebaseClient();
            logger.LaunchFirebaseClient();
        }

        private void StartFirebaseClient(object sender, EventArgs args) => logger.LaunchFirebaseClient();

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