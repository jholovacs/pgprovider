using System;
using System.Collections.Specialized;
using System.Web.Security;
using NUnit.Framework;

namespace pgProvider.Tests.MembershipProvider
{
	[TestFixture]
	public class UserLists
	{

		#region Setup

		private pgMembershipProvider provider;
		private NameValueCollection config;
		private MembershipCreateStatus status;
		private MembershipUser user;
		private string defaultPassword;

		[SetUp]
		protected void TestSetup()
		{
			provider = new pgMembershipProvider();
			config = new NameValueCollection();
			defaultPassword = "foo12345";

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
			provider.DeleteUser("testUser", true);
			user = provider.CreateUser(
				"testUser",
				defaultPassword,
				"test@foo.com",
				"What is your favorite color?",
				"Blue!",
				true,
				null,
				out status);
		}

		[TearDown]
		protected void TestTeardown()
		{
			if (user != null)
			{
				provider.DeleteUser(user.UserName, true);
			}
		}

		#endregion

		#region Tests

		[Test]
		public void UserListAll()
		{
			TestInitialize();
			int totalRecords;
			var list = provider.GetAllUsers(0, 50, out totalRecords);
			Assert.IsTrue(totalRecords > 0);
		}

		[Test]
		public void UserListFindByEmail()
		{
			TestInitialize();
			int totalRecords;
			var list = provider.FindUsersByEmail("test", 0, 50, out totalRecords);
			Assert.IsTrue(totalRecords > 0);
		}

		[Test]
		public void UserListFindByUserName()
		{
			TestInitialize();
			int totalRecords;
			var list = provider.FindUsersByName("User", 0, 50, out totalRecords);
			Assert.IsTrue(totalRecords > 0);
		}

		[Test]
		public void GetUserNameByEmail()
		{
			TestInitialize();
			var username = provider.GetUserNameByEmail("test@foo.com");
			Assert.IsTrue(username == "testUser");
		}

		[Test]
		public void GetUserNameByEmailFailsIfEmailNotUnique()
		{
			config["requiresUniqueEmail"] = "false";
			Assert.Throws<InvalidOperationException>(() => provider.GetUserNameByEmail("test@foo.com"));
		}

		#endregion

	}
}
