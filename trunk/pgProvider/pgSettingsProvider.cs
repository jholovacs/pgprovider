using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace pgProvider
{
	public class pgSettingsProvider:SettingsProvider
	{
		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			throw new NotImplementedException();
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			throw new NotImplementedException();
		}
	}
}
