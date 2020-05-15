using Abp.Logging;
using Abp.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CharonX.ResulFilter
{
    public class CustomUserFriendlyException : UserFriendlyException
    {
        public CustomUserFriendlyException()
        {
        }

        public CustomUserFriendlyException(string message) : base(message)
        {
        }

        public CustomUserFriendlyException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {
        }

        public CustomUserFriendlyException(string message, LogSeverity severity) : base(message, severity)
        {
        }

        public CustomUserFriendlyException(int code, string message) : base(code, message)
        {
        }

        public CustomUserFriendlyException(string message, string details) : base(message, details)
        {
        }

        public CustomUserFriendlyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CustomUserFriendlyException(int code, string message, string details) : base(code, message, details)
        {
        }

        public CustomUserFriendlyException(string message, string details, Exception innerException) : base(message, details, innerException)
        {
        }
    }
}
