using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Data.Transferring.Internal
{
	[TestClass]
	public class EntityImporterTest
	{
		#region Methods

		protected internal virtual async Task<EntityImporter<object, object>> CreateEntityImporterAsync()
		{
			await Task.CompletedTask;

			var entityImporterMock = new Mock<EntityImporter<object, object>>(Mock.Of<ILoggerFactory>()) {CallBase = true};

			return entityImporterMock.Object;
		}

		[TestMethod]
		public async Task UpdateRelationsAsync_Test1()
		{
			var now = DateTime.UtcNow;

			var entityRelations = new List<ClientSecret>
			{
				new ClientSecret
				{
					Created = new DateTime(2000, 1, 1),
					Description = "Description-1",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-1",
					Value = "Value-1"
				},
				new ClientSecret
				{
					Created = new DateTime(2000, 1, 1),
					Description = "Description-2",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-2",
					Value = "Value-2"
				},
				new ClientSecret
				{
					Description = "Description-5",
					Type = "Type-5",
					Value = "Value-5"
				}
			};

			var importRelations = new List<ClientSecret>
			{
				new ClientSecret
				{
					Description = "Description-1",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-1",
					Value = "Value-1"
				},
				new ClientSecret
				{
					Created = now,
					Description = "Description-2 (changed)",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-2",
					Value = "Value-2"
				},
				new ClientSecret
				{
					Description = "Description-3",
					Type = "Type-3",
					Value = "Value-3"
				},
				new ClientSecret
				{
					Description = "Description-4",
					Type = "Type-4",
					Value = "Value-4"
				}
			};

			var entityImporter = await this.CreateEntityImporterAsync();
			var entity = new object();
			var import = new object();

			await entityImporter.UpdateRelationsAsync(
				(relations, importRelationItemToMatch) =>
				{
					// We can add more conditions here if necessary.
					return relations.FirstOrDefault(relation => string.Equals(relation.Value, importRelationItemToMatch.Value, StringComparison.Ordinal));
				},
				(entityItem, importItem) =>
				{
					const StringComparison stringComparison = StringComparison.Ordinal;

					if(
						!string.Equals(entityItem.Description, importItem.Description, stringComparison) ||
						Nullable.Compare(entityItem.Expiration, importItem.Expiration) != 0 ||
						!string.Equals(entityItem.Type, importItem.Type, stringComparison) ||
						!string.Equals(entityItem.Value, importItem.Value, stringComparison)
					)
						entityItem.Created = importItem.Created;

					entityItem.Description = importItem.Description;
					entityItem.Expiration = importItem.Expiration;
					entityItem.Type = importItem.Type;
					entityItem.Value = importItem.Value;
				},
				instance =>
				{
					if(ReferenceEquals(instance, entity))
						return entityRelations;

					if(ReferenceEquals(instance, import))
						return importRelations;

					throw new InvalidOperationException("Invalid test-instance");
				},
				new Dictionary<object, object> {{entity, import}}
			);

			Assert.AreEqual(4, entityRelations.Count);

			var entityRelation = entityRelations[0];
			Assert.AreEqual(new DateTime(2000, 1, 1), entityRelation.Created);
			Assert.AreEqual("Description-1", entityRelation.Description);
			Assert.AreEqual(new DateTime(5000, 1, 1), entityRelation.Expiration);
			Assert.AreEqual("Type-1", entityRelation.Type);
			Assert.AreEqual("Value-1", entityRelation.Value);

			entityRelation = entityRelations[1];
			Assert.AreEqual(now, entityRelation.Created);
			Assert.AreEqual("Description-2 (changed)", entityRelation.Description);
			Assert.AreEqual(new DateTime(5000, 1, 1), entityRelation.Expiration);
			Assert.AreEqual("Type-2", entityRelation.Type);
			Assert.AreEqual("Value-2", entityRelation.Value);

			entityRelation = entityRelations[2];
			Assert.AreEqual("Description-3", entityRelation.Description);
			Assert.IsNull(entityRelation.Expiration);
			Assert.AreEqual("Type-3", entityRelation.Type);
			Assert.AreEqual("Value-3", entityRelation.Value);

			entityRelation = entityRelations[3];
			Assert.AreEqual("Description-4", entityRelation.Description);
			Assert.IsNull(entityRelation.Expiration);
			Assert.AreEqual("Type-4", entityRelation.Type);
			Assert.AreEqual("Value-4", entityRelation.Value);
		}

		[TestMethod]
		public async Task UpdateRelationsAsync_Test2()
		{
			var now = DateTime.UtcNow;

			var entityRelations = new List<ClientSecret>
			{
				new ClientSecret
				{
					Description = "Description-5",
					Type = "Type-5",
					Value = "Value-5"
				},
				new ClientSecret
				{
					Created = new DateTime(2000, 1, 1),
					Description = "Description-2",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-2",
					Value = "Value-2"
				},
				new ClientSecret
				{
					Created = new DateTime(2000, 1, 1),
					Description = "Description-1",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-1",
					Value = "Value-1"
				}
			};

			var importRelations = new List<ClientSecret>
			{
				new ClientSecret
				{
					Description = "Description-1",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-1",
					Value = "Value-1"
				},
				new ClientSecret
				{
					Created = now,
					Description = "Description-2 (changed)",
					Expiration = new DateTime(5000, 1, 1),
					Type = "Type-2",
					Value = "Value-2"
				},
				new ClientSecret
				{
					Description = "Description-3",
					Type = "Type-3",
					Value = "Value-3"
				},
				new ClientSecret
				{
					Description = "Description-4",
					Type = "Type-4",
					Value = "Value-4"
				}
			};

			var entityImporter = await this.CreateEntityImporterAsync();
			var entity = new object();
			var import = new object();

			await entityImporter.UpdateRelationsAsync(
				(relations, importRelationItemToMatch) =>
				{
					// We can add more conditions here if necessary.
					return relations.FirstOrDefault(relation => string.Equals(relation.Value, importRelationItemToMatch.Value, StringComparison.Ordinal));
				},
				(entityItem, importItem) =>
				{
					const StringComparison stringComparison = StringComparison.Ordinal;

					if(
						!string.Equals(entityItem.Description, importItem.Description, stringComparison) ||
						Nullable.Compare(entityItem.Expiration, importItem.Expiration) != 0 ||
						!string.Equals(entityItem.Type, importItem.Type, stringComparison) ||
						!string.Equals(entityItem.Value, importItem.Value, stringComparison)
					)
						entityItem.Created = importItem.Created;

					entityItem.Description = importItem.Description;
					entityItem.Expiration = importItem.Expiration;
					entityItem.Type = importItem.Type;
					entityItem.Value = importItem.Value;
				},
				instance =>
				{
					if(ReferenceEquals(instance, entity))
						return entityRelations;

					if(ReferenceEquals(instance, import))
						return importRelations;

					throw new InvalidOperationException("Invalid test-instance");
				},
				new Dictionary<object, object> {{entity, import}}
			);

			Assert.AreEqual(4, entityRelations.Count);

			var entityRelation = entityRelations[0];
			Assert.AreEqual("Description-3", entityRelation.Description);
			Assert.IsNull(entityRelation.Expiration);
			Assert.AreEqual("Type-3", entityRelation.Type);
			Assert.AreEqual("Value-3", entityRelation.Value);

			entityRelation = entityRelations[1];
			Assert.AreEqual(now, entityRelation.Created);
			Assert.AreEqual("Description-2 (changed)", entityRelation.Description);
			Assert.AreEqual(new DateTime(5000, 1, 1), entityRelation.Expiration);
			Assert.AreEqual("Type-2", entityRelation.Type);
			Assert.AreEqual("Value-2", entityRelation.Value);

			entityRelation = entityRelations[2];
			Assert.AreEqual(new DateTime(2000, 1, 1), entityRelation.Created);
			Assert.AreEqual("Description-1", entityRelation.Description);
			Assert.AreEqual(new DateTime(5000, 1, 1), entityRelation.Expiration);
			Assert.AreEqual("Type-1", entityRelation.Type);
			Assert.AreEqual("Value-1", entityRelation.Value);

			entityRelation = entityRelations[3];
			Assert.AreEqual("Description-4", entityRelation.Description);
			Assert.IsNull(entityRelation.Expiration);
			Assert.AreEqual("Type-4", entityRelation.Type);
			Assert.AreEqual("Value-4", entityRelation.Value);
		}

		#endregion
	}
}