using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class InvalidQuestionException : Exception
	{
		public InvalidQuestionException() : base() { }
		public InvalidQuestionException(string message) : base(message) { }
		public InvalidQuestionException(string message, Exception innerException) : base(message, innerException) { }
		public InvalidQuestionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}