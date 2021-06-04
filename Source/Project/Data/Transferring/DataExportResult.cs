using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;
using HansKindberg.IdentityServer.Identity.Models;
using HansKindberg.IdentityServer.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rsk.Saml.Models;
using Rsk.WsFederation.Models;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataExportResult : IDataExportResult
	{
		#region Fields

		private static readonly IContractResolver _defaultJsonContractResolver = new DataExportContractResolver();

		#endregion

		#region Properties

		public virtual IEnumerable<ApiResource> ApiResources => this.Instances.TryGetValue(typeof(ApiResource), out var apiResources) ? apiResources.OfType<ApiResource>() : Enumerable.Empty<ApiResource>();
		public virtual IEnumerable<ApiScope> ApiScopes => this.Instances.TryGetValue(typeof(ApiScope), out var apiScopes) ? apiScopes.OfType<ApiScope>() : Enumerable.Empty<ApiScope>();
		public virtual IEnumerable<Client> Clients => this.Instances.TryGetValue(typeof(Client), out var clients) ? clients.OfType<Client>() : Enumerable.Empty<Client>();
		protected internal virtual IContractResolver DefaultJsonContractResolver => _defaultJsonContractResolver;
		public virtual IEnumerable<IdentityResource> IdentityResources => this.Instances.TryGetValue(typeof(IdentityResource), out var identityResources) ? identityResources.OfType<IdentityResource>() : Enumerable.Empty<IdentityResource>();

		[JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public virtual IDictionary<Type, IEnumerable<object>> Instances { get; } = new Dictionary<Type, IEnumerable<object>>();

		public virtual IEnumerable<RelyingParty> RelyingParties => this.Instances.TryGetValue(typeof(RelyingParty), out var relyingParties) ? relyingParties.OfType<RelyingParty>() : Enumerable.Empty<RelyingParty>();
		public virtual IEnumerable<ServiceProvider> ServiceProviders => this.Instances.TryGetValue(typeof(ServiceProvider), out var serviceProviders) ? serviceProviders.OfType<ServiceProvider>() : Enumerable.Empty<ServiceProvider>();
		public virtual IEnumerable<UserLogin> UserLogins => this.Instances.TryGetValue(typeof(UserLogin), out var userLogins) ? userLogins.OfType<UserLogin>() : Enumerable.Empty<UserLogin>();
		public virtual IEnumerable<User> Users => this.Instances.TryGetValue(typeof(User), out var users) ? users.OfType<User>() : Enumerable.Empty<User>();

		#endregion

		#region Methods

		protected internal virtual JsonSerializerSettings CreateJsonSerializerSettings(IContractResolver contractResolver, Formatting? formatting, NullValueHandling? nullValueHandling)
		{
			return new JsonSerializerSettings
			{
				ContractResolver = contractResolver ?? this.DefaultJsonContractResolver,
				Formatting = formatting ?? Formatting.Indented,
				NullValueHandling = nullValueHandling ?? NullValueHandling.Ignore
			};
		}

		public virtual string ToJson(IContractResolver contractResolver = null, Formatting? formatting = null, NullValueHandling? nullValueHandling = null)
		{
			var settings = this.CreateJsonSerializerSettings(contractResolver, formatting, nullValueHandling);

			return JsonConvert.SerializeObject(this, settings);
		}

		#endregion
	}
}