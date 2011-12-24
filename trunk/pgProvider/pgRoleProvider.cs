using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Security;
using log4net;
using Npgsql;
using pgProvider.Exceptions;
using System.Linq;

namespace pgProvider
{
    public class pgRoleProvider : RoleProvider
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

                if (_ApplicationName.Length > 250) throw new ProviderConfigurationException("The maximum length for an application name is 250 characters.");

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
            if (usernames.Any(x => x == null) || roleNames.Any(x => x == null))
            {
                throw new ArgumentNullException();
            }
            if (usernames.Any(x => x.Trim() == string.Empty) || (roleNames.Any(x => x.Trim() == string.Empty)))
            {
                throw new ArgumentException("One or more of the supplied usernames or role names are empty.");
            }

            try
            {
                Log.Debug(string.Format("Adding users ({0}) to roles ({1})...", string.Join(", ", usernames), string.Join(", ", roleNames)));
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("assign_users_to_roles", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.Add("_users", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, 250).Value = usernames;
                        comm.Parameters.Add("_roles", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, 250).Value = roleNames;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
                        comm.ExecuteNonQuery();
                    }
                }
                Log.Info(string.Format("Added users ({0}) to roles ({1}).", string.Join(", ", usernames), string.Join(", ", roleNames)));
            }
            catch (NpgsqlException ex)
            {
                if (ex.Code == "MSING")
                {
                    throw new ProviderException("One or more role names and/ or user names specified do not exist in the current application.", ex);
                }
                throw ex;
            }
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
            if (roleName == null) throw new ArgumentNullException();
            if (roleName.Trim() == string.Empty) throw new ArgumentException("A role name cannot be empty.");
            if (roleName.Contains(",")) throw new ArgumentException("A role name cannot contain commas.  Blame Microsoft for that rule!");
            if (roleName.Length > 250) throw new ArgumentException("The maximum length for a Role name is 250 characters.");

            try
            {
                Log.Debug(string.Format("Creating role '{0}'...", roleName));
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("create_role", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.Add("_role_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
                        comm.Parameters.Add("_role_description", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
                        comm.ExecuteNonQuery();
                    }
                }
                Log.Debug(string.Format("Created role '{0}'...", roleName));
            }
            catch (NpgsqlException ex)
            {
                if (ex.Code == "DUPRL")
                {//Role already exists
                    Log.Error("Duplicate Role.", ex);
                    throw new ProviderException("The specified role already exists for this application.", ex);
                }
                throw ex;
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (roleName == null ) throw new ArgumentNullException();
            if (roleName.Trim() == string.Empty) throw new ArgumentException("The specified role name cannot be empty.");
            Log.Debug(string.Format("Creating role '{0}'...", roleName));
            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("delete_role", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.Add("_role_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = roleName;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
                        comm.Parameters.Add("_throw_on_populated", NpgsqlTypes.NpgsqlDbType.Boolean).Value = throwOnPopulatedRole;
                        comm.ExecuteNonQuery();
                        Log.Debug(string.Format("Created role '{0}'...", roleName));
                        return true;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                switch (ex.Code)
                {
                    case 'RLPOP':
                        Log.Error("The role to be deleted is populated, aborting.", ex);
                        throw new ProviderException("The specified role is populated; cannot delete.", ex);
                    case 'NOROL':
                        Log.Error("The specified role does not exist in this application.", ex);
                        throw new ArgumentException("The specified role does not exist in this application.", ex);
					default:
						throw ex;
                }
            }
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
