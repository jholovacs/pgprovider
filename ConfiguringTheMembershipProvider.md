# Introduction #

The pgMembershipProvider class follows the standard Microsoft ASP.NET provider pattern; to see how a MembershipProvider is generally implemented, please see the Microsoft documentation at http://msdn.microsoft.com/en-us/library/yh26yfzy.aspx.

# Example #
A typical implementation of the configuration in the web.config file will resemble this:
```
<?xml version="1.0"?>
<configuration>
	<configSections>
		<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
	</configSections>
	<connectionStrings>
		<add name="pgProvider" connectionString="Server=10.10.10.10;Port=5432;Database=security;User Id=security;Password=S3cUr1#Y;" />
	</connectionStrings>
	<startup>
		<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
	</startup>
	<system.web>
		<membership defaultProvider="pgMembershipProvider">
			<providers>
				<clear />
				<add name="pgMembershipProvider" type="pgProvider.pgMembershipProvider, pgProvider" applicationName="pgProvider.Tests"/>
			</providers>
		</membership>
		<roleManager defaultProvider="pgRoleProvider">
			<providers>
				<clear />
				<add name="pgRoleProvider" type="pgProvider.pgRoleProvider, pgProvider" applicationName="pgProvider.Tests"/>
			</providers>
		</roleManager>
	</system.web>
	<log4net>
		<appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
			<layout type="log4net.Layout.PatternLayout">
				<conversionPattern value="%date [%thread] %-5level %logger [%property{NDC}] - %message%newline" />
			</layout>
		</appender>
		<root>
			<level value="DEBUG" />
			<appender-ref ref="ConsoleAppender" />
		</root>
	</log4net>
</configuration>
```

# Configuration Options #
The following options are attributes that can be applied to the provider in the `web.config` file.  Please note that the attribute names are _case-sensitive_, and will always start with a lowercase letter.

  * _connectionStringName_.  (string, default "pgProvider") Sets the provider to reference a named connection string in the `connectionStrings` section.  The connection string must be valid for Npgsql connections.  **If the connection string does not exist or is invalid, the provider will not allow you to continue.**
  * _passwordStrengthRegularExpression_.  (string, default "") Allows the user to specify a pattern that all passwords must adhere to to be considered valid.  **Note:** reset passwords will ignore the password complexity checks.
  * _applicationName_.  (string, default "") Sets the name of the application using the provider.
  * _enablePasswordReset_.  (boolean, default "true") Allows a user's password to be reset.  Attempting to reset a password with this disabled will result in an `InvalidOperationException`.  Default is **true**.
  * _enablePasswordRetrieval_.  (boolean, default "false") Specifies whether or not the password can be extracted from the database and read.  If this is set to true, the method for storing password data will switch from the default SHA384 hash mechanism to an RSA-encrypted byte array.  For this reason, if _enablePasswordRetrieval_ is set to `true`, the _encryptionKey_ attribute must also be set.
  * _requiresQuestionAndAnswer_. (boolean, default "false") Specifies that a password recovery cleartext question and a corresponding encrypted answer will be stored to allow a user to reset their own account online.  **Note** The password reset functionality behaves differently depending on this value.  If this is set to false, the answer parameter in the `ResetPassword()` will be ignored, assuming the reset is being done by someone with administrative privileges.  If set to true, the answer parameter will be examined, assuming the user in question is resetting their own password.
  * _requiresUniqueEmail_. (boolean, default "true") Specifies that a user's email address will be unique for each user.  This will prevent duplicate email addresses from being added to different users.
  * _maxInvalidPasswordAttempts_.  (integer, default "5") Specifies that after the configured number of consecutive failed logon attempts for a bad username/ password in the specified _passwordAttemptWindow_, the user will be locked out of their account to discourage and/ or prevent brute force password attacks.  Setting this to 0 will disable user lockout.
  * _minRequiredNonAlphanumericCharacters_.  (integer, default "0") Specifies that passwords must contain at least the configured number of non-alphanumeric characters, i.e. special characters.  Password changes will not be accepted without the minimum non-alphanumeric characters.  **Note:** reset passwords will ignore the password complexity checks.
  * _passwordAttemptWindow_.  (integer, default "5") Specifies the window, in minutes, that will be considered for determining if the _maxInvalidPasswordAttempts_ failed attempts are consecutive.
  * _encryptionKey_.  (string, default "") The base-64 encoded symmetric encryption key used for storing passwords in reversible format in the database.  This is required for configurations where the _enablePasswordRetrieval_ attribute is set to `true`.  To generate a new encryption key for your application, _see_ [Create a New Encryption Key](CreateANewEncryptionKey.md).  This setting is ignored if _enablePasswordRetrieval_ is set to `false`.
  * _minSaltCharacters_.  (integer, default "30") The minimum number of randomly-generated salt characters for hashing passwords.  The password hashes and the password-reset hashes both use this value.  This can be set to as low as 0 or as high as 250.  It cannot be set higher than _maxSaltCharacters_.
  * _maxSaltCharacters_.  (integer, default "60") The maximum number of randomly-generated salt characters for hashing passwords.  The password hashes and the password-reset hashes both use this value.  This can be set to as low as 1 or as high as 250.  It cannot be set lower than _minSaltCharacters_.
  * _minRequiredPasswordLength_.  (integer, default "6") The minimum number of characters allowed to be used in a password.  It cannot be set below 0.  The password reset mechanism will auto-generate a password of this length, if it is set.
  * _lockoutTime_.  (integer, default "0") (Note that due to a MS limitation, the config attribute for this value is "timeToLockout") The number of minutes from a user lockout event before the user can attempt to log in again.  This allows an automatic "unlock" after the configured minutes.  The default value of 0 requires the user to be explicitly unlocked.
  * _sessionTime_.  (integer, default "15") The number of minutes since activity was detected on a user account before the user is considered to be "offline."
  * _dbOwner_. (string, default "security") The owner of the provider database.  Changing this can affect how some objects are created.