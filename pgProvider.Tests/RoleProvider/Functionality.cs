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

        [TestFixtureSetUp]
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

    }


}
