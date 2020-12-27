using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Models.Views.Diagnostics;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.Json.Serialization;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using RegionOrebroLan.Web.Authentication.Extensions;

namespace Application.Controllers
{
	[FeatureGate(Feature.Diagnostics)]
	public class DiagnosticsController : SiteController
	{
		#region Fields

		private static JsonSerializerSettings _jsonSerializerSettings;

		#endregion

		#region Constructors

		public DiagnosticsController(IApplicationDiscriminator applicationDiscriminator, IConfiguration configuration, IFacade facade, IOptions<KeyManagementOptions> keyManagementOptions) : base(facade)
		{
			this.ApplicationConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.ApplicationDiscriminator = applicationDiscriminator ?? throw new ArgumentNullException(nameof(applicationDiscriminator));
			this.KeyManagementOptions = keyManagementOptions ?? throw new ArgumentNullException(nameof(keyManagementOptions));
		}

		#endregion

		#region Properties

		protected internal virtual IConfiguration ApplicationConfiguration { get; }
		protected internal virtual IApplicationDiscriminator ApplicationDiscriminator { get; }

		protected internal virtual JsonSerializerSettings JsonSerializerSettings => _jsonSerializerSettings ??= new JsonSerializerSettings
		{
			ContractResolver = new ContractResolver(),
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
			PreserveReferencesHandling = PreserveReferencesHandling.None,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		};

		protected internal virtual IOptions<KeyManagementOptions> KeyManagementOptions { get; }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> AuthenticationScheme(string id)
		{
			var model = new AuthenticationSchemeViewModel();

			var items = (await this.Facade.AuthenticationSchemeLoader.GetDiagnosticsAsync(this.HttpContext.RequestServices)).OrderBy(item => item.Key.Name).ToArray();

			model.AuthenticationSchemesMissing = !items.Any();

			foreach(var (authenticationScheme, options) in items.Where(item => id == null || string.Equals(item.Key.Name, id, StringComparison.OrdinalIgnoreCase)))
			{
				var json = options != null ? JsonConvert.SerializeObject(options, this.JsonSerializerSettings) : null;

				model.Items.Add(authenticationScheme, json);
			}

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> Configuration()
		{
			var model = new ConfigurationViewModel();

			if(this.ApplicationConfiguration is IConfigurationRoot configurationRoot)
			{
				foreach(var provider in configurationRoot.Providers)
				{
					var providerModel = new ConfigurationProviderViewModel
					{
						Heading = provider.GetType().Name
					};

					if(provider is FileConfigurationProvider fileConfigurationProvider)
					{
						var fullPath = fileConfigurationProvider.Source.FileProvider.GetFileInfo(fileConfigurationProvider.Source.Path).PhysicalPath;
						providerModel.Information = fileConfigurationProvider.Source.Path;

						if(System.IO.File.Exists(fullPath))
							providerModel.Value = await System.IO.File.ReadAllTextAsync(fullPath);
						else
							providerModel.Information += $" ({this.Localizer.GetString("Missing")})";
					}
					else
					{
						foreach(var child in provider.GetChildKeys(Enumerable.Empty<string>(), null))
						{
							providerModel.Value += child;

							if(provider.TryGet(child, out var value))
								providerModel.Value += " = " + value;

							providerModel.Value += Environment.NewLine;
						}
					}

					model.ConfigurationProviders.Add(providerModel);
				}
			}

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> DataProtection()
		{
			var model = new DataProtectionViewModel
			{
				ApplicationDiscriminator = this.ApplicationDiscriminator,
				KeyManagementOptions = this.KeyManagementOptions.Value
			};

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> Index()
		{
			var model = new DiagnosticsViewModel
			{
				ClientCertificate = this.HttpContext.Connection.ClientCertificate,
				ConnectionId = this.HttpContext.Connection.Id,
				LocalIpAddress = this.HttpContext.Connection.LocalIpAddress,
				LocalPort = this.HttpContext.Connection.LocalPort,
				MachineName = Environment.MachineName,
				RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress,
				RemotePort = this.HttpContext.Connection.RemotePort
			};

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> Options()
		{
			var model = new OptionsViewModel();

			model.Options.Add(typeof(ExtendedIdentityServerOptions), JsonConvert.SerializeObject(this.Facade.IdentityServer.Value, this.JsonSerializerSettings));

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> RequestHeaders()
		{
			return await Task.FromResult(this.View());
		}

		public virtual async Task<IActionResult> ResponseHeaders()
		{
			return await Task.FromResult(this.View());
		}

		#endregion
	}
}