//using System;
//using System.Threading.Tasks;
//using Duende.IdentityServer.EntityFramework.Interfaces;
//using Duende.IdentityServer.Validation;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.FeatureManagement;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using HansKindberg.IdentityServer.Data.Transferring;

//namespace UnitTests.Models.Data.Transferring
//{
//	[TestClass]
//	public class DataImporterTest
//	{
//		#region Methods

//		protected internal virtual async Task<DataImporter> CreateDataImporterAsync(bool identityFeatureEnabled)
//		{
//			return new DataImporter(Mock.Of<IClientConfigurationValidator>(), new Mock<DbContext>().As<IConfigurationDbContext>().Object, await this.CreateFeatureManagerAsync(identityFeatureEnabled), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceProvider>());
//		}

//		protected internal virtual async Task<IFeatureManager> CreateFeatureManagerAsync(bool identityFeatureEnabled)
//		{
//			await Task.CompletedTask;

//			var featureManagerMock = new Mock<IFeatureManager>();

//			featureManagerMock.Setup(featureManager => featureManager.IsEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(identityFeatureEnabled));

//			return featureManagerMock.Object;
//		}

//		#endregion
//	}
//}

