using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Models;
using Application.Models.Configuration;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Application.Controllers
{
	public class HomeController : SiteController
	{
		#region Constructors

		public HomeController(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IOptions<RoleServiceOptions> roleServiceOptions) : base(loggerFactory)
		{
			this.HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			this.RoleServiceOptions = roleServiceOptions ?? throw new ArgumentNullException(nameof(roleServiceOptions));
		}

		#endregion

		#region Properties

		protected internal virtual IHttpClientFactory HttpClientFactory { get; }
		protected internal virtual IOptions<RoleServiceOptions> RoleServiceOptions { get; }

		#endregion

		#region Methods

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		public virtual async Task<IActionResult> Error()
		{
			return await Task.FromResult(this.View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier}));
		}

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		public virtual async Task<IActionResult> Privacy()
		{
			return await Task.FromResult(this.View());
		}

		[Authorize]
		public virtual async Task<IActionResult> Roles()
		{
			var token = await this.HttpContext.GetTokenAsync("access_token");

			using(var httpClient = this.HttpClientFactory.CreateClient())
			{
				httpClient.SetBearerToken(token);

				var response = await httpClient.GetStringAsync(new Uri(this.RoleServiceOptions.Value.Url));
				this.ViewBag.Json = JArray.Parse(response).ToString();
			}

			return this.View();
		}

		#endregion
	}
}