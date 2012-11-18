using System.Collections.Specialized;
using NUnit.Framework;
using System;
using System.Configuration.Provider;

namespace pgProvider.Tests.RoleProvider
{
	[TestFixture]
	public class ProviderCompliance
	{
		#region Setup
		private pgRoleProvider provider;
		private NameValueCollection config;
		private NameValueCollection mconfig;
		private System.Web.Security.MembershipCreateStatus status;
		private static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetCurrentClassLogger();

		[SetUp]
		public void Setup()
		{
			provider = new pgRoleProvider();
			config = new NameValueCollection();
			config.Add("connectionStringName", "pgProvider");
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
			mconfig.Add("encryptionKey", "");
			mconfig.Add("minSaltCharacters", "30");
			mconfig.Add("maxSaltCharacters", "60");
			mconfig.Add("minRequiredPasswordLength", "6");
		}

		public void Initialize()
		{
			provider.Initialize("pgRoleProvider", config);
		}

		#endregion

		[Test]
		public void RoleNameCannotHaveCommas()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.CreateRole("testRole, interrupted"));
		}

		[Test]
		public void RoleNameCannotBeEmpty()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.CreateRole(string.Empty));
		}

		[Test]
		public void RoleNameCannotBeJustWhiteSpace()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.CreateRole("     "));
		}

		[Test]
		public void RoleNameCannotExceed250Chars()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.CreateRole(
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901"));
		}

		[Test]
		public void RoleNameCannotBeNull()
		{
			Initialize();
			Assert.Throws<ArgumentNullException>(() => provider.CreateRole(null));
		}

		[Test]
		public void RoleAddedToUsersMustExistInApplication()
		{
			Initialize();
			var mprov = new pgMembershipProvider();
			mprov.Initialize("pgMembershipProvider", mconfig);
			mprov.DeleteUser("roleTestUser", true);
			var user = mprov.CreateUser("roleTestUser", "foo12345", "foo@foo", "", "", true, null, out status);
			Assert.Throws<ProviderException>(() => provider.AddUsersToRoles(new string[] { user.UserName }, new string[] { "NonExistentRole" }));
			mprov.DeleteUser("roleTestUser", true);
		}

		[Test]
		public void UserAddedToRolesMustExistInTheApplication()
		{
			Initialize();
			if (provider.RoleExists("testRole")) provider.DeleteRole("testRole", false);
			provider.CreateRole("testRole");
			Assert.Throws<ProviderException>(() => provider.AddUsersToRoles(new string[] { "NonExistantUser" }, new string[] { "testRole" }));
			provider.DeleteRole("testRole", false);
		}

		[Test]
		public void RoleNameCannotAlreadyExistWhenBeingAdded()
		{
			Initialize();
			if (provider.RoleExists("testRole")) provider.DeleteRole("testRole", false);
			provider.CreateRole("testRole");
			Assert.Throws<ProviderException>(() => provider.CreateRole("testRole"));
			provider.DeleteRole("testRole", false);
		}

		[Test]
		public void WhenThrowOnPopulatedIsSetPreventDeleteRoleWhenPopulated()
		{
			Initialize();
			var mprov = new pgMembershipProvider();
			mprov.Initialize("pgMembershipProvider", mconfig);
			mprov.DeleteUser("roleTestUser", true);
			var user = mprov.CreateUser("roleTestUser", "foo12345", "foo@foo", "", "", true, null, out status);
			if (provider.RoleExists("testRole")) provider.DeleteRole("testRole", false);
			provider.CreateRole("testRole");
			provider.AddUsersToRoles(new string[] { user.UserName }, new string[] { "testRole" });
			Assert.Throws<ProviderException>(() => provider.DeleteRole("testRole", true));
			mprov.DeleteUser("roleTestUser", true);
			provider.DeleteRole("testRole", false);
		}

		[Test]
		public void AllowDeleteOfPopulatedRoleWhenSpecified()
		{
			Initialize();
			var mprov = new pgMembershipProvider();
			mprov.Initialize("pgMembershipProvider", mconfig);
			mprov.DeleteUser("roleTestUser", true);
			var user = mprov.CreateUser("roleTestUser", "foo12345", "foo@foo", "", "", true, null, out status);
			Assert.IsNotNull(user, "User was not properly created.");
			if (provider.RoleExists("testRole")) provider.DeleteRole("testRole", false);
			provider.CreateRole("testRole");
			Assert.IsTrue(provider.RoleExists("testRole"));
			Log.Debug(string.Format("Available Roles: {0}", string.Join(", ", provider.GetAllRoles())));
			provider.AddUsersToRoles(new string[] { "roleTestUser" }, new string[] { "testRole" });
			Assert.IsTrue(provider.DeleteRole("testRole", false));
			mprov.DeleteUser("roleTestUser", true);
		}

		[Test]
		public void DeleteRoleDoesNotAcceptNullRoleNames()
		{
			Initialize();
			Assert.Throws<ArgumentNullException>(() => provider.DeleteRole(null, false));
		}

		[Test]
		public void DeleteRoleDoesNotAcceptEmptyRoleNames()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.DeleteRole(string.Empty, false));
		}

		[Test]
		public void DeleteRoleDoesNotAcceptWhiteSpaceRoleNames()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.DeleteRole("     ", false));
		}

		[Test]
		public void DeleteRoleThrowsWhenRoleDoesNotExist()
		{
			Initialize();
			Assert.Throws<ArgumentException>(() => provider.DeleteRole("NonExistentRole", false));
		}

		[Test]
		public void FindUsersInRoleThrowsProviderExceptionWhenRoleDoesNotExist()
		{
			Initialize();
			Assert.Throws<ProviderException>(() => provider.FindUsersInRole("NonexistantRole", ""));
		}

	}
}
