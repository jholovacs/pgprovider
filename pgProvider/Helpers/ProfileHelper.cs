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


		protected static System.Web.HttpContext GetCurrentContext()
		{
			return System.Web.HttpContext.Current;
		}

		protected static IDictionary<string, string> GetProfile()
		{
			var context = GetCurrentContext();
			if (context == null) return null;
			if (context.Session["profileInfo"] == null || Configuration.AlwaysGetFromDataStore)
			{
				context.Session["profileInfo"] = CollectPersistedProfile(context);
			}
			var profile = ((IDictionary<string, string>)context.Session["profileInfo"]);
			return profile;
		}

		protected static IDictionary<string, string> CollectPersistedProfile(HttpContext context)
		{
			if (!context.User.Identity.IsAuthenticated)
			{
				Log.Debug("The user is not authenticated; no profile is persisted.");
				return new Dictionary<string, string>();
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

		protected static IDictionary<string, string> GetProfileFromReader(IDataReader dr)
		{
			if (dr == null) throw new ArgumentNullException();
			var propertyNameColumn = dr.GetOrdinal("property_name");
			var propertyValueColumn = dr.GetOrdinal("property_value");

			var r = new Dictionary<string, string>();
			while (dr.Read())
			{
				if (dr.IsDBNull(propertyValueColumn))
				{
					r.Add(dr.GetString(propertyNameColumn), null);
				}
				else
				{
					r.Add(dr.GetString(propertyNameColumn), dr.GetString(propertyValueColumn));
				}
			}
			return r;
		}

		protected static void PersistProfile(IDictionary<string, string> profile)
		{

		}
	}
}
