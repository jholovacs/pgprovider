using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Profile;
using Npgsql;
using pgProvider.Exceptions;
using System.Configuration;
using log4net;

namespace pgProvider
{
	public class pgProfileProvider:ProfileProvider
	{
		protected string _Name = string.Empty;
		protected string _ApplicationName = string.Empty;
		protected string _ConnectionStringName = "pgProvider";
		protected string ConnectionString = string.Empty;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(pgProfileProvider));


		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			_Name = name ?? _Name;

			_ApplicationName = config["applicationName"] ?? _ApplicationName;
			Log.Debug(string.Format("_ApplicationName: {0}", _ApplicationName));

			_ConnectionStringName = config["connectionStringName"] ?? _ConnectionStringName;
			Log.Debug(string.Format("_ConnectionStringName: {0}", _ConnectionStringName));






			#region validate database config and connectivity
			Log.Debug("Checking to make sure the specified connection string exists...");
			var cs = ConfigurationManager.ConnectionStrings[_ConnectionStringName];
			if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
			{
				throw new ProviderConfigurationException(
					string.Format("The membership provider connection string, '{0}', is not defined.", _ConnectionStringName));
			}

			ConnectionString = ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString;
			Log.Debug(string.Format("ConnectionString: {0}", ConnectionString));

			Log.Debug("Checking to make sure the specified connection string can connect...");
			using (var conn = new NpgsqlConnection(ConnectionString))
			{
				conn.Open();
				using (var comm = new NpgsqlCommand("select 1", conn))
				{
					comm.CommandType = System.Data.CommandType.Text;
					comm.ExecuteNonQuery();
				}
			}
			#endregion


		}

		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			var option = string.Empty;
			switch (authenticationOption)
			{
				case ProfileAuthenticationOption.Anonymous:
					option = "anonymous";
					break;
				case ProfileAuthenticationOption.Authenticated:
					option = "authenticated";
					break;
				case ProfileAuthenticationOption.All:
					option = "all";
					break;
				default:
					throw new ArgumentException("authenticationOption");
			}


			//todo: finish.
			return 0;
		}

		public override int DeleteProfiles(string[] usernames)
		{
			throw new NotImplementedException();
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException();
		}

		public override string ApplicationName
		{
			get
			{
				return _ApplicationName;
			}
			set
			{
				_ApplicationName = value;
			}
		}

		public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection collection)
		{
			throw new NotImplementedException();
		}

		public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection collection)
		{
			throw new NotImplementedException();
		}
	}
}
