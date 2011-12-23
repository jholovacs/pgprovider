using System.Collections.Specialized;
using NUnit.Framework;
using System.Web.Security;

namespace pgProvider.Tests.MembershipProvider
{
	[TestFixture]
	public class AccountCreationAndDeletion
	{
		#region Setup

		private pgMembershipProvider provider;
		private NameValueCollection config;
		private MembershipCreateStatus status;

		[TestFixtureSetUp]
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

		#region Tests
		[Test]
		public void BaselineUserCreationAndDeletion()
		{
			TestInitialize();
			provider.CreateUser(
				"testUser",
				"foo12345",
				"test@foo.com",
				"What is your favorite color?",
				"Blue!",
				true,
				null,
				out status);

			Assert.IsTrue(status == MembershipCreateStatus.Success);

			provider.DeleteUser("testUser", true);
			Assert.IsTrue(provider.GetUser("testUser", false) == null);
		}

		#endregion
	}
}
