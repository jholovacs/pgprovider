using System;
using System.Web.Security;
using log4net;
using System.Configuration;
using pgProvider.Exceptions;
using Npgsql;

namespace pgProvider
{
	public class pgRoleProvider:RoleProvider
	{
		protected static readonly ILog Log = LogManager.GetLogger(typeof(pgRoleProvider));
		protected string _Name = string.Empty;
		protected string _ConnectionStringName = "pgProvider";
		protected string _ApplicationName = string.Empty;
		protected string ConnectionString = string.Empty;

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			try
			{
				Log.Debug("pgRoleProvider Initialize() invoked.");

				_ConnectionStringName = config["connectionStringName"] ?? _ConnectionStringName;
				Log.Debug(string.Format("_ConnectionStringName: {0}", _ConnectionStringName));

				_ApplicationName = config["applicationName"] ?? _ApplicationName;
				Log.Debug(string.Format("_ApplicationName: {0}", _ApplicationName));

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
			catch (Exception ex)
			{
				var message = "Error initializing the role configuration settings";
				Log.Error(message, ex);
				throw new ProviderConfigurationException(message, ex);
			}
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			Log.Debug(string.Format("Adding users ({0}) to roles ({1})...", string.Join(", ", usernames), string.Join(", ", roleNames)));
			using (var conn = new NpgsqlConnection(ConnectionString))
			{
				conn.Open();
				using (var comm = new NpgsqlCommand("assign_users_to_roles", conn))
				{
					comm.CommandType = System.Data.CommandType.StoredProcedure;
					comm.Parameters.Add("users", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, 250).Value = usernames;
					comm.Parameters.Add("roles", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, 250).Value = roleNames;
					comm.ExecuteNonQuery();
				}
			}
			Log.Info(string.Format("Added users ({0}) to roles ({1}).", string.Join(", ", usernames), string.Join(", ", roleNames)));
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

		public override void CreateRole(string roleName)
		{
			Log.Debug(string.Format("Creating role '{0}'...", roleName));
			using (var conn = new NpgsqlConnection(ConnectionString))
			{
				conn.Open();
				using (var comm = new NpgsqlCommand("create_role", conn))
				{
					comm.CommandType = System.Data.CommandType.StoredProcedure;
					comm.Parameters.Add("role_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.Parameters.Add("role_description", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
					comm.ExecuteNonQuery();
				}
			}
			Log.Debug(string.Format("Created role '{0}'...", roleName));
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		public override string[] GetAllRoles()
		{
			throw new NotImplementedException();
		}

		public override string[] GetRolesForUser(string username)
		{
			throw new NotImplementedException();
		}

		public override string[] GetUsersInRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool IsUserInRole(string username, string roleName)
		{
			throw new NotImplementedException();
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override bool RoleExists(string roleName)
		{
			throw new NotImplementedException();
		}
		public override string Name
		{
			get
			{
				return _Name;
			}
		}
		public override string Description
		{
			get
			{
				return "PostgreSQL ASP.Net Role Provider class";
			}
		}
	}
}
