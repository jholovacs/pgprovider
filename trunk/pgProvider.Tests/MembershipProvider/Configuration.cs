using System.Collections.Specialized;
using NUnit.Framework;
using pgProvider.Exceptions;
using System;

namespace pgProvider.Tests.MembershipProvider
{
	[TestFixture]
	public class Configuration
	{
		#region Setup

		private pgMembershipProvider provider;
		private NameValueCollection config;

		[SetUp]
		protected void TestSetup()
		{
			provider = new pgMembershipProvider();
			config = new NameValueCollection();
			config.Add("connectionStringName", "pgProvider");
			config.Add("enablePasswordRetrieval", "false");
			config.Add("enablePasswordReset", "true");
			config.Add("maxInvalidPasswordAttempts", "5");
			config.Add("minRequiredNonAlphanumericCharacters", "0");
			config.Add("passwordAttemptWindow", "5");
			config.Add("lockoutTime", "0");
			config.Add("sessionTime", "15");
			config.Add("passwordStrengthRegularExpression", "");
			config.Add("requiresQuestionAndAnswer", "false");
			config.Add("requiresUniqueEmail", "true");
			config.Add("applicationName", "NUnit Provider Test");
			config.Add("encryptionKey", "");
			config.Add("minSaltCharacters", "30");
			config.Add("maxSaltCharacters", "60");
			config.Add("minRequiredPasswordLength", "6");
		}

		protected void TestInitialize()
		{
			provider.Initialize("pgMembershipProvider", config);
		}

		#endregion

		#region Configuration Settings Tests

		[Test]
		public void Baseline()
		{
			TestInitialize();
			Assert.IsTrue(provider.EnablePasswordRetrieval == false);
			Assert.IsTrue(provider.EnablePasswordReset == true);
			Assert.IsTrue(provider.ApplicationName == "NUnit Provider Test");
			Assert.IsTrue(provider.MaxInvalidPasswordAttempts == 5);
			Assert.IsTrue(provider.MinRequiredNonAlphanumericCharacters == 0);
			Assert.IsTrue(provider.MinRequiredPasswordLength == 6);
			Assert.IsTrue(provider.Name == "pgMembershipProvider");
			Assert.IsTrue(provider.PasswordAttemptWindow == 5);
			Assert.IsTrue(provider.PasswordFormat == System.Web.Security.MembershipPasswordFormat.Hashed);
			Assert.IsTrue(provider.PasswordStrengthRegularExpression == "");
			Assert.IsTrue(provider.RequiresQuestionAndAnswer == false);
			Assert.IsTrue(provider.RequiresUniqueEmail == true);
			Assert.IsTrue(provider.Description == "PostgreSQL ASP.Net MembershipProvider class");
		}

		[Test]
		public void EnablePasswordRetreival()
		{
			config["enablePasswordRetrieval"] = "true";
			config["encryptionKey"] = Encryption.GenerateAESKey().ToBase64(); //throws config error if this is not provided (see test)
			TestInitialize();
			Assert.IsTrue(provider.EnablePasswordRetrieval == true);
		}

