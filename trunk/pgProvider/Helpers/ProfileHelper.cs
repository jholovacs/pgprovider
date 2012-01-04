using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using Npgsql;
using log4net;
using System.Data;

namespace pgProvider
{
	public class ProfileHelper
	{
		protected static readonly ProfileConfigurationSection Configuration =
			(ProfileConfigurationSection)System.Configuration.ConfigurationManager.GetSection("profileConfiguration");
		protected static readonly string ConnectionString = ConfigurationManager.ConnectionStrings[Configuration.ConnectionStringName].ConnectionString;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ProfileHelper));
		protected static readonly pgSettingsProvider settingsProvider = new pgSettingsProvider();

		//public static object this[string propertyName]
		//{
		//    get
		//    {
		//        var profile = GetProfile();
		//        if (profile.ContainsKey(propertyName)) return profile[propertyName];
		//        return null;
		//    }
		//    set
		//    {
		//        var profile = GetProfile();
		//        profile[propertyName].PropertyValue = value;
		//        PersistProfile(profile);
		//    }
		//}

		protected static System.Web.HttpContext GetCurrentContext()
		{
			return System.Web.HttpContext.Current;
		}

		protected static IDictionary<string, SettingsPropertyValue> GetProfile()
		{
			var context = GetCurrentContext();
			if (context == null) return null;
			if (context.Session["profileInfo"] == null || Configuration.AlwaysGetFromDataStore)
			{
				context.Session["profileInfo"] = CollectPersistedProfile(context);
			}
			var profile = ((IDictionary<string, SettingsPropertyValue>)context.Session["profileInfo"]);
			return profile;
		}

		protected static IDictionary<string, SettingsPropertyValue> CollectPersistedProfile(HttpContext context)
		{
			if (!context.User.Identity.IsAuthenticated)
			{
				Log.Debug("The user is not authenticated; no profile is persisted.");
				return new Dictionary<string, SettingsPropertyValue>();
			}

			using (var conn = new NpgsqlConnection(ConnectionString))
			{
				conn.Open();
				using (var comm = new NpgsqlCommand("get_user_profile"))
				{
					comm.CommandType = System.Data.CommandType.StoredProcedure;
					comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = context.User.Identity.Name;
					comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = Configuration.ApplicationName;
					using (var dr = comm.ExecuteReader())
					{
						return GetProfileFromReader(dr);
					}
				}
			}

		}

		protected static IDictionary<string, SettingsPropertyValue> GetProfileFromReader(IDataReader dr)
		{
			if (dr == null) throw new ArgumentNullException();
			var propertyNameColumn = dr.GetOrdinal("property_name");
			var propertyTypeColumn = dr.GetOrdinal("property_type");
			var propertyValueColumn = dr.GetOrdinal("property_value");

			var r = new Dictionary<string, SettingsPropertyValue>();
			while (dr.Read())
			{
				//var spv = new SettingsPropertyValue(
				//    new SettingsProperty(dr.GetString(propertyNameColumn), Type.GetType(dr.GetString(propertyTypeColumn)), 
			}

			//todo: finish this.
			return null;
		}

		protected static void PersistProfile(IDictionary<string, SettingsPropertyValue> profile)
		{

		}
	}
}
