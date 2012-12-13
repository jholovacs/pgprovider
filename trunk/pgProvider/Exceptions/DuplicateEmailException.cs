using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class DuplicateEmailException : Exception
	{
		public DuplicateEmailException() : base() { }
		public DuplicateEmailException(string message) : base(message) { }
		public DuplicateEmailException(string message, Exception innerException) : base(message, innerException) { }
		public DuplicateEmailException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
