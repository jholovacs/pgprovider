using System;

namespace pgProvider.Exceptions
{
	public class ProviderConfigurationException:Exception
	{
		public ProviderConfigurationException() : base() { }
		public ProviderConfigurationException(string Message) : base(Message) { }
		public ProviderConfigurationException(string Message, Exception ex) : base(Message, ex) { }
	}
}
