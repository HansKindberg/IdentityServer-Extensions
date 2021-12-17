using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HansKindberg.IdentityServer.Application.Models.Views.Consent;
using HansKindberg.IdentityServer.Application.Models.Views.Device;
using HansKindberg.IdentityServer.Application.Models.Views.Device.Extensions;
using HansKindberg.IdentityServer.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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

		protected internal virtual async Task AddInvalidUserCodeErrorAsync(string userCode)
		{
			await Task.CompletedTask;

			this.ModelState.AddModelError(nameof(QueryStringKeys.UserCode), this.Localizer.GetString("errors/invalid-usercode", userCode));
		}

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(AuthorizationRequest authorizationRequest, string userCode)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			var model = await this.CreateConsentViewModelAsync(authorizationRequest, null, null);

			model.Form.UserCode(userCode);

			return model;
		}

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(AuthorizationRequest authorizationRequest, ConsentForm postedForm)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			var model = await this.CreateConsentViewModelAsync(authorizationRequest, postedForm, null);

			return model;
		}

		protected internal virtual async Task<DeviceViewModel> CreateDeviceViewModelAsync(string userCode)
		{
			var model = new DeviceViewModel
			{
				Form =
				{
					UserCode = userCode
				}
			};

			return await Task.FromResult(model);
		}

		protected internal virtual async Task<AuthorizationRequest> GetAuthorizationRequestAsync(string userCode)
		{
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
			if(!string.IsNullOrEmpty(userCode) && this.ModelState.IsValid)
			{
				var authorizationRequest = await this.GetAuthorizationRequestAsync(userCode);

				if(authorizationRequest != null)
					return this.View(this.ConsentViewPath, await this.CreateConsentViewModelAsync(authorizationRequest, userCode));

				await this.AddInvalidUserCodeErrorAsync(userCode);
			}

			var model = await this.CreateDeviceViewModelAsync(userCode);

			return this.View(nameof(this.Index), model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Index(ConsentForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var userCode = form.UserCode();

			var authorizationRequest = await this.GetAuthorizationRequestAsync(userCode);

			if(authorizationRequest == null)
				return this.View(nameof(this.Index), await this.CreateDeviceViewModelAsync(userCode));

			if(form.Accept)
				await this.ValidateConsentAsync(authorizationRequest, form);

			if(!this.ModelState.IsValid)
				return this.View(this.ConsentViewPath, await this.CreateConsentViewModelAsync(authorizationRequest, form));

			var consentResponse = await (form.Accept ? this.AcceptConsentAsync(authorizationRequest, form) : this.RejectConsentAsync(authorizationRequest));

			await this.DeviceInteraction.HandleRequestAsync(userCode, consentResponse);

			return this.View("Confirmation");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> UserCode(DeviceForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			return await this.Index(form.UserCode);
		}

		#endregion
	}
}