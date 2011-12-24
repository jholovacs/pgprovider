using System.Collections.Specialized;
using System.Web.Security;
using log4net;
using NUnit.Framework;

namespace pgProvider.Tests.MembershipProvider
{
    [TestFixture]
    public class SecurityFeatures
    {

        #region Setup

        private pgMembershipProvider provider;
        private NameValueCollection config;
        private MembershipCreateStatus status;
        private MembershipUser user;
        private string defaultPassword;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityFeatures));

        [TestFixtureSetUp]
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

        protected void Initialize()
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

        [TestFixtureTearDown]
        protected void TestTeardown()
        {
            provider.DeleteUser("testUser", true);
        }

        #endregion

        #region Tests

        [Test]
        public void AccountLocksOut()
        {
            Initialize();
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            var refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsTrue(refreshedUser.IsLockedOut);
        }

        [Test]
        public void AccountDoesntLockOutBeforeItsSupposedTo()
        {
            Initialize();
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            var refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsFalse(refreshedUser.IsLockedOut);
        }

        [Test]
        public void AccountCanBeUnlocked()
        {
            Initialize();
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            var refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsTrue(refreshedUser.IsLockedOut);
            provider.UnlockUser(user.UserName);
            refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsFalse(refreshedUser.IsLockedOut);
        }

        [Test]
        public void AccountDoesNotLockOutWhenMaxPasswordAttemptsIsSetTo0()
        {
            config["maxInvalidPasswordAttempts"] = "0";
            Initialize();
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            Assert.IsFalse(provider.ValidateUser(user.UserName, "foofoofoo"));
            var refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsFalse(refreshedUser.IsLockedOut);
        }

        [Test]
        public void LockedOutUserCannotValidate()
        {
            Initialize();
            provider.ValidateUser(user.UserName, "foofoofoo");
            provider.ValidateUser(user.UserName, "foofoofoo");
            provider.ValidateUser(user.UserName, "foofoofoo");
            provider.ValidateUser(user.UserName, "foofoofoo");
            provider.ValidateUser(user.UserName, "foofoofoo");
            provider.ValidateUser(user.UserName, "foofoofoo");
            var refreshedUser = provider.GetUser(user.UserName, false);
            Assert.IsTrue(refreshedUser.IsLockedOut);
            Assert.IsFalse(provider.ValidateUser(user.UserName, defaultPassword));
        }

        [Test]
        public void NotApprovedUserCannotValidate()
        {
            Initialize();
            user.IsApproved = false;
            provider.UpdateUser(user);
            Assert.IsFalse(provider.ValidateUser(user.UserName, defaultPassword));
        }

        #endregion

    }
}
