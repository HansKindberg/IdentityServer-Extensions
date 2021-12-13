using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Application.Models.Web.Authentication.Saml2p;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.AspNetCore.Authentication.Saml2p;

namespace Application
{
	public class Startup
	{
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

			services.AddAuthentication(options =>
				{
					options.DefaultChallengeScheme = Saml2PAuthenticationDefaults.AuthenticationScheme;
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				})
				.AddCookie()
				.AddSaml2p(Saml2PAuthenticationDefaults.AuthenticationScheme, options =>
				{
					const string samlAuthenticationSectionKey = "Authentication:Saml2p";
					this.Configuration.GetSection(samlAuthenticationSectionKey).Bind(options);

					if(options.IdentityProviderMetadataAddress == null)
					{
						options.IdentityProviderOptions ??= new IdpOptions();

						if(options.IdentityProviderOptions.EntityId != null)
						{
							options.IdentityProviderOptions.SingleLogoutEndpoint = new SamlEndpoint($"{options.IdentityProviderOptions.EntityId.TrimEnd('/')}/saml/slo", SamlBindingTypes.HttpRedirect);
							options.IdentityProviderOptions.SingleSignOnEndpoint = new SamlEndpoint($"{options.IdentityProviderOptions.EntityId.TrimEnd('/')}/saml/sso", SamlBindingTypes.HttpRedirect);
						}

						var identityProviderSigningCertificateValues = new List<string>();
						this.Configuration.GetSection($"{samlAuthenticationSectionKey}:{nameof(options.IdentityProviderOptions)}:{nameof(options.IdentityProviderOptions.SigningCertificates)}").Bind(identityProviderSigningCertificateValues);
						// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
						foreach(var identityProviderSigningCertificateValue in identityProviderSigningCertificateValues)
						{
							var identityProviderSigningCertificateBytes = Convert.FromBase64String(identityProviderSigningCertificateValue);
							var identityProviderSigningCertificate = new X509Certificate2(identityProviderSigningCertificateBytes);
							options.IdentityProviderOptions.SigningCertificates.Add(identityProviderSigningCertificate);
						}
						// ReSharper restore ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
					}

					options.ServiceProviderOptions ??= new SpOptions();
					var serviceProviderSigningCertificateValue = this.Configuration.GetSection($"{samlAuthenticationSectionKey}:{nameof(options.ServiceProviderOptions)}:{nameof(options.ServiceProviderOptions.SigningCertificate)}").Value;
					if(serviceProviderSigningCertificateValue != null)
					{
						var serviceProviderSigningCertificateBytes = Convert.FromBase64String(serviceProviderSigningCertificateValue);
						var serviceProviderSigningCertificate = new X509Certificate2(serviceProviderSigningCertificateBytes);
						options.ServiceProviderOptions.SigningCertificate = serviceProviderSigningCertificate;
					}

					//options.Events.OnTicketReceived = async context =>
					//{
					//	await Task.CompletedTask;

					//	// ReSharper disable PossibleNullReferenceException
					//	var claims = context.Principal.Claims.ToList();

					//	var authenticationInstantClaim = claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.AuthenticationInstant, StringComparison.OrdinalIgnoreCase));
					//	if(authenticationInstantClaim != null)
					//		claims.Remove(authenticationInstantClaim);

					//	var authenticationMethodClaim = claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.AuthenticationMethod, StringComparison.OrdinalIgnoreCase));
					//	if(authenticationMethodClaim != null)
					//		claims.Remove(authenticationMethodClaim);

					//	context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Principal.Identity.AuthenticationType));

					//	// ReSharper restore PossibleNullReferenceException
					//};

					options.SignInScheme = options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				});

			services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				// Avoid conflicts with the cookie for IdentityServer-client when this application is hosted on the same host/domain.
				options.Cookie.Name += this.Configuration.GetValue<string>("Authentication:Cookies:NameSuffix");
			});

			services.AddControllersWithViews();
		}

		#endregion
	}
}