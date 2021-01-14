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
        private ToolStrips strips;

        public App()
        {
            notifyIcon = new NotifyIcon
            {
                // Todo: Add Icon here
                Icon = new Icon(@"C:\Users\Jesse\Desktop\dev\icons\Logger.ico"),
                Text = "Activities",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };

            var logger = new Logger();

            logger.OnInitializedLogger += OnLoggerInitialized;
            logger.OnInitializedLoggerFailed += OnLoggerInitializedError;

            Task.Run(logger.InitializeLogger);
        }

        private void OnLoggerInitialized(object sender, InitializedLoggerEventArgs args)
        {
            strips = new ToolStrips
            {
                RestartLogger = new ToolStripMenuItem("Restart", null, Restart),
                ExitLogger = new ToolStripMenuItem("Exit", null, Exit),

                StartFirebaseClient = new ToolStripMenuItem("Start Firebase Client", null, StartFirebaseClient),
                RestartFirebaseClient = new ToolStripMenuItem("Restart Firebase Client", null, RestartFirebaseClient),
                ExitFirebaseClient = new ToolStripMenuItem("Stop Firebase Client", null, ExitFirebaseClient),
            };

            notifyIcon.ContextMenuStrip.Items.Add(strips.RestartLogger);
            notifyIcon.ContextMenuStrip.Items.Add(strips.ExitLogger);

            if (args.logger.FirebaseClient == null)
            {
                notifyIcon.ContextMenuStrip.Items.Add(strips.StartFirebaseClient);
                notifyIcon.ContextMenuStrip.Update();
                return;
            }

            notifyIcon.ContextMenuStrip.Items.Add(strips.RestartFirebaseClient);
            notifyIcon.ContextMenuStrip.Items.Add(strips.ExitFirebaseClient);

            notifyIcon.ContextMenuStrip.Update();
        }

        private void OnLoggerInitializedError(object sender, InitializedLoggerFailedEventArgs args)
        {
            Console.WriteLine(args.ExceptionMessage);
        }

        protected override void OnLoad(EventArgs e)
        {
            ShowInTaskbar = false;
            Visible = false;
            Opacity = 0;
            base.OnLoad(e);
        }

        private void Restart(object sender, EventArgs args)
        {
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripLabel("ahh"));
            // Todo: Restart App here
        }

        private void Exit(object sender, EventArgs args)
        {
            // Todo: End App here
        }

        private void ExitFirebaseClient(object sender, EventArgs args)
        {
            // Todo: Close Firebase Client
        }

        private void RestartFirebaseClient(object sender, EventArgs args)
        {
            // Todo: Restart Firebase Client
        }

        private void StartFirebaseClient(object sender, EventArgs args)
        {
            // Todo: Start Firebase Client
        }

        private class ToolStrips
        {
            // Activity Logger
            public ToolStripMenuItem RestartLogger { get; set; }
            public ToolStripMenuItem ExitLogger { get; set; }

            // FirebaseClient
            public ToolStripMenuItem StartFirebaseClient { get; set; }
            public ToolStripMenuItem RestartFirebaseClient { get; set; }
            public ToolStripMenuItem ExitFirebaseClient { get; set; }

            public ToolStrips(ToolStripMenuItem restartLogger,
                ToolStripMenuItem exitLogger,
                ToolStripMenuItem startFirebaseClient,
                ToolStripMenuItem restartFirebaseClient,
                ToolStripMenuItem exitFirebaseClient)
            {
                RestartLogger = restartLogger;
                ExitLogger = exitLogger;
                StartFirebaseClient = startFirebaseClient;
                RestartFirebaseClient = restartFirebaseClient;
                ExitFirebaseClient = exitFirebaseClient;
            }

            public ToolStrips()
            {
            }
        }
    }
}