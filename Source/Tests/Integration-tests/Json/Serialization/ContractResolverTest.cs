using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rsk.Saml.Models;

namespace IntegrationTests.Json.Serialization
{
	[TestClass]
	public class ContractResolverTest
	{
		#region Fields

		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new ContractResolver(),
			Formatting = Formatting.None,
			NullValueHandling = NullValueHandling.Ignore
		};

		#endregion

		#region Properties

		protected internal virtual JsonSerializerSettings JsonSerializerSettings => _jsonSerializerSettings;

		#endregion

		#region Methods

		[TestMethod]
		public async Task Serialize_SamlServiceProvider_Test()
		{
			await Task.CompletedTask;

			var serviceProvider = new ServiceProvider();
			var json = JsonConvert.SerializeObject(serviceProvider, this.JsonSerializerSettings);
			Assert.AreEqual("{\"AllowIdpInitiatedSso\":false,\"EncryptAssertions\":false,\"RequireSamlMessageDestination\":true,\"SignAssertions\":true}", json);

			serviceProvider = new ServiceProvider
			{
				ClaimsMapping = new Dictionary<string, string>()
			};
			json = JsonConvert.SerializeObject(serviceProvider, this.JsonSerializerSettings);
			Assert.AreEqual("{\"AllowIdpInitiatedSso\":false,\"EncryptAssertions\":false,\"RequireSamlMessageDestination\":true,\"SignAssertions\":true}", json);

			serviceProvider = new ServiceProvider
			{
				ClaimsMapping = new Dictionary<string, string>
				{
					{"Key-1", "Value-1"},
					{"Key-2", "Value-2"},
					{"Key-3", "Value-3"}
				}
			};
			json = JsonConvert.SerializeObject(serviceProvider, this.JsonSerializerSettings);
			Assert.AreEqual("{\"AllowIdpInitiatedSso\":false,\"ClaimsMapping\":{\"Key-1\":\"Value-1\",\"Key-2\":\"Value-2\",\"Key-3\":\"Value-3\"},\"EncryptAssertions\":false,\"RequireSamlMessageDestination\":true,\"SignAssertions\":true}", json);
		}

		#endregion
	}
}