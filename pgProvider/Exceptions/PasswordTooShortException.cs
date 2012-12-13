using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class PasswordTooShortException : Exception
	{
		public PasswordTooShortException() : base() { }
		public PasswordTooShortException(string message) : base(message) { }
		public PasswordTooShortException(string message, Exception innerException) : base(message, innerException) { }
		public PasswordTooShortException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
