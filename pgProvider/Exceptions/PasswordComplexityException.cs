using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class PasswordComplexityException : Exception
	{
		public PasswordComplexityException() : base() { }
		public PasswordComplexityException(string message) : base(message) { }
		public PasswordComplexityException(string message, Exception innerException) : base(message, innerException) { }
		public PasswordComplexityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
