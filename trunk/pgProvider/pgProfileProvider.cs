//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.Profile;
//using Npgsql;
//using pgProvider.Exceptions;
//using System.Configuration;
//using log4net;
//using System.Data;

//namespace pgProvider
//{
//    public class pgProfileProvider:ProfileProvider
//    {
//        protected string _Name = string.Empty;
//        protected string _ApplicationName = string.Empty;
//        protected string _ConnectionStringName = "pgProvider";
//        protected string ConnectionString = string.Empty;
//        protected static readonly ILog Log = LogManager.GetLogger(typeof(pgProfileProvider));

//        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
//        {
//            _Name = name ?? _Name;

//            _ApplicationName = config["applicationName"] ?? _ApplicationName;
//            Log.Debug(string.Format("_ApplicationName: {0}", _ApplicationName));

//            _ConnectionStringName = config["connectionStringName"] ?? _ConnectionStringName;
//            Log.Debug(string.Format("_ConnectionStringName: {0}", _ConnectionStringName));

//            #region validate database config and connectivity
//            Log.Debug("Checking to make sure the specified connection string exists...");
//            var cs = ConfigurationManager.ConnectionStrings[_ConnectionStringName];
//            if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
//            {
//                throw new ProviderConfigurationException(
//                    string.Format("The membership provider connection string, '{0}', is not defined.", _ConnectionStringName));
//            }

//            ConnectionString = ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString;
//            Log.Debug(string.Format("ConnectionString: {0}", ConnectionString));

//            Log.Debug("Checking to make sure the specified connection string can connect...");
//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("select 1", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.Text;
//                    comm.ExecuteNonQuery();
//                }
//            }
//            #endregion


//        }
//        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
//        {
//            var option = GetAuthValue(authenticationOption);

//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("delete_inactive_profiles", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    comm.Parameters.Add("_profile_type", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = option;
//                    comm.Parameters.Add("_cutoff_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = userInactiveSinceDate;
//                    var retval = comm.ExecuteNonQuery();
//                    return retval;
//                }
//            }
//        }
//        public override int DeleteProfiles(string[] usernames)
//        {
//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("delete_inactive_profiles", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    comm.Parameters.Add("_users", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, 250).Value = usernames;
//                    var retval = comm.ExecuteNonQuery();
//                    return retval;
//                }
//            }
//        }
//        public override int DeleteProfiles(ProfileInfoCollection profiles)
//        {
//            return DeleteProfiles(profiles.OfType<ProfileInfo>().Select(p => p.UserName).ToArray());
//        }
//        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
//        {
//            return PaginateProfileInfoCollection(GetProfileInfoCollectionFromUserName(usernameToMatch, authenticationOption, false, true, userInactiveSinceDate), pageIndex, pageSize, out totalRecords);
//        }
//        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
//        {
//            return PaginateProfileInfoCollection(GetProfileInfoCollectionFromUserName(usernameToMatch, authenticationOption, true, true, null), pageIndex, pageSize, out totalRecords);
//        }
//        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
//        {
//            return PaginateProfileInfoCollection(GetProfileInfoCollectionFromUserName(string.Empty, authenticationOption, false, true, userInactiveSinceDate), pageIndex, pageSize, out totalRecords);
//        }
//        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
//        {
//            return PaginateProfileInfoCollection(GetProfileInfoCollectionFromUserName(string.Empty, authenticationOption, true, true, null), pageIndex, pageSize, out totalRecords);
//        }
//        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
//        {
//            var option = GetAuthValue(authenticationOption);

