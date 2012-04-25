using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.Specialized;

namespace pgProvider.Tests.RoleProvider
{
    [TestFixture]
    public class RoleCreationAndDeletion
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

        [Test]
        public void CreateRole()
        {
            Initialize();
            provider.CreateRole("testRole");
            provider.DeleteRole("testRole", true);
            Assert.IsTrue(true);
        }


    }
}
