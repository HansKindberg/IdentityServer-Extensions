using System;
using HansKindberg.IdentityServer.Configuration;
using IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Data.Transferring
{
	[TestClass]
	public class DataExporterTest
	{
		#region Methods

		[TestCleanup]
		public void Cleanup()
		{
			AppDomain.CurrentDomain.SetData(ConfigurationKeys.DataDirectoryPath, null);
			DatabaseHelper.DeleteIdentityServerDatabase();
		}

		[TestInitialize]
		public void Initialize()
		{
			this.Cleanup();
		}

		#endregion
	}
}