//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("delete_inactive_profiles", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    comm.Parameters.Add("_cutoff_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = userInactiveSinceDate;
//                    comm.Parameters.Add("_profile_type", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = option;
//                    return (int)comm.ExecuteScalar();
//                }
//            }
//        }
//        public override string ApplicationName
//        {
//            get
//            {
//                return _ApplicationName;
//            }
//            set
//            {
//                _ApplicationName = value;
//            }
//        }
//        public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection collection)
//        {
//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("get_profile", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = Convert.ToString(context["UserName"]);
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    return(XMLToSettingsPropertyValueCollection((string)comm.ExecuteScalar()));
//                }
//            }
//        }
//        public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection collection)
//        {
//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("get_profile", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = Convert.ToString(context["UserName"]);
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    comm.Parameters.Add("_is_anonymous", NpgsqlTypes.NpgsqlDbType.Boolean).Value = !Convert.ToBoolean(context["IsAuthenticated"]);
//                    comm.Parameters.Add("_profile_data", NpgsqlTypes.NpgsqlDbType.Xml, -1).Value = SettingsPropertyValueCollectionToXML(collection);
//                    comm.ExecuteNonQuery();
//                }
//            }
//        }
//        protected string GetAuthValue(ProfileAuthenticationOption authenticationOption)
//        {
//            var option = string.Empty;
//            switch (authenticationOption)
//            {
//                case ProfileAuthenticationOption.Anonymous:
//                    option = "anonymous";
//                    break;
//                case ProfileAuthenticationOption.Authenticated:
//                    option = "authenticated";
//                    break;
//                case ProfileAuthenticationOption.All:
//                    option = "all";
//                    break;
//                default:
//                    throw new ArgumentException("authenticationOption");
//            }
//            return option;
//        }
//        protected ProfileInfoCollection GetProfileInfoCollectionFromUserName(string username,
//            ProfileAuthenticationOption authenticationOption, bool getActive, bool getInactive, DateTime? cutoffTimeStamp)
//        {
//            using (var conn = new NpgsqlConnection(ConnectionString))
//            {
//                conn.Open();
//                using (var comm = new NpgsqlCommand("delete_inactive_profiles", conn))
//                {
//                    comm.CommandType = System.Data.CommandType.StoredProcedure;
//                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
//                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = _ApplicationName;
//                    comm.Parameters.Add("_profile_type", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = GetAuthValue(authenticationOption);
//                    comm.Parameters.Add("_active_profiles", NpgsqlTypes.NpgsqlDbType.Boolean).Value = getActive;
//                    comm.Parameters.Add("_inactive_profiles", NpgsqlTypes.NpgsqlDbType.Boolean).Value = getInactive;
//                    if (cutoffTimeStamp != null)
//                    {
//                        comm.Parameters.Add("_cutoff_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = cutoffTimeStamp;
//                    }
//                    else
//                    {
//                        comm.Parameters.Add("_cutoff_date", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.MinValue;
//                    }

//                    using (var dr = comm.ExecuteReader())
//                    {
//                        return DataReaderToProfileInfoCollection(dr);
//                    }
//                }
//            }
//        }
//        protected ProfileInfoCollection DataReaderToProfileInfoCollection(IDataReader dr)
//        {
//            var retval = new ProfileInfoCollection();
//            if (dr == null) throw new ArgumentNullException();
//            var usernameColumn = dr.GetOrdinal("user_name");
//            var isAnonymousColumn = dr.GetOrdinal("is_anonymous");
//            var sizeColumn = dr.GetOrdinal("size");
//            var lastActivityColumn = dr.GetOrdinal("last_activity_date");
//            var lastUpdatedColumn = dr.GetOrdinal("last_updated_date");

//            while (dr.Read())
//            {
//                var p = new ProfileInfo(
//                    dr.GetString(usernameColumn),
//                    dr.GetBoolean(isAnonymousColumn),
//                    dr.IsDBNull(lastActivityColumn) ? DateTime.MinValue : dr.GetDateTime(lastActivityColumn),
//                    dr.IsDBNull(lastUpdatedColumn) ? DateTime.MinValue : dr.GetDateTime(lastUpdatedColumn),
//                    dr.GetInt32(sizeColumn));
//                retval.Add(p);
//            }

//            return retval;
//        }
//        protected ProfileInfoCollection PaginateProfileInfoCollection(ProfileInfoCollection source, int pageIndex, int pageSize, out int totalRecords)
//        {
//            if (pageSize < 0) throw new ArgumentException("Page size cannot be less that 0.");
//            totalRecords = source.Count;
//            if (pageSize == 0) return source;

//            if (totalRecords > pageSize)
//            {
//                var firstRecord = pageIndex * pageSize;
//                var newCollection = new ProfileInfoCollection();
//                foreach (var profileInfo in source.OfType<ProfileInfo>().Skip(firstRecord).Take(pageSize))
//                {
//                    newCollection.Add(profileInfo);
//                }
//                return newCollection;
//            }
//            else
//            {
//                return source;
//            }
//        }
//        protected SettingsPropertyValueCollection XMLToSettingsPropertyValueCollection(string source)
//        {

//        }

//        protected string SettingsPropertyValueCollectionToXML(SettingsPropertyValueCollection source)
//        {

//        }
//    }
//}
