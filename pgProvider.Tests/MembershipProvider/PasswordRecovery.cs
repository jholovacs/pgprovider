using System;
using System.Collections.Specialized;
using System.Web.Security;
using NUnit.Framework;

namespace pgProvider.Tests.MembershipProvider
{
	[TestFixture]
	public class PasswordRecovery
	{
		#region Setup

		private pgMembershipProvider provider;
		private NameValueCollection config;
		private MembershipCreateStatus status;
		private MembershipUser user;
		private string defaultPassword;
		private static string encryptionKey = EncryptionHelper.GenerateAESKey().ToBase64();

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
			config.Add("encryptionKey", encryptionKey);
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
		public void PasswordCanBeRetrievedBaseline()
		{
			config["enablePasswordRetrieval"] = "true";
			TestInitialize();
			var password = provider.GetPassword(user.UserName, "Blue!");
			Assert.IsTrue(password == defaultPassword);
		}

		[Test]
		public void PasswordCannotBeRetrievedWithBadAnswer()
		{
			config["enablePasswordRetrieval"] = "true";
			TestInitialize();
			var password = provider.GetPassword(user.UserName, "No, yellow!");
			Assert.IsTrue(password != defaultPassword);
		}

		[Test]
		public void AnswerTextOnlyLooksAtAlphanumericsCaseInsensitive()
		{
			config["enablePasswordRetrieval"] = "true";
			TestInitialize();
			var password = provider.GetPassword(user.UserName, "blue.");
			Assert.IsTrue(password == defaultPassword);
		}

		[Test]
		public void PasswordCannotBeRetrievedIfDisabled()
		{
			config["enablePasswordRetrieval"] = "false";
			TestInitialize();
			Assert.Throws<InvalidOperationException>(()=>provider.GetPassword(user.UserName, "Blue!"));
		}

		[Test]
		public void ResetPasswordResetsPassword()
		{
			config["enablePasswordReset"] = "true";
			config["requiresQuestionAndAnswer"] = "true";
			TestInitialize();
			var newpass = provider.ResetPassword(user.UserName, "Blue!");
			Assert.IsTrue(provider.ValidateUser(user.UserName, newpass));
		}

		[Test]
		public void ResetPasswordFailsWithoutEnabledPasswordReset()
		{
			config["enablePasswordReset"] = "false";
			config["requiresQuestionAndAnswer"] = "true";
			TestInitialize();
			Assert.Throws<InvalidOperationException>(()=>provider.ResetPassword(user.UserName, "Blue!"));
		}

		[Test]
		public void ResetPasswordWorksWithoutRequiredQuestionAndAnswer()
		{
			config["enablePasswordReset"] = "true";
			config["requiresQuestionAndAnswer"] = "false";
			TestInitialize();
			var newPass = provider.ResetPassword(user.UserName, string.Empty);
            Assert.IsTrue(provider.ValidateUser(user.UserName, newPass));
		}

		[Test]
		public void RetreivedPasswordCanBeValidated()
		{
			config["enablePasswordRetrieval"] = "true";
			TestInitialize();
			var password = provider.GetPassword(user.UserName, "Blue!");
			Assert.IsTrue(provider.PasswordFormat == MembershipPasswordFormat.Encrypted);
			Assert.IsTrue(provider.ValidateUser(user.UserName, password));
		}

		[Test]
		public void HashedPasswordCanBeValidated()
		{
			config["enablePasswordRetrieval"] = "false";
			TestInitialize();
			Assert.IsTrue(provider.PasswordFormat == MembershipPasswordFormat.Hashed);
			Assert.IsTrue(provider.ValidateUser(user.UserName, defaultPassword));
		}

		[Test]
		public void QuestionAndAnswerCanBeChanged()
		{
			config["enablePasswordRetrieval"] = "true";
			config["requiresQuestionAndAnswer"] = "true";
			TestInitialize();
			var altQuestion = "What is the average velocity of a laden swallow?";
			var altAnswer = "African or European?";
			provider.ChangePasswordQuestionAndAnswer(user.UserName, defaultPassword, altQuestion, altAnswer);
			var newUser = provider.GetUser(user.UserName, false);
			Assert.IsTrue(newUser.PasswordQuestion == altQuestion);
			Assert.IsTrue(provider.GetPassword(user.UserName, altAnswer) == defaultPassword);
		}

		#endregion
	}
}
