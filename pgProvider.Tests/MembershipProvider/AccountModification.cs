using System.Collections.Specialized;
using System.Web.Security;
using NUnit.Framework;
using System;

namespace pgProvider.Tests.MembershipProvider
{
	[TestFixture]
	public class AccountModification
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
		public void LastLoginUpdates()
		{
			TestInitialize();
			var user1 = provider.GetUser(user.UserName, false);
			Assert.IsTrue(user1.LastLoginDate == DateTime.MinValue, "Expected no last login date, there was a login date.");
			Assert.IsTrue(provider.ValidateUser(user.UserName, defaultPassword));
			var user2 = provider.GetUser(user.UserName, false);
			Assert.IsTrue(user2.LastLoginDate != DateTime.MinValue, "Expected a last login date, there wasn't a login date.");
		}

		[Test]
		public void CaseInsensitiveGetUser()
		{
			TestInitialize();
			var user2 = provider.GetUser(user.UserName.ToLowerInvariant(), false);
			var user3 = provider.GetUser(user.UserName.ToUpperInvariant(), false);
			Assert.IsTrue((int)user.ProviderUserKey == (int)user2.ProviderUserKey);
			Assert.IsTrue((int)user.ProviderUserKey == (int)user3.ProviderUserKey);
		}

		[Test]
		public void CaseInsensitiveGetUserNameByEmail()
		{
			TestInitialize();
			var user2 = provider.GetUserNameByEmail(user.Email.ToLowerInvariant());
			var user3 = provider.GetUserNameByEmail(user.Email.ToUpperInvariant());
			Assert.IsTrue(user.UserName == user2);
			Assert.IsTrue(user.UserName == user3);
		}

		[Test]
		public void CaseInsensitiveValidate()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(user.UserName.ToUpperInvariant(), defaultPassword));
			Assert.IsTrue(provider.ValidateUser(user.UserName.ToLowerInvariant(), defaultPassword));
		}

		[Test]
		public void CommentChangesSave()
		{
			TestInitialize();
			user.Comment = "foo";
			provider.UpdateUser(user);
			var modifiedUser = provider.GetUser(user.UserName, false);
			Assert.IsTrue(modifiedUser.Comment == "foo");
		}

		[Test]
		public void ApprovalChangesSave()
		{
			TestInitialize();
			user.IsApproved = false;
			provider.UpdateUser(user);
			var modifiedUser = provider.GetUser(user.UserName, false);
			Assert.IsTrue(modifiedUser.IsApproved == false);
		}

		[Test]
		public void GoodValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(user.UserName, defaultPassword));
		}

		[Test]
		public void BadPasswordValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(user.UserName, "foo54321") == false);
		}

		[Test]
		public void EmptyPasswordValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(user.UserName, string.Empty) == false);
		}

		[Test]
		public void NullPasswordValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(user.UserName, null) == false);
		}

		[Test]
		public void NullUserNameValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(null, defaultPassword) == false);
		}

		[Test]
		public void EmptyUserNameValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser(string.Empty, defaultPassword) == false);
		}

		[Test]
		public void WrongUserNameValidation()
		{
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser("bogusUser", defaultPassword) == false);
		}

		[Test]
		public void ChangePasswordBaseline()
		{
			TestInitialize();
			Assert.IsTrue(provider.ChangePassword(user.UserName, defaultPassword, "foo54321"));
			Assert.IsTrue(provider.ValidateUser(user.UserName, "foo54321"));
			Assert.IsTrue(provider.ValidateUser(user.UserName, defaultPassword) == false);
		}

		[Test]
		public void ChangePasswordMinLengthBorderCase()
		{
			config["minRequiredPasswordLength"] = "8";
			TestInitialize();
			Assert.IsTrue(provider.ChangePassword(user.UserName, defaultPassword, "foo54321"));
			Assert.IsTrue(provider.ValidateUser(user.UserName, "foo54321"));
		}

		[Test]
		public void ChangePasswordMinLengthUnderLimit()
		{
			config["minRequiredPasswordLength"] = "8";
			TestInitialize();
			Assert.IsTrue(provider.ChangePassword(user.UserName, defaultPassword, "foo5432") == false);
			Assert.IsTrue(provider.ValidateUser(user.UserName, defaultPassword));
		}

		[Test]
		public void ChangePasswordMinNonAlphanumericsBaseline()
		{
			config["minRequiredNonAlphanumericCharacters"] = "2";
			defaultPassword = "foo12345#$";
			TestInitialize();
			Assert.IsTrue(provider.ValidateUser("testUser", defaultPassword));
		}

		[Test]
		public void ChangePasswordMinNonAlphanumericsNoNAN()
		{
			config["minRequiredNonAlphanumericCharacters"] = "2";
			TestInitialize();
			Assert.IsTrue(user == null);
		}

		[Test]
		public void ChangePasswordRegExBaseline()
		{
			config["passwordStrengthRegularExpression"] = @"^.*(?=.{6,})(?=.*\d).*$";
			TestInitialize();
			Assert.IsTrue(user != null);
		}

		[Test]
		public void ChangePasswordRegExNoMatch()
		{
			config["passwordStrengthRegularExpression"] = @"^.*(?=.{6,})(?=.*\d).*$";
			defaultPassword = "foofoofoo"; //no digits
			TestInitialize();
			Assert.IsTrue(user == null);
		}

        [Test]
        public void UniqueEmailConstraintPreventsDuplicateEmail()
        {
            TestInitialize();
            var testUser2 = provider.CreateUser(
                "testUser2",
                defaultPassword,
                "test@foo.com",
                "What is your favorite color?",
                "Blue!",
                true,
                null,
                out status);
            Assert.IsNull(testUser2);
            Assert.IsTrue(status == MembershipCreateStatus.DuplicateEmail);
        }

        [Test]
        public void DuplicateUserNameReturnsProperMessage()
        {
            TestInitialize();
            var testUser2 = provider.CreateUser(
                "testUser",
                defaultPassword,
                "test@foo4U.com",
                "What is your favorite color?",
                "Blue!",
                true,
                null,
                out status);
            Assert.IsNull(testUser2);
            Assert.IsTrue(status == MembershipCreateStatus.DuplicateUserName);
         }

		#endregion
	}
}
