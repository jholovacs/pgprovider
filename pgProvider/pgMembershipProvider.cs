#region Header

// Solution: pgProvider
// Project: pgProvider
// File: pgMembershipProvider.cs
// Created 2014-02-27 3:40 PM
// Last Modified by Jeremy Holovacs 2014-02-27 3:41 PM

#endregion

#region

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using Npgsql;
using pgProvider.Exceptions;

#endregion

namespace pgProvider
{
    public class pgMembershipProvider : MembershipProvider
    {
        #region protected variables

        protected static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetCurrentClassLogger();
        protected string ConnectionString = string.Empty;
        protected EncryptionMethods EncryptionMethod = EncryptionMethods.Undefined;
        protected string _ApplicationName = string.Empty;
        protected string _ConnectionStringName = "pgProvider";
        protected bool _EnablePasswordReset = true;
        protected bool _EnablePasswordRetrieval = false;
        protected byte[] _EncryptionKey;
        protected int _LockoutTime = 0;
        protected int _MaxInvalidPasswordAttempts = 5;
        protected int _MaxSaltCharacters = 60;
        protected int _MinRequiredNonAlphanumericCharacters = 0;
        protected int _MinRequiredPasswordLength = 6;
        protected int _MinSaltCharacters = 30;
        protected string _Name = string.Empty;
        protected int _PasswordAttemptWindow = 5;
        protected string _PasswordStrengthRegularExpression = string.Empty;
        protected bool _RequiresQuestionAndAnswer = false;
        protected bool _RequiresUniqueEmail = false;
        protected int _SessionTime = 15;
        protected string _dbOwner = "security";

        #endregion

        public override string ApplicationName
        {
            get { return _ApplicationName; }
            set { _ApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _EnablePasswordRetrieval; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                if (EncryptionMethod == EncryptionMethods.Hash)
                {
                    return MembershipPasswordFormat.Hashed;
                }
                return MembershipPasswordFormat.Encrypted;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _RequiresUniqueEmail; }
        }

        public override string Description
        {
            get { return "PostgreSQL ASP.Net MembershipProvider class"; }
        }

