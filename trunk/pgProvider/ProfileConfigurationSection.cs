using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace pgProvider
{
	public class ProfileConfigurationSection:ConfigurationSection
	{
		[ConfigurationProperty("connectionStringName", DefaultValue = "pgProvider", IsRequired = false)]
		public string ConnectionStringName
		{
			get { return (string)this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
		}

		[ConfigurationProperty("applicationName", DefaultValue = "", IsRequired = false)]
		public string ApplicationName
		{
			get { return (string)this["applicationName"]; }
			set { this["applicationName"] = value; }
		}

		[ConfigurationProperty("persistOnSet", DefaultValue = "false", IsRequired = false)]
		public bool PersistOnSet
		{
			get { return Convert.ToBoolean(this["persistOnSet"]); }
			set { this["persistOnSet"] = value.ToString(); }
		}

		[ConfigurationProperty("alwaysGetFromDataStore", DefaultValue = "false", IsRequired = false)]
		public bool AlwaysGetFromDataStore
		{
			get { return Convert.ToBoolean(this["alwaysGetFromDataStore"]); }
			set { this["alwaysGetFromDataStore"] = value.ToString(); }
		}


	}
}
