using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Application.Models.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Application
{
	public class Startup
	{
		#region Fields

		public const string OpenIdConnectScheme = "OpenIdConnect";

		#endregion

		#region Constructors

		public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
		}

		#endregion

		#region Properties

		protected internal virtual IConfiguration Configuration { get; }
		protected internal virtual IWebHostEnvironment HostEnvironment { get; }

		#endregion

		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			applicationBuilder
				.UseDeveloperExceptionPage()
				.UseStaticFiles()
				.UseRouting()
				.UseAuthentication()
				.UseAuthorization()
				.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

			services.Configure<RoleServiceOptions>(this.Configuration.GetSection(ConfigurationKeys.RoleServicePath));

			services.AddAuthentication(options =>
				{
					options.DefaultChallengeScheme = OpenIdConnectScheme;
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				})
				.AddCookie(options =>
				{
					options.Cookie.Name = "ASP.NET-Core-MVC-Windows";
				})
				.AddOpenIdConnect(OpenIdConnectScheme, options =>
				{
					this.Configuration.GetSection(OpenIdConnectScheme).Bind(options);

					options.ClaimActions.MapAllExcept("aud", "c_hash", "exp", "iat", "iss", "nbf", "nonce");

					var uiLocales = this.Configuration.GetSection($"{OpenIdConnectScheme}:{nameof(OpenIdConnectParameterNames.UiLocales)}").Value;

					if(uiLocales == null)
						return;

					options.Events.OnRedirectToIdentityProvider = async context =>
					{
						context.ProtocolMessage.UiLocales = uiLocales;

						await Task.CompletedTask;
					};

					options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
					{
						context.ProtocolMessage.UiLocales = uiLocales;

						await Task.CompletedTask;
					};
				});

			services.AddControllersWithViews();

			services.AddHttpClient();
		}

		#endregion
	}
}