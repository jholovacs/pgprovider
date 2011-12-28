using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.Specialized;
using System.Web.Security;

namespace pgProvider.Tests.MembershipProvider
{	
	[TestFixture]
	public class Compliance
	{
		[Test]
		public void SameUserNameDifferentApplicationsDifferentUsers()
		{
			var provider = new pgMembershipProvider();
			var config = new NameValueCollection();
			var provider2 = new pgMembershipProvider();
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

			provider.Initialize("pgMembershipProvider", config);
			config["applicationName"] = "NUnit Provider Test2";
			provider2.Initialize("pgMembershipProvider2", config);

			provider.DeleteUser("foo", true);
			provider2.DeleteUser("foo", true);

			MembershipCreateStatus status;
			var user1 = provider.CreateUser("foo", "bar12345", "foo@bar.com", string.Empty, string.Empty, true, null, out status);
			Assert.IsNotNull(user1);
			Assert.IsTrue(status == MembershipCreateStatus.Success);

			var user2 = provider2.CreateUser("foo", "bar12345", "foo@bar.com", string.Empty, string.Empty, true, null, out status);
			Assert.IsNotNull(user2);
			Assert.IsTrue(status == MembershipCreateStatus.Success);

			provider.DeleteUser("foo", true);
			provider2.DeleteUser("foo", true);
		}

	}
}
