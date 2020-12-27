using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Application.Models.Views.DataTransfer;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.Data.Extensions;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Json.Serialization;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RegionOrebroLan.Logging.Extensions;

namespace Application.Controllers
{
	[FeatureGate(Feature.DataTransfer)]
	public class DataTransferController : SiteController
	{
		#region Fields

		private static JsonSerializerSettings _exportJsonSerializerSettings;

		#endregion

		#region Constructors

		public DataTransferController(IConfigurationDbContext configurationDatabaseContext, IDataExporter dataExporter, IDataImporter dataImporter, IFacade facade) : base(facade)
		{
			this.ConfigurationDatabaseContext = configurationDatabaseContext ?? throw new ArgumentNullException(nameof(configurationDatabaseContext));
			this.DataExporter = dataExporter ?? throw new ArgumentNullException(nameof(dataExporter));
			this.DataImporter = dataImporter ?? throw new ArgumentNullException(nameof(dataImporter));
		}

		#endregion

		#region Properties

		protected internal virtual IConfigurationDbContext ConfigurationDatabaseContext { get; }
		protected internal virtual IDataExporter DataExporter { get; }
		protected internal virtual IDataImporter DataImporter { get; }

		protected internal virtual JsonSerializerSettings ExportJsonSerializerSettings => _exportJsonSerializerSettings ??= new JsonSerializerSettings
		{
			ContractResolver = new DataExportContractResolver(),
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		#endregion

		#region Methods

		protected internal virtual async Task<IConfiguration> CreateConfigurationAsync()
		{
			var configurationBuilder = new ConfigurationBuilder();
			var files = this.HttpContext.Request.Form.Files;

			if(!files.Any())
				return configurationBuilder.Build();

			const string key = nameof(ImportForm.Files);

			this.ModelState.Remove(key);

			var errors = new List<string>();

			foreach(var file in this.HttpContext.Request.Form.Files)
			{
				if(!string.Equals(file.ContentType, MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add(this.Localizer.GetString("errors/InvalidContentType", file.FileName));
					continue;
				}

				await using(var stream = file.OpenReadStream())
				{
					using(var streamReader = new StreamReader(stream))
					{
						var content = await streamReader.ReadToEndAsync();
						try
						{
							var jsonToken = JToken.Parse(content);

							if(jsonToken is JArray)
							{
								errors.Add(this.Localizer.GetString("errors/InvalidJsonArrayFile", file.FileName));
								continue;
							}
						}
						catch(JsonReaderException)
						{
							errors.Add(this.Localizer.GetString("errors/InvalidJsonFile", file.FileName));
							continue;
						}
					}
				}

				configurationBuilder.AddJsonStream(file.OpenReadStream());
			}

			foreach(var error in errors)
			{
				this.ModelState.AddModelError(key, error);
			}

			try
			{
				return configurationBuilder.Build();
			}
			catch(FormatException formatException)
			{
				this.Logger.LogErrorIfEnabled(formatException, "Could not build configuration.");
				this.ModelState.AddModelError(key, this.Localizer.GetString("errors/ConfigurationBuildException"));
				configurationBuilder.Sources.Clear();
				return configurationBuilder.Build();
			}
		}

		protected internal virtual ExportViewModel CreateExportViewModel(ExportForm form = null)
		{
			var model = new ExportViewModel();
			var selectedTypes = (form?.Types ?? Enumerable.Empty<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);

			foreach(var type in this.DataExporter.ExportableTypes.OrderBy(type => type.Name))
			{
				model.Form.TypeList.Add(new SelectListItem
				{
					Selected = selectedTypes.Contains(type.AssemblyQualifiedName),
					Text = type.Name,
					Value = type.AssemblyQualifiedName
				});
			}

			return model;
		}

		public virtual async Task<IActionResult> Export()
		{
			return await Task.FromResult(this.View(this.CreateExportViewModel()));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IActionResult> Export(ExportForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var model = this.CreateExportViewModel(form);

			// ReSharper disable InvertIf
			if(this.ModelState.IsValid)
			{
				try
				{
					var result = await this.DataExporter.ExportAsync(form.Types.Select(type => Type.GetType(type, true)));

					var json = JsonConvert.SerializeObject(result, this.ExportJsonSerializerSettings);

					return this.Content(json, MediaTypeNames.Application.Json);
				}
				catch(Exception exception)
				{
					this.Logger.LogErrorIfEnabled(exception, "Could not export.");
					this.ModelState.AddModelError(nameof(ImportForm.Files), this.Localizer.GetString("errors/ExportException"));
				}
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> Import()
		{
			return await Task.FromResult(this.View(new ImportViewModel()));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IActionResult> Import(ImportForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var configuration = await this.CreateConfigurationAsync();

			var model = new ImportViewModel {Form = form};

			// ReSharper disable InvertIf
			if(this.ModelState.IsValid)
			{
				try
				{
					var result = await this.DataImporter.ImportAsync(configuration, new ImportOptions {DeleteAllOthers = form.DeleteAllOthers, VerifyOnly = form.VerifyOnly});

					if(result.Errors.Any())
					{
						foreach(var error in result.Errors)
						{
							this.ModelState.AddModelError(nameof(ImportForm.Files), error);
						}
					}
					else
					{
						model.Confirmation = !form.VerifyOnly;
						model.Form = null;
						model.Result = result;
						this.ModelState.Clear();
					}
				}
				catch(Exception exception)
				{
					this.Logger.LogErrorIfEnabled(exception, "Could not import.");
					this.ModelState.AddModelError(nameof(ImportForm.Files), this.Localizer.GetString("errors/ImportException"));
				}
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(this.View(model));
		}

		public virtual async Task<IActionResult> Index()
		{
			var model = new DataTransferViewModel();

			model.ExistingData.Add(typeof(ApiResource).FriendlyName(), await this.ConfigurationDatabaseContext.ApiResources.CountAsync());
			model.ExistingData.Add(typeof(ApiResourceClaim).FriendlyName(), await this.ConfigurationDatabaseContext.ApiResourceClaims().CountAsync());
			model.ExistingData.Add(typeof(ApiResourceProperty).FriendlyName(), await this.ConfigurationDatabaseContext.ApiResourceProperties().CountAsync());
			model.ExistingData.Add(typeof(ApiResourceScope).FriendlyName(), await this.ConfigurationDatabaseContext.ApiResourceScopes().CountAsync());
			model.ExistingData.Add(typeof(ApiResourceSecret).FriendlyName(), await this.ConfigurationDatabaseContext.ApiResourceSecrets().CountAsync());
			model.ExistingData.Add(typeof(ApiScope).FriendlyName(), await this.ConfigurationDatabaseContext.ApiScopes.CountAsync());
			model.ExistingData.Add(typeof(ApiScopeClaim).FriendlyName(), await this.ConfigurationDatabaseContext.ApiScopeClaims().CountAsync());
			model.ExistingData.Add(typeof(ApiScopeProperty).FriendlyName(), await this.ConfigurationDatabaseContext.ApiScopeProperties().CountAsync());
			model.ExistingData.Add(typeof(Client).FriendlyName(), await this.ConfigurationDatabaseContext.Clients.CountAsync());
			model.ExistingData.Add(typeof(ClientCorsOrigin).FriendlyName(), await this.ConfigurationDatabaseContext.ClientCorsOrigins.CountAsync());
			model.ExistingData.Add(typeof(ClientClaim).FriendlyName(), await this.ConfigurationDatabaseContext.ClientClaims().CountAsync());
			model.ExistingData.Add(typeof(ClientGrantType).FriendlyName(), await this.ConfigurationDatabaseContext.ClientGrantTypes().CountAsync());
			model.ExistingData.Add(typeof(ClientIdPRestriction).FriendlyName(), await this.ConfigurationDatabaseContext.ClientIdentityProviderRestrictions().CountAsync());
			model.ExistingData.Add(typeof(ClientPostLogoutRedirectUri).FriendlyName(), await this.ConfigurationDatabaseContext.ClientPostLogoutRedirectUris().CountAsync());
			model.ExistingData.Add(typeof(ClientProperty).FriendlyName(), await this.ConfigurationDatabaseContext.ClientProperties().CountAsync());
			model.ExistingData.Add(typeof(ClientRedirectUri).FriendlyName(), await this.ConfigurationDatabaseContext.ClientRedirectUris().CountAsync());
			model.ExistingData.Add(typeof(ClientScope).FriendlyName(), await this.ConfigurationDatabaseContext.ClientScopes().CountAsync());
			model.ExistingData.Add(typeof(ClientSecret).FriendlyName(), await this.ConfigurationDatabaseContext.ClientSecrets().CountAsync());
			model.ExistingData.Add(typeof(IdentityResource).FriendlyName(), await this.ConfigurationDatabaseContext.IdentityResources.CountAsync());
			model.ExistingData.Add(typeof(IdentityResourceClaim).FriendlyName(), await this.ConfigurationDatabaseContext.IdentityResourceClaims().CountAsync());
			model.ExistingData.Add(typeof(IdentityResourceProperty).FriendlyName(), await this.ConfigurationDatabaseContext.IdentityResourceProperties().CountAsync());

			var identityContext = this.Facade.Identity.DatabaseContext;

			model.ExistingData.Add(typeof(IdentityRoleClaim<string>).FriendlyName(), await identityContext.RoleClaims.CountAsync());
			model.ExistingData.Add(typeof(IdentityUserClaim<string>).FriendlyName(), await identityContext.UserClaims.CountAsync());
			model.ExistingData.Add(typeof(IdentityUserLogin<string>).FriendlyName(), await identityContext.UserLogins.CountAsync());
			model.ExistingData.Add(typeof(IdentityUserRole<string>).FriendlyName(), await identityContext.UserRoles.CountAsync());
			model.ExistingData.Add(typeof(IdentityUserToken<string>).FriendlyName(), await identityContext.UserTokens.CountAsync());
			model.ExistingData.Add(typeof(Role).FriendlyName(), await identityContext.Roles.CountAsync());
			model.ExistingData.Add(typeof(User).FriendlyName(), await identityContext.Users.CountAsync());

			return await Task.FromResult(this.View(model));
		}

		#endregion
	}
}