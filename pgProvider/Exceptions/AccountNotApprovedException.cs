using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class AccountNotApprovedException : Exception
	{
		public AccountNotApprovedException() : base() { }
		public AccountNotApprovedException(string message) : base(message) { }
		public AccountNotApprovedException(string message, Exception innerException) : base(message, innerException) { }
		public AccountNotApprovedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
