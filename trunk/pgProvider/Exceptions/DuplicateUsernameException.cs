using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class DuplicateUsernameException : Exception
	{
		public DuplicateUsernameException() : base() { }
		public DuplicateUsernameException(string message) : base(message) { }
		public DuplicateUsernameException(string message, Exception innerException) : base(message, innerException) { }
		public DuplicateUsernameException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
