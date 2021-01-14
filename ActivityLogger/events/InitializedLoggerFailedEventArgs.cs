using System;

namespace ActivityLogger.events
{
    public class InitializedLoggerFailedEventArgs : EventArgs
    {
        public Exception ExceptionMessage { get; set; }
    }
}