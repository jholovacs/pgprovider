using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class InvalidAnswerException : Exception
	{
		public InvalidAnswerException() : base() { }
		public InvalidAnswerException(string message) : base(message) { }
		public InvalidAnswerException(string message, Exception innerException) : base(message, innerException) { }
		public InvalidAnswerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}