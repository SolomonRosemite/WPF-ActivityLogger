using System;

namespace ActivityLogger.events
{
    public class InitializedLoggerEventArgs : EventArgs
    {
        public Logger logger { get; set; }
    }
}