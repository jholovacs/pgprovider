using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace pgProvider
{
    public class DDLManager
    {
        private static readonly Common.Logging.ILog _logger = Common.Logging.LogManager.GetLogger<DDLManager>();
        private const string FUNCTION_EXISTS = "select count(*) from pg_proc p where p.prorettype <> 0 and (p.pronargs = 0 or oidvectortypes(p.proargtypes) <> '') and p.proname = @procname;";
        private const string TABLE_EXISTS = "select count(*) from pg_tables where schemaname = 'public' and tablename = @tablename;";
        private const string IS_SUPERUSER = "select count(*) from pg_user where usename=current_user and usesuper = true;";
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
                    _logger.Debug(d => d("Checking for v1.1 schema..."));
                    if (!TableExists("users", conn, trans))
                    {
                        _logger.Info(i => i("The database does not seem to be compatible with v1.1.  Updating..."));
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
                    _logger.Debug(d => d("Checking for v1.2 schema..."));
                    if (!FunctionExists("purge_activity", conn, trans))
                    {
                        _logger.Info(i => i("The database does not seem to be compatible with v1.2.  Updating..."));
                        RunStatement(GetDDLResource("v1._2.purge_activity.sql"), null, conn, trans);
                    }
                    #endregion

                    #region v1.3
                    //test up to v1.3.  This makes users, roles, and applications case-insensitive.
                    //also adds the version table to check against future versions.
                    _logger.Debug(d => d("Checking for v1.3 schema..."));
                    if (!TableExists("versions", conn, trans))
                    {
                        _logger.Info(i => i("The database does not seem to be compatible with v1.3.  Updating..."));
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

                    //v1.3 is the first version that includes version tracking in the database.
                    var currentVersion = GetDBVersion(conn, trans);

                    #region v1.4
                    _logger.Debug(d => d("Checking for v1.4 schema (no DDL action required)..."));
                    if (currentVersion == "1.3") currentVersion = "1.4";
                    #endregion

                    #region v1.5
                    _logger.Debug(d => d("Checking for v1.5 schema (no DDL action required)..."));
                    if (currentVersion == "1.4") currentVersion = "1.5";
                    #endregion

                    #region v1.6
                    if (currentVersion == "1.5")
                    {
                        //v1.6 fixes an update problem where the last login time was not being persisted.
                        RunStatement(GetDDLResource("v1._6.record_login_event.sql"), null, conn, trans);
                        currentVersion = "1.6";
                    }
                    #endregion

                    #region v1.7

                    if (currentVersion == "1.6")
                    {
                        RunStatement(GetDDLResource("v1._7.get_number_of_users_online.sql"), null, conn, trans);
                        RunStatement(GetDDLResource("v1._7.get_users_online.sql"), null, conn, trans);
                        currentVersion = "1.7";
                    }
                    #endregion

                    /*
					 * Other checks and updates will go here.
					 * 
					 * 
					 */

                    SetDBVersion(conn, trans, currentVersion);
                    trans.Commit();
                }
            }
        }
        protected static bool FunctionExists(string functionName, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            using (var cmd = new Npgsql.NpgsqlCommand(FUNCTION_EXISTS, conn, trans))
            {
                cmd.Parameters.Add("@procname", NpgsqlTypes.NpgsqlDbType.Varchar, 255).Value = functionName;

                return HasRows(cmd);
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
                return HasRows(cmd);
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
                return HasRows(cmd);
            }
        }
        protected static string GetDDLResource(string resourceName)
        {
            var fqResourceName = string.Format("pgProvider.ddl.{0}", resourceName);
            var assy = typeof(DDLManager).Assembly;
            _logger.Debug(d => d("Collecting resource '{0}'...", fqResourceName));

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
                _logger.Debug(d => d("Script command: {0}", sql));

                using (var cmd = new Npgsql.NpgsqlCommand(sql, conn, trans))
                {
                    foreach (var parameter in parameters.Keys)
                    {
                        if (sql.Contains(parameter))
                        {
                            var value = parameters[parameter];
                            _logger.Debug(d => d("The command contains the parameter '{0}', setting value to '{1}'...", parameter, value.Value));
                            cmd.Parameters.Add(value);
                        }
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            _logger.Debug(d => d("Script complete."));
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
                        _logger.Debug(d => d("The command contains the parameter '{0}', setting value to '{1}'...", parameter, value.Value));
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
        protected static string GetDBVersion(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            using (var cmd = new Npgsql.NpgsqlCommand("select version from versions where name='application' limit 1;", conn, trans))
            {
                cmd.CommandType = System.Data.CommandType.Text;
                return (string)cmd.ExecuteScalar();
            }
        }
        protected static void SetDBVersion(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans, string versionNumber)
        {
            using (var cmd = new Npgsql.NpgsqlCommand("update versions set version = @version where name = 'application';", conn, trans))
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Add("@version", NpgsqlTypes.NpgsqlDbType.Varchar, 15).Value = versionNumber;
                cmd.ExecuteNonQuery();
            }
        }

        protected static bool HasRows(IDbCommand cmd)
        {
            int scalar;
            var value = cmd.ExecuteScalar().ToString();
            if (int.TryParse(value, out scalar))
            {
                return true;
            }
            return false;
        }
    }
}
