using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class AccountLockedException : Exception
	{
		public AccountLockedException() : base() { }
		public AccountLockedException(string message) : base(message) { }
		public AccountLockedException(string message, Exception innerException) : base(message, innerException) { }
		public AccountLockedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
