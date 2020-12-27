using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using HansKindberg.IdentityServer.Extensions;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Data.Transferring.Internal
{
	[TestClass]
	public class PartialImporterTest
	{
		#region Methods

		protected internal virtual async Task<PartialImporter<Client>> CreatePartialImporterAsync()
		{
			return await this.CreatePartialImporterAsync<Client>("ClientId", client => client.ClientId);
		}

		protected internal virtual async Task<PartialImporter<T>> CreatePartialImporterAsync<T>(string modelIdentifierName, Func<T, string> modelIdentifierSelector)
		{
			await Task.CompletedTask;

			var partialImporterMock = new Mock<PartialImporter<T>>(new NullLoggerFactory()) {CallBase = true};

			partialImporterMock.Setup(partialImporter => partialImporter.ModelIdentifierName).Returns(modelIdentifierName);
			partialImporterMock.Setup(partialImporter => partialImporter.ModelIdentifierSelector).Returns(modelIdentifierSelector);

			return partialImporterMock.Object;
		}

		[TestMethod]
		public async Task FilterOutDuplicateModelsAsync_ShouldFilterOutDuplicateIdentifiers()
		{
			const string clientId = "client";
			var partialImporter = await this.CreatePartialImporterAsync();

			var clients = new List<Client>
			{
				new Client {ClientId = clientId},
				new Client {ClientId = clientId}
			};
			var result = new DataImportResult();
			await partialImporter.FilterOutDuplicateModelsAsync(clients, result);
			Assert.IsFalse(clients.Any());
			Assert.AreEqual(1, result.Errors.Count);
			Assert.AreEqual($"Client.ClientId {clientId.ToStringRepresentation()} has 1 duplicate.", result.Errors.First());

			clients = new List<Client>
			{
				new Client {ClientId = clientId},
				new Client {ClientId = clientId},
				new Client {ClientId = clientId}
			};
			result = new DataImportResult();
			await partialImporter.FilterOutDuplicateModelsAsync(clients, result);
			Assert.IsFalse(clients.Any());
			Assert.AreEqual(1, result.Errors.Count);
			Assert.AreEqual($"Client.ClientId {clientId.ToStringRepresentation()} has 2 duplicates.", result.Errors.First());
		}

		[TestMethod]
		public async Task FilterOutInvalidModelsAsync_ShouldFilterOutEmptyIdentifiers()
		{
			var clients = new List<Client>
			{
				new Client {ClientId = string.Empty}
			};
			var partialImporter = await this.CreatePartialImporterAsync();
			var result = new DataImportResult();
			await partialImporter.FilterOutInvalidModelsAsync(clients, result);
			Assert.IsFalse(clients.Any());
			Assert.AreEqual(1, result.Errors.Count);
			Assert.AreEqual("Client.ClientId \"\" can not be empty.", result.Errors.First());
		}

		[TestMethod]
		public async Task FilterOutInvalidModelsAsync_ShouldFilterOutNullIdentifiers()
		{
			var clients = new List<Client>
			{
				new Client {ClientId = null}
			};
			var partialImporter = await this.CreatePartialImporterAsync();
			var result = new DataImportResult();
			await partialImporter.FilterOutInvalidModelsAsync(clients, result);
			Assert.IsFalse(clients.Any());
			Assert.AreEqual(1, result.Errors.Count);
			Assert.AreEqual("Client.ClientId can not be null.", result.Errors.First());
		}

		[TestMethod]
		public async Task FilterOutInvalidModelsAsync_ShouldFilterOutWhitespacesOnlyIdentifiers()
		{
			var clients = new List<Client>
			{
				new Client {ClientId = "    "}
			};
			var partialImporter = await this.CreatePartialImporterAsync();
			var result = new DataImportResult();
			await partialImporter.FilterOutInvalidModelsAsync(clients, result);
			Assert.IsFalse(clients.Any());
			Assert.AreEqual(1, result.Errors.Count);
			Assert.AreEqual("Client.ClientId \"    \" can not be whitespaces only.", result.Errors.First());
		}

		#endregion
	}
}