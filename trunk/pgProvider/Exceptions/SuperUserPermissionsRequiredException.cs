using System;

namespace pgProvider.Exceptions
{
	public class SuperUserPermissionsRequiredException : Exception
	{
		public SuperUserPermissionsRequiredException() : base() { }
		public SuperUserPermissionsRequiredException(string message) : base(message) { }
		public SuperUserPermissionsRequiredException(string message, Exception innerException) : base(message, innerException) { }
	}
}
