using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Models.Views.Grants;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Collections.Generic.Extensions;

namespace Application.Controllers
{
	[Authorize]
	public class GrantsController : SiteController
	{
		#region Constructors

		public GrantsController(IFacade facade, IResourceStore resourceStore) : base(facade)
		{
			this.ResourceStore = resourceStore ?? throw new ArgumentNullException(nameof(resourceStore));
		}

		#endregion

		#region Properties

		protected internal virtual IResourceStore ResourceStore { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<GrantsViewModel> CreateGrantsViewModelAsync()
		{
			var model = new GrantsViewModel();
			var grants = await this.Facade.Interaction.GetAllUserGrantsAsync();

			foreach(var grant in grants)
			{
				var client = await this.Facade.ClientStore.FindClientByIdAsync(grant.ClientId);

				if(client == null)
					continue;

				var resources = await this.ResourceStore.FindResourcesByScopeAsync(grant.Scopes);

				var item = new GrantViewModel()
				{
					Client = client,
					Created = grant.CreationTime,
					Description = grant.Description,
					Expiration = grant.Expiration,
				};

				item.ApiScopes.Add(resources.ApiScopes.ToArray());
				item.IdentityResources.Add(resources.IdentityResources.ToArray());

				model.Grants.Add(item);
			}

			return model;
		}

		public virtual async Task<IActionResult> Index()
		{
			return this.View(await this.CreateGrantsViewModelAsync());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Revoke(string clientId)
		{
			await this.Facade.Interaction.RevokeUserConsentAsync(clientId);
			await this.Facade.Events.RaiseAsync(new GrantsRevokedEvent(this.User.GetSubjectId(), clientId));

			return this.RedirectToAction(nameof(this.Index));
		}

		#endregion
	}
}