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
		private NameValueCollection mconfig;
		private MembershipCreateStatus status;

		[TestFixtureSetUp]
		protected void TestSetup()
		{
			provider = new pgMembershipProvider();
			mconfig = new NameValueCollection();
			mconfig.Add("connectionStringName", "pgProvider");
			mconfig.Add("enablePasswordRetrieval", "false");
			mconfig.Add("enablePasswordReset", "true");
			mconfig.Add("maxInvalidPasswordAttempts", "5");
			mconfig.Add("minRequiredNonAlphanumericCharacters", "0");
			mconfig.Add("passwordAttemptWindow", "5");
			mconfig.Add("lockoutTime", "0");
			mconfig.Add("sessionTime", "15");
			mconfig.Add("passwordStrengthRegularExpression", "");
			mconfig.Add("requiresQuestionAndAnswer", "false");
			mconfig.Add("requiresUniqueEmail", "true");
			mconfig.Add("applicationName", "NUnit Provider Test");
			mconfig.Add("encryptionKey", "");
			mconfig.Add("minSaltCharacters", "30");
			mconfig.Add("maxSaltCharacters", "60");
			mconfig.Add("minRequiredPasswordLength", "6");
		}

		protected void TestInitialize()
		{
			provider.Initialize("pgMembershipProvider", mconfig);
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

			Assert.IsTrue(status == MembershipCreateStatus.Success, "MembershipCreateStatus was not Success.");

			provider.DeleteUser("testUser", true);
			Assert.IsTrue(provider.GetUser("testUser", false) == null);
		}

		#endregion
	}
}
