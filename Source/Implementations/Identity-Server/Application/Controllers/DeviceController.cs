using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HansKindberg.IdentityServer.Application.Models.Views.Consent;
using HansKindberg.IdentityServer.Application.Models.Views.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	[Authorize]
	public class DeviceController : ConsentControllerBase
	{
		#region Fields

		private const string _consentViewPath = "~/Views/Consent/Index.cshtml";

		#endregion

		#region Constructors

		public DeviceController(IFacade facade, IDeviceFlowInteractionService deviceInteraction) : base(facade)
		{
			this.DeviceInteraction = deviceInteraction ?? throw new ArgumentNullException(nameof(deviceInteraction));
		}

		#endregion

		#region Properties

		protected internal virtual string ConsentViewPath => _consentViewPath;
		protected internal virtual IDeviceFlowInteractionService DeviceInteraction { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(AuthorizationRequest authorizationRequest, string userCode)
		{
			return await this.CreateConsentViewModelAsync(authorizationRequest, null, userCode);
		}

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(AuthorizationRequest authorizationRequest, ConsentForm postedForm, string userCode)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			var model = await this.CreateConsentViewModelAsync(authorizationRequest, postedForm);

			model.Dictionary[nameof(DeviceViewModel.UserCode)] = userCode;

			return model;
		}

		protected internal virtual async Task<DeviceViewModel> CreateDeviceViewModelAsync(string userCode, bool userCodeIsInvalid)
		{
			var model = new DeviceViewModel
			{
				UserCode = userCode,
				UserCodeIsInvalid = userCodeIsInvalid
			};

			return await Task.FromResult(model);
		}

		protected internal virtual async Task<AuthorizationRequest> GetAuthorizationRequestAsync(string userCode)
		{
			if(string.IsNullOrWhiteSpace(userCode))
				return null;

			var deviceAuthorizationRequest = await this.DeviceInteraction.GetAuthorizationContextAsync(userCode);

			if(deviceAuthorizationRequest == null)
				return null;

			return new AuthorizationRequest
			{
				Client = deviceAuthorizationRequest.Client,
				ValidatedResources = deviceAuthorizationRequest.ValidatedResources
			};
		}

		public virtual async Task<IActionResult> Index(string userCode)
		{
			if(userCode != null)
			{
				var authorizationRequest = await this.GetAuthorizationRequestAsync(userCode);

				if(authorizationRequest != null)
					return this.View(this.ConsentViewPath, await this.CreateConsentViewModelAsync(authorizationRequest, userCode));
			}

			var model = await this.CreateDeviceViewModelAsync(userCode, userCode != null);

			return this.View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Index(ConsentForm form, string userCode)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var authorizationRequest = await this.GetAuthorizationRequestAsync(userCode);

			if(authorizationRequest == null)
				return this.View(await this.CreateDeviceViewModelAsync(userCode, userCode != null));

			if(form.Accept)
				await this.ValidateConsentAsync(authorizationRequest, form);

			if(!this.ModelState.IsValid)
				return this.View(this.ConsentViewPath, await this.CreateConsentViewModelAsync(authorizationRequest, form, userCode));

			var consentResponse = await (form.Accept ? this.AcceptConsentAsync(authorizationRequest, form) : this.RejectConsentAsync(authorizationRequest));

			await this.DeviceInteraction.HandleRequestAsync(userCode, consentResponse);

			return this.View("Confirmation");
		}

		#endregion
	}
}