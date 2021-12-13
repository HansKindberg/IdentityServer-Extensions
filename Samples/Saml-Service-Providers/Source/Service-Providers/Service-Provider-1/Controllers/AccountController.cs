using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Application.Models.Views.Account;
using Application.Models.Web.Authentication.Saml2p;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rsk.AspNetCore.Authentication.Saml2p;

namespace Application.Controllers
{
	[Authorize]
	public class AccountController : SiteController
	{
		#region Constructors

		public AccountController(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		[AllowAnonymous]
		public virtual async Task<IActionResult> AccessDenied()
		{
			return await Task.FromResult(this.View());
		}

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		[AllowAnonymous]
		public virtual async Task<IActionResult> SignedOut(bool locally)
		{
			if(this.User?.Identity != null && this.User.Identity.IsAuthenticated)
				return this.Redirect("/");

			return await Task.FromResult(this.View(new SignedOutViewModel { Locally = locally }));
		}

		[AllowAnonymous]
		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IActionResult> SignIn(bool forceAuthentication, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateReturnUrlAsync(returnUrl);

			var authenticationProperties = new SamlChallengeProperties
			{
				ForceAuthentication = forceAuthentication,
				RedirectUri = returnUrl
			};

			return await Task.FromResult(this.Challenge(authenticationProperties, Saml2PAuthenticationDefaults.AuthenticationScheme));
		}

		public virtual async Task<IActionResult> SignOut(bool locally)
		{
			var model = new SignOutViewModel
			{
				Form =
				{
					Locally = locally
				}
			};
			return await Task.FromResult(this.View(model));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> SignOut(SignOutForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			// ReSharper disable InvertIf
			if(form.Locally)
			{
				await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				return this.Redirect(this.Url.Action("SignedOut", new { Locally = true }));
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(this.SignOut(new AuthenticationProperties { RedirectUri = this.Url.Action("SignedOut") }, CookieAuthenticationDefaults.AuthenticationScheme, Saml2PAuthenticationDefaults.AuthenticationScheme));
		}

		#endregion
	}
}