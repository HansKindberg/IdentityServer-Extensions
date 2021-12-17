using System;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Application.Models.Views.Shared.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public class CultureCookieController : SiteController
	{
		#region Constructors

		public CultureCookieController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Delete(DeleteCultureCookieForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			this.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);

			return this.Redirect(await this.GetResolvedReturnUrlAsync(form.ReturnUrl));
		}

		protected internal virtual async Task<string> GetResolvedReturnUrlAsync(string returnUrl)
		{
			if(!string.IsNullOrWhiteSpace(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out var url) && !url.IsAbsoluteUri)
				return returnUrl;

			return await Task.FromResult("/");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Save(SaveCultureCookieForm form)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var options = this.Facade.RequestLocalization.CurrentValue;

			var comparison = StringComparison.OrdinalIgnoreCase;

			if(options.SupportedCultures.Any(culture => string.Equals(culture.Name, form.Culture, comparison)) && options.SupportedUICultures.Any(uiCulture => string.Equals(uiCulture.Name, form.UiCulture, comparison)))
			{
				this.HttpContext.Response.Cookies.Append(
					CookieRequestCultureProvider.DefaultCookieName,
					CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(form.Culture, form.UiCulture)),
					new CookieOptions { MaxAge = TimeSpan.FromDays(365) }
				);
			}

			return this.Redirect(await this.GetResolvedReturnUrlAsync(form.ReturnUrl));
		}

		#endregion
	}
}