		[Test]
		public void EnablePasswordRetrievalWithoutEncryptionKey()
		{
			config["enablePasswordRetrieval"] = "true";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void EnablePasswordReset()
		{
			config["enablePasswordReset"] = "false";
			TestInitialize();
			Assert.IsTrue(provider.EnablePasswordReset == false);
		}

		[Test]
		public void MaxInvalidPasswordAttempts()
		{
			config["maxInvalidPasswordAttempts"] = "10";
			TestInitialize();
			Assert.IsTrue(provider.MaxInvalidPasswordAttempts == 10);
		}

		[Test]
		public void MaxInvalidPasswordAttemptsBelowZero()
		{
			config["maxInvalidPasswordAttempts"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MinRequiredNonAlphanumericCharacters()
		{
			config["minRequiredNonAlphanumericCharacters"] = "22";
			TestInitialize();
			Assert.IsTrue(provider.MinRequiredNonAlphanumericCharacters == 22);
		}

		[Test]
		public void MinRequiredNonAlphanumericCharactersLessThanZero()
		{
			config["minRequiredNonAlphanumericCharacters"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void PasswordAttemptWindow()
		{
			config["passwordAttemptWindow"] = "22";
			TestInitialize();
			Assert.IsTrue(provider.PasswordAttemptWindow == 22);
		}

		[Test]
		public void PasswordAttemptWindowLessThanZero()
		{
			config["passwordAttemptWindow"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void LockoutTime()
		{
			config["lockoutTime"] = "22";
			TestInitialize();
			Assert.IsTrue(true); //just checking for failures here, there is no exposed functionality in the provider for this.
		}

		[Test]
		public void LockoutTimeLessThanZero()
		{
			config["lockoutTime"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void SessionTime()
		{
			config["sessionTime"] = "22";
			TestInitialize();
			Assert.IsTrue(true); //just checking for failures here, there is no exposed functionality in the provider for this.
		}

		[Test]
		public void SessionTimeLessThanOne()
		{
			config["sessionTime"] = "0";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void PasswordStrengthRegularExpression()
		{
			config["passwordStrengthRegularExpression"] = "foo";
			TestInitialize();
			Assert.IsTrue(provider.PasswordStrengthRegularExpression == "foo");
		}

		[Test]
		public void PasswordFormatHashed()
		{
			TestInitialize();
			Assert.IsTrue(provider.PasswordFormat == System.Web.Security.MembershipPasswordFormat.Hashed);
		}

		[Test]
		public void PasswordFormatEncrypted()
		{
			config["enablePasswordRetrieval"] = "true";
			config["encryptionKey"] = Encryption.GenerateAESKey().ToBase64(); //throws config error if this is not provided (see test)
			TestInitialize();
			Assert.IsTrue(provider.PasswordFormat == System.Web.Security.MembershipPasswordFormat.Encrypted);
		}

		[Test]
		public void RequiresQuestionAndAnswer()
		{
			config["requiresQuestionAndAnswer"] = "true";
			TestInitialize();
			Assert.IsTrue(provider.RequiresQuestionAndAnswer == true);
		}

		[Test]
		public void RequiresUniqueEmail()
		{
			config["requiresUniqueEmail"] = "false";
			TestInitialize();
			Assert.IsTrue(provider.RequiresUniqueEmail == false);
		}

		[Test]
		public void ApplicationName(){
			config["applicationName"] = "foo";
			TestInitialize();
			Assert.IsTrue(provider.ApplicationName == "foo");
		}

		[Test]
		public void MinSaltCharactersLessThanZero()
		{
			config["minSaltCharacters"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MaxSaltCharactersLessThanZero()
		{
			config["minSaltCharacters"] = "-1";
			config["maxSaltCharacters"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MinSaltCharactersMoreThan250()
		{
			config["minSaltCharacters"] = "251";
			config["maxSaltCharacters"] = "251";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MaxSaltCharactersMoreThan250()
		{
			config["maxSaltCharacters"] = "251";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MaxSaltCharactersLessThanMinSaltCharacters()
		{
			config["maxSaltCharacters"] = "25";
			config["minSaltCharacters"] = "30";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MinRequiredPasswordLength()
		{
			config["minRequiredPasswordLength"] = "22";
			TestInitialize();
			Assert.IsTrue(provider.MinRequiredPasswordLength == 22);
		}

		[Test]
		public void MinRequiredPasswordLengthLessThanZero()
		{
			config["minRequiredPasswordLength"] = "-1";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void MinRequiredPasswordLengthMoreThan100()
		{
			config["minRequiredPasswordLength"] = "101";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void BadConnectionStringName()
		{
			config["connectionStringName"] = "yoMomma";
			Assert.Throws<ProviderConfigurationException>(
				() => TestInitialize());
		}

		[Test]
		public void EnablingPasswordRetrievalSetsToEncrypted()
		{
			config["enablePasswordRetrieval"] = "true";
			config["encryptionKey"] = Encryption.GenerateAESKey().ToBase64(); //throws config error if this is not provided (see test)
			TestInitialize();
			Assert.IsTrue(provider.PasswordFormat == System.Web.Security.MembershipPasswordFormat.Encrypted);
		}

		[Test]
		public void DisablingPasswordRetrievalSetsToHashed()
		{
			TestInitialize();
			Assert.IsTrue(provider.PasswordFormat == System.Web.Security.MembershipPasswordFormat.Hashed);
		}

		#endregion

	}
}
