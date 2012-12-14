using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace pgProvider
{
	public class DDLManager
	{
		protected static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetCurrentClassLogger();
		private const string FUNCTION_EXISTS = "select null from pg_proc p where p.prorettype <> 0 and (p.pronargs = 0 or oidvectortypes(p.proargtypes) <> '') and p.proname = @procname;";
		private const string TABLE_EXISTS = "select null from pg_tables where schemaname = 'public' and tablename = @tablename;";
		private const string IS_SUPERUSER = "select null from pg_user where usename=current_user and usesuper = true;";
		protected static string _ConnectionStringName;

		internal static void ValidateVersion(string connectionStringName, string owner)
		{
			_ConnectionStringName = connectionStringName;
			//open the connection, start the transaction
			using (var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					#region v1.1
					//test up to v1.1.  This is basicially the entire original schema.
					Log.Debug(d => d("Checking for v1.1 schema..."));
					if (!TableExists("users", conn, trans))
					{
						Log.Info(i => i("The database does not seem to be compatible with v1.1.  Updating..."));
						RunScript(GetDDLResource("v1._1.InitializeSettings.sql"), null, conn, trans);
						RunScript(GetDDLResource("v1._1.Tables.sql"), null, conn, trans);
						RunScript(GetDDLResource("v1._1.Types.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_all_users.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_users_by_email.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_users_by_username.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_users_online.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_online_count.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_user_by_username.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_user_by_id.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.create_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.create_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.delete_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.delete_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_user_credentials.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_user_name_by_email.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.record_login_event.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.unlock_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.update_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.update_user_password.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.update_user_q_and_a.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_users_in_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_roles_for_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.get_all_roles.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.user_is_in_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.remove_users_from_roles.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.role_exists.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._1.assign_users_to_roles.sql"), null, conn, trans);
					}
					#endregion

					#region v1.2
					//test up to v1.2.  This adds the purge activity feature, for cleaning up old data.
					Log.Debug(d => d("Checking for v1.2 schema..."));
					if (!FunctionExists("purge_activity", conn, trans))
					{
						Log.Info(i => i("The database does not seem to be compatible with v1.2.  Updating..."));
						RunStatement(GetDDLResource("v1._2.purge_activity.sql"), null, conn, trans);
					}
					#endregion

					#region v1.3
					//test up to v1.3.  This makes users, roles, and applications case-insensitive.
					//also adds the version table to check against future versions.
					Log.Debug(d => d("Checking for v1.3 schema..."));
					if (!TableExists("versions", conn, trans))
					{
						Log.Info(i => i("The database does not seem to be compatible with v1.3.  Updating..."));
						RunScript(GetDDLResource("v1._3.TableChanges.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.assign_users_to_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.create_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.create_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.delete_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.delete_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_all_roles.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_all_users.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_online_count.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_roles_for_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_user_by_username.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_user_credentials.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_user_name_by_email.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_users_by_email.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_users_by_username.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_users_in_role.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.get_users_online.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.record_login_event.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.role_exists.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.remove_users_from_roles.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.unlock_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.update_user.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.update_user_password.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.update_user_q_and_a.sql"), null, conn, trans);
						RunStatement(GetDDLResource("v1._3.user_is_in_role.sql"), null, conn, trans);
					}

					#endregion

					/*
					 * Other checks and updates will go here.
					 * 
					 * 
					 */

					trans.Commit();
				}
			}
		}
		protected static bool FunctionExists(string functionName, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
		{
			using (var cmd = new Npgsql.NpgsqlCommand(FUNCTION_EXISTS, conn, trans))
			{
				cmd.Parameters.Add("@procname", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = functionName;
				using (var r = cmd.ExecuteReader())
				{
					return r.HasRows;
				}
			}
		}
		protected static bool FunctionExists(string functionName)
		{
			using (var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					var value = FunctionExists(functionName, conn, trans);
					trans.Commit();
					return value;
				}
			}
		}
		protected static bool TableExists(string tableName, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
		{
			using (var cmd = new Npgsql.NpgsqlCommand(TABLE_EXISTS, conn, trans))
			{
				cmd.Parameters.Add("@tableName", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = tableName;
				using (var r = cmd.ExecuteReader())
				{
					return r.HasRows;
				}
			}
		}
		protected static bool TableExists(string tableName)
		{
			using (var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					var value = TableExists(tableName, conn, trans);
					trans.Commit();
					return value;
				}
			}
		}
		protected static bool IsSuperUser()
		{
			using (var conn = new Npgsql.NpgsqlConnection(ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					var value = IsSuperUser(conn, trans);
					trans.Commit();
					return value;
				}
			}
		}
		protected static bool IsSuperUser(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
		{
			using (var cmd = new Npgsql.NpgsqlCommand(IS_SUPERUSER, conn, trans))
			{
				using (var r = cmd.ExecuteReader())
				{
					return r.HasRows;
				}
			}
		}
		protected static string GetDDLResource(string resourceName)
		{
			var fqResourceName = string.Format("pgProvider.ddl.{0}", resourceName);
			var assy = typeof(DDLManager).Assembly;
			Log.Debug(d => d("Collecting resource '{0}'...", fqResourceName));

			using (var sr = new System.IO.StreamReader(assy.GetManifestResourceStream(fqResourceName)))
			{
				var value = sr.ReadToEnd();
				return value;
			}
		}
		protected static void RunScript(string script, IDictionary<string, Npgsql.NpgsqlParameter> parameters, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
		{
			if (parameters == null) parameters = new Dictionary<string, Npgsql.NpgsqlParameter>();

			foreach (var commandText in script.Split(';'))
			{
				var sql = commandText + ";";
				Log.Debug(d => d("Script command: {0}", sql));

				using (var cmd = new Npgsql.NpgsqlCommand(sql, conn, trans))
				{
					foreach (var parameter in parameters.Keys)
					{
						if (sql.Contains(parameter))
						{
							var value = parameters[parameter];
							Log.Debug(d => d("The command contains the parameter '{0}', setting value to '{1}'...", parameter, value.Value));
							cmd.Parameters.Add(value);
						}
					}
					cmd.ExecuteNonQuery();
				}
			}
			Log.Debug(d => d("Script complete."));
		}
		protected static void RunStatement(string statement, IDictionary<string, Npgsql.NpgsqlParameter> parameters, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
		{
			if (parameters == null) parameters = new Dictionary<string, Npgsql.NpgsqlParameter>();
			using (var cmd = new Npgsql.NpgsqlCommand(statement, conn, trans))
			{
				foreach (var parameter in parameters.Keys)
				{
					if (statement.Contains(parameter))
					{
						var value = parameters[parameter];
						Log.Debug(d => d("The command contains the parameter '{0}', setting value to '{1}'...", parameter, value.Value));
						cmd.Parameters.Add(value);
					}
				}
				cmd.ExecuteNonQuery();
			}
		}
		protected static void NeedsSuperUser()
		{
			if (!IsSuperUser())
			{
				throw new pgProvider.Exceptions.SuperUserPermissionsRequiredException(
					string.Format("The database schema is out of date, but the current credentials do not have the superuser access required to update the schema."));
			}
		}
	}
}
