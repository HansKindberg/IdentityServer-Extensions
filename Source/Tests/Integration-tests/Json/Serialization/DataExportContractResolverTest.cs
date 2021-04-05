using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity.Models;
using HansKindberg.IdentityServer.Json.Serialization;
using IdentityServer4.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rsk.Saml.Models;

namespace IntegrationTests.Json.Serialization
{
	[TestClass]
	public class DataExportContractResolverTest
	{
		#region Fields

		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new DataExportContractResolver(),
			Formatting = Formatting.None,
			NullValueHandling = NullValueHandling.Ignore
		};

		#endregion

		#region Properties

		protected internal virtual JsonSerializerSettings JsonSerializerSettings => _jsonSerializerSettings;

		#endregion

		#region Methods

		[TestMethod]
		public async Task Serialize_ApiResource_Test()
		{
			await Task.CompletedTask;

			var apiResource = new ApiResource();
			var json = JsonConvert.SerializeObject(apiResource, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			apiResource = new ApiResource
			{
				Name = "name"
			};
			json = JsonConvert.SerializeObject(apiResource, this.JsonSerializerSettings);
			Assert.AreEqual("{\"Name\":\"name\"}", json);
		}

		[TestMethod]
		public async Task Serialize_ApiScope_Test()
		{
			await Task.CompletedTask;

			var apiScope = new ApiScope();
			var json = JsonConvert.SerializeObject(apiScope, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			apiScope = new ApiScope
			{
				Name = "name"
			};
			json = JsonConvert.SerializeObject(apiScope, this.JsonSerializerSettings);
			Assert.AreEqual("{\"Name\":\"name\"}", json);
		}

		[TestMethod]
		public async Task Serialize_Client_Test()
		{
			await Task.CompletedTask;

			var client = new Client();
			var json = JsonConvert.SerializeObject(client, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			client = new Client
			{
				ClientId = "client"
			};
			json = JsonConvert.SerializeObject(client, this.JsonSerializerSettings);
			Assert.AreEqual("{\"ClientId\":\"client\"}", json);

			client = new Client
			{
				AllowedCorsOrigins = new HashSet<string>(new[] {"http://origin-a", "http://origin-b", "http://origin-c"}, StringComparer.OrdinalIgnoreCase),
				ClientId = "client",
				Description = "Description",
				LogoUri = "http://logo-uri/"
			};
			json = JsonConvert.SerializeObject(client, this.JsonSerializerSettings);
			Assert.AreEqual("{\"AllowedCorsOrigins\":[\"http://origin-a\",\"http://origin-b\",\"http://origin-c\"],\"ClientId\":\"client\",\"Description\":\"Description\",\"LogoUri\":\"http://logo-uri/\"}", json);
		}

		[TestMethod]
		public async Task Serialize_IdentityResource_Test()
		{
			await Task.CompletedTask;

			var identityResource = new IdentityResource();
			var json = JsonConvert.SerializeObject(identityResource, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			identityResource = new IdentityResource
			{
				Name = "name"
			};
			json = JsonConvert.SerializeObject(identityResource, this.JsonSerializerSettings);
			Assert.AreEqual("{\"Name\":\"name\"}", json);
		}

		[TestMethod]
		public async Task Serialize_SamlServiceProvider_Test()
		{
			await Task.CompletedTask;

			var serviceProvider = new ServiceProvider();
			var json = JsonConvert.SerializeObject(serviceProvider, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			serviceProvider = new ServiceProvider
			{
				AllowIdpInitiatedSso = true,
				AssertionConsumerServices =
				{
					new Service
					{
						Binding = "Binding-1"
					}
				},
				ClaimsMapping = new Dictionary<string, string>
				{
					{"Key-1", "Value-1"}
				},
				SingleLogoutServices =
				{
					new Service
					{
						Binding = "Binding-1",
						Index = 1,
						IsDefault = true,
						Location = "Location-1"
					},
					new Service
					{
						Binding = "Binding-2"
					}
				}
			};
			json = JsonConvert.SerializeObject(serviceProvider, this.JsonSerializerSettings);
			Assert.AreEqual("{\"AllowIdpInitiatedSso\":true,\"AssertionConsumerServices\":[{\"Binding\":\"Binding-1\"}],\"ClaimsMapping\":{\"Key-1\":\"Value-1\"},\"SingleLogoutServices\":[{\"Binding\":\"Binding-1\",\"Index\":1,\"IsDefault\":true,\"Location\":\"Location-1\"},{\"Binding\":\"Binding-2\"}]}", json);
		}

		[TestMethod]
		public async Task Serialize_User_Test()
		{
			await Task.CompletedTask;

			var user = new User();
			var json = JsonConvert.SerializeObject(user, this.JsonSerializerSettings);
			Assert.AreEqual("{}", json);

			user = new User
			{
				Email = "user@example.com",
				Password = "?",
				UserName = "user-name"
			};
			json = JsonConvert.SerializeObject(user, this.JsonSerializerSettings);
			Assert.AreEqual("{\"Email\":\"user@example.com\",\"Password\":\"?\",\"UserName\":\"user-name\"}", json);
		}

		#endregion
	}
}