        public override string Name
        {
            get { return _Name; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            _Name = name ?? config["name"];
            Log.Debug(d => d("'{0}' Initializing...", _Name));

            try
            {
                Log.Debug("pgMembershipProvider Initialize() invoked.");

                #region Set the configuration values

                _ConnectionStringName = config["connectionStringName"] ?? _ConnectionStringName;
                Log.Debug(d => d("_ConnectionStringName: {0}", _ConnectionStringName));

                _PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"] ??
                                                     _PasswordStrengthRegularExpression;
                Log.Debug(d => d("_PasswordStrengthRegularExpression: {0}", _PasswordStrengthRegularExpression));

                _ApplicationName = config["applicationName"] ?? _ApplicationName;
                Log.Debug(d => d("_ApplicationName: {0}", _ApplicationName));

                if (config["enablePasswordReset"] != null)
                {
                    _EnablePasswordReset = Convert.ToBoolean(config["enablePasswordReset"]);
                }
                Log.Debug(d => d("_EnablePasswordReset: {0}", _EnablePasswordReset));

                if (config["enablePasswordRetrieval"] != null)
                {
                    _EnablePasswordRetrieval = Convert.ToBoolean(config["enablePasswordRetrieval"]);
                }
                Log.Debug(d => d("_EnablePasswordRetrieval: {0}", _EnablePasswordRetrieval));

                if (config["requiresQuestionAndAnswer"] != null)
                {
                    _RequiresQuestionAndAnswer = Convert.ToBoolean(config["requiresQuestionAndAnswer"]);
                }
                Log.Debug(d => d("_RequiresQuestionAndAnswer: {0}", _RequiresQuestionAndAnswer));

                if (config["requiresUniqueEmail"] != null)
                {
                    _RequiresUniqueEmail = Convert.ToBoolean(config["requiresUniqueEmail"]);
                }
                Log.Debug(d => d("_RequiresUniqueEmail: {0}", _RequiresUniqueEmail));

                if (config["maxInvalidPasswordAttempts"] != null)
                {
                    _MaxInvalidPasswordAttempts = Convert.ToInt32(config["maxInvalidPasswordAttempts"]);
                    if (_MaxInvalidPasswordAttempts < 0)
                    {
                        throw new ProviderConfigurationException("MaxInvalidPasswordAttempts must be 0 or greater.");
                    }
                }
                Log.Debug(d => d("_MaxInvalidPasswordAttempts: {0}", _MaxInvalidPasswordAttempts));

                if (config["minRequiredNonAlphanumericCharacters"] != null)
                {
                    _MinRequiredNonAlphanumericCharacters =
                        Convert.ToInt32(config["minRequiredNonAlphanumericCharacters"]);
                    if (_MinRequiredNonAlphanumericCharacters < 0)
                    {
                        throw new ProviderConfigurationException(
                            "MinRequiredNonAlphanumericCharacters must be 0 or greater.");
                    }
                }
                Log.Debug(d => d("_MinRequiredNonAlphanumericCharacters: {0}", _MinRequiredNonAlphanumericCharacters));

                if (config["passwordAttemptWindow"] != null)
                {
                    _PasswordAttemptWindow = Convert.ToInt32(config["passwordAttemptWindow"]);
                    if (_PasswordAttemptWindow < 0)
                    {
                        throw new ProviderConfigurationException("Password Attempt window must be 0 or greater.");
                    }
                }
                Log.Debug(d => d("_PasswordAttemptWindow: {0}", _PasswordAttemptWindow));

                if (config["encryptionKey"] != null)
                {
                    _EncryptionKey = Convert.FromBase64String(config["encryptionKey"]);
                }
                Log.Debug(d => d("_EncryptionKey: {0}", _EncryptionKey));

                if (config["minSaltCharacters"] != null)
                {
                    _MinSaltCharacters = Convert.ToInt32(config["minSaltCharacters"]);
                    if (_MinSaltCharacters < 0 || _MinSaltCharacters > 250)
                    {
                        throw new ProviderConfigurationException("MinSaltCharacters must be between 0 and 250.");
                    }
                }
                Log.Debug(d => d("_MinSaltCharacters: {0}", _MinSaltCharacters));

                if (config["maxSaltCharacters"] != null)
                {
                    _MaxSaltCharacters = Convert.ToInt32(config["maxSaltCharacters"]);
                    if (_MaxSaltCharacters < 1 || _MaxSaltCharacters > 250)
                    {
                        throw new ProviderConfigurationException("MaxSaltCharacters must be between 1 and 250.");
                    }
                }
                Log.Debug(d => d("_MaxSaltCharacters: {0}", _MaxSaltCharacters));

                if (config["minRequiredPasswordLength"] != null)
                {
                    _MinRequiredPasswordLength = Convert.ToInt32(config["minRequiredPasswordLength"]);
                    if (_MinRequiredPasswordLength < 0 || _MinRequiredPasswordLength > 100)
                    {
                        throw new ProviderConfigurationException(
                            "Minimum password characters must be between 0 and 100.");
                    }
                }
                Log.Debug(d => d("_MinRequiredPasswordLength: {0}", _MinRequiredPasswordLength));

                if (config["lockoutTime"] != null)
                {
                    _LockoutTime = Convert.ToInt32(config["lockoutTime"]);
                    if (_LockoutTime < 0)
                    {
                        throw new ProviderConfigurationException("Minimum lockout time is 0 minutes.");
                    }
                }
                Log.Debug(d => d("_LockoutTime: {0}", _LockoutTime));

                if (config["sessionTime"] != null)
                {
                    _SessionTime = Convert.ToInt32(config["sessionTime"]);
                    if (_SessionTime < 1)
                    {
                        throw new ProviderConfigurationException("Minimum session time is 1 minute.");
                    }
                }
                Log.Debug(d => d("_SessionTime: {0}", _SessionTime));

                if (config["dbOwner"] != null)
                {
                    _dbOwner = config["dbOwner"];
                }
                Log.Debug(d => d("_dbOwner: {0}", _dbOwner));

                #endregion

                #region validate configuration

                #region validate database config and connectivity

                Log.Debug(d => d("Checking to make sure the specified connection string exists..."));
                var cs = ConfigurationManager.ConnectionStrings[_ConnectionStringName];
                if (cs == null || string.IsNullOrEmpty(cs.ConnectionString))
                {
                    throw new ProviderConfigurationException(
                        string.Format("The membership provider connection string, '{0}', is not defined.",
                            _ConnectionStringName));
                }

                ConnectionString = ConfigurationManager.ConnectionStrings[_ConnectionStringName].ConnectionString;
                Log.Debug(d => d("ConnectionString: {0}", ConnectionString));

                Log.Debug(d => d("Checking to make sure the specified connection string can connect..."));
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("select 1", conn))
                    {
                        comm.CommandType = CommandType.Text;
                        comm.ExecuteNonQuery();
                    }
                }

                #endregion

                #region validate encryption mechanism

                if (_EnablePasswordRetrieval)
                {
                    Log.Debug(d => d("Password Retrieval is enabled, checking for a valid encryption key..."));
                    if (_EncryptionKey == null || _EncryptionKey.Length != 32)
                    {
                        throw new ProviderConfigurationException(
                            "When password retrieval is enabled, a Base-64 encoded 32-byte encryption key must be supplied.");
                    }
                    EncryptionMethod = EncryptionMethods.ReversibleSymmetric;
                }
                else
                {
                    EncryptionMethod = EncryptionMethods.Hash;
                }

                #endregion

                #region detect conflicting settings

                if (_MaxSaltCharacters < _MinSaltCharacters)
                {
                    throw new ProviderConfigurationException(
                        "MaxSaltCharacters value cannot be less than MinSaltCharacters value.");
                }

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                var message = "Error initializing the membership configuration settings";
                Log.Error(message, ex);
                throw new ProviderConfigurationException(message, ex);
            }

