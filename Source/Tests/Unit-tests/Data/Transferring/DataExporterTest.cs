using System;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Data.Transferring
{
	[TestClass]
	public class DataExporterTest
	{
		#region Methods

		protected internal virtual async Task<DataExporter> CreateDataExporterAsync()
		{
			return await Task.FromResult(new DataExporter(new Mock<DbContext>().As<IConfigurationDbContext>().Object, Mock.Of<IFeatureManager>(), Mock.Of<IIdentityFacade>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceProvider>()));
		}

		[TestMethod]
		public async Task ExportableTypes_Test()
		{
			var dataExporter = await this.CreateDataExporterAsync();
			Assert.AreEqual(6, dataExporter.ExportableTypes.Count());
		}

		#endregion
	}
}