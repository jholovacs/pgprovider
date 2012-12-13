using System;

namespace pgProvider.Exceptions
{
	[Serializable]
	public class ProviderConfigurationException:Exception
	{
		public ProviderConfigurationException() : base() { }
		public ProviderConfigurationException(string Message) : base(Message) { }
		public ProviderConfigurationException(string Message, Exception ex) : base(Message, ex) { }
		public ProviderConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