            DDLManager.ValidateVersion(_ConnectionStringName, _dbOwner);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            Log.Debug(d => d("ChangePassword(string, string, string) called for '{0}'", username));
            if (ValidateUser(username, oldPassword))
            {
                try
                {
                    ValidatePasswordComplexity(newPassword);
                    ChangePassword(username, newPassword);
                    Log.Info(i => i("Password changed for '{0}'", username));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error attempting to change password.", ex);
                }
            }

            return false;
        }

        public void ChangePassword(string username, string newPassword)
        {
            Log.Debug(d => d("ChangePassword(string, string) called for '{0}'", username));
            var salt = string.Empty;
            byte[] hash;

            switch (EncryptionMethod)
            {
                case EncryptionMethods.Hash:
                    Log.Debug(d => d("Hashing new password..."));
                    salt = Encryption.GenerateSalt(_MinSaltCharacters, _MaxSaltCharacters);
                    hash = Encryption.GenerateHash(newPassword, salt);
                    break;
                case EncryptionMethods.ReversibleSymmetric:
                    hash = Encryption.EncryptString(newPassword, _EncryptionKey);
                    break;
                default:
                    throw new ProviderConfigurationException(
                        "The encryption method for the membership provider cannot be determined.");
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                Log.Debug(d => d("Updating the password/ salt data for '{0}' in the database...", username));
                conn.Open();
                using (var comm = new NpgsqlCommand("update_user_password", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    comm.Parameters.Add("_salt", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = salt;
                    comm.Parameters.Add("_password", NpgsqlTypes.NpgsqlDbType.Bytea).Value = hash;
                    comm.ExecuteNonQuery();
                }
            }

            Log.Debug(d => d("Unlocking the account for '{0}' if it was locked...", username));
            UnlockUser(username);

            Log.Debug(d => d("ChangePassword(string, string) for '{0}' complete.", username));
        }

        protected virtual void ValidatePasswordComplexity(string newPassword)
        {
            if (newPassword.Length < _MinRequiredPasswordLength)
            {
                throw new PasswordTooShortException();
            }
            if ((newPassword.Length - OnlyTheAlphanumericLowercase(newPassword).Length) <
                _MinRequiredNonAlphanumericCharacters)
            {
                throw new PasswordComplexityException(
                    string.Format("Password requires a minimum of {0} non-alphanumeric characters.",
                        _MinRequiredNonAlphanumericCharacters));
            }
            if (!string.IsNullOrEmpty(_PasswordStrengthRegularExpression) &&
                (!Regex.IsMatch(newPassword, _PasswordStrengthRegularExpression)))
            {
                throw new PasswordComplexityException("Password does not meet the required complexity pattern.");
            }
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
            string newPasswordQuestion, string newPasswordAnswer)
        {
            Log.Debug(
                d =>
                    d("ChangePasswordQuestionAndAnswer(string, string, string, string) for '{0}' beginning...", username));
            if (ValidateUser(username, password))
            {
                try
                {
                    UpdateQAndA(username, newPasswordQuestion, newPasswordAnswer);
                    Log.Info(i => i("Password Question and answer have been updated for '{0}'", username));
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error attempting to change the user's question and answer.", ex);
                }
                Log.Info(i => i("Password validation failed for user '{0}'", username));
            }
            return false;
        }

        public override MembershipUser CreateUser(string username, string password, string email,
            string passwordQuestion,
            string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            try
            {
                Log.Debug(d => d("Creating user record for '{0}'...", username));
                var userId = CreateUser(username, email, isApproved);
            }
            catch (NpgsqlException ex)
            {
                Log.Error("Database Error creating the user record.", ex);
                switch (ex.Code.ToUpper())
                {
                    case "DUPEM":
                        status = MembershipCreateStatus.DuplicateEmail;
                        return null;
                    case "DUPUN":
                        status = MembershipCreateStatus.DuplicateUserName;
                        return null;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error("General Error creating the user record.", ex);
                throw ex;
            }

            try
            {
                Log.Debug(d => d("Setting the password..."));
                ValidatePasswordComplexity(password);
                ChangePassword(username, password);
            }
            catch (PasswordTooShortException ex)
            {
                Log.Error("Supplied password is too short.", ex);
                status = MembershipCreateStatus.InvalidPassword;
                DeleteUser(username, true);
                return null;
            }
            catch (PasswordComplexityException ex)
            {
                Log.Error("Password does not meet complexity requirements.", ex);
                status = MembershipCreateStatus.InvalidPassword;
                DeleteUser(username, true);
                return null;
            }

            if (_EnablePasswordRetrieval || _RequiresQuestionAndAnswer)
            {
                Log.Debug(d => d("Password questions are used; updating the question and answer for the user..."));
                try
                {
                    UpdateQAndA(username, passwordQuestion, passwordAnswer);
                }
                catch (NpgsqlException ex)
                {
                    Log.Error("Database Error updating the password/ answer.", ex);
                    DeleteUser(username, true);
                    throw ex;
                }
                catch (InvalidQuestionException ex)
                {
                    Log.Error("Invalid Question.", ex);
                    status = MembershipCreateStatus.InvalidQuestion;
                    DeleteUser(username, true);
                    return null;
                }
                catch (InvalidAnswerException ex)
                {
                    Log.Error("Invalid Answer.", ex);
                    status = MembershipCreateStatus.InvalidAnswer;
                    DeleteUser(username, true);
                    return null;
                }
                catch (Exception ex)
                {
                    Log.Error("General Error.", ex);
                    DeleteUser(username, true);
                    throw ex;
                }
            }

            status = MembershipCreateStatus.Success;
            Log.Info(i => i("Created user '{0}'.", username));
            return GetUser(username, false);
        }

        protected void UpdateQAndA(string username, string question, string answer)
        {
            Log.Debug(d => d("UpdateQAndA(string, string, string) for user '{0}' beginning...", username));
            var modifiedAnswer = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(question)) throw new InvalidQuestionException();
                if (string.IsNullOrEmpty(answer)) throw new InvalidAnswerException();
                modifiedAnswer = OnlyTheAlphanumericLowercase(answer);
                if (string.IsNullOrEmpty(modifiedAnswer)) throw new InvalidAnswerException();
            }
            catch (Exception ex)
            {
                if (_RequiresQuestionAndAnswer)
                {
                    Log.Error("Questions and Answers are required to be provided; but they are not properly supplied.",
                        ex);
                    throw ex;
                }

                Log.Warn(
                    w =>
                        w(
                            "Questions and Answers are enabled, but they have not been properly provided.  No updates to the Questions and Answers will be performed at this time.",
                            ex));
            }

            var salt = Encryption.GenerateSalt(_MinSaltCharacters, _MaxSaltCharacters);
            var hash = Encryption.GenerateHash(modifiedAnswer, salt);

            Log.Debug(d => d("Updating the Q&A for '{0}' in the database...", username));
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("update_user_q_and_a", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    comm.Parameters.Add("questiontext", NpgsqlTypes.NpgsqlDbType.Varchar, 1000).Value = question;
                    comm.Parameters.Add("answersalt", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = salt;
                    comm.Parameters.Add("answerhash", NpgsqlTypes.NpgsqlDbType.Bytea).Value = hash;
                    comm.ExecuteNonQuery();
                }
            }
            Log.Debug(d => d("Successfully updated the database with question and answer info."));
        }

        protected string OnlyTheAlphanumericLowercase(string original)
        {
            if (string.IsNullOrEmpty(original)) return original;
            var cleanedUp = Regex.Replace(original, @"[^a-zA-Z0-9]", string.Empty);
            return cleanedUp.ToLower();
        }

        protected int CreateUser(string username, string email, bool isApproved)
        {
            Log.Debug(d => d("CreateUser('{0}', '{1}', '{2}') beginning...", username, email, isApproved));

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("create_user", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    comm.Parameters.Add("emailaddress", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = email;
                    comm.Parameters.Add("approved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = isApproved;
                    comm.Parameters.Add("email_is_unique", NpgsqlTypes.NpgsqlDbType.Boolean).Value =
                        _RequiresUniqueEmail;
                    var retval = Convert.ToInt32(comm.ExecuteScalar());
                    Log.Debug(d => d("CreateUser('{0}', '{1}', '{2}') complete.", username, email, isApproved));
                    return retval;
                }
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("username parameter is required.");
            try
            {
                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("delete_user", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                            _ApplicationName;
                        comm.Parameters.Add("delete_related", NpgsqlTypes.NpgsqlDbType.Boolean).Value =
                            deleteAllRelatedData;
                        comm.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to delete user '{0}'.", username), ex);
                return false;
            }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
            out int totalRecords)
        {
            if (string.IsNullOrEmpty(emailToMatch))
            {
                return GetAllUsers(pageIndex, pageSize, out totalRecords);
            }

            if (pageIndex < 0 || pageSize < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_users_by_email", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("partial_email", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = emailToMatch;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var users = GetUsersFromReader(reader);
                            return PaginateUserCollection(users, pageIndex, pageSize, out totalRecords);
                        }
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }
                }
            }
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
            out int totalRecords)
        {
            if (string.IsNullOrEmpty(usernameToMatch))
            {
                return GetAllUsers(pageIndex, pageSize, out totalRecords);
            }

            if (pageIndex < 0 || pageSize < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_users_by_username", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("partial_username", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        usernameToMatch;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var users = GetUsersFromReader(reader);
                            return PaginateUserCollection(users, pageIndex, pageSize, out totalRecords);
                        }
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }
                }
            }
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            Log.Debug(d => d("GetAllUsers({0}, {1}, out int)", pageIndex, pageSize));
            if (pageIndex < 0 || pageSize < 1)
            {
                totalRecords = 0;
                return new MembershipUserCollection();
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_all_users", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var users = GetUsersFromReader(reader);
                            Log.Debug(d => d("{0} record(s) collected.", users.Count));
                            return PaginateUserCollection(users, pageIndex, pageSize, out totalRecords);
                        }
                        Log.Debug(d => d("No records collected."));
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }
                }
            }
        }

        public override int GetNumberOfUsersOnline()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_number_of_users_online", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("session", NpgsqlTypes.NpgsqlDbType.Integer).Value = _SessionTime;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    return Convert.ToInt32(comm.ExecuteScalar());
                }
            }
        }

        public override string GetPassword(string username, string answer)
        {
            Log.Debug(d => d("GetPassword: username={0}, answer={1}", username, answer));

            if (PasswordFormat != MembershipPasswordFormat.Encrypted)
            {
                throw new InvalidOperationException(
                    "GetPassword() is only valid if the passwords are stored in Encrypted form in the database.");
            }

            if (ValidateAnswer(username, answer))
            {
                var password = DecryptPassword(GetCredentials(username).PasswordHash).ToCharacterString();
                Log.Debug(d => d("GetPassword: username={0}, answer={1}, password={2}", username, answer, password));
                return password;
            }
            Log.Warn(w => w("Password fetch failed for user '{0}'; the answer was incorrect.", username));
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("username parameter is required.");

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_user_by_username", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    comm.Parameters.Add("online", NpgsqlTypes.NpgsqlDbType.Boolean).Value = userIsOnline;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return GetUsersFromReader(reader).OfType<MembershipUser>().FirstOrDefault();
                        }
                        return null;
                    }
                }
            }
        }

        public MembershipUserCollection GetUsersFromReader(IDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("Datareader object cannot be null.");
            if (reader.IsClosed)
                throw new InvalidOperationException("This datareader cannot be read from.  It is closed.");

            var users = new MembershipUserCollection();

            var userIdColumn = reader.GetOrdinal("user_id");
            var userNameColumn = reader.GetOrdinal("user_name");
            var lastActivityColumn = reader.GetOrdinal("last_activity");
            var createdColumn = reader.GetOrdinal("created");
            var emailColumm = reader.GetOrdinal("email");
            var approvedColumn = reader.GetOrdinal("approved");
            var lastLockoutColumn = reader.GetOrdinal("last_lockout");
            var lastLoginColumn = reader.GetOrdinal("last_login");
            var lastPasswordChangedColumn = reader.GetOrdinal("last_password_changed");
            var passwordQuestionColumn = reader.GetOrdinal("password_question");
            var commentsColumn = reader.GetOrdinal("comment");

            while (reader.Read())
            {
                var user = new MembershipUser(
                    _Name,
                    reader.IsDBNull(userNameColumn) ? string.Empty : reader.GetString(userNameColumn),
                    reader.IsDBNull(userIdColumn) ? 0 : reader.GetInt32(userIdColumn),
                    reader.IsDBNull(emailColumm) ? string.Empty : reader.GetString(emailColumm),
                    reader.IsDBNull(passwordQuestionColumn) ? string.Empty : reader.GetString(passwordQuestionColumn),
                    reader.IsDBNull(commentsColumn) ? string.Empty : reader.GetString(commentsColumn),
                    reader.IsDBNull(approvedColumn) ? false : reader.GetBoolean(approvedColumn),
                    reader.IsDBNull(lastLockoutColumn) ? false : LockoutInEffect(reader.GetDateTime(lastLockoutColumn)),
                    reader.IsDBNull(createdColumn) ? DateTime.MinValue : reader.GetDateTime(createdColumn),
                    reader.IsDBNull(lastLoginColumn) ? DateTime.MinValue : reader.GetDateTime(lastLoginColumn),
                    reader.IsDBNull(lastActivityColumn) ? DateTime.MinValue : reader.GetDateTime(lastActivityColumn),
                    reader.IsDBNull(lastPasswordChangedColumn)
                        ? DateTime.MinValue
                        : reader.GetDateTime(lastPasswordChangedColumn),
                    reader.IsDBNull(lastLockoutColumn) ? DateTime.MinValue : reader.GetDateTime(lastLockoutColumn)
                    );

                users.Add(user);
            }

            return users;
        }

        protected bool LockoutInEffect(DateTime dateTime)
        {
            //there is no lockout if the dateTime is null
            if (dateTime == null || dateTime == DateTime.MinValue) return false;

            //there is no auto unlock if the LockoutTime is zero.
            if (_LockoutTime == 0) return true;

            return dateTime.AddMinutes(_LockoutTime) > DateTime.Now;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            int userId;
            if (!int.TryParse(providerUserKey.ToString(), out userId))
            {
                throw new ArgumentException("Bad user key; expecting an integer.");
            }

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_user_by_id", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Integer).Value = userId;
                    comm.Parameters.Add("online", NpgsqlTypes.NpgsqlDbType.Boolean).Value = userIsOnline;
                    using (var reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return GetUsersFromReader(reader).OfType<MembershipUser>().FirstOrDefault();
                        }
                        return null;
                    }
                }
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email parameter cannot be empty.");
            if (!_RequiresUniqueEmail)
                throw new InvalidOperationException(
                    "Cannot use GetUserNameByEmail() unless the RequiresUniqueEmail attribute is set to 'true'.");

            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_user_name_by_email", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("emailaddress", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = email;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    return comm.ExecuteScalar().ToString();
                }
            }
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!_EnablePasswordReset)
                throw new InvalidOperationException("ResetPassword() cannot be used; password resets are disabled.");

            if (_RequiresQuestionAndAnswer && !ValidateAnswer(username, answer))
            {
                Log.WarnFormat("User '{0}' failed to validate the security question.", username);
                return null;
            }

            var newPass = Encryption.GenerateSalt(8, 10);
            ChangePassword(username, newPass);
            Log.Info(
                i =>
                    i(
                        "User '{0}' has reset their password.  The supplied password has bypassed complexity requirements.",
                        username));
            return newPass;
        }

        public override bool UnlockUser(string userName)
        {
            try
            {
                Log.Debug(d => d("Unlocking user '{0}'...", userName));
                if (string.IsNullOrEmpty(userName)) throw new ArgumentException("username parameter is required.");

                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("unlock_user", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = userName;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                            _ApplicationName;
                        comm.ExecuteNonQuery();
                        Log.Info(i => i("User '{0}' has been unlocked.", userName));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to unlock user.", ex);
                return false;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            try
            {
                Log.Debug(d => d("Persisting changes to user {0} ({1})...", user.ProviderUserKey, user.UserName));
                if (user == null) throw new ArgumentException("User parameter is required.");

                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("update_user", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Integer).Value =
                            (int) user.ProviderUserKey;
                        comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = user.UserName;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                            _ApplicationName;
                        comm.Parameters.Add("emailaddress", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = user.Email;
                        comm.Parameters.Add("isapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = user.IsApproved;
                        comm.Parameters.Add("comments", NpgsqlTypes.NpgsqlDbType.Varchar, -1).Value = user.Comment;
                        comm.Parameters.Add("email_is_unique", NpgsqlTypes.NpgsqlDbType.Boolean).Value =
                            _RequiresUniqueEmail;
                        comm.ExecuteNonQuery();
                        Log.Info(i => i("User '{0}' has been updated.", user.UserName));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to update user.", ex);
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                Log.Warn(w => w("Password null or empty for use '{0}'.", username));
                return false;
            }

            var creds = GetCredentials(username);
            if (creds == null)
            {
                Log.Warn(w => w("Credentials were not available for user '{0}'.", username));
                return false;
            }

            if (LockoutInEffect(creds.LockedOutAsOf))
            {
                Log.Warn(w => w("User '{0}' is locked out and cannot authenticate.", username));
                return false;
            }

            bool validated;

            switch (EncryptionMethod)
            {
                case EncryptionMethods.Hash:
                    var passwordHash = Encryption.GenerateHash(password, creds.PasswordSalt);
                    validated = (passwordHash.ToBase64() == creds.PasswordHash.ToBase64());
                    break;
                case EncryptionMethods.ReversibleSymmetric:
                    var persistedPassword =
                        Encryption.DecryptString(creds.PasswordHash, _EncryptionKey).ToCharacterString();
                    validated = (persistedPassword == password);
                    break;
                default:
                    Log.Error("The encryption method was not properly set.  User validation is not possible.");
                    return false;
            }

            RecordLoginEvent(username, validated);
            var user = GetUser(username, true);

            if (validated && user.IsApproved)
            {
                Log.Info(i => i("User '{0}' successfully authenticated.", username));
                return true;
            }
            if (validated && !user.IsApproved)
            {
                Log.Warn(w => w("User '{0}' successfully authenticated, but the account is not approved.", username));
            }
            else
            {
                Log.Warn(w => w("User '{0}' failed to authenticate.", username));
            }

            return false;
        }

        protected void RecordLoginEvent(string username, bool success)
        {
            try
            {
                Log.Debug(d => d("Recording Login Event: {0}, success: {1}...", username, success));
                if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username parameter is required.");

                using (var conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (var comm = new NpgsqlCommand("record_login_event", conn))
                    {
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                        comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                            _ApplicationName;
                        comm.Parameters.Add("origin", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                            string.Format("Machine: {0}, Application: {1}", Environment.MachineName, _ApplicationName);
                        comm.Parameters.Add("success_indicator", NpgsqlTypes.NpgsqlDbType.Boolean).Value = success;
                        comm.Parameters.Add("attempt_window", NpgsqlTypes.NpgsqlDbType.Integer).Value =
                            _PasswordAttemptWindow;
                        comm.Parameters.Add("attempt_count", NpgsqlTypes.NpgsqlDbType.Integer).Value =
                            _MaxInvalidPasswordAttempts;
                        comm.ExecuteNonQuery();
                        Log.Debug(d => d("Login event recorded: {0}, success: {1}", username, success));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to update user.", ex);
            }
        }

        protected override byte[] DecryptPassword(byte[] encodedPassword)
        {
            return Encryption.DecryptString(encodedPassword, _EncryptionKey);
        }

        protected override byte[] EncryptPassword(byte[] password)
        {
            return Encryption.EncryptString(password, _EncryptionKey);
        }

        protected bool ValidateAnswer(string username, string answer)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username parameter is required.");
            if (string.IsNullOrEmpty(answer)) throw new ArgumentException("Answer string is required.");
            var creds = GetCredentials(username);
            if (creds == null)
                throw new InvalidOperationException("Credentials were not available for the specified user.");

            var cleanedAnswer = OnlyTheAlphanumericLowercase(answer);
            var hashedAnswer = Encryption.GenerateHash(cleanedAnswer, creds.AnswerSalt);
            return (hashedAnswer.ToBase64() == creds.AnswerHash.ToBase64());
        }

        protected CredentialPackage GetCredentials(string username)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("get_user_credentials", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("_user_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value = username;
                    comm.Parameters.Add("_application_name", NpgsqlTypes.NpgsqlDbType.Varchar, 250).Value =
                        _ApplicationName;
                    using (IDataReader dr = comm.ExecuteReader())
                    {
                        var saltColumn = dr.GetOrdinal("salt");
                        var passwordColumn = dr.GetOrdinal("password");
                        var passwordAnswerColumn = dr.GetOrdinal("password_answer");
                        var answerSaltColumn = dr.GetOrdinal("answer_salt");
                        var lastLockoutColumn = dr.GetOrdinal("last_lockout");

                        if (dr.Read())
                        {
                            return new CredentialPackage
                            {
                                PasswordSalt = dr.IsDBNull(saltColumn) ? string.Empty : dr.GetString(saltColumn),
                                PasswordHash =
                                    dr.IsDBNull(passwordColumn) ? new byte[] {} : (byte[]) dr.GetValue(passwordColumn),
                                AnswerSalt =
                                    dr.IsDBNull(answerSaltColumn) ? string.Empty : dr.GetString(answerSaltColumn),
                                AnswerHash =
                                    dr.IsDBNull(passwordAnswerColumn)
                                        ? new byte[] {}
                                        : (byte[]) dr.GetValue(passwordAnswerColumn),
                                LockedOutAsOf =
                                    dr.IsDBNull(lastLockoutColumn)
                                        ? DateTime.MinValue
                                        : dr.GetDateTime(lastLockoutColumn)
                            };
                        }
                        return null;
                    }
                }
            }
        }

        protected MembershipUserCollection PaginateUserCollection(MembershipUserCollection source, int pageIndex,
            int pageSize, out int totalRecords)
        {
            Log.Debug(d => d("PaginateUserCollection(MembershipUserCollection, {0}, {1}, out int)", pageIndex, pageSize));
            totalRecords = source.Count;
            if (pageSize < 0) throw new ArgumentException("The page size cannot be less than zero.");
            if (pageSize == 0) return source;
            Log.Debug(d => d("The MembershipUserCollection source contains {0} record(s).", source.Count));

            if (totalRecords > pageSize)
            {
                var firstRecord = pageIndex*pageSize;
                var newCollection = new MembershipUserCollection();
                foreach (var user in source.OfType<MembershipUser>().Skip(firstRecord).Take(pageSize))
                {
                    newCollection.Add(user);
                }
                Log.Debug(d => d("The returning MembershipUserCollection contains {0} record(s).", newCollection.Count));
                return newCollection;
            }
            return source;
        }

        public void PurgeActivity(long secondsOld)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var comm = new NpgsqlCommand("purge_activity", conn))
                {
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("olderthan", NpgsqlTypes.NpgsqlDbType.Bigint).Value = secondsOld;
                    comm.ExecuteNonQuery();
                }
            }
        }
    }
}