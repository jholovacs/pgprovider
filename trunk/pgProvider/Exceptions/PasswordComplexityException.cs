using System;

namespace pgProvider.Exceptions
{
	public class PasswordComplexityException : Exception
	{
		public PasswordComplexityException() : base() { }
		public PasswordComplexityException(string message) : base(message) { }
	}
}
