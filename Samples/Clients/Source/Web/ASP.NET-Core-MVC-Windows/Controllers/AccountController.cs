using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
		public virtual async Task<IActionResult> SignIn(string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateReturnUrl(returnUrl);

			return await Task.FromResult(this.Challenge(new AuthenticationProperties {RedirectUri = returnUrl}, Startup.OpenIdConnectScheme));
		}

		public virtual async Task<IActionResult> SignOut(string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateReturnUrl(returnUrl);

			return await Task.FromResult(this.SignOut(new AuthenticationProperties {RedirectUri = returnUrl}, CookieAuthenticationDefaults.AuthenticationScheme, Startup.OpenIdConnectScheme));
		}

		#endregion
	}
}