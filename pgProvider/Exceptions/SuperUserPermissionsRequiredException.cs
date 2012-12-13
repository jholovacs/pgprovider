using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class SuperUserPermissionsRequiredException : Exception
	{
		public SuperUserPermissionsRequiredException() : base() { }
		public SuperUserPermissionsRequiredException(string message) : base(message) { }
		public SuperUserPermissionsRequiredException(string message, Exception innerException) : base(message, innerException) { }
		public SuperUserPermissionsRequiredException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
