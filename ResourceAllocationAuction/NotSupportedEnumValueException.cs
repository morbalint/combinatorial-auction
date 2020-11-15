using System;

namespace ResourceAllocationAuction
{
    [Serializable]
    public class NotSupportedEnumValueException<T> : NotSupportedException
        where T : struct, Enum
    {
        public NotSupportedEnumValueException()
        {
        }

        public NotSupportedEnumValueException(T unsupportedValue) : base(getMessage(unsupportedValue))
        {
        }

        public NotSupportedEnumValueException(string message) : base(message)
        {
        }

        public NotSupportedEnumValueException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotSupportedEnumValueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        private static string getMessage(T unsupportedValue)
        {
            var validValues = Enum.GetNames(typeof(T));
            return $"Enum({typeof(T).Name}) does not support {unsupportedValue}. Only {string.Join(',', validValues)} are supported.";
        }
    }
}
