using System.Collections.Specialized;
using NUnit.Framework;

namespace pgProvider.Tests.RoleProvider
{
    [TestFixture]
    public class Functionality
    {
        #region Setup
        private pgRoleProvider provider;
        private NameValueCollection config;

        [SetUp]
        public void Setup()
        {
            provider = new pgRoleProvider();
            config = new NameValueCollection();
            config.Add("connectionStringName", "pgProvider");
        }

        public void Initialize()
        {
            provider.Initialize("pgRoleProvider", config);
        }

        #endregion

		#region Tests
		[Test]
		public void ThisIsTheTestThatTestsNothing()
		{
			//because I didn't want to delete the test file until I remembered what I made it for.
			Assert.IsTrue(true);
		}

		#endregion

	}


}
