using Abp.Logging;
using Abp.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CharonX.ResulFilter
{
    public class AppUserFriendlyException : UserFriendlyException
    {
        public AppUserFriendlyException()
        {
        }

        public AppUserFriendlyException(string message) : base(message)
        {
        }

        public AppUserFriendlyException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }

        public AppUserFriendlyException(string message, LogSeverity severity) : base(message, severity)
        {
        }

        public AppUserFriendlyException(int code, string message) : base(code, message)
        {
        }

        public AppUserFriendlyException(string message, string details) : base(message, details)
        {
        }

        public AppUserFriendlyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AppUserFriendlyException(int code, string message, string details) : base(code, message, details)
        {
        }

        public AppUserFriendlyException(string message, string details, Exception innerException) : base(message, details, innerException)
        {
        }
    }